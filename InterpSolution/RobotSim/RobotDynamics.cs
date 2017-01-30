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
        public List<RbWheel> wheels = new List<RbWheel>(4);

        public TracksDummy tDummy { get; set; }
        public RbSurfFloor floor;



        //public List<RobotWheel> Wheels { get; set; } = new List<RobotWheel>();
        const string DEFNAME = "Enviroment";
        public RobotDynamics(string name = DEFNAME) :base(name) {
            l = 0.2; //z
            h = 0.05;//y
            w = 0.1;//x
            Body.Mass.Value = 0.5;
            Body.Name = "Body";
            AddChild(Body);
            Body.SynchMeAfter += SynchWheelsToBodyPos;

            //FloorForce = new ForceCenter(1,new Position3D(0,1,0),null);
            //Body.AddForce(FloorForce);
            SynchMassGeometry();

            for(int i = 0; i < 4; i++) {
                wheels.Add(RbWheel.GetStandart());
            }
            wheels[0].localPos = new Vector3D(w / 2,0,l / 2);
            wheels[1].localPos = new Vector3D(w / 2,0,-l / 2);
            wheels[2].localPos = new Vector3D(-w / 2,0,l / 2);
            wheels[3].localPos = new Vector3D(-w / 2,0,-l / 2);
            //double moment = 0.1;
            //foreach(var w in wheels) {
            //    Body.AddChild(w);
            //    w.Mx = moment;
            //    moment += moment;
            //}

            tDummy = new TracksDummy();
            AddChild(tDummy);
        }


        public void SynchMassGeometry() {
            Body.Mass3D.Ix = Body.Mass.Value * (h * h + l * l) / 12d;
            Body.Mass3D.Iy = Body.Mass.Value * (w * w + l * l) / 12d;
            Body.Mass3D.Iz = Body.Mass.Value * (h * h + w * w) / 12d;
        }

        public void SynchWheelsToBodyPos(double t = 0) {
            var matr = Body.WorldTransform;
            foreach(var wheel in wheels) {
                wheel.Vec3D = matr * wheel.localPos;
            }


            
        }

        public Vector3D GetUgol(int ugolInd) {
            switch(ugolInd) {
                case 0: {
                    return Body.WorldTransform*(new Vector3D(w / 2,-h / 2,l / 2));
                }
                case 1: {
                    return Body.WorldTransform * (new Vector3D(w / 2,-h / 2,-l / 2));
                }
                case 2: {
                    return Body.WorldTransform * (new Vector3D(-w / 2,-h / 2,-l / 2));
                }
                case 3: {
                    return Body.WorldTransform * (new Vector3D(-w / 2,-h / 2,l / 2));
                }
                case 4: {
                    return Body.WorldTransform * (new Vector3D(w / 2,h / 2,l / 2));
                }
                case 5: {
                    return Body.WorldTransform * (new Vector3D(w / 2,h / 2,-l / 2));
                }
                case 6: {
                    return Body.WorldTransform * (new Vector3D(-w / 2,h / 2,-l / 2));
                }
                case 7: {
                    return Body.WorldTransform * (new Vector3D(-w / 2,h / 2,l / 2));
                }
                default:
                return Body.WorldTransform * (Vector3D.Zero);
              
            }
        }

    }


    public class TracksDummy:ScnObjDummy {
        public List<RbTrack> Tracks { get; set; }
        public Force f3, f2;
        public TracksDummy() {

            Name = "TrackDummy";
            int n = 77;
            Tracks = new List<RbTrack>(n);
            var tr1 = RbTrack.GetFlat();
            var gForce = new Force(tr1.Mass.Value*9.8,new RelativePoint(0,-1,0));
            tr1.AddForce(gForce);

            var p1 = new Vector3D(10,10,-3);
            tr1.SetPosition(3,p1);
            tr1.SetPosition(2,p1 + new Vector3D(0,-10,-10),3);
            tr1.SetPosition(1,p1 + new Vector3D(10,0,0),2,3);
            var p2 = tr1.GetConnPWorld(2);
            Tracks.Add(tr1);
            AddChild(tr1);
            for(int i = 1; i < n; i++) {
                var tr = RbTrack.GetFlat();
                tr.AddForce(gForce);
                tr.SetPosition(3,Tracks[i - 1].GetConnPWorld(1));
                tr.SetPosition(2,Tracks[i - 1].GetConnPWorld(0),3);
                tr.SetPosition(1,Tracks[i - 1].GetConnPWorld(0) + new Vector3D(10,0,0),2,3);
                Tracks.Add(tr);
                AddChild(tr);

                RbTrack.ConnectTracks(Tracks[i - 1],tr,1,3,0,2);
            }

            f3 = new Force(0,new RelativePoint(0,0,0),new RelativePoint(tr1.ConnP[3],tr1));
            tr1.AddForce(f3);
            f3.SynchMeBefore += t => {
                var ff = Phys3D.GetKMuForce(tr1.GetConnPWorld(3),tr1.GetConnPVelWorld(3),p1,Vector3D.Zero,1000,100,0);
                f3.Value = ff.GetLength();
                f3.Direction.Vec3D = ff;
                //f3.AppPoint.Vec3D = tr1.GetConnPWorld(3);
            };

            f2 = new Force(0,new RelativePoint(0,0,0),new RelativePoint(tr1.ConnP[2],tr1));
            tr1.AddForce(f2);
            f2.SynchMeBefore += t => {
                var ff = Phys3D.GetKMuForce(tr1.GetConnPWorld(2),tr1.GetConnPVelWorld(2),p2,Vector3D.Zero,1000,100,0);
                f2.Value = ff.GetLength();
                f2.Direction.Vec3D = ff;
                //f3.AppPoint.Vec3D = tr1.GetConnPWorld(3);
            };
        }
    }
}
