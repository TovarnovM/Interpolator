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



        public List<RobotWheel> Wheels { get; set; } = new List<RobotWheel>();
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

    public class RobotWheel: MaterialObjectNewton {
        public Vector3D Center;
        public Vector3D CenterVel;
        public Vector3D OXNorm;
        public IOrient3D Body { get; set; }

        public ForceCenter ConnectionForce { get; set; } 
        public ForceCenter ConnectionVelForce { get; set; }
        public ForceCenter ConnectionMoment { get; set; }

        public void SynchMassGeometry() {
            Mass.Ix = Mass.Value * Radius * Radius * 0.5;
            Mass.Iy = 0.25 * Mass.Value * Radius * Radius + Mass.Value * H * H / 12d;
            Mass.Iz = Mass.Iy;
        }

        public double Radius { get; set; } = 0.05;
        public double H { get; set; } = 0.01;

        public double Kx { get; set; } = 1000;
        public double Mu_x { get; set; } = 500;

        public double Kyz { get; set; } = 100;
        public double Mu_yz { get; set; } = 100;

        const string DEFNAME = "Wheel";
        public RobotWheel(IOrient3D body, Vector3D center,Vector3D centerVel,Vector3D oxNorm, string name = DEFNAME):base(center, name) {
            Body = body;
            Center = center;
            CenterVel = centerVel;
            OXNorm = oxNorm;

            Mass.Value = 0.05;
            SynchMassGeometry();

            RotateOXtoVec(OXNorm);

            ConnectionForce = new ForceCenter(0, null,null,"ConnectionForce");
            AddChild(ConnectionForce);

            ConnectionVelForce = new ForceCenter(0,null,null,"ConnectionVelForce");
            AddChild(ConnectionVelForce);

            SynchMeBefore += ConnForcesSynch;


        }

        public void ConnForcesSynch(double t) {
            var r = Vec3D - Body.WorldTransform*Center;
            var n = Body.WorldTransform * OXNorm;
            var rx = (r*n)*n;
            var ryz = r - rx;

            var kFx = - rx * Kx;
            var kFyz = -ryz * Kyz;
            var kF = kFx + kFyz;

            ConnectionForce.Value = kF.GetLength();
            ConnectionForce.Direction.Vec3D = kF.Norm;

            var votn = Vel.Vec3D - Body.WorldTransform * CenterVel;
            var votnx = (votn * n) * n;
            var votnyz = votn - votnx;

            var muFx = -votnx * Mu_x;
            var muFyz = -votnyz * Mu_yz;
            var muF = muFx + muFyz;

            ConnectionVelForce.Value = muF.GetLength();
            ConnectionVelForce.Direction.Vec3D = muF.Dir;

        }

    }
}
