using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoubleEnumGenetic.DetermOptimization {
    public interface ISearchMethod {
        
        IList<ChromosomeD> FirstCalculation(ChromosomeD startPoint);
        IList<ChromosomeD> WhatCalculateNext();
        void EndCurrentStep();
        IList<ChromosomeD> Solutions { get; }
        ChromosomeD BestSolution { get; }

        bool HasReached();
        event EventHandler BestChromosomeChanged;

    }
}
