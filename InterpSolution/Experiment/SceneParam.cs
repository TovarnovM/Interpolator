using System;
using Interpolator;


namespace Experiment {

    public delegate double SetValFunct(params double[] t);

    /// <summary>
    /// Параметр системы
    /// </summary>
    /// <typeparam name="T">Тип параметра</typeparam>
    public interface IScnPrm : INamedChild {
        /// <summary>
        /// Получить значение параметра, синхонизированного по времени t
        /// </summary>
        /// <returns>значение параметра</returns>
        Func<double, double> GetVal { get; set; }
        Action<double> SetVal { get; set; }
        /// <summary>
        /// Значение параметра (последнего синхронизированного)
        /// </summary>
        IScnPrm MyDiff { get; set; }
        void SealInterp(InterpXY interp);
        bool IsDiff { get; }
        bool IsNeedSynch { get; set; }
    }

    public interface INamedChild {
        string Name { get; set; }
        string FullName { get; }
        /// <summary>
        /// Хозяин параметра
        /// </summary>
        IScnObj Owner { get; set; }
    }

    public class ScnPrm : IScnPrm {
        private double _value;
        private IScnPrm myDiff; 
        public IScnPrm MyDiff {
            get { return myDiff; }
            set {
                myDiff = value;
                IsDiff = myDiff != null;
            }
        }

        public string Name { get; set; }
        public IScnObj Owner { get; set; }

        public Action<double> SetVal { get; set; }
        public Func<double, double> GetVal { get; set; }

        public void SetValMethod(double val) {
            _value = val;
        }
        public double GetValMethod(double t) {
            return _value;
        }

        public void SealInterp(InterpXY interp) {
            MyDiff = null;
            IsNeedSynch = true;
            Action<double> old = new Action<double>(SetVal);
            SetVal = t => old(interp.GetV(t));
        }

        public ScnPrm(string name, IScnObj owner, double val = 0.0) {
            Name = name;
            Owner = owner;
            _value = val;
            Owner?.AddParam(this);
            GetVal = this.GetValMethod;
            SetVal = this.SetValMethod;
        }
        public ScnPrm() {

        }

        public bool IsDiff { get; private set; } = false;

        public bool IsNeedSynch { get; set; } = false;

        public string FullName {
            get {
                return Owner != null ? Owner.FullName + '.' + Name : Name;
            }
        }
    }
}
