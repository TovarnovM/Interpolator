using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EqOptimizer.Data {
    public class MultyData: List<OnePoint> {
        public double MaxDiffY {
            get {
                return this.Max(op => op.answer) - this.Min(op => op.answer);
            }
        }
        public double[] MaxDiffVars {
            get {
                int n = this.First().vars.Count();
                var maxi = new double[n];
                var mini = new double[n];
                for (int i = 0; i < Count; i++) {
                    for (int j = 0; j < n; j++) {
                        if (this[i].vars[j] > maxi[j])
                            maxi[j] = this[i].vars[j];
                        else if (this[i].vars[j] < mini[j])
                            mini[j] = this[i].vars[j];
                    }
                }
                for (int j = 0; j < n; j++) {
                    maxi[j] -= mini[j];
                }
                return maxi;
            }
        }
    }
}
