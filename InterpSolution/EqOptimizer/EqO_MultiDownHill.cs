using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EqOptimizer.Criterias;
using EqOptimizer.Data;
using EqOptimizer.Equations;
using MoreLinq;

namespace EqOptimizer {
    public class EqO_MultiDownHill : EqO_TaskExecutor_fitness {
        public double minPar = -100000, maxPar = 100000;
        public int iterations = 100;
        public double eps = 1e-10;
        public EqO_MultiDownHill(EquationBase Equation, MultyData Data, CriteriaBase Crit, double minPar = -100000, double maxPar = 100000) : base(Equation, Data, Crit) {
            this.minPar = minPar;
            this.maxPar = maxPar;
        }

        public override (EquationBase eq, double crit) PerformOptimization() {
            var tasks = Enumerable
                .Range(0, iterations)
                .Select(i => ConvertFrom(GetNewChromo()))
                .Select(eq =>
                    new Task<(EquationBase eq, double crit)>(
                        state => {
                            var opt = new EqO_downhill(state as EquationBase, Data, Crit, minPar, maxPar);
                            opt.Multithread = false;
                            opt.epsFitn = eps;
                            return opt.PerformOptimization();
                        },
                    eq, TaskCreationOptions.LongRunning))
               .ToArray();
            foreach (var t in tasks) {
                t.Start();
            }
            Task.WaitAll(tasks);
            return tasks
                .Select(t => t.Result)
                .MinBy(tup => Crit.GetMaxError(tup.eq, Data));

        }
    }
}
