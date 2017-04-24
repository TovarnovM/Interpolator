using EqOptimizer.Data;
using EqOptimizer.Equations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EqOptimizer.Criterias {
    public abstract class CriteriaBase {
        public abstract double GetCriteria(EquationBase equation,MultyData data);
        public double GetMaxError(EquationBase equation, MultyData data) {
            return data
                .Select(od => Math.Abs(equation.GetSolution(od.vars) - od.answer))
                .Max();
        }
    }
}
