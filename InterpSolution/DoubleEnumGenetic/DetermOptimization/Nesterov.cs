using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoubleEnumGenetic.DetermOptimization {
    public class Nesterov : DownHill {
        public double etta = 0.975;
        IDictionary<string,double> Vt = new Dictionary<string,double>();

        public override void EndCurrentStep() {
            
            var jac = GetJacobian(currentPoints4Jacob);
            if(Vt.Count != jac.Count) {
                Vt.Clear();
                foreach(var item in jac) {
                    Vt.Add(item.Key,item.Value* lambda);
                }
            } else {
                foreach(var item in jac) {
                    Vt[item.Key] = Vt[item.Key] * etta + item.Value * lambda;
                }
            }
            var center = currentPoints4Jacob.First(p => p.DopInfo == null);
            var nextCenter = center.CloneWithoutFitness();
            foreach(var j in Vt) {
                nextCenter[j.Key] += lambda * j.Value;
            }
            Solutions.Add(nextCenter);
            if(_bs == null)
                _bs = center;
            if(_bs != null && center.Fitness.Value > _bs.Fitness.Value)
                _bs = center;
        }

        public override bool HasReached() {
            if(Vt.Count == 0)
                return false;
            if(!Vt.Any(v => shagDict[v.Key] * v.Value * lambda > eps)) {
                ChromosomeD last = null;
                for(int i = Solutions.Count - 1; i >= 0; i--) {
                    if(!Solutions[i].Fitness.HasValue)
                        continue;
                    if(last == null)
                        last = Solutions[i];                    

                    if(last != null)
                        break;

                }
                if(last == null || _bs == null)
                    return false;
                

                return Math.Abs(last.Fitness.Value - _bs.Fitness.Value) < eps;
                
            }          
            else
                return false;          
        }
    }
}
