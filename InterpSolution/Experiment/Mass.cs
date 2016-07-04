namespace Experiment {
    public interface IMass {
        double Value { get; set; }
        IScnPrm pValue { get; set; }
    }

    public class Mass : ScnObjDummy, IMass {
        public double Value { get; set; }
        public IScnPrm pValue { get; set; }

        public static string DefName = "Mass";
        public Mass(double value) {
            Name = DefName;
            Value = value;
            Resetparams();
        }

        public override void Resetparams() {
            RemoveParam("Value");
            pValue = new ScnPrm("Value",this,Value);
            pValue.GetVal = t => Value;
            pValue.SetVal = val => Value = val;
        }
    }
}
