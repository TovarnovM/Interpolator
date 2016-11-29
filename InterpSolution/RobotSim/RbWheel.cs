using Sharp3D.Math.Core;
using SimpleIntegrator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotSim {
    public class RbWheel: Orient3D {
        public double Ix { get; set; }
        public double Mass { get; set; }
        public double Mx { get; set; }

        public double R0;
        public double X0;
        public double kR;
        public double muR;
        public double kX;
        public double muX;

        public int n_shag;
        public double Rmax;

        public double OmegaX { get; set; }
        public IScnPrm pOmegaX { get; set; }

        public double Betta { get; set; }
        public IScnPrm pBetta { get; set; }

        public RbWheel() {
            AddDiffPropToParam(pBetta,pOmegaX);
        }

        public Vector3D GetWorldPos(int spikeIndex) {

        }
    }
}
