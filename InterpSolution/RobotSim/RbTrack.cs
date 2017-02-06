using Sharp3D.Math.Core;
using SimpleIntegrator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RobotSim {
    public class RbTrack: MaterialObjectNewton {
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
                    f = w.WorldTransformRot * w.GetLocalR_wheelForce(trP);
                    
                } else {
                    f = w.WorldTransformRot * w.GetLocalKTauForce(trP);
                    f += w.WorldTransformRot * w.GetLocalH_wheelForce(trP);
                }

                forceFromWheelField.Value = f.GetLength();
                forceFromWheelField.Direction.Vec3D = f;
                w.ForcesFromTracksNegative.Add(forceFromWheelField);
            }

        }

        public static void ConnectTracks(RbTrack tr1, RbTrack tr2, int indMe0, int indTo0, int indMe1, int indTo1, double k = 1d, double mu = 10d) {
            var f0_1 = new ForceBetween2Points(tr1,tr2,tr1.ConnP[indMe0],tr2.ConnP[indTo0],k,mu);
            var f1_1 = new ForceBetween2Points(tr1,tr2,tr1.ConnP[indMe1],tr2.ConnP[indTo1],k,mu);
            tr1.AddForce(f0_1);
            tr1.AddForce(f1_1);

            var f0_2 = new ForceBetween2Points(tr2,tr1,tr2.ConnP[indTo0],tr1.ConnP[indMe0],k,mu);
            var f1_2 = new ForceBetween2Points(tr2,tr1,tr2.ConnP[indTo1],tr1.ConnP[indMe1],k,mu);
            tr2.AddForce(f0_2);
            tr2.AddForce(f1_2);
        }

        public static RbTrack GetStandart(double w = 0.02, double l = 0.005,double h = 0.005, double shagConnL = 0.009,double connH = 0.00d, double mass = 0.037) {
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
            return res;

        }
        public static RbTrack GetFlat(double w = 0.02,double l = 0.005,double mass = 0.037) {
            return GetStandart(w,l,0,l,0,mass);
        }
        #region Силы с колесами
        




        #endregion

    }

    /// <summary>
    /// Сила между двумя объектами.... сила приложена к 0 точке
    /// </summary>
    public class ForceBetween2Points : Force {
        public IMaterialObject sk0, sk1;
        public IPosition3D p0_loc, p1_loc;
        public double k = 100, mu = 100, x0 = 0;
        public ForceBetween2Points(
            IMaterialObject sk0,
            IMaterialObject sk1,
            Vector3D p0_loc,
            Vector3D p1_loc,
            double k = 1000d,
            double mu = 100d,
            double x0 = 0d) : this(sk0,sk1,new Position3D(p0_loc), new Position3D(p1_loc),k,mu,x0) {

        }
        public ForceBetween2Points(
            IMaterialObject sk0,
            IMaterialObject sk1, 
            IPosition3D p0_loc, 
            IPosition3D p1_loc, 
            double k = 1000d, 
            double mu = 100d, 
            double x0 = 0d) : base(0,new RelativePoint(1,0,0),null) {

            this.sk0 = sk0;
            this.sk1 = sk1;
            this.p0_loc = p0_loc;
            this.p1_loc = p1_loc;
            AppPoint = new RelativePoint(0,0,0);
            this.k = k;
            this.mu = mu;
            this.x0 = x0;
            SynchMeBefore += synchMeBeforeAct;
        }

        void synchMeBeforeAct(double t) {
            var p0loc = p0_loc.Vec3D;
            var p1loc = p1_loc.Vec3D;

            var f = Phys3D.GetKMuForce(
                sk0.WorldTransform * p0loc,
                sk0.GetVelWorld(p0loc),
                sk1.WorldTransform * p1loc,
                sk1.GetVelWorld(p1loc),
                k,mu,x0
                );
            Value = f.GetLength();
            Direction.Vec3D = f;
            AppPoint.Vec3D = sk0.WorldTransform * p0loc;
        }
    }

    public static class Phys3D {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3D GetKMuForce(IVelPos3D me,IVelPos3D toThis, double k, double mu, double x0) {
            return GetKMuForce(me.Vec3D,me.Vel.Vec3D,toThis.Vec3D,toThis.Vel.Vec3D,k,mu,x0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3D GetKMuForce(Vector3D mePos, Vector3D meVel, Vector3D toThisPos,Vector3D toThisVel,double k,double mu,double x0) {
            var deltPos = toThisPos - mePos;
            double deltX = x0 - deltPos.GetLength();
            var deltPosNorm = deltPos.Norm;
            var Vpr_me = meVel * deltPosNorm;
            var Vpr_toThis = toThisVel * deltPosNorm;
            var VotnPr_me = Vpr_me - Vpr_toThis;

            return (-k * deltX - mu * VotnPr_me) * deltPosNorm;
        }
    }
}
