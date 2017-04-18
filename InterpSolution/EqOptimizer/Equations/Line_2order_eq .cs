using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EqOptimizer.Equations {
    public class Line_2order_eq : EquationBase {
        public Line_2order_eq(int varsCount) : base(varsCount,2*varsCount + (varsCount* varsCount - varsCount) /2 +1) {
            var sb = new StringBuilder();
            for (int i = 0; i < varNames.Count; i++) {
                sb.Append($"{parNames[i]}*{varNames[i]}^2 + ");
            }
            int ind = varNames.Count;
            for (int i = 0; i < varNames.Count; i++) {
                for (int j = i+1; j < varNames.Count; j++) {
                    sb.Append($"{parNames[ind++]}*{varNames[i]}*{varNames[j]} + ");
                }
                
            }
            for (int i = 0; i < varNames.Count; i++) {
                sb.Append($"{parNames[ind++]}*{varNames[i]} + ");
            }
            sb.Append(parNames[ind]);
            ps = sb.ToString();
        }
        string ps;
        public override string PatternStr => ps;

        //public override string EqStr {
        //    get {
        //        var sb = new StringBuilder();
        //        for (int i = 0; i < varNames.Count; i++) {
        //            string sig = pars[i+1] >= 0d ? "+" : "";
        //            sb.Append($"{pars[i]}*{varNames[i]}^2 {sig} ");
        //        }
        //        int ind = varNames.Count;
        //        for (int i = 0; i < varNames.Count; i++) {
        //            for (int j = i + 1; j < varNames.Count; j++) {
        //                string sig = pars[ind+1] >= 0d ? "+" : "";
        //                sb.Append($"{pars[ind++]}*{varNames[i]}*{varNames[j]} {sig} ");
        //            }

        //        }
        //        for (int i = 0; i < varNames.Count; i++) {
        //            string sig = pars[ind+1] >= 0d ? "+" : "";
        //            sb.Append($"{pars[ind++]}*{varNames[i]} {sig} ");
        //        }
        //        sb.Append(pars[ind]);
        //        return sb.ToString();
        //    }
        //}

        public override double GetSolution(params double[] vars) {
            if (vars.Length != VarsCount)
                throw new ArgumentOutOfRangeException();
            double answ = 0d;
            for (int i = 0; i < varNames.Count; i++) {
                answ += pars[i] * vars[i] * vars[i];
            }
            int ind = varNames.Count;
            for (int i = 0; i < varNames.Count; i++) {
                for (int j = i + 1; j < varNames.Count; j++) {
                    answ += pars[ind++] * vars[i] * vars[j];
                }

            }
            for (int i = 0; i < varNames.Count; i++) {
                answ += pars[ind++] * vars[i];
            }
            answ += pars[ind];
            return answ;
        }
    }
}
