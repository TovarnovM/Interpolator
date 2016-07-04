using Microsoft.Research.Oslo;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Experiment {

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
        void SynchMe(double t);
        void AddDiffPropToParam(IScnPrm prm, IScnPrm dPrmDt, bool removeOldDt, bool getNewName);
        void Resetparams();
    }

    public abstract class ScnObjDummy : IScnObj {
        public IScnPrm[] DiffArr { get; private set; }
        public int DiffArrN { get; private set; } = -1;
        public List<IScnPrm> Prms { get; set; } = new List<IScnPrm>();
        public List<IScnObj> Children { get; set; } = new List<IScnObj>();
        public IScnObj Owner { get; set; }

        public string Name { get; set; }

        public string FullName {
            get {
                return Owner != null ? Owner.FullName + '.' + Name : Name;
            }
        }

        public List<string> AllParamsNames { get; set; } = new List<string>();

        public void SynchMeTo(double t,ref Vector y) {
            if (DiffArrN != y.Length)
                throw new InvalidOperationException("разные длины векторов in out");
     
            for (int i = 0; i < DiffArrN; i++) {
                DiffArr[i].SetVal(y[i]);
            }

            SynchMe(t);
        }

        public Vector f(double t, Vector y) {
            SynchMeTo(t,ref y);
            var result = Vector.Zeros(y.Length);
            for (int i = 0; i < DiffArrN; i++) {
                result[i] = DiffArr[i].MyDiff.GetVal(t);
            }
            return result;
        }

        public Vector Rebuild(double toTime = 0.0d) {
            AllParamsNames.Clear();
            foreach(var prm in GetAllParams()) {
                AllParamsNames.Add(prm.FullName);
            }

            var diffArrSeq = GetDiffPrms();
            foreach (var child in Children) {
                diffArrSeq = diffArrSeq.Concat(child.GetDiffPrms());
            }
            DiffArr = diffArrSeq.ToArray();
            DiffArrN = DiffArr.Length;
            return new Vector(diffArrSeq.Select(prm => prm.GetVal(toTime)).ToArray());
        }

        public IEnumerable<IScnPrm> GetDiffPrms() {
            return Prms.Where(a => a.MyDiff != null);
        }

        public int AddParam(IScnPrm prm) {
            if (!Prms.Contains(prm)) {
                if (prm.Owner != null && prm.Owner != this)
                    prm.Owner.RemoveParam(prm);

                Prms.Add(prm);
                prm.Owner = this;
            }
            return Prms.Count;
        }

        public virtual void SynchMe(double t) {

            for(int i = 0; i < Prms.Count; i++) {
                if(Prms[i].IsNeedSynch)
                    Prms[i].SetVal(t);
            }

            foreach(var child in Children) {
                child.SynchMe(t);
            }
        }

        public void AddDiffPropToParam(IScnPrm prm, IScnPrm dPrmDt, bool removeOldDt = true, bool getNewName = true) {
            if (!Prms.Contains(prm))
                AddParam(prm);
            if (removeOldDt && prm.MyDiff != null && Prms.Contains(prm.MyDiff))
                Prms.Remove(prm.MyDiff);
            if (getNewName)
                dPrmDt.Name = prm.Name + "'";
            prm.MyDiff = dPrmDt;
            if(dPrmDt.Owner == null)
                AddParam(dPrmDt);
        }

        public void RemoveParam(IScnPrm prm) {
            if (Prms.Contains(prm))
                Prms.Remove(prm);
        }

        public void RemoveParam(String prmName) {
            var pX = FindParam(prmName);
            if(pX != null)
                RemoveParam(pX);
        }


        public void AddChild(IScnObj child) {
            Children.Add(child);
            child.Owner = this;
        }

        public void RemoveChild(IScnObj child) {
            Children.Remove(child);
            child.Owner = null;
        }

        public IScnPrm FindParam(string paramName) {
            return Prms.Where(elem => elem.Name == paramName).FirstOrDefault();
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

        public abstract void Resetparams();
    }
}
