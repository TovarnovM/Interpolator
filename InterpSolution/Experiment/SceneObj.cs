using Interpolator;
using Microsoft.Research.Oslo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Experiment {
    public delegate bool FlagFunct(params SolPoint[] sp);

    public interface IScnObj : INamedChild {
        List<string> AllParamsNames { get; }
        IEnumerable<IScnPrm> GetAllParams();
        Vector GetAllParamsValues(double t,Vector y);
        List<IScnObj> Children { get; }
        IScnPrm FindParam(string paramName);
        int AddParam(IScnPrm prm);
        void RemoveParam(IScnPrm prm);
        void AddChild(IScnObj child);
        void RemoveChild(IScnObj child);
        Vector Rebuild(double toTime);
        IEnumerable<IScnPrm> GetDiffPrms();
        void AddDiffPropToParam(IScnPrm prm,IScnPrm dPrmDt,bool removeOldDt,bool getNewName);
        int ResetAllParams();
        void ResetParam(string nameOfProp);
        void SynchMe(double t);
        void RebuildStruct();
        Action<double> SynchMeBefore { get; set; }
        Action<double> SynchMeAfter { get; set; }
        Action RebuildStructureAction { get; set; }
        void SetParam(string name,object value);

        List<ILaw> Laws { get; }
        void AddLaw(ILaw newLaw);
        bool ApplyLaws();
        Vector f(double t,Vector y);

        Dictionary<string,FlagFunct> FlagDict { get; }
    }

    public class ScnObjDummy : NamedChild, IScnObj {
        public IScnPrm[] DiffArr { get; private set; }
        public int DiffArrN { get; private set; } = -1;
        public List<IScnPrm> Prms { get; set; } = new List<IScnPrm>();
        public List<IScnObj> Children { get; set; } = new List<IScnObj>();
        public List<string> AllParamsNames { get; set; } = new List<string>();
        public List<ILaw> Laws { get; } = new List<ILaw>();
        public Action<double> SynchMeBefore { get; set; } = null;
        public Action<double> SynchMeAfter { get; set; } = null;
        public Action RebuildStructureAction { get; set; } = null;
        public Dictionary<string,FlagFunct> FlagDict { get; set; } = new Dictionary<string,FlagFunct>();

        public void SynchMeTo(double t,ref Vector y) {
            //if(DiffArrN != y.Length)
            //    throw new InvalidOperationException("разные длины векторов in out");
            for(int i = 0; i < DiffArrN; i++) {
                DiffArr[i].SetVal(y[i]);
            }
            SynchMe(t);
        }

        public Vector f(double t,Vector y) {
            SynchMeTo(t,ref y);
            var result = Vector.Zeros(DiffArrN);
            for(int i = 0; i < DiffArrN; i++) {
                result[i] = DiffArr[i].MyDiff.GetVal(t);
            }
            return result;
        }

        public Vector Rebuild(double toTime = 0.0d) {
            RebuildStruct();
            if(!ApplyLaws())
                throw new Exception("Структура неправильная. Невозможно реализовать все Laws");

            AllParamsNames.Clear();
            foreach(var prm in GetAllParams()) {
                AllParamsNames.Add(prm.FullName);
            }

            var diffArrSeq = GetDiffPrms();
            foreach(var child in Children) {
                diffArrSeq = diffArrSeq.Concat(child.GetDiffPrms());
            }
            DiffArr = diffArrSeq.ToArray();
            DiffArrN = DiffArr.Length;
            return new Vector(diffArrSeq.Select(prm => prm.GetVal(toTime)).ToArray());
        }

        public bool ApplyLaws() {
            foreach(var law in Laws) {
                if(!law.ApplyMe())
                    return false;
            }

            foreach(var child in Children) {
                if(!child.ApplyLaws())
                    return false;
            }

            return true;
        }

        public IEnumerable<IScnPrm> GetDiffPrms() {
            return Prms.Where(a => a.MyDiff != null);
        }

        public void SynchMe(double t) {

            for(int i = 0; i < Prms.Count; i++) {
                if(Prms[i].IsNeedSynch)
                    Prms[i].SetVal(t);
            }

            SynchMeBefore?.Invoke(t);

            foreach(var child in Children) {
                child.SynchMe(t);
            }

            SynchMeAfter?.Invoke(t);
        }

        public int AddParam(IScnPrm prm) {
            if(!Prms.Contains(prm)) {
                if(prm.Owner != null && prm.Owner != this)
                    prm.Owner.RemoveParam(prm);

                Prms.Add(prm);
                prm.Owner = this;
            }
            return Prms.Count;
        }
        public void AddDiffPropToParam(IScnPrm prm,IScnPrm dPrmDt,bool removeOldDt = true,bool getNewName = false) {
            if(!Prms.Contains(prm))
                AddParam(prm);
            if(removeOldDt && prm.MyDiff != null && Prms.Contains(prm.MyDiff))
                Prms.Remove(prm.MyDiff);
            if(getNewName)
                dPrmDt.Name = prm.Name + "'";
            prm.MyDiff = dPrmDt;
            if(dPrmDt.Owner == null)
                AddParam(dPrmDt);
        }
        public void RemoveParam(IScnPrm prm) {
            if(Prms.Contains(prm)) {
                Prms.Remove(prm);
                prm.GetVal = null;
                prm.SetVal = null;
                prm.MyDiff = null;
            }

        }
        public void RemoveParam(String prmName) {
            var pX = FindParam(prmName);
            if(pX != null)
                RemoveParam(pX);
        }
        public IScnPrm FindParam(string paramName) {
            var reg = new Regex("\\b" + paramName,RegexOptions.IgnoreCase);
            return Prms.Where(elem => reg.IsMatch(elem.Name)).FirstOrDefault();
        }

        public void AddChild(IScnObj newChild) {
            IScnObj hasWThisName = null;
            int ind = 0;
            do {
                hasWThisName = Children.FirstOrDefault(ch => ch.Name == newChild.Name);
                newChild.Name =
                    hasWThisName != null ?
                    Regex.Replace(hasWThisName.Name,@"\d+$","") + (++ind).ToString() :
                    newChild.Name;

            } while(hasWThisName != null);

            Children.Add(newChild);
            newChild.Owner = this;
        }
        public void RemoveChild(IScnObj child) {
            if(Children.Remove(child))
                child.Owner = null;
        }

        public Vector GetAllParamsValues(SolPoint sp) {
            return GetAllParamsValues(sp.T,sp.X);
        }
        public Vector GetAllParamsValues(double t,Vector y) {
            SynchMeTo(t,ref y);
            var res = Vector.Zeros(AllParamsNames.Count());
            int i = 0;
            foreach(var prm in GetAllParams()) {
                res[i++] = prm.GetVal(t);
            }
            return res;
        }
        public IEnumerable<IScnPrm> GetAllParams() {
            var res = Prms.Select(prm => prm);
            foreach(var child in Children) {
                res = res.Concat(child.GetAllParams());
            }
            return res;
        }

        public void AddLaw(ILaw newLaw) {
            var alredyHas = Laws.FirstOrDefault(lw => lw.Name == newLaw.Name);

            if(alredyHas != null)
                throw new KeyNotFoundException("Такой закон уже тут есть!");

            Laws.Add(newLaw);
            newLaw.Owner = this;

        }

        public void ResetParam(string nameOfProp) {
            RemoveParam(nameOfProp);
            var currVal = (double)this.GetType().GetProperty(nameOfProp).GetGetMethod().Invoke(this,null);

            var scnParam = new ScnPrm(nameOfProp,this,currVal);
            this.GetType().GetProperty("p" + nameOfProp).GetSetMethod().Invoke(this,new object[] { scnParam });

            var GetDeleg = (Func<double>)this.GetType().GetProperty(nameOfProp).GetGetMethod().CreateDelegate(typeof(Func<double>),this);
            var SetDeleg = (Action<double>)this.GetType().GetProperty(nameOfProp).GetSetMethod().CreateDelegate(typeof(Action<double>),this);

            scnParam.GetVal = t => GetDeleg();
            scnParam.SetVal = SetDeleg;
        }

        public int ResetAllParams() {
            var propsNames =
                from prInf in this.GetType().GetProperties()
                join prInfP in this.GetType().GetProperties()
                on "p" + prInf.Name equals prInfP.Name
                select prInf.Name;
            int i = 0;
            foreach(var propname in propsNames) {
                ResetParam(propname);
                ++i;
            }
            return i;


        }

        public void RebuildStruct() {
            RebuildStructureAction?.Invoke();
            foreach(var child in Children) {
                child.RebuildStruct();
            }
        }

        public ScnObjDummy() {
            FlagDict.Add("TimeLimit",TimeFlag);
            ResetAllParams();
        }

        public void SetParam(string name,object value) {
            var param = this.FindParam(name);
            if(param == null)
                return;
            if(IsNumber(value)) {
                param.SetVal(ToDouble(value));
            } else if(value is InterpXY) {
                param.SealInterp((InterpXY)value);
            }
        }

        public static bool IsNumber(object value) {
            return value is sbyte
                    || value is byte
                    || value is short
                    || value is ushort
                    || value is int
                    || value is uint
                    || value is long
                    || value is ulong
                    || value is float
                    || value is double
                    || value is decimal;
        }

        public static double ToDouble(object value) {
            return value is sbyte ? (sbyte)value:
                   value is byte ? (byte)value :
                   value is short ? (short)value :
                   value is ushort ? (ushort)value :
                   value is int ? (int)value :
                   value is uint ? (uint)value :
                   value is long ? (long)value :
                   value is ulong ? (ulong)value :
                   value is float ? (float)value :
                   value is double ? (double)value :
                   value is decimal ? Decimal.ToDouble((decimal)value) :
                   0d;
        }

        public double TimeLimit { get; set; } = 300d;
        public bool TimeFlag(params SolPoint[] sp) {
            return sp[0].T >= TimeLimit;
        }
    }
}
