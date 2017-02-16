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
    public class RbWheel: MaterialObjectNewton {
        public double R;
        public double R_max;
        public double H_wheel;
        public double kR;
        public double muR;
        public double H_zac, H_zac2;
        public double kH;
        public double muH;

        public int n_shag;
        public Vector3D[] Zubya, Zubya_n;
        public List<Force> ForcesFromTracksNegative;
        
        public double gamma;

        public Force MomentX;
        public Vector3D 
            p0_body_loc = new Vector3D(0,0,0), 
            n0_body_loc = new Vector3D(1,0,0);
        public RbWheel(
            int n, 
            double R, 
            double R_max,
            double mass,
            double H_wheel, 
            double H_zac,
            double kR = 1E5, 
            double muR = 1E2,
            double kH = 1E5,
            double muH = 1E2) :base("Wheel") 
            
        {
            n_shag = n;
            this.H_zac = H_zac;
            this.H_zac2 = H_zac * 0.2;
            this.H_wheel = H_wheel;
            this.R_max = R_max;
            this.R = R;
            this.kR = kR;
            this.muR = muR;
            this.kH = kH;
            this.muH = muH;

            gamma = 2 * PI / n;
            Zubya = new Vector3D[n];
            Zubya_n = new Vector3D[n];
            for(int i = 0; i < n; i++) {
                Zubya[i] = new Vector3D(0,R * Math.Cos(gamma * i),R * Math.Sin(gamma * i));
                Zubya_n[i] = new Vector3D(0,Math.Cos(gamma * i + Math.PI/2),Math.Sin(gamma * i + Math.PI / 2));
            }

            ForcesFromTracksNegative = new List<Force>();
            //for(int i = 0; i < n; i++) {
            //    ForcesFromTracksNegative[2 * i] = Force.GetForce(0d,new Vector3D(1,0,0),null,new Vector3D(1,0,0),null);
            //    ForcesFromTracksNegative[2 * i+1] = Force.GetForce(0d,new Vector3D(1,0,0),null,new Vector3D(1,0,0),null);
            //}

            Mass3D.Ix = mass * R * R / 2;
            Mass3D.Iy = mass * (3*R * R + H_wheel* H_wheel) / 12;
            Mass3D.Iz = Mass3D.Iy;

            MomentX = Force.GetMoment(0,new Vector3D(1,0,0),this);
            AddMoment(MomentX);
        }
        public static RbWheel GetStandart() {
            return GetStandart(Vector3D.Zero);
        }

        public static RbWheel GetStandart(Vector3D localPos) {
            int n = 17;
            double r_real = 0.015;
            double R_ideal = 0.0095 / (2 * Sin(PI / n));//0.009 / (2 * Sin(PI / n));
            var mass = PI * r_real * r_real * 0.021 * 1080*50;
            var res = new RbWheel(
                n:n,
                R:R_ideal,
                R_max: R_ideal + 0.0005,
                mass: mass,
                H_wheel: 0.03,
                H_zac:0.009 / 4);
            res.Mass.Value = mass;
            return res;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasInteraction(Vector3D globalPoint) {
            var localPoint = worldTransform_1 * globalPoint;
            if(Abs(localPoint.X) > H_wheel * 0.5)
                return false;
            if((new Vector3D(0,localPoint.Y,localPoint.Z)).GetLengthSquared() > R_max * R_max)
                return false;
            return true;
        }

        /// <summary>
        /// Вычисляется глобальная сила, действующая в радиальном направлении и направлении оси Х
        /// </summary>
        /// <param name="globalPoint"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3D GetLocalH_wheelForce(Vector3D globalPoint) {
            var localPoint = worldTransform_1 * globalPoint;
            var rlocalVec = new Vector3D(0,localPoint.Y,localPoint.Z);
            var rp = rlocalVec.GetLength();
            if(rp>R_max)
                return Vector3D.Zero;

            return Abs(localPoint.X) > H_wheel * 0.5
                ? Vector3D.Zero 
                : -kH * (new Vector3D(localPoint.X,0,0));
        }

        /// <summary>
        /// Вычисляется глобальная сила, действующая в радиальном направлении
        /// </summary>
        /// <param name="globalPoint"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3D GetLocalR_wheelForce(Vector3D globalPoint, Vector3D globalVel) {
            var localPoint = worldTransform_1 * globalPoint;
            var rlocalVec = new Vector3D(0,localPoint.Y,localPoint.Z);
            var rp = rlocalVec.GetLength();
            if(rp > R)
                return Vector3D.Zero;

            var localVel = WorldTransformRot_1 * globalVel;
            var myLocalPos = rlocalVec.Norm * R;
            var myLocalVel = Vector3D.Zero;
            return Phys3D.GetKMuForce(rlocalVec,localVel,myLocalPos,myLocalVel,kR,muR,0);
            return  (R - rp) * kR * rlocalVec.Norm;

        }

        /// <summary>
        /// Вычисляется глобальная сила, действующая в тангенциальном направлении (только в окрестностях "зубцов")
        /// </summary>
        /// <param name="globalPoint"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3D GetLocalKTauForce(Vector3D globalPoint, Vector3D globalVel) {
            var localPoint = worldTransform_1 * globalPoint;
            
            var rlocalVec = new Vector3D(0,localPoint.Y,localPoint.Z);
            var rp = rlocalVec.GetLength();
            if(rp > R || Abs(localPoint.X) > H_wheel * 0.5)
                return Vector3D.Zero;

            int closestInd = GetClosestInd(localPoint);
            var angle0 = gamma * closestInd;//Betta % (2 * PI) + gamma * closestInd;
            

            var n0 = new Vector3D(0,Cos(angle0+0.5*PI),Sin(angle0 + 0.5 * PI));
            var dh = n0 * rlocalVec;

            if(Abs(dh) > H_zac || Abs(dh) < H_zac2)
                return Vector3D.Zero;

            var localVel = worldTransform_1 * globalVel;
            var myVelLocal = GetVelLocal(Zubya[closestInd]);
            return Phys3D.GetKMuForce(rlocalVec,localVel,Zubya[closestInd],myVelLocal,kH,muH,H_zac2);
           // return -kH * Sign(dh)*Abs(dh-H_zac2) * n0;
        }

        /// <summary>
        /// Вычисляется глобальная сила, действующая в тангенциальном направлении (только в окрестностях "зубцов")
        /// </summary>
        /// <param name="globalPoint"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3D GetLocalKTauForce_Experimental(Vector3D globalPoint,Vector3D globalVel) {
            var localPoint = worldTransform_1 * globalPoint;

            var rlocalVec = new Vector3D(0,localPoint.Y,localPoint.Z);
            var rp = rlocalVec.GetLength();
            if(rp > R_max || Abs(localPoint.X) > H_wheel * 0.5)
                return Vector3D.Zero;

            int closestInd = GetClosestInd(localPoint);
            var angle0 = gamma * closestInd;//Betta % (2 * PI) + gamma * closestInd;



            var n0 = new Vector3D(0,Cos(angle0 + 0.5 * PI),Sin(angle0 + 0.5 * PI));
            var dh = n0 * rlocalVec;


            var localVel = WorldTransformRot_1 * globalVel;
            var myLocalPos = Zubya[closestInd].Norm * rp;
            var myVelLocal = GetVelLocal(myLocalPos);
            double mn = rp > R ? (R_max - rp)/(R_max - R) : 1d;
            return Phys3D.GetKMuForce(localPoint,localVel,myLocalPos,myVelLocal,kH,muH,0)*mn;
            // return -kH * Sign(dh)*Abs(dh-H_zac2) * n0;
        }

        /// <summary>
        /// Получить индекс ближайшего зубца
        /// </summary>
        /// <param name="localPoint"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetClosestInd(Vector3D localPoint) {
            const double _2PI = 2 * PI;
            var angle = Atan2(localPoint.Z,localPoint.Y);
            angle += angle < 0 ? _2PI : 0;
            var betta = 0d;//Betta % _2PI;
            var bettaOtn = angle - betta;
            bettaOtn += bettaOtn < 0 ? _2PI : 0;
            int answ = (int)Round(bettaOtn / gamma);
            answ = answ == n_shag ? 0 : answ;
            return answ;
        }

        public void AddMomentFunct(Func<double,double> mom_func, double maxOmegaX) {
            MomentX.SynchMeAfter += t => {
                var mom = mom_func(t);
                MomentX.Value = Sign(mom) == Sign(Omega.X) && Abs(Omega.X) < maxOmegaX ? mom : 0d;
            };
        }

        public void AddMomentFunct(double mom_const,double maxOmegaX) {
            MomentX.SynchMeAfter += t => {
                var mom = mom_const;
                MomentX.Value = Sign(mom) == Sign(Omega.X) && Abs(Omega.X) < maxOmegaX ? mom : 0d;
            };
        }

    }

}
