using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EqOptimizer {
    class Program {
        static void Main(string[] args) {
            var sample = new Equations.Line_2order_eq(2);
            sample.pars[0] = -1;
            sample.pars[1] = -2;
            sample.pars[2] = -3;
            sample.pars[3] = 0;

            Console.WriteLine(sample.PatternStr);
            Console.WriteLine(sample.EqStr);
            Console.WriteLine(sample.GetSolutionString(-1,2));
            Console.ReadKey();
        }
    }
}
