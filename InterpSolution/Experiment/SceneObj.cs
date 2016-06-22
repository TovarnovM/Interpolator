using Microsoft.Research.Oslo;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Experiment {

    interface IScnObj : INamedChild {
        List<IScnObj> Children { get; }
        int AddParam(IScnPrm prm);
        void RemoveParam(IScnPrm prm);
        void Rebuild();
        IEnumerable<IScnPrm> GetDiffPrms();
        void SynchMe(double t);
        void AddDiffPropToParam(IScnPrm prm, IScnPrm dPrmDt, bool removeOldDt, bool getNewName);
    }

    class ScnObjDummy : IScnObj {
        public IScnPrm[] DiffArr { get; private set; }
        public int DiffArrN { get; private set; } = -1;
        public List<IScnPrm> Prms { get; set; } = new List<IScnPrm>();
        public List<IScnObj> Children { get; set; } = new List<IScnObj>();
        public IScnObj Owner { get; set; }

        public string Name { get; set; }

        public Vector f(double t, Vector y) {
            if (DiffArrN != y.Length)
                throw new InvalidOperationException("разные длины векторов in out");
            var result = Vector.Zeros(y.Length);
            for (int i = 0; i < DiffArrN; i++) {
                DiffArr[i].SetVal(y[i]);
            }

            SynchMe(t);

            for (int i = 0; i < DiffArrN; i++) {
                result[i] = DiffArr[i].MyDiff.GetVal(t);
            }
            return result;
        }

        public void Rebuild() {
            var diffArrSeq = GetDiffPrms();
            foreach (var child in Children) {
                diffArrSeq = diffArrSeq.Concat(child.GetDiffPrms());
            }
            DiffArr = diffArrSeq.ToArray();
            DiffArrN = DiffArr.Length;
        }

        public IEnumerable<IScnPrm> GetDiffPrms() {
            return Prms.Where(a => a.MyDiff != null);
        }

        public int AddParam(IScnPrm prm) {
            if (Prms.Contains(prm))
                throw new IndexOutOfRangeException("Такой параметр уже есть!");
            if (prm.Owner != null && prm.Owner != this)
                prm.Owner.RemoveParam(prm);

            Prms.Add(prm);
            prm.Owner = this;
            return Prms.Count;
        }

        public virtual void SynchMe(double t) {
            foreach (var child in Children) {
                SynchMe(t);
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

            AddParam(dPrmDt);


        }

        public void RemoveParam(IScnPrm prm) {
            if (Prms.Contains(prm))
                Prms.Remove(prm);
        }
    }
}
