using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EqOptimizer.Equations {
    public class Line_2order_eq : EquationBase {
        public Line_2order_eq(int varsCount,bool fillData = true) : base(varsCount,2*varsCount + (varsCount* varsCount - varsCount) /2 +1,fillData) {

        }

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
            for (int i = 0; i < VarNames.Count; i++) {
                answ += Pars[i] * vars[i] * vars[i];
            }
            int ind = VarNames.Count;
            for (int i = 0; i < VarNames.Count; i++) {
                for (int j = i + 1; j < VarNames.Count; j++) {
                    answ += Pars[ind++] * vars[i] * vars[j];
                }

            }
            for (int i = 0; i < VarNames.Count; i++) {
                answ += Pars[ind++] * vars[i];
            }
            answ += Pars[ind];
            return answ;
        }

        public override string CreatePatternStr() {
            var sb = new StringBuilder();
            for (int i = 0; i < VarNames.Count; i++) {
                sb.Append($"{ParNames[i]}*{VarNames[i]}^2 + ");
            }
            int ind = VarNames.Count;
            for (int i = 0; i < VarNames.Count; i++) {
                for (int j = i + 1; j < VarNames.Count; j++) {
                    sb.Append($"{ParNames[ind++]}*{VarNames[i]}*{VarNames[j]} + ");
                }

            }
            for (int i = 0; i < VarNames.Count; i++) {
                sb.Append($"{ParNames[ind++]}*{VarNames[i]} + ");
            }
            sb.Append(ParNames[ind]);
            return sb.ToString();
        }

        public override EquationBase Clone() {
            var c = new Line_2order_eq(VarsCount,false);
            c.ps = ps;
            c.VarNames.AddRange(VarNames);
            c.ParNames.AddRange(ParNames);
            c.Pars.AddRange(Pars);
            return c;
        }
    }
}
