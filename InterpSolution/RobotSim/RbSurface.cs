using Sharp3D.Math.Core;
using SimpleIntegrator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RobotSim {
    public interface IRbSurf {
        double K { get; set; }
        double Mu { get; set; }
        Vector3D N0 { get; }
        Vector3D GetNForce(Vector3D WorldPos,Vector3D WorldVel);
        bool HasTouch(Vector3D WorldPos);
        double GetDistance(Vector3D posWorld);
    }

    public class RbSurfFloor : IRbSurf {
        protected double k, mu;

        public double K {
            get {
                return k;
            }

            set {
                k = value;
            }
        }

        public double Mu {
            get {
                return mu;
            }

            set {
                mu = value;
            }
        }

        public Vector3D N0 {
            get {
                return Vector3D.YAxis;
            }
        }

        public Vector3D p0;

        public RbSurfFloor(double k,double mu,Vector3D p0) {
            this.p0 = p0;
            this.k = k;
            this.mu = mu;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual Vector3D GetNForce(Vector3D WorldPos,Vector3D WorldVel) {
            if(!HasTouch(WorldPos))
                return Vector3D.Zero;
            double dy = p0.Y - WorldPos.Y;
            var res = new Vector3D(0,dy * k,0);
            if(WorldVel.Y > 0)
                return res;
            res.Y -= WorldVel.Y * mu;
            return res;

        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual bool HasTouch(Vector3D WorldPos) {
            return WorldPos.Y <= p0.Y;
        }

        public virtual double GetDistance(Vector3D posWorld) {
            return posWorld.Y - p0.Y;
        }
    }

    public class RbSurfAngleFloor : RbSurfFloor {
        Vector3D n0;
        public new Vector3D N0 {
            get {
                return n0;
            }
        }
        public RbSurfAngleFloor(double k,double mu,Vector3D p0,Vector3D n0) : base(k,mu,p0) {
            this.n0 = n0.Norm;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool HasTouch(Vector3D WorldPos) {
            return (WorldPos - p0) * n0 <= 0;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override Vector3D GetNForce(Vector3D WorldPos,Vector3D WorldVel) {
            if(!HasTouch(WorldPos))
                return Vector3D.Zero;
            var dy = (WorldPos - p0) * n0;
            var res = dy * k + (WorldVel * n0 < 0 ? mu * WorldVel * n0 : 0);
            return -n0 * res;

        }
        public override double GetDistance(Vector3D posWorld) {
            return ((posWorld - p0) * n0);


        }
    }


    public class GroundForce : Force {
        IRbSurf surf;
        MaterialObjectNewton who;
        public GroundForce(MaterialObjectNewton who,Vector3D localP,IRbSurf surf) : base(0,new RelativePoint(Vector3D.YAxis),new RelativePoint(localP,who)) {
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
        IRbSurf surf;
        MaterialObjectNewton who;
        Func<double,double> f_ot_dist;
        public MagneticForce(MaterialObjectNewton who,Vector3D localP,IRbSurf surf, Func<double,double> f_ot_dist) : base(0,new RelativePoint(Vector3D.YAxis),new RelativePoint(localP,who)) {
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
