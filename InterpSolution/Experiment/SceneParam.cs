using System;


namespace Experiment {

    public delegate void FillDtFunc(double t, ref double[] y, out double[] dy);

    /// <summary>
    /// Параметр системы
    /// </summary>
    /// <typeparam name="T">Тип параметра</typeparam>
    interface IScnPrm : INamedChild {
        /// <summary>
        /// Получить значение параметра, синхонизированного по времени t
        /// </summary>
        /// <returns>значение параметра</returns>
        Func<double, double> GetVal { get; set; }
        Func<double, object> SetVal { get; set; }
        /// <summary>
        /// Значение параметра (последнего синхронизированного)
        /// </summary>
        double Val { get; }
        IScnPrm MyDiff { get; set; }
    }

    interface INamedChild {
        string Name { get; set; }
        /// <summary>
        /// Хозяин параметра
        /// </summary>
        IScnObj Owner { get; set; }
    }

    class ScnPrmConst : IScnPrm {
        private double _value;
        public IScnPrm MyDiff { get; set; } = null;
        public double GetValMethod(double t) {
            return _value;
        }
        public Func<double, double> GetVal { get; set; }
        public bool IsDiff { get; private set; } = false;
        public string Name { get; set; }
        public IScnObj Owner { get; set; }
        public object SetValmethod(double val) {
            _value = val;
            return null;
        }
        public Func<double, object> SetVal { get; set; }
        public double Val {
            get {
                return _value;
            }
            set {
                _value = value;
            }
        }
        public ScnPrmConst(string name, IScnObj owner, double val = 0.0) {
            Name = name;
            Owner = owner;
            Val = val;
            GetVal = this.GetValMethod;
            SetVal = this.SetValmethod;
        }

    }
}
