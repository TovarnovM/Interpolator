using Interpolator;
using Sharp3D.Math.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Experiment {
    public class MissileTarget: Position3D {
        public IPosition3D Vel { get; set; }
        public MissileTarget(Vector3D initPos, IPosition3D vel):base(initPos, "Trg") {
            Vel = vel;
            Vel.Name = "Vel";
            AddChild(Vel);
            AddDiffVect(Vel);
        }
    }


}
