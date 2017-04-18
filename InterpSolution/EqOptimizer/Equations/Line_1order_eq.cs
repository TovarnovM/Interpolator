using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EqOptimizer.Equations {
    public class Line_1order_eq : EquationBase {
        public Line_1order_eq(int varsCount) : base(varsCount,varsCount + 1) {
            var sb = new StringBuilder();
            for (int i = 0; i < varNames.Count; i++) {
                sb.Append($"{parNames[i]}*{varNames[i]} + ");
            }
            sb.Append(parNames.Last());
            ps = sb.ToString();
        }
        string ps;
        public override string PatternStr => ps;

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
                answ += pars[i] * vars[i];
            }
            answ += pars.Last();
            return answ;

        }
    }
}
