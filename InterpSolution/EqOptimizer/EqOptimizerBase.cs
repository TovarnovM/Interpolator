using EqOptimizer.Data;
using EqOptimizer.Criterias;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EqOptimizer.Equations;

namespace EqOptimizer {
    public abstract class EqOptimizerBase {
        public MultyData Data { get; set; }
        public CriteriaBase Crit { get; set; }
        public EquationBase EquationInit { get; set; }
        public EqOptimizerBase(EquationBase Equation,MultyData Data,CriteriaBase Crit) {
            this.EquationInit = Equation;
            this.Data = Data;
            this.Crit = Crit;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>optimizated params</returns>
        public abstract (EquationBase eq, double crit) PerformOptimization();
        public Task<(EquationBase eq, double crit)> PerformOptimizationAsync() {
            return Task.Factory.StartNew(() => PerformOptimization());
        }
    }
}
