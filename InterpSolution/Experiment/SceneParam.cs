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
        Action<double> SetVal { get; set; }
        /// <summary>
        /// Значение параметра (последнего синхронизированного)
        /// </summary>
        IScnPrm MyDiff { get; set; }
    }

    interface INamedChild {
        string Name { get; set; }
        /// <summary>
        /// Хозяин параметра
        /// </summary>
        IScnObj Owner { get; set; }
    }

    class ScnPrm : IScnPrm {
        private double _value;
        public IScnPrm MyDiff { get; set; } = null;
        public double GetValMethod(double t) {
            return _value;
        }
        public Func<double, double> GetVal { get; set; }
        public bool IsDiff { get; private set; } = false;
        public string Name { get; set; }
        public IScnObj Owner { get; set; }
        public void SetValMethod(double val) {
            _value = val;
        }
        public Action<double> SetVal { get; set; }

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


    }
}
