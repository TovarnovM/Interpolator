using Sharp3D.Math.Core;
using SimpleIntegrator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static System.Math;

namespace RobotSim {
    public class TrackParams {
        public double w, l, h, shagConnL, connH, mass;
        protected TrackParams() {

        }
        public static TrackParams GetStandart() {
            var t = new TrackParams() {
                w = 0.025,
                l = 0.005,
                h = 0.005,
                shagConnL = 0.009,
                connH = 0.00d,
                mass = 0.037
            };
            return t;
        }
        
    }

    public class RbTrack: MaterialObjectNewton {
        public int logId = -1;
        public double W, L, H;
        public Vector3D[] ConnP = new Vector3D[7];
        public List<RbWheel> WheelsInteractsWithMe = new List<RbWheel>();
        public Force ForceFromWheel4, ForceFromWheel5 ,ForceFromWheel6;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3D GetConnPVelWorld(int index) {
             return WorldTransformRot * (Omega.Vec3D & ConnP[index])+ Vel.Vec3D;        
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3D GetConnPWorld(int index) {
            return WorldTransform * ConnP[index];
        }

        /// <summary>
        /// Поворачивает трэк точкой pointIndex к точке worldPos. При зафиксированных точках fixedPoints
        /// Если фикс. тоек нет, то трэк поворачивается и "передвигается" до точки worldPos
        /// </summary>
        /// <param name="pointIndex">Какую точку надо повернуть</param>
        /// <param name="worldPos">К какой точке повернуть</param>
        /// <param name="fixedPoints"></param>
        public void SetPosition(int pointIndex, Vector3D worldPos, params int[] fixedPoints) {
            int n = fixedPoints.Length;
            var fp = new Vector3D[n];
            for(int i = 0; i < n; i++) {
                fp[i] = GetConnPWorld(fixedPoints[i]);
            }

            SetPosition(GetConnPWorld(pointIndex),worldPos,fp);
            //int n = fixedPoints.Count();
            //if(n > 2)
            //    return;
            //if(fixedPoints.Contains(pointIndex))
            //    return;
            //var center = Vec3D;

            //if(n == 0) {
            //    RotateVecToVec(GetConnPWorld(pointIndex) - center,worldPos - center);
            //    Vec3D += worldPos - GetConnPWorld(pointIndex);               
            //} else
            //if(n == 1) {
            //    var fixedPoint = GetConnPWorld(fixedPoints[0]);               
            //    RotateVecToVec(GetConnPWorld(pointIndex) - fixedPoint,worldPos - fixedPoint);
            //    Vec3D += fixedPoint - GetConnPWorld(fixedPoints[0]);
            //} else
            //if(n == 2) {
            //    var fixedPoint0 = GetConnPWorld(fixedPoints[0]);
            //    var fixedPoint1 = GetConnPWorld(fixedPoints[1]);
            //    var os0 = (fixedPoint1 - fixedPoint0).Norm;

            //    var vec0 = GetConnPWorld(pointIndex) - fixedPoint0;
            //    var vec0n = vec0 - (vec0 * os0) * os0;

            //    var vec1 = worldPos - fixedPoint0;
            //    var vec1n = vec1 - (vec1 * os0) * os0;

            //    RotateVecToVec(vec0n,vec1n);
            //    Vec3D += fixedPoint0 - GetConnPWorld(fixedPoints[0]);
            //}

            //SynchQandM();
        }

        public void UpdateForcesInteracktWheels(double t) {
            foreach(var w in WheelsInteractsWithMe) {
                InteractWithWheel(w,ForceFromWheel4);
                InteractWithWheel(w,ForceFromWheel5);
                InteractWithWheel(w,ForceFromWheel6,false);
            }
        }

        void InteractWithWheel(RbWheel w, Force forceFromWheelField, bool RH = true) {
            var trP = forceFromWheelField.AppPoint.Vec3D_World;
            if(w.HasInteraction(trP)) {
                Vector3D f;
                if(RH) {
                    f = (w.WorldTransformRot * w.GetLocalR_wheelForce(trP,GetVelWorld(forceFromWheelField.AppPoint.Vec3D)));
                    
                } else {
                    f = w.WorldTransformRot * w.GetLocalKTauForce_Experimental(trP,GetVelWorld(forceFromWheelField.AppPoint.Vec3D));                   
                }

                forceFromWheelField.Value = f.GetLength();
                forceFromWheelField.Direction.Vec3D = f;
                w.ForcesFromTracksNegative.Add(forceFromWheelField);
            }

        }

        public static void ConnectTracks(RbTrack tr1, RbTrack tr2, int indMe0, int indTo0, int indMe1, int indTo1, double k = 10000d, double mu = 100d, double k2 = 100000d,double mu2 = 100d, double x02 = 0.00015) {
            var f0_1 = new ForceBetween2Points(tr1,tr2,tr1.ConnP[indMe0],tr2.ConnP[indTo0],k,mu, 0,k2,mu2,x02);
            var f1_1 = new ForceBetween2Points(tr1,tr2,tr1.ConnP[indMe1],tr2.ConnP[indTo1],k,mu,0,k2,mu2,x02);
            tr1.AddForce(f0_1);
            tr1.AddForce(f1_1);

            var f0_2 = new ForceBetween2Points(tr2,tr1,tr2.ConnP[indTo0],tr1.ConnP[indMe0],k,mu,0,k2,mu2,x02);
            var f1_2 = new ForceBetween2Points(tr2,tr1,tr2.ConnP[indTo1],tr1.ConnP[indMe1],k,mu,0,k2,mu2,x02);
            tr2.AddForce(f0_2);
            tr2.AddForce(f1_2);
        }

        public static RbTrack GetStandart(TrackParams options, int logId = -1) {
            return GetStandart(options.w,options.l,options.h,options.shagConnL,options.connH,options.mass, logId);
        }

        public static RbTrack GetStandart(double w = 0.02, double l = 0.005,double h = 0.005, double shagConnL = 0.009,double connH = 0.00d, double mass = 0.037, int logId = -1) {
            var res = new RbTrack() {
                W = w,
                L = l,
                H = h,
                Name = "Track"
            };
            res.ConnP[0] = new Vector3D(0.5 * shagConnL,connH,-0.5 * w);
            res.ConnP[1] = new Vector3D(0.5 * shagConnL,connH,0.5 * w);
            res.ConnP[2] = new Vector3D(-0.5 * shagConnL,connH,-0.5 * w);
            res.ConnP[3] = new Vector3D(-0.5 * shagConnL,connH,0.5 * w);

            res.ConnP[4] = new Vector3D(0L,connH*2,-0.333 * w);
            res.ConnP[5] = new Vector3D(0,connH*2,0.333 * w);
            res.ConnP[6] = new Vector3D(0,connH * 2,0);

            res.Mass.Value = mass;
            res.Mass3D.Ix = mass * (w * w + h * h) / 12d;
            res.Mass3D.Iy = mass * (w * w + l * l) / 12d;
            res.Mass3D.Iz = mass * (l * l + h * h) / 12d;


            res.ForceFromWheel4 = Force.GetForce(0d,new Vector3D(1,0,0),null,res.ConnP[4],res);
            res.ForceFromWheel5 = Force.GetForce(0d,new Vector3D(1,0,0),null,res.ConnP[5],res);
            res.ForceFromWheel6 = Force.GetForce(0d,new Vector3D(1,0,0),null,res.ConnP[6],res);
            res.AddForce(res.ForceFromWheel4);
            res.AddForce(res.ForceFromWheel5);
            res.AddForce(res.ForceFromWheel6);

            res.SynchMeAfter = res.NewtonLawWithFrict;

            res.logId = logId;
            return res;

        }
        public static RbTrack GetFlat(double w = 0.02,double l = 0.005,double mass = 0.037) {
            return GetStandart(w,l,0,l,0,mass);
        }
        #region ЗЕМЛЯ
        public class SurfFrictInfo {
            public FlatSurf surf;
            public double k_tr;
            public List<GroundForce> gForces;
            public SurfFrictInfo(FlatSurf surf,double k_tr) {
                this.surf = surf;
                this.k_tr = k_tr;
                gForces = new List<GroundForce>();
            }
        }

        public List<SurfFrictInfo> SurfList = new List<SurfFrictInfo>();
        public void AddSurf(FlatSurf surf, double k_tr = 1.0) {
            var surfInfo = new SurfFrictInfo(surf,k_tr);
            SurfList.Add(surfInfo);
            var surfFList = new List<Force>();
            for(int i = 0; i < 4; i++) {
                var force = new GroundForce(this,ConnP[i],surf, logId);
                surfInfo.gForces.Add(force);
                AddForce(force);
            }
            

        }

        public void NewtonLawWithFrict(double t) {
            var fsumWorld = ForceWorldSumm(t);
            foreach(var si in SurfList) {
                ApplyFrictForces(ref fsumWorld,si);
            }        
            var momSumWorld = GetMomentsWorldSum(t);

            
            var momSumLocal = WorldTransformRot_1 * GetMomentsWorldSum(t);

            UpdateAcc(fsumWorld);
            UpdatedQdt(momSumLocal);
        }


        void ApplyFrictForces(ref Vector3D fsumWorld,SurfFrictInfo surfInfo) {
            double Nforce = 0d;
            foreach(var gf in surfInfo.gForces) {
                Nforce += gf.Value;
            }
            if(Nforce < 1E-12)
                return;
            var n0 = surfInfo.surf.N0.Norm;
            var tauVel = Vel.Vec3D - (Vel.Vec3D * n0) * n0;           

            var tauSummForce = fsumWorld - (fsumWorld * n0) * n0;
            var frictTauForceMax = Nforce * surfInfo.k_tr;
            if(tauVel.GetLengthSquared() > 1E-7) {
                fsumWorld = fsumWorld - tauVel.Norm * frictTauForceMax;
                return;
            }

            if(frictTauForceMax >= tauSummForce.GetLength()) {
                fsumWorld = (fsumWorld * n0) * n0;
                return;
            } else {
                fsumWorld = fsumWorld - tauSummForce.Norm * frictTauForceMax;
                return;
            }


            //var n0SumMoment = (momSumWorld * n0) * n0;
            //var frictN0Moment = -n0SumMoment / n;
            //foreach(var gf in nonZeroGforces) {
            //    var appPDirWorld = gf.AppPoint.Vec3D_Dir_World;
            //    var appPTauDirWorld = appPDirWorld - (appPDirWorld * n0) * n0;
            //    var frictMomTauForce = frictN0Moment & appPTauDirWorld;

            //    var frictFullForce = frictTauForce + frictMomTauForce;
            //    var maxFrictForceValue = gf.Value * surfInfo.k_tr;
            //    if(maxFrictForceValue > frictFullForce.GetLength())
            //}

        }


        #endregion

    }



    
}
