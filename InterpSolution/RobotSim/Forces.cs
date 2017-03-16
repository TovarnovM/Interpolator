using Sharp3D.Math.Core;
using SimpleIntegrator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RobotSim {

    public class FrictMoment : Force {
        public Func<double,double> k_func;
        public FrictMoment(Func<double,double> k_func,RelativePoint direction) : base(0,direction,null) {
            this.k_func = k_func;
            //Sy
        }

    }

    /// <summary>
    /// Сила между двумя объектами.... сила приложена к 0 точке
    /// </summary>
    public class ForceBetween2Points : Force {
        public IMaterialObject sk0, sk1;
        public IPosition3D p0_loc, p1_loc;
        public bool Zeros = false;
        public double k = 100, mu = 100, x0 = 0, k2 = 1000, mu2 = 100, x02 = -1d;
        public ForceBetween2Points(
            IMaterialObject sk0,
            IMaterialObject sk1,
            Vector3D p0_loc,
            Vector3D p1_loc,
            double k = 1000d,
            double mu = 100d,
            double x0 = 0d,
            double k2 = 30000d,
            double mu2 = 200d,
            double x02 = -1d) : this(sk0,sk1,new Position3D(p0_loc),new Position3D(p1_loc),k,mu,x0,k2,mu2,x02) {

        }
        public ForceBetween2Points(
            IMaterialObject sk0,
            IMaterialObject sk1,
            IPosition3D p0_loc,
            IPosition3D p1_loc,
            double k = 1000d,
            double mu = 100d,
            double x0 = 0d,
            double k2 = 30000d,
            double mu2 = 200d,
            double x02 = -1d) : base(0,new RelativePoint(1,0,0),null) {

            this.sk0 = sk0;
            this.sk1 = sk1;
            this.p0_loc = p0_loc;
            this.p1_loc = p1_loc;
            AppPoint = new RelativePoint(0,0,0);
            this.k = k;
            this.mu = mu;
            this.x0 = x0;
            this.k2 = k2;
            this.mu2 = mu2;
            this.x02 = x02;
            SynchMeBefore += synchMeBeforeAct;
        }

        void synchMeBeforeAct(double t) {
            Value = 0d;
            if(Zeros) {
                return;
            }
            var p0loc = p0_loc.Vec3D;
            var p1loc = p1_loc.Vec3D;

            var f = Phys3D.GetKMuForce(
                sk0.WorldTransform * p0loc,
                sk0.GetVelWorld(p0loc),
                sk1.WorldTransform * p1loc,
                sk1.GetVelWorld(p1loc),
                k,mu,x0
                );
            if(x02 > 0d) {
                f += Phys3D.GetKMuForce_Step(
                sk0.WorldTransform * p0loc,
                sk0.GetVelWorld(p0loc),
                sk1.WorldTransform * p1loc,
                sk1.GetVelWorld(p1loc),
                k2,mu2,x02
                );
            }
            Value = f.GetLength();
            Direction.Vec3D = f;
            AppPoint.Vec3D = sk0.WorldTransform * p0loc;
        }
    }

    public static class Phys3D {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3D GetKMuForce(IVelPos3D me,IVelPos3D toThis,double k,double mu,double x0) {
            return GetKMuForce(me.Vec3D,me.Vel.Vec3D,toThis.Vec3D,toThis.Vel.Vec3D,k,mu,x0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3D GetKMuForce(Vector3D mePos,Vector3D meVel,Vector3D toThisPos,Vector3D toThisVel,double k,double mu,double x0) {
            var deltPos = toThisPos - mePos;
            double deltX = x0 - deltPos.GetLength();
            var deltPosNorm = deltPos.Norm;
            var Vpr_me = meVel * deltPosNorm;
            var Vpr_toThis = toThisVel * deltPosNorm;
            var VotnPr_me = Vpr_me - Vpr_toThis;

            return (-k * deltX - mu * VotnPr_me) * deltPosNorm;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3D GetKMuForce_Step(Vector3D mePos,Vector3D meVel,Vector3D toThisPos,Vector3D toThisVel,double k,double mu,double x0) {
            var deltPos = toThisPos - mePos;

            double deltX = x0 - deltPos.GetLength();
            if(deltX > 0)
                return Vector3D.Zero;
            var deltPosNorm = deltPos.Norm;
            var Vpr_me = meVel * deltPosNorm;
            var Vpr_toThis = toThisVel * deltPosNorm;
            var VotnPr_me = Vpr_me - Vpr_toThis;

            return (-k * deltX - mu * VotnPr_me) * deltPosNorm;
        }
    }
    public class GroundForce : Force {
        FlatSurf surf;
        MaterialObjectNewton who;
        public GroundForce(MaterialObjectNewton who,Vector3D localP,FlatSurf surf) : base(0,new RelativePoint(Vector3D.YAxis),new RelativePoint(localP,who)) {
            this.surf = surf;
            this.who = who;
            Direction.Vec3D = surf.N0;
            SynchMeBefore += SynchAction;


        }
        public void SynchAction(double t) {
            var f = surf.GetNForce(AppPoint.Vec3D_World,who.GetVelWorld(AppPoint.Vec3D));

            Value = f.GetLength();
            Direction.Vec3D = f.Norm;
        }
    }

    public class MagneticForce : Force {
        FlatSurf surf;
        MaterialObjectNewton who;
        Func<double,double> f_ot_dist;
        public MagneticForce(MaterialObjectNewton who,Vector3D localP,FlatSurf surf,Func<double,double> f_ot_dist) : base(0,new RelativePoint(Vector3D.YAxis),new RelativePoint(localP,who)) {
            this.surf = surf;
            this.who = who;
            this.f_ot_dist = f_ot_dist;
            Direction.Vec3D = -surf.N0;
            SynchMeBefore += SynchAction;
        }
        public void SynchAction(double t) {
            Value = f_ot_dist(surf.GetDistance(AppPoint.Vec3D_World));
            Direction.Vec3D = -surf.N0;
        }
    }
}
