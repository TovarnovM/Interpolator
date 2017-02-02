using Sharp3D.Math.Core;
using SimpleIntegrator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RobotSim {
    public class RobotDynamics: ScnObjDummy {

        public MaterialObjectNewton Body { get; set; } = new MaterialObjectNewton();
        public double l, h, w;
        public Vector3D CenterPosOtnCM;
        public List<RbWheel> wheels = new List<RbWheel>(4);

        public TracksDummy tDummy { get; set; }
        public List<RbTrack> Tracks { get; set; }
        public RbSurfFloor floor;



        //public List<RobotWheel> Wheels { get; set; } = new List<RobotWheel>();
        const string DEFNAME = "Enviroment";
        public RobotDynamics(string name = DEFNAME) :base(name) {
            l = 0.2; //x
            h = 0.05;//y
            w = 0.1;//z
            CenterPosOtnCM = new Vector3D(0,0,0);
            Body.Mass.Value = 0.5;
            Body.Name = "Body";
            AddChild(Body);
            //Body.SynchMeBefore += SynchWheelsToBodyPos;

            //FloorForce = new ForceCenter(1,new Position3D(0,1,0),null);
            //Body.AddForce(FloorForce);
            SynchMassGeometry();

            for(int i = 0; i < 4; i++) {
                var wheel = RbWheel.GetStandart();
                wheel.Name = "wheel" + i.ToString();
                wheels.Add(wheel);
                Body.AddChild(wheel);
            }
            wheels[0].p0_body_loc = GetUgolLocal(0) + Vector3D.YAxis * h / 4;
            wheels[0].n0_body_loc = Vector3D.ZAxis;
            
            wheels[1].p0_body_loc = GetUgolLocal(1) + Vector3D.YAxis * h / 4;
            wheels[1].n0_body_loc = Vector3D.ZAxis;
            
            wheels[2].p0_body_loc = GetUgolLocal(2) + Vector3D.YAxis * h / 4;
            wheels[2].n0_body_loc = -Vector3D.ZAxis;
            
            wheels[3].p0_body_loc = GetUgolLocal(3) + Vector3D.YAxis * h / 4;
            wheels[3].n0_body_loc = -Vector3D.ZAxis;

            foreach(var w in wheels) {
                ConnectWheelToBody(w);
            }
            var m = 0;
            foreach(var w in wheels) {
                w.MomentX.Value = (m++)*0.01;
                //break;
            }

            //double moment = 0.1;
            //foreach(var w in wheels) {
            //    Body.AddChild(w);
            //    w.Mx = moment;
            //    moment += moment;
            //}

            //tDummy = new TracksDummy();
//AddChild(tDummy);
            Tracks = GetTracks(wheels[0],wheels[1]);
            foreach(var t in Tracks) {
                AddChild(t);
            }
            
        }


        public void SynchMassGeometry() {
            Body.Mass3D.Iz = Body.Mass.Value * (h * h + l * l) / 12d;
            Body.Mass3D.Iy = Body.Mass.Value * (w * w + l * l) / 12d;
            Body.Mass3D.Ix = Body.Mass.Value * (h * h + w * w) / 12d;
        }

        public void SynchWheelsToBodyPos(double t = 0) {
            foreach(var w in wheels) {
                SynchWheelPos(w);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3D GetUgol(int ugolInd) {
            return Body.WorldTransform * GetUgolLocal(ugolInd);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3D GetUgolLocal(int ugolInd) {
            switch(ugolInd) {
                case 0: {
                    return (new Vector3D(-l / 2,-h / 2,w / 2) + CenterPosOtnCM);
                }
                case 1: {
                    return (new Vector3D(l / 2,-h / 2,w / 2) + CenterPosOtnCM);
                }
                case 2: {
                    return (new Vector3D(l / 2,-h / 2,-w / 2) + CenterPosOtnCM);
                }
                case 3: {
                    return (new Vector3D(-l / 2,-h / 2,-w / 2) + CenterPosOtnCM);
                }
                case 4: {
                    return (new Vector3D(-l / 2,h / 2,w / 2) + CenterPosOtnCM);
                }
                case 5: {
                    return (new Vector3D(l / 2,h / 2,w / 2) + CenterPosOtnCM);
                }
                case 6: {
                    return (new Vector3D(l / 2,h / 2,-w / 2) + CenterPosOtnCM);
                }
                case 7: {
                    return (new Vector3D(-l / 2,h / 2,-w / 2) + CenterPosOtnCM);
                }
                default:
                return CenterPosOtnCM;

            }
        }

        #region Насаживаем Колеса
        public double L_osi = 0.02;
        public void ConnectWheelToBody(RbWheel wheel, double k = 100, double mu= 100) {
            var p0_wheel_loc = new Vector3D(0,0,0);
            var p1_wheel_loc = p0_wheel_loc + Vector3D.XAxis * L_osi;
            var p1_body_loc = wheel.p0_body_loc + wheel.n0_body_loc * L_osi;

            //wheel.SetPosition(wheel.WorldTransform * p0_wheel_loc,Body.WorldTransform * wheel.p0_body_loc);
            wheel.Vec3D = Body.WorldTransform * wheel.p0_body_loc;
            wheel.SynchQandM();
            wheel.SetPosition(wheel.WorldTransform * p1_wheel_loc,Body.WorldTransform * (wheel.p0_body_loc + wheel.n0_body_loc * L_osi),wheel.WorldTransform * p0_wheel_loc);

            var f0_toWheel = new ForceBetween2Points(wheel,Body,p0_wheel_loc,wheel.p0_body_loc,k,mu);
            var f1_toWheel = new ForceBetween2Points(wheel,Body,p1_wheel_loc,p1_body_loc,k,mu);
            wheel.AddForce(f0_toWheel);
            wheel.AddForce(f1_toWheel);

            var f0_toBody = new ForceBetween2Points(Body,wheel,wheel.p0_body_loc,p0_wheel_loc,k,mu);
            var f1_toBody = new ForceBetween2Points(Body,wheel,p1_body_loc,p1_wheel_loc,k,mu);
            Body.AddForce(f0_toBody);
            Body.AddForce(f1_toBody);

            Body.AddMomentNegative(wheel.MomentX);
        }

        public void SynchWheelPos(RbWheel wheel) {
            var p0_wheel_loc = new Vector3D(0,0,0);
            var p1_wheel_loc = p0_wheel_loc + Vector3D.XAxis * L_osi;
            var p1_body_loc = wheel.p0_body_loc + wheel.n0_body_loc * L_osi;
            //wheel.SetPosition(wheel.WorldTransform * p0_wheel_loc,Body.WorldTransform * wheel.p0_body_loc);
            wheel.Vec3D = Body.WorldTransform * wheel.p0_body_loc;
            wheel.SynchQandM();
            wheel.SetPosition(wheel.WorldTransform * p1_wheel_loc,Body.WorldTransform * (wheel.p0_body_loc + wheel.n0_body_loc * L_osi),wheel.WorldTransform * p0_wheel_loc);
        }


        #endregion

        #region насаживаем трэки
        double RotateWheels(RbWheel w1, RbWheel w2) {
            var r21 = w2.Vec3D - w1.Vec3D;
            var r21n = r21.Norm;
            w1.SetPosition_LocalPoint_LocalFixed(
                new Vector3D(0,1,0),w1.Vec3D - Body.WorldTransformRot * Vector3D.YAxis,
                new Vector3D(0,0,0),new Vector3D(1,0,0));
            w2.SetPosition_LocalPoint_LocalFixed(
                new Vector3D(0,1,0),w2.Vec3D + Body.WorldTransformRot * Vector3D.YAxis,
                new Vector3D(0,0,0),new Vector3D(1,0,0));
            int n = w1.n_shag / 2;
            return (w1.WorldTransform * w1.Zubya[0] - w2.WorldTransform * w2.Zubya[n]).GetLength();
        }

        List<RbTrack> GetTracks(RbWheel w1,RbWheel w2) {
            var l21 = RotateWheels(w1,w2);
            var lst = new List<RbTrack>();
            var r21 = (w2.Vec3D - w1.Vec3D);
            var r21norm = r21.Norm;

            int n_z = w1.n_shag;
            RbWheel w = w1;
            for(int i = 0; i < n_z; i++) {
                var v_dir = w.WorldTransformRot * w.Zubya[i];
                if(v_dir * r21norm > 0)
                    continue;
                var v1 = w.WorldTransform * w.Zubya[i];
                var t0 = RbTrack.GetStandart();
                t0.RotateOXtoVec(new Vector3D(32,-11,-55));
                t0.Vec3D = v1;
                t0.SynchQandM();
                t0.SetPosition_LocalPoint_LocalFixed(new Vector3D(0,0,1),w.WorldTransform * (w.Zubya[i] + new Vector3D(1,0,0)),new Vector3D(0,0,0));
                t0.SetPosition_LocalPoint_LocalFixed(new Vector3D(1,0,0),w.WorldTransform * (w.Zubya[i] + w.Zubya_n[i]),new Vector3D(0,0,0),new Vector3D(0,0,1));
                lst.Add(t0);
            }
            w = w2;
            for(int i = 0; i < n_z; i++) {
                var v_dir = w.WorldTransformRot * w.Zubya[i];
                if(v_dir * r21norm < 0)
                    continue;
                var v1 = w.WorldTransform * w.Zubya[i];
                var t0 = RbTrack.GetStandart();
                t0.RotateOXtoVec(new Vector3D(32,-11,-55));
                t0.Vec3D = v1;
                t0.SynchQandM();
                t0.SetPosition_LocalPoint_LocalFixed(new Vector3D(0,0,1),w.WorldTransform * (w.Zubya[i] + new Vector3D(1,0,0)),new Vector3D(0,0,0));
                t0.SetPosition_LocalPoint_LocalFixed(new Vector3D(1,0,0),w.WorldTransform * (w.Zubya[i] + w.Zubya_n[i]),new Vector3D(0,0,0),new Vector3D(0,0,1));
                lst.Add(t0);
            }


            return lst;



            //var shag = (t0.ConnP[3] - t0.ConnP[1]).GetLength();
            //int nVnizu = (int)Math.Floor(l21 / shag);
            //double shagFact = l21 / nVnizu;

        }
        #endregion

    }


    public class TracksDummy:ScnObjDummy {
        public List<RbTrack> Tracks { get; set; }
        public Force f3, f2;
        public TracksDummy() {

            Name = "TrackDummy";
            int n = 1;
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
