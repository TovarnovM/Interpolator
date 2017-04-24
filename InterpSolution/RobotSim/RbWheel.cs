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
            p0_body_loc = new Vector3D(0,0,0), // положение центра колеса в СК тела, к которому оно прикреплено
            n0_body_loc = new Vector3D(1,0,0), // направление оси вращения колеса (ось ОХ) в СК тела, к которому оно прикреплено
            betta0r_body_loc = new Vector3D(0,1,0); // направление линии, определяющей нулевой угол поворота колеса в СК тела, к которому оно прикреплено
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

            Mass3D.Ix = 20*mass * R * R / 2;
            Mass3D.Iy = mass * (3*R * R + H_wheel* H_wheel) / 12;
            Mass3D.Iz = Mass3D.Iy;

            MomentX = Force.GetMoment(0,new Vector3D(1,0,0),this);
            AddMoment(MomentX);
        }
        public static RbWheel GetStandart(double shagConnL = 0.009,int n = 17) {
            return GetStandart(Vector3D.Zero,shagConnL,n);
        }

        public static RbWheel GetStandart(Vector3D localPos,double shagConnL = 0.009, int n = 17) {
            double r_real = 0.015;
           
            double R_ideal = (shagConnL*1.05) / (2 * Sin(PI / n));//0.009 / (2 * Sin(PI / n));
            var mass = PI * r_real * r_real * 0.021 * 1080;
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
                MomentX.Value = mom;
                if(Sign(mom) != Sign(Omega.X))
                    MomentX.Value = mom;
                else if(Abs(Omega.X) < maxOmegaX)
                    MomentX.Value = mom;
                else
                    MomentX.Value = 0d;
            };
        }

        #region Blocked
        private bool blocked = false;
        public ForceBetween2Points f_wb,f_wbMirror;
        public void ConnectBlockToBody(MaterialObjectNewton body, double k = 100000, double mu = 10000) {
            var z0 = Zubya[0]*10;
            var z0uBody = body.WorldTransform_1 * (WorldTransform * z0);
            f_wb = new ForceBetween2Points(this,body,z0,z0uBody,k,mu,0);
            var Mirrz0uBody = body.WorldTransform_1 * (WorldTransform * (-z0));
            f_wbMirror = new ForceBetween2Points(this,body,-z0,Mirrz0uBody,k,mu,0);
            f_wb.Zeros = true;
            f_wbMirror.Zeros = true;
            AddForce(f_wb);
            body.AddForceNegative(f_wb);
            AddForce(f_wbMirror);
            body.AddForceNegative(f_wbMirror);

        }

        public bool Blocked {
            get { return blocked; }
            set {              
                if(f_wb == null)
                    return;
                blocked = value;
                f_wb.Zeros = !blocked;
                f_wbMirror.Zeros = !blocked;
                //if(!blocked && value) {                   
                var z0 = Zubya[0]*10;
                f_wb.p1_loc.Vec3D = f_wb.sk1.WorldTransform_1 * (WorldTransform * z0);
                var Mirrz0 = -z0;
                f_wbMirror.p1_loc.Vec3D = f_wbMirror.sk1.WorldTransform_1 * (WorldTransform * Mirrz0);
                //}
                

            }
        }


        #endregion

        #region TestConnect(hard)
        public MaterialObjectNewton BodyMaster = null;
        public Force forceToBody, momentToBody;
        public double Betta { get; set; } = 0d;
        public IScnPrm pBetta { get; set; }
        public double dBetta { get; set; } = 0d;
        public IScnPrm pdBetta { get; set; }
        public double ddBetta { get; set; } = 0d;
        public IScnPrm pddBetta { get; set; }

        public double BettaOtn {
            get {
                var b = Betta % (2 * PI);
                b += b < 0d ? (2 * PI) : 0d;
                return b;
            }
        }

        public double GetBettaFact() {
            var betta0r = WorldTransformRot_1 * (BodyMaster.WorldTransformRot * betta0r_body_loc);
            return Acos(Vector3D.YAxis * betta0r.Norm);
        }

        public void InitBettaFact() {
            if(BodyMaster != null)
                Betta = GetBettaFact();

        }

        public void SynchMeToBodyAndBetta(double t = 0d) {
            if(BodyMaster == null)
                return;
            var sk_glob = BodyMaster.WorldTransform * p0_body_loc;
            Vec3D = sk_glob;
            SynchQandM();
            var p0_plus_n0 = sk_glob + (BodyMaster.WorldTransformRot*n0_body_loc).Norm;
            SetPosition_LocalPoint_LocalFixed(Vector3D.XAxis,p0_plus_n0,new Vector3D(0,0,0));

            //развернуть согласно Бэтте

            var p0_plus_betta0r = sk_glob + (BodyMaster.WorldTransformRot * betta0r_body_loc);
            SetPosition_LocalPoint_LocalFixed(Vector3D.YAxis,p0_plus_betta0r,-Vector3D.XAxis,Vector3D.XAxis);
            if(Abs(Betta % PI) < 0.001 && Abs(Betta % (2*PI)) > 0.01) {
                double tmp = 0.6d * PI;
                SetPosition_LocalPoint_LocalMoveToIt_LocalFixed(Vector3D.YAxis,Vector3D.YAxis * Cos(Betta - tmp) + Vector3D.ZAxis * Sin(Betta - tmp),-Vector3D.XAxis,Vector3D.XAxis);
                SetPosition_LocalPoint_LocalMoveToIt_LocalFixed(Vector3D.YAxis,Vector3D.YAxis * Cos(tmp) + Vector3D.ZAxis * Sin(tmp),-Vector3D.XAxis,Vector3D.XAxis);
            } else {
                SetPosition_LocalPoint_LocalMoveToIt_LocalFixed(Vector3D.YAxis,Vector3D.YAxis * Cos(Betta) + Vector3D.ZAxis * Sin(Betta),-Vector3D.XAxis,Vector3D.XAxis);
            }
            //var xaxis_world = WorldTransformRot * Vector3D.XAxis;
            //var quat = QuaternionD.FromAxisAngle(xaxis_world,Betta);
            //q = quat * q;
            //SynchQandM();
        }

        public void NewtonLaw3D4Wheel(double t) {
            var fsummWorld = ForceWorldSumm(t);
            forceToBody.Vec3D_Dir_World = fsummWorld;
            var momSum = Vector3D.Zero;
            foreach(var mom in Moments) {
                momSum += mom.Vec3D_Dir_World;
            }
            foreach(var mom in MomentsNegative) {
                momSum -= mom.Vec3D_Dir_World * mom.ValueMultyplyer4Negative;
            }
            var xaxisWorld = WorldTransformRot * Vector3D.XAxis;

            var momToBodyVector = momSum - (momSum * xaxisWorld) * xaxisWorld.Norm;
            momentToBody.Vec3D_Dir_World = momToBodyVector;

            foreach(var force in Forces) {
                momSum += force.GetMoment_World(Vec3D);
            }
            foreach(var force in ForcesNegative) {
                momSum -= force.GetMoment_World(Vec3D) * force.ValueMultyplyer4Negative;
            }

            var momSumLocal = WorldTransformRot_1 * momSum;

                ddBetta = (momSumLocal * Vector3D.XAxis) / Mass3D.Ix;
                Omega.X = dBetta;

        }

        public void ConnectMeToBody(MaterialObjectNewton connectBody,double L_osi, double k = 100000,double mu = 1000) {
            var wheel = this;
            var p0_wheel_loc = new Vector3D(0,0,0);
            var p1_wheel_loc = p0_wheel_loc + Vector3D.XAxis * L_osi;
            var p1_body_loc = p0_body_loc + wheel.n0_body_loc * L_osi;

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
            wheel.ConnectBlockToBody(connectBody,k,mu);
        }

        public void ConnectMeToBody_newVariant(MaterialObjectNewton connectBody,double k = 100000,double mu = 1000) {
            pX.MyDiff = null;
            pY.MyDiff = null;
            pZ.MyDiff = null;
            Vel.pX.MyDiff = null;
            Vel.pY.MyDiff = null;
            Vel.pZ.MyDiff = null;
            pQw.MyDiff = null;
            pQx.MyDiff = null;
            pQy.MyDiff = null;
            pQz.MyDiff = null;
            pdQWdt.MyDiff = null;
            pdQXdt.MyDiff = null;
            pdQYdt.MyDiff = null;
            pdQZdt.MyDiff = null;
            Omega.pX.MyDiff = null;
            Omega.pY.MyDiff = null;
            Omega.pZ.MyDiff = null;

            SynchMeBefore = SynchMeToBodyAndBetta;
            SynchMeAfter = NewtonLaw3D4Wheel;
            AddDiffPropToParam(pBetta,pdBetta);
            AddDiffPropToParam(pdBetta,pddBetta);
            BodyMaster = connectBody;
            forceToBody = Force.GetForce(Vector3D.Zero,null,p0_body_loc,connectBody);
            connectBody.AddForce(forceToBody);
            momentToBody = Force.GetMoment(0d,Vector3D.XAxis);
            connectBody.AddMoment(momentToBody);

            SynchMeToBodyAndBetta();
            connectBody.AddMomentNegative(MomentX);
            ConnectBlockToBody(connectBody,k,mu);
        }
        #endregion

    }

}
