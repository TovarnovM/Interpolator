using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotIM.Core {
    public class ActionStream {
        public Action<double> HitAction = null;
        public double t0 = 0 ,timeToNextHit,minimumDelta = 0.001;
        public RndDouble deltat;
        public int nMax = -1, n = 0;
        public ActionStream(double deltatMO, double deltatSKO, int n = -1):this(new NormDouble(deltatMO, deltatSKO),n) {
        }
        public ActionStream(RndDouble deltat, int n = -1) {
            this.deltat = deltat;
            this.nMax = n;
            Reset(0);
        }
        public bool Hit(double t) {
            if (nMax > 0 && n >= nMax)
                return false;

            if (t >= t0 + timeToNextHit) {
                HitAction?.Invoke(t);
                t0 = t;
                ResetTimeToNextHit();
                n++;
                return true;
            }
            return false;
        }
        public void Reset(double t) {
            t0 = t;
            n = 0;
            ResetTimeToNextHit();
        }
        void ResetTimeToNextHit() {
            timeToNextHit = deltat.Reset();
            timeToNextHit = timeToNextHit < minimumDelta ? minimumDelta : timeToNextHit;
        }

    }


}
