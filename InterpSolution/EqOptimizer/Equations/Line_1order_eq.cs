using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EqOptimizer.Equations {
    public class Line_1order_eq : EquationBase {
        public Line_1order_eq(int varsCount, bool fillParVarsPs = true) : base(varsCount,varsCount + 1,fillParVarsPs) {

        }

        //public override string EqStr {
        //    get {
        //        var sb = new StringBuilder();
        //        for (int i = 0; i < varNames.Count; i++) {
        //            string sig = pars[i+1] >= 0d ? "+" : "";
        //            sb.Append($"{pars[i]}*{varNames[i]} {sig} ");
        //        }
        //        sb.Append(pars.Last());
        //        return sb.ToString();
        //    }
        //}

        public override double GetSolution(params double[] vars) {
            if (vars.Length != VarsCount)
                throw new ArgumentOutOfRangeException();
            double answ = 0d;
            for (int i = 0; i < vars.Length; i++) {
                answ += Pars[i] * vars[i];
            }
            answ += Pars.Last();
            return answ;

        }

        public override EquationBase Clone() {
            var c = new Line_1order_eq(VarsCount,false);
            c.ps = ps;
            c.VarNames.AddRange(VarNames);
            c.ParNames.AddRange(ParNames);
            c.Pars.AddRange(Pars);
            return c;
        }

        public override string CreatePatternStr() {
            var sb = new StringBuilder();
            for (int i = 0; i < VarNames.Count; i++) {
                sb.Append($"{ParNames[i]}*{VarNames[i]} + ");
            }
            sb.Append(ParNames.Last());
            return sb.ToString();
        }
    }
}
