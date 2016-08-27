using Microsoft.Research.Oslo;
using System.Collections.Generic;

namespace Experiment {
    public class SolverOptions {
        public string ODEMethodName { get; set; }
        public double StepODE { get; set; }
        public double StepOut { get; set; }
        public List<int> MyProperty { get; set; }


    }

    public class Solver {
        public List<string> Names { get; set; } = new List<string>();
        public List<Vector> Params { get; set; } = new List<Vector>();

    }
}
