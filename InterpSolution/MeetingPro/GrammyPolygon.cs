using Microsoft.Research.Oslo;
using Sharp3D.Math.Core;

namespace MeetingPro {
    public class GrammyPolygon {
        public Vector v1, v2, v3;
        public Vector3D p1, p2, p3;
        public TrInterpolator interps = null;
        public GrammyPolygon(Vector v1, Vector v2, Vector v3) {
            this.v1 = v1;
            this.v2 = v2;
            this.v3 = v3;
            p1 = new Vector3D(v1[6], v1[7], v1[8]);
            p2 = new Vector3D(v2[6], v2[7], v2[8]);
            p3 = new Vector3D(v3[6], v3[7], v3[8]);
        }
        public bool IsCross(Vector3D p_ray, Vector3D ray_dir, ref Vector3D cross_p, ref double dist) {
            var D = ray_dir.Norm;
            var E1 = p2 - p1;
            var E2 = p3 - p1;
            var T = p_ray - p1;
            var P = D & E2;
            var Q = T & E1;
            var znam = P * E1;
            var t = (Q * E2) / znam;
            var u = (P * T) / znam;
            var v = (Q * D) / znam;
            var t1 = 1 - u - v;
            var cross_plane = p_ray + t * D;
            bool intersect = !(u < 0 || v < 0 || t1 < 0);
            if (!intersect) {
                var (d1, cp1) = DistanceToSegment(cross_plane, p1, p2);
                var (d2, cp2) = DistanceToSegment(cross_plane, p2, p3);
                var (d3, cp3) = DistanceToSegment(cross_plane, p1, p3);
                if (d1 < d2) {
                    if(d1 < d3) {
                        cross_p = cp1;
                        dist = d1;
                    } else {
                        cross_p = cp3;
                        dist = d3;
                    }
                } else {
                    if (d2 < d3) {
                        cross_p = cp2;
                        dist = d2;
                    } else {
                        cross_p = cp3;
                        dist = d3;
                    }
                }
            } else {
                cross_p = cross_plane;
                dist = 0d;
            }
            return intersect;
        }
        public static (double d, Vector3D closestP) DistanceToSegment(Vector3D p, Vector3D lp1, Vector3D lp2) {
            var v = lp2 - lp1;
            var w = p - lp1;

            var c1 = w * v;
            if (c1 <= 0)
                return ((p - lp1).GetLength(), lp1);

            double c2 = v*v;
            if (c2 <= c1)
                return ((p - lp2).GetLength(), lp2);

            double b = c1 / c2;
            var Pb = lp1 + b * v;
            return ((p - Pb).GetLength(), Pb);
        }
        public Vector InterpV(Vector3D p) {
            var x1 = (p2 - p1).Norm;
            var z1 = (x1 & (p3 - p1)).Norm;
            var y1 = x1 & z1;

            if(interps == null)
                interps = new TrInterpolator(
                    new Vector2D(0, 0),
                    new Vector2D((p2 - p1) * x1, (p2 - p1) * y1),
                    new Vector2D((p3 - p1) * x1, (p3 - p1) * y1),
                    v1,
                    v2,
                    v3);

            var p_loc = new Vector2D(x1 * (p - p1), y1 * (p - p1));
            return interps.Interp(p_loc);
        }
    }
}
