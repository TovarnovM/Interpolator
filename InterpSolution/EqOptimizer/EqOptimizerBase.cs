using EqOptimizer.Data;
using EqOptimizer.Criterias;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EqOptimizer.Equations;
using DoubleEnumGenetic;
using GeneticSharp.Infrastructure.Framework.Threading;
using GeneticSharp.Infrastructure.Threading;
using GeneticSharp.Domain.Fitnesses;
using GeneticSharp.Domain.Chromosomes;

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

    public abstract class EqO_TaskExecutor_fitness : EqOptimizerBase, IFitness {
        public IList<IGeneDE> GInfo { get; set; }
        protected static SmartThreadPoolTaskExecutor getSTPTE() {
            return new SmartThreadPoolTaskExecutor() { MaxThreads = 8 };
        }
        protected ITaskExecutor taskEx = getSTPTE();
        private bool multiThread = true;

        public bool Multithread {
            get { return multiThread; }
            set {
                if (multiThread != value) {
                    if (value)
                        taskEx = getSTPTE();
                    else
                        taskEx = new LinearTaskExecutor();
                }
                multiThread = value;
            }
        }

        public ChromosomeD GetNewChromo() {
            var c = new ChromosomeD(GInfo);
            return c;

        }

        public ChromosomeD GetNewChromo(EquationBase eq) {
            var c = GetNewChromo();
            foreach (var name in GInfo.Select(gi=>gi.Name)) {
                c[name] = eq[name];
            }
            return c;
        }

        public double Evaluate(IChromosome chromosome) {
            return -Crit.GetCriteria(ConvertFrom(chromosome), Data);
        }

        protected EquationBase ConvertFrom(IChromosome chromosome) {
            var c = chromosome as ChromosomeD;
            if (c == null)
                throw new Exception("Не та хромосома");
            var pars = new double[EquationInit.ParsCount];
            for (int i = 0; i < EquationInit.ParsCount; i++) {
                var pn = EquationInit.ParNames[i];
                pars[i] = c[pn];
            }

            return EquationInit.Clone(pars);
        }



        public EqO_TaskExecutor_fitness(EquationBase Equation, MultyData Data, CriteriaBase Crit, double minPar = -1e5, double maxPar = 1e5) : base(Equation, Data, Crit) {
            GInfo = new List<IGeneDE>(Equation.ParsCount);
            foreach (var pn in Equation.ParNames) {
                GInfo.Add(new GeneDoubleRange(pn, minPar, maxPar));
            }

        }
    }
}
