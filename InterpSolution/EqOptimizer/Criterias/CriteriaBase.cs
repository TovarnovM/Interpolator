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
    }
}
