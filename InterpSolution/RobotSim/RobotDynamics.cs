using MoreLinq;
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
        public double _l, _h, _w;
        public Vector3D CenterPosOtnCM;
        public List<RbWheel> wheels = new List<RbWheel>(6);

        public TracksDummy tDummy { get; set; } = new TracksDummy(0);
        //public List<RbTrack> Tracks1  = new List<RbTrack>();
        //public List<RbTrack> Tracks2  = new List<RbTrack>();
        public List<RbTrack> TracksAll = new List<RbTrack>();
        public List<IRbSurf> surfs = new List<IRbSurf>();



        //public List<RobotWheel> Wheels { get; set; } = new List<RobotWheel>();
        const string DEFNAME = "Enviroment";
        public RobotDynamics(double mass, double l, double h, double w, Vector3D centerOtnCM, string name = DEFNAME):base(name) {
            _l = l; //x
            _h = h;//y
            _w = w;//z
            CenterPosOtnCM = centerOtnCM;
            Body.Mass.Value = mass;
            Body.Name = "Body";
            AddChild(Body);
            SynchMeBefore += UpdateWheelTrackInteraction;
            //Body.SynchMeBefore += SynchWheelsToBodyPos;

            //FloorForce = new ForceCenter(1,new Position3D(0,1,0),null);
            //Body.AddForce(FloorForce);
            SynchMassGeometry();
        }
        public RobotDynamics(string name = DEFNAME) :this(0.5,0.175,0.05,0.15,new Vector3D(0,0,0),name) {
        }


        public void SynchMassGeometry() {
            Body.Mass3D.Iz = Body.Mass.Value * (_h * _h + _l * _l) / 12d;
            Body.Mass3D.Iy = Body.Mass.Value * (_w * _w + _l * _l) / 12d;
            Body.Mass3D.Ix = Body.Mass.Value * (_h * _h + _w * _w) / 12d;
        }

        public void AddSurf(IRbSurf surf, bool connectTracks = true) {
            surfs.Add(surf);
            if(connectTracks) {
                foreach(var tr in TracksAll) {
                    tr.AddSurf(surf);
                }
            }
            

        }

        public void UpdateWheelTrackInteraction(double t) {
            foreach(var w in wheels) {
                foreach(var fn in w.ForcesFromTracksNegative) {
                    w.ForcesNegative.Remove(fn);
                }
                w.ForcesFromTracksNegative.Clear();
                w.SynchQandM();
            }
            foreach(var tr in TracksAll) {
                tr.ForceFromWheel4.Value = 0d;
                tr.ForceFromWheel5.Value = 0d;
                tr.SynchQandM();
            }

            foreach(var tr in TracksAll) {
                tr.UpdateForcesInteracktWheels(t);
            }

            foreach(var w in wheels) {
                foreach(var f_neg in w.ForcesFromTracksNegative) {
                    int num = w.ForcesNegative.Count;
                    w.ForcesNegative.Add(f_neg);
                }
            }
        }


        int n_wheels = 0, n_tracks = 0;
        public double wheelBody_k = 100000, wheelBody_mu = 100;
        /// <summary>
        /// Создает 2 колеса и гусеничную ленту, прикрепленную к connectBody
        /// </summary>
        /// <param name="connectBody">к кому прикреплять колеса</param>
        /// <param name="locPosW1">позиция w1</param>
        /// <param name="localN0">ось OX у w1</param>
        /// <param name="otnApproxPosw2">относительная позиция w2 относительно w1</param>
        /// <returns>ссылку на wheel1</returns>
        public RbWheel CreateWheelPairWithTracks(MaterialObjectNewton connectBody, Vector3D locPosW1, Vector3D localN0, Vector3D otnApproxPosw2) {
            var wheel1 = RbWheel.GetStandart();
            wheel1.Name = "wheel" + (n_wheels++).ToString();
            wheels.Add(wheel1);
            connectBody.AddChild(wheel1);

            var wheel2 = RbWheel.GetStandart();
            wheel2.Name = "wheel" + (n_wheels++).ToString();
            wheels.Add(wheel2);
            connectBody.AddChild(wheel2);

            wheel1.n0_body_loc = localN0;
            wheel1.p0_body_loc = locPosW1;

            wheel2.n0_body_loc = localN0;
            wheel2.p0_body_loc = locPosW1 + otnApproxPosw2;// - localN0.Norm*(otnApproxPosw2*localN0.Norm);

            ConnectWheelToBody(connectBody,wheel1,wheelBody_k,wheelBody_mu);
            ConnectWheelToBody(connectBody,wheel2,wheelBody_k,wheelBody_mu);

            var Tracks1 = GetTracks(wheel1,wheel2,connectBody);
            foreach(var t in Tracks1) {
                t.WheelsInteractsWithMe.Add(wheel1);
                t.WheelsInteractsWithMe.Add(wheel2);
                t.Name = "track" + n_tracks++.ToString();
                connectBody.AddChild(t);
            }
            TracksAll.AddRange(Tracks1);


            return wheel1;
        }

        public void Create4GUS(double moment = 100d, double maxOmega = 6d) {
            var trackw05 = 0.01;
            var w0 = CreateWheelPairWithTracks(
                Body,
                GetUgolLocal(0) - Vector3D.XAxis*0.0484 + Vector3D.YAxis * _h / 4 + Vector3D.ZAxis * trackw05,
                Vector3D.ZAxis,
                -Vector3D.XAxis * 0.07);

            var w1 = CreateWheelPairWithTracks(
                Body,
                GetUgolLocal(1) + Vector3D.YAxis * _h / 4 + Vector3D.ZAxis * trackw05,
                Vector3D.ZAxis,
                Vector3D.XAxis * 0.07);

            var w3 = CreateWheelPairWithTracks(
                Body,
                GetUgolLocal(3) - Vector3D.XAxis * 0.0484 + Vector3D.YAxis * _h / 4 - Vector3D.ZAxis * trackw05,
                -Vector3D.ZAxis,
                -Vector3D.XAxis * 0.07);

            var w2 = CreateWheelPairWithTracks(
                Body,
                GetUgolLocal(2) + Vector3D.YAxis * _h / 4 - Vector3D.ZAxis * trackw05,
                -Vector3D.ZAxis,
                Vector3D.XAxis * 0.07);

            w0.AddMomentFunct(moment,maxOmega);
            w1.AddMomentFunct(moment,maxOmega);
            w2.AddMomentFunct(-moment,maxOmega);
            w3.AddMomentFunct(-moment,maxOmega);
            //w0.MomentX.Value = moment;
            //w0.MomentX.SynchMeAfter += _ => {
            //    w0.MomentX.Value = w0.Omega.X > 6 ? 0d : moment;
            //};
        }



        public void CreateWheelsSample() {
            for(int i = 0; i < 6; i++) {
                var wheel = RbWheel.GetStandart();
                wheel.Name = "wheel" + (n_wheels++).ToString();
                wheels.Add(wheel);
                Body.AddChild(wheel);
            }
            var trackw05 = 0.01;
            wheels[0].n0_body_loc = Vector3D.ZAxis;
            wheels[0].p0_body_loc = GetUgolLocal(0) + Vector3D.YAxis * _h / 4 + wheels[0].n0_body_loc * trackw05;
            
            wheels[1].n0_body_loc = Vector3D.ZAxis;
            wheels[1].p0_body_loc = GetUgolLocal(1) + Vector3D.YAxis * _h / 4 + wheels[1].n0_body_loc* trackw05;
            
            wheels[2].n0_body_loc = -Vector3D.ZAxis;
            wheels[2].p0_body_loc = GetUgolLocal(2) + Vector3D.YAxis * _h / 4 + wheels[2].n0_body_loc * trackw05;

            wheels[3].n0_body_loc = -Vector3D.ZAxis;
            wheels[3].p0_body_loc = GetUgolLocal(3) + Vector3D.YAxis * _h / 4 + wheels[3].n0_body_loc * trackw05;

            wheels[4].n0_body_loc = Vector3D.ZAxis;
            wheels[4].p0_body_loc = 0.5*(GetUgolLocal(0)+ GetUgolLocal(1)) + Vector3D.YAxis * _h / 4 + wheels[0].n0_body_loc * trackw05;
            wheels[5].n0_body_loc = -Vector3D.ZAxis;
            wheels[5].p0_body_loc = 0.5 * (GetUgolLocal(2) + GetUgolLocal(3)) + Vector3D.YAxis * _h / 4 + wheels[3].n0_body_loc * trackw05;
            for(int i = 0; i < 4; i++) {
                var w = wheels[i];
                ConnectWheelToBody(Body,w);
            }
            for(int i = 4; i < 6; i++) {
                var w = wheels[i];
                ConnectWheelToBody(Body,w,100000,100);
            }

        }

        public void SynchWheelsToBodyPos(double t = 0) {
            foreach(var w in wheels) {
                SynchWheelPos(Body,w);
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
                    return (new Vector3D(-_l / 2,-_h / 2,_w / 2) + CenterPosOtnCM);
                }
                case 1: {
                    return (new Vector3D(_l / 2,-_h / 2,_w / 2) + CenterPosOtnCM);
                }
                case 2: {
                    return (new Vector3D(_l / 2,-_h / 2,-_w / 2) + CenterPosOtnCM);
                }
                case 3: {
                    return (new Vector3D(-_l / 2,-_h / 2,-_w / 2) + CenterPosOtnCM);
                }
                case 4: {
                    return (new Vector3D(-_l / 2,_h / 2,_w / 2) + CenterPosOtnCM);
                }
                case 5: {
                    return (new Vector3D(_l / 2,_h / 2,_w / 2) + CenterPosOtnCM);
                }
                case 6: {
                    return (new Vector3D(_l / 2,_h / 2,-_w / 2) + CenterPosOtnCM);
                }
                case 7: {
                    return (new Vector3D(-_l / 2,_h / 2,-_w / 2) + CenterPosOtnCM);
                }
                default:
                return CenterPosOtnCM;
            }
        }

        #region Насаживаем Колеса
        public double L_osi = 0.02;
        public void ConnectWheelToBody(MaterialObjectNewton connectBody, RbWheel wheel, double k = 100000, double mu= 1000) {
            var p0_wheel_loc = new Vector3D(0,0,0);
            var p1_wheel_loc = p0_wheel_loc + Vector3D.XAxis * L_osi;
            var p1_body_loc = wheel.p0_body_loc + wheel.n0_body_loc * L_osi;

            //wheel.SetPosition(wheel.WorldTransform * p0_wheel_loc,connectBody.WorldTransform * wheel.p0_body_loc);
            wheel.Vec3D = connectBody.WorldTransform * wheel.p0_body_loc;
            wheel.SynchQandM();
            wheel.SetPosition(wheel.WorldTransform * p1_wheel_loc,connectBody.WorldTransform * (wheel.p0_body_loc + wheel.n0_body_loc * L_osi),wheel.WorldTransform * p0_wheel_loc);

            var f0_toWheel = new ForceBetween2Points(wheel,connectBody,p0_wheel_loc,wheel.p0_body_loc,k,mu);
            var f1_toWheel = new ForceBetween2Points(wheel,connectBody,p1_wheel_loc,p1_body_loc,k,mu);
            wheel.AddForce(f0_toWheel);
            wheel.AddForce(f1_toWheel);

            var f0_toBody = new ForceBetween2Points(connectBody,wheel,wheel.p0_body_loc,p0_wheel_loc,k,mu);
            var f1_toBody = new ForceBetween2Points(connectBody,wheel,p1_body_loc,p1_wheel_loc,k,mu);
            connectBody.AddForce(f0_toBody);
            connectBody.AddForce(f1_toBody);

            connectBody.AddMomentNegative(wheel.MomentX);
        }

        public void SynchWheelPos(MaterialObjectNewton connectBody, RbWheel wheel) {
            var p0_wheel_loc = new Vector3D(0,0,0);
            var p1_wheel_loc = p0_wheel_loc + Vector3D.XAxis * L_osi;
            var p1_body_loc = wheel.p0_body_loc + wheel.n0_body_loc * L_osi;
            //wheel.SetPosition(wheel.WorldTransform * p0_wheel_loc,Body.WorldTransform * wheel.p0_body_loc);
            wheel.Vec3D = connectBody.WorldTransform * wheel.p0_body_loc;
            wheel.SynchQandM();
            wheel.SetPosition(wheel.WorldTransform * p1_wheel_loc,connectBody.WorldTransform * (wheel.p0_body_loc + wheel.n0_body_loc * L_osi),wheel.WorldTransform * p0_wheel_loc);
        }


        #endregion

        #region насаживаем трэки
        double RotateWheels(RbWheel w1, RbWheel w2, IOrient3D connectBody) {
            var r21 = w2.Vec3D - w1.Vec3D;
            var r21n = r21.Norm;
            w1.SetPosition_LocalPoint_LocalFixed(
                new Vector3D(0,1,0),w1.Vec3D - connectBody.WorldTransformRot * Vector3D.YAxis,
                new Vector3D(0,0,0),new Vector3D(1,0,0));
            w2.SetPosition_LocalPoint_LocalFixed(
                new Vector3D(0,1,0),w2.Vec3D + connectBody.WorldTransformRot * Vector3D.YAxis,
                new Vector3D(0,0,0),new Vector3D(1,0,0));
            int n = w1.n_shag / 2;
            w1.SynchQandM();
            w2.SynchQandM();


            return (w1.WorldTransform * w1.Zubya[0] - w2.WorldTransform * w2.Zubya[n]).GetLength();
        }

        public void CreateTracks() {
            var Tracks1 = GetTracks(wheels[0],wheels[1],Body);
            foreach(var t in Tracks1) {
                t.WheelsInteractsWithMe.Add(wheels[0]);
                t.WheelsInteractsWithMe.Add(wheels[1]);
                t.WheelsInteractsWithMe.Add(wheels[4]);
                Body.AddChild(t);
            }
            TracksAll.AddRange(Tracks1);
            var Tracks2 = GetTracks(wheels[2],wheels[3],Body);
            foreach(var t in Tracks2) {

                t.WheelsInteractsWithMe.Add(wheels[2]);
                t.WheelsInteractsWithMe.Add(wheels[3]);
                t.WheelsInteractsWithMe.Add(wheels[5]);
                Body.AddChild(t);
            }
            TracksAll.AddRange(Tracks2);
        }

        public void CreateTrackDummy(int n = 11) {
            tDummy = new TracksDummy(n);
            AddChild(tDummy);
        }

        List<RbTrack> GetTracks(RbWheel w1,RbWheel w2, IOrient3D connectBody) {
            var wl= w1.X < w2.X ? w1 : w2;
            var wr = w1.X >= w2.X ? w1 : w2;

            if(w1.XAxis * (connectBody.WorldTransformRot * Vector3D.ZAxis) < 0) {
                var wtemp = wl;
                wl = wr;
                wr = wtemp;
            }

            w1 = wl;
            w2 = wr;

            var l21 = RotateWheels(w1,w2,connectBody);
            var lst0 = new List<RbTrack>();
            var r21 = w2.Vec3D - w1.Vec3D;
            var r21norm = r21.Norm;

            int n_z = w1.n_shag;
            RbWheel w = w1;
            RbTrack b0 = null, b1, u0, u1 = null;
            for(int i = 0; i < n_z; i++) {
                var v_dir = w.WorldTransformRot * w.Zubya[i];
                if(v_dir * r21norm > 0)
                    continue;
                var v1 = w.WorldTransform * w.Zubya[i];
                var t0 = RbTrack.GetStandart();
                t0.SetPosition_LocalPoint(new Vector3D(1,1,1),new Vector3D(22,33,-11));
                t0.Vec3D = v1;
                t0.SynchQandM();
                t0.SetPosition_LocalPoint_LocalFixed(new Vector3D(0,0,1),w.WorldTransform * (w.Zubya[i] + new Vector3D(1,0,0)),new Vector3D(0,0,0));
                t0.SetPosition_LocalPoint_LocalFixed(new Vector3D(1,0,0),w.WorldTransform * (w.Zubya[i] + w.Zubya_n[i]),new Vector3D(0,0,0),new Vector3D(0,0,1));
                lst0.Add(t0);
                //if(b0==null)
                //    b0 = t0;

            }
            b0 = lst0
                .MinBy(yt => 
                    yt.ConnP.Select(p => (connectBody.WorldTransform_1 * (yt.WorldTransform * p)).Y).Min()
                );

            u0 = lst0
                .MaxBy(yt => 
                    yt.ConnP.Select(p => (connectBody.WorldTransform_1 * (yt.WorldTransform * p)).Y).Max()
                );

            //u0 = lst0
            //    .Select(tr => new { y3 = (connectBody.WorldTransform_1 * tr.GetConnPWorld(3)).Y,tr })
            //    .MaxBy(yt => yt.y3)
            //    .tr;

            var lst1 = new List<RbTrack>();
            w = w2;
            for(int i = 0; i < n_z; i++) {
                var v_dir = w.WorldTransformRot * w.Zubya[i];
                if(v_dir * r21norm < 0)
                    continue;
                var v1 = w.WorldTransform * w.Zubya[i];
                var t0 = RbTrack.GetStandart();
                t0.SetPosition_LocalPoint(new Vector3D(1,1,1),new Vector3D(22,33,-11));
                t0.Vec3D = v1;
                t0.SynchQandM();
                t0.SetPosition_LocalPoint_LocalFixed(new Vector3D(0,0,1),w.WorldTransform * (w.Zubya[i] + new Vector3D(1,0,0)),new Vector3D(0,0,0));
                t0.SetPosition_LocalPoint_LocalFixed(new Vector3D(1,0,0),w.WorldTransform * (w.Zubya[i] + w.Zubya_n[i]),new Vector3D(0,0,0),new Vector3D(0,0,1));
                lst1.Add(t0);
                //if(u1==null)
                //    u1 = t0;
            }

            b1= lst1
    .MinBy(yt =>
        yt.ConnP.Select(p => (connectBody.WorldTransform_1 * (yt.WorldTransform * p)).Y).Min()
    );

            u1 = lst1
                .MaxBy(yt =>
                    yt.ConnP.Select(p => (connectBody.WorldTransform_1 * (yt.WorldTransform * p)).Y).Max()
                );

            //b1 = lst1
            //    .Select(tr => new { y3 = (connectBody.WorldTransform_1 * tr.GetConnPWorld(3)).Y,tr })
            //    .MinBy(yt => yt.y3)
            //    .tr;

            lst0.AddRange(GetTrackBetween2Tracks(b0,b1));
            lst0.AddRange(GetTrackBetween2Tracks(u1,u0));


            lst0.AddRange(lst1);

            //foreach(var t in lst0) {
            //    t.SetPosition_LocalPoint_LocalMoveToIt(new Vector3D(1,0,0),new Vector3D(1.01,0,0));
            //    t.SetPosition_LocalPoint_LocalMoveToIt(new Vector3D(0,1,0),new Vector3D(0,1.01,0));
            //}



            return ConnectTrackLoop(lst0);
            //return lst0;


            //var shag = (t0.ConnP[3] - t0.ConnP[1]).GetLength();
            //int nVnizu = (int)Math.Floor(l21 / shag);
            //double shagFact = l21 / nVnizu;

        }

        List<RbTrack> ConnectTrackLoop(List<RbTrack> trackList) {
            var tcurr = trackList.First();
            var lst1 = new List<RbTrack>(trackList.Count);
            while(trackList.Count != lst1.Count) {
                var p1curr = tcurr.GetConnPWorld(1);
                var tnext = trackList
                    .Select(tr => new { p3 = tr.GetConnPWorld(3),tr })
                    .MinBy(rec => (rec.p3 - p1curr).GetLength())
                    .tr;
                RbTrack.ConnectTracks(tcurr,tnext,1,3,0,2);
                lst1.Add(tcurr);
                tcurr = tnext;
            }
            return lst1;
        }

        List<RbTrack> GetTrackBetween2Tracks(RbTrack b0, RbTrack b1) {
            var r_b0_1 = b0.GetConnPWorld(1);
            var r_b1b0_31 = b1.GetConnPWorld(3) - r_b0_1;
            var r_b1b0_31_norm = r_b1b0_31.Norm;

            var r_b0_0 = b0.GetConnPWorld(0);
            var r_b1b0_20 = b1.GetConnPWorld(2) - r_b0_0;
            var r_b1b0_20_norm = r_b1b0_20.Norm;

            var l0 = (b0.ConnP[3] - b0.ConnP[1]).GetLength();
            int n = (int)Math.Floor(r_b1b0_20.GetLength() / l0);
            var lfact = r_b1b0_20.GetLength() / n;
            var dl = (r_b1b0_20.GetLength() - l0 * n) / (n + 1);

            var r0_1 = r_b0_1 + r_b1b0_31_norm * dl;
            var r0_0 = r_b0_0 + r_b1b0_20_norm * dl;

            var lst0 = new List<RbTrack>();
            for(int i = 0; i < n; i++) {
                var t0 = RbTrack.GetStandart();
                t0.SetPosition_LocalPoint(new Vector3D(1,1,1),new Vector3D(22,9933,-11));
                t0.SetPosition_LocalPoint(t0.ConnP[3],r0_1 + r_b1b0_20_norm * ((dl + l0) * i));
                t0.SetPosition_LocalPoint_LocalFixed(t0.ConnP[2],r0_0 + r_b1b0_20_norm * ((dl + l0) * i),t0.ConnP[3]);
                t0.SetPosition_LocalPoint_LocalFixed(t0.ConnP[1],r0_1 + r_b1b0_31_norm * ((dl + l0) * (i + 1)),t0.ConnP[2],t0.ConnP[3]);
                lst0.Add(t0);

            }
            return lst0;
        }
        #endregion

        public void AddGForcesToAll() {
            AddGForcesToAll(new Vector3D(0,-1,0));
        }
        public void AddGForcesToAll(Vector3D dir, double g = 9.8) {
            var allMP = Children
                .Flatten(ch => ch.Children)
                .Where(ch => ch is IMaterialPoint)
                .Cast<IMaterialPoint>()
                .ToList();
            foreach(var mp in allMP) {
                mp.AddGForce(dir, g);
            }

        }
    }


    public class TracksDummy:ScnObjDummy {
        public List<RbTrack> Tracks { get; set; }
        public Force f3, f2;
        public TracksDummy(int n = 11) {

            Name = "TrackDummy";
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
