using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EqOptimizer.Data;
using EqOptimizer.Equations;

namespace EqOptimizer.Criterias {
    public class KolomSmirn : CriteriaBase {
        public override double GetCriteria(EquationBase equation,MultyData data) {
            double maxError = 0d;
            for (int i = 0; i < data.Count; i++) {
                var answer = equation.GetSolution(data[i].vars);
                var delta = answer - data[i].answer;
                var delta2 = delta*delta;
                if (delta2 > maxError)
                    maxError = delta2;
            }
            return maxError;
        }
    }
}
