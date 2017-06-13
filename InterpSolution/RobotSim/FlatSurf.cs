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
    //public interface IRbSurf {
    //    double K { get; set; }
    //    double Mu { get; set; }
    //    Vector3D N0 { get; }
    //    Vector3D GetNForce(Vector3D WorldPos,Vector3D WorldVel);
    //    bool HasTouch(Vector3D WorldPos);
    //    double GetDistance(Vector3D posWorld);
    //}

    public class FlatSurf {//: IRbSurf {
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
                return n0;
            }
        }
        public Vector3D p0;
        public Vector3D n0;
        public FlatSurf(double k,double mu,Vector3D p0):this(k,mu,p0,Vector3D.YAxis) {  }
        public FlatSurf(double k,double mu,Vector3D p0,Vector3D n0) {
            this.p0 = p0;
            this.k = k;
            this.mu = mu;
            this.n0 = n0.Norm;
        }
        public List<(int id, Vector3D p, double value)> LogFromStep = new List<(int id, Vector3D p, double value)>();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3D GetNForce(Vector3D WorldPos,Vector3D WorldVel, int logId = -1) {
            if(!HasTouch(WorldPos))
                return Vector3D.Zero;
            var dy = (WorldPos - p0) * n0;
            var res = dy * k + (WorldVel * n0 < 0 ? mu * WorldVel * n0 : 0);
            var l  = GetNormal(WorldPos);
            if(logId != -1)
                LogFromStep.Add((logId,l.p1, Abs(res)));
            return -n0 * res;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasTouch(Vector3D WorldPos) {
            return (WorldPos - p0) * n0 <= 0;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double GetDistance(Vector3D posWorld) {
            return ((posWorld - p0) * n0);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3D GetIntersect(Line3D line) {
            var u = line.u;
            var dot = n0 * u;
            if (Math.Abs(dot) > 1E-8) {
                var w = line.p0 - p0;
                var fac = -(n0 * w) / dot;
                return line.p0 + (u * fac);
            }
            return Vector3D.Zero;

        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool BelongPoint(Vector3D point, double eps = 1E-9) {
            var r = p0 - point;
            return r * n0 < eps;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool BelongLine(Line3D l,double eps = 1E-9) {
            return BelongPoint(l.p0,eps) && BelongPoint(l.p1,eps);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Line3D GetNormal(Vector3D point) {
            var r = p0 - point;
            var rn = (r * n0) * n0;
            return new Line3D(point,point + rn);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Line3D GetIntersect(FlatSurf s1,FlatSurf s2) {
            var u = s1.n0 & s2.n0;
            if(u.GetLengthSquared() < 1E-10)
                return null;
            u.Normalize();
            var un1 = u & s1.n0;
            var ln = new Line3D(s1.p0,s1.p0 + un1);
            var lp0 = s2.GetIntersect(ln);
            return new Line3D(lp0,lp0 + un1);
        }
        public static Line3D GetH(Vector3D point, Vector3D dir, IEnumerable<FlatSurf> surfs) {
            var ps = surfs
                .Select(s => s.GetIntersect(new Line3D(point,point + dir)))
                .Where(p => (p - point) * dir > 0)
                .OrderBy(p => (p - point).GetLengthSquared())
                .ToList();
            if(ps.Count == 0)
                return new Line3D(point,point);
            return new Line3D(point,ps[0]);
        }
        public static Line3D GetH(Vector3D point,IEnumerable<FlatSurf> surfs) {
            var ps = surfs
                .Select(s => s.GetNormal(point))
                .OrderBy(l => l.GetLength())
                .ToList();
            if(ps.Count == 0)
                return new Line3D(point,point);
            return ps[0];
        }

    }

    public class Line3D {       
        public Vector3D p1, p0;
        public Vector3D u {
            get {
                return (p1 - p0).Norm;
            }
        }
        public Line3D(Vector3D p0, Vector3D p1) {
            this.p1 = p1;
            this.p0 = p0;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equal(Line3D l) {
            var cr1 = u & l.u;
            if(cr1.GetLengthSquared() > 1E-9)
                return false;
            var cr2 = (p0 - l.p0) & u;
            if(cr2.GetLengthSquared() > 1E-9)
                return false;
            return true;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double GetLength() {
            return (p0 - p1).GetLength();
        }
    }



}
