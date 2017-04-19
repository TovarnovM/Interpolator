using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EqOptimizer.Data {
    public struct OnePoint {
        public double[] vars;
        public double answer;
        public OnePoint(params double[] vars_and_answer) {
            vars = new double[vars_and_answer.Length - 1];
            Array.Copy(vars_and_answer,vars,vars.Length);
            answer = vars_and_answer[vars_and_answer.Length - 1];
        }
        public OnePoint(double answer, double[] vars) {
            this.vars = vars;
            this.answer = answer;
        }
    }
}
