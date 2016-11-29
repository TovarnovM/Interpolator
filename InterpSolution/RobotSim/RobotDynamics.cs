using Sharp3D.Math.Core;
using SimpleIntegrator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotSim {
    public class RobotDynamics: ScnObjDummy {

        public MaterialObjectNewton Body { get; set; } = new MaterialObjectNewton();
        public double l, h, w;



        //public List<RobotWheel> Wheels { get; set; } = new List<RobotWheel>();
        const string DEFNAME = "Robot";
        public RobotDynamics(string name = DEFNAME) :base(name) {
            l = 0.2;
            h = 0.05;
            w = 0.1;
            Body.Mass.Value = 0.5;
            SynchMassGeometry();
            

        }


        public void SynchMassGeometry() {
            Body.Mass.Ix = Body.Mass.Value * (h * h + w * w) / 12d;
            Body.Mass.Iy = Body.Mass.Value * (h * h + l * l) / 12d;
            Body.Mass.Iz = Body.Mass.Value * (l * l + w * w) / 12d;
        }


    }

}
