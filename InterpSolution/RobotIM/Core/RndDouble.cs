using MyRandomGenerator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotIM.Core {
    public abstract class RndDouble {
        protected MyRandom _rnd = new MyRandom();
        public RndDouble(double MO, double SKO) {
            this.MO = MO;
            this.SKO = SKO;
            Reset();
        }

        public double SKO { get; protected set; }
        public double MO { get; protected set; }
        public double Value;

        public double Reset() {
            Value = ResetFunc();
            return Value;
        }
        public abstract double ResetFunc();
    }

    public class NormDouble : RndDouble {
        public NormDouble(double MO, double SKO) : base(MO, SKO) {
        }

        public override double ResetFunc() {
            return _rnd.GetNorm(MO, SKO);
        }
    }

    public class UintDouble : RndDouble {
        public double X1 { get; protected set; }
        public double X2 { get; protected set; }
        public UintDouble(double X1, double X2) : base(X1, X2) {
            this.X1 = X1;
            this.X2 = X2;
        }

        public override double ResetFunc() {
            return _rnd.GetDouble(X1, X2);
        }
    }
}
