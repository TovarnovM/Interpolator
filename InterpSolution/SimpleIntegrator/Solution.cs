using Microsoft.Research.Oslo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleIntegrator {
    public class Solution<T> where T :IScnObj{
        public List<SolPoint> solPoints { get; } = new List<SolPoint>();
        public T Instance { get; }
        public Func<T> InitFunct { get; }
        public Solution(Func<T> InitFunct) {
            this.InitFunct = InitFunct;
        }
        public T GetInstance(SolPoint sp) {
            Instance.SynchMeTo(sp);
            return Instance;
        }
        public T GetInstanceToIndex(int ind) {
            return GetInstance(solPoints[ind]);
        }

    }
}
