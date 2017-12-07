using Microsoft.Research.Oslo;
using Sharp3D.Math.Core;
using SimpleIntegrator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeetingPro {
    public class Grammy {
        public Vector vUp, vDown, vLeft, vRight, vCenter;
        //0 - temperat
        //1 - time
        //2 - vel
        //3 - alph
        //4 - bet
        //5 - thetta

        //6 - x
        //7 - y
        //8 - z
        //9 - Vx
        //10 - Vy
        //11 - Vz
        Vector FormVector(OneWay ow, Orient3D sk0) {
            var res = Vector.Zeros(vecLength);
            res[0] = ow.Vec1.Temperature;
            res[1] = ow.Vec1.T;
            res[2] = ow.Vec1.V;
            res[3] = ow.Vec1.Alpha;
            res[4] = ow.Vec1.Betta;
            res[5] = ow.Vec1.Thetta;
            var vec1 = sk0.WorldTransform_1 * ow.Pos1.GetPos0();
            res[6] = vec1.X;
            res[7] = vec1.Y;
            res[8] = vec1.Z;
            var vel1 = sk0.WorldTransformRot_1 * ow.Pos1.GetVel0();
            res[9] = vel1.X;
            res[10] = vel1.Y;
            res[11] = vel1.Z;
            return res;
        }
        public GrammyPolygon[] polygons;
        public Vector vBegin;
        public int vecBeginLength = 6;
        public int vecLength = 12;
        public Grammy() {
            vBegin = Vector.Zeros(vecBeginLength);
            vUp = Vector.Zeros(vecLength);
            vDown = Vector.Zeros(vecLength);
            vLeft = Vector.Zeros(vecLength);
            vRight = Vector.Zeros(vecLength);
            vCenter = Vector.Zeros(vecLength);
            IntiPolygons();
        }
        public Vector ToOneVector() {
            var res = new double[vecBeginLength + vecLength * 5];
            int i = 0;
            for (int j = 0; j < vBegin.Length; j++) {
                res[i] = vBegin[j];
                i++;
            }
            for (int j = 0; j < vUp.Length; j++) {
                res[i] = vUp[j];
                i++;
            }
            for (int j = 0; j < vDown.Length; j++) {
                res[i] = vDown[j];
                i++;
            }
            for (int j = 0; j < vLeft.Length; j++) {
                res[i] = vLeft[j];
                i++;
            }
            for (int j = 0; j < vRight.Length; j++) {
                res[i] = vRight[j];
                i++;
            }
            return new Vector(res);
        }
        public void FromOneVector(Vector vec) {
            int i = 0;
            for (int j = 0; j < vBegin.Length; j++) {
                 vBegin[j] = vec[i];
                i++;
            }
            for (int j = 0; j < vUp.Length; j++) {
                vUp[j] = vec[i];
                i++;
            }
            for (int j = 0; j < vDown.Length; j++) {
                vDown[j] = vec[i];
                i++;
            }
            for (int j = 0; j < vLeft.Length; j++) {
                vLeft[j] = vec[i];
                i++;
            }
            for (int j = 0; j < vRight.Length; j++) {
                vRight[j] = vec[i];
                i++;
            }
            IntiPolygons();
        }
        public void FromOneWayList(List<OneWay> list) {
            var uniq = list
                .AddCoord(180,180)
                .Uniquest();

            var sk0 = new Orient3D();

            var ow0 = uniq[0].ow;
            var pos0 = ow0.Pos0;

            sk0.SetPosition_LocalPoint_LocalFixed(Vector3D.XAxis, new Vector3D(pos0.V_x, 0, pos0.V_z), -Vector3D.YAxis, Vector3D.YAxis);
            sk0.Vec3D = pos0.GetPos0();
            sk0.SynchQandM();

            vBegin[0] = ow0.Vec0.Temperature;
            vBegin[1] = ow0.Vec0.T;
            vBegin[2] = ow0.Vec0.V;
            vBegin[3] = ow0.Vec0.Alpha;
            vBegin[4] = ow0.Vec0.Betta;
            vBegin[5] = ow0.Vec0.Thetta;

            vUp = FormVector(uniq.Find(tp => tp.ow.GetPos().EqualsApprox(new Vector2D(0, 1))).ow, sk0);
            vLeft = FormVector(uniq.Find(tp => tp.ow.GetPos().EqualsApprox(new Vector2D(-1, 0))).ow, sk0);
            vRight = FormVector(uniq.Find(tp => tp.ow.GetPos().EqualsApprox(new Vector2D(1, 0))).ow, sk0);
            vDown = FormVector(uniq.Find(tp => tp.ow.GetPos().EqualsApprox(new Vector2D(0, -1))).ow, sk0);
            vCenter = FormVector(uniq.Find(tp => tp.ow.GetPos().EqualsApprox(new Vector2D(0, 0))).ow, sk0);
            IntiPolygons();
        }
        public void IntiPolygons() {
            polygons = new GrammyPolygon[4];
            polygons[0] = new GrammyPolygon(vCenter, vUp, vLeft);
            polygons[1] = new GrammyPolygon(vCenter, vLeft, vDown);
            polygons[2] = new GrammyPolygon(vCenter, vDown, vRight);
            polygons[3] = new GrammyPolygon(vCenter, vRight, vUp);
        }

        public double Temperature => vBegin[0];
        public double T => vBegin[1];
        public double V => vBegin[2];
        public double Alpha => vBegin[3];
        public double Betta => vBegin[4];
        public double Thetta => vBegin[5];

        //public Vector PolygonsIntercept(Vector3D ray) {

        //}

        //public bool PolyIntercept(Vector3D ray, Vector[] polygon, ref Vector3D intercP) {

        //}
    }

    public class GrammyPolygon {
        public Vector v1, v2, v3;
        public Vector3D p1, p2, p3;
        public TrInterpolator interps;
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

    public class GrammyCluster {
        public double[][] dems;
        public double[] dems_dx;
        public Grammy[
            //0 - temperat
            ,//1 - time
            ,//2 - vel
            ,//3 - alph
            ,//4 - bet
            ,//5 - thetta
            ] data; //6 - dem
        public GrammyCluster(List<Grammy> lst) {
            InitDems(lst);
            foreach (var (igr0, i0) in lst.GroupBy(gr => Math.Round(gr.Temperature)).OrderBy(igr => igr.Key).Select((igr,i)=> (igr, i))) {
                foreach (var (igr1, i1) in igr0.GroupBy(gr => Math.Round(gr.T)).OrderBy(igr => igr.Key).Select((igr, i) => (igr, i))) {
                    foreach (var (igr2, i2) in igr1.GroupBy(gr => Math.Round(gr.V)).OrderBy(igr => igr.Key).Select((igr, i) => (igr, i))) {
                        foreach (var (igr3, i3) in igr2.GroupBy(gr => Math.Round(gr.Alpha)).OrderBy(igr => igr.Key).Select((igr, i) => (igr, i))) {
                            foreach (var (igr4, i4) in igr3.GroupBy(gr => Math.Round(gr.Betta)).OrderBy(igr => igr.Key).Select((igr, i) => (igr, i))) {
                                foreach (var (igr5, i5) in igr4.GroupBy(gr => Math.Round(gr.Thetta)).OrderBy(igr => igr.Key).Select((igr, i) => (igr, i))) {
                                    data[i0, i1, i2, i3, i4, i5] = igr5.First();
                                }
                            }
                        }
                    }
                }
            }

        }

        public static Task<GrammyCluster> CreateAsync(List<Grammy> lst) {
            return Task.Factory.StartNew<GrammyCluster>(()=>new GrammyCluster(lst));
        }

        public void InitDems(List<Grammy> lst) {
            var temperat = lst
                .GroupBy(gr => Math.Round(gr.Temperature))
                .Select(igr => igr.Key)
                .OrderBy(k => k)
                .ToArray();
            var time = lst
                .GroupBy(gr => Math.Round(gr.T))
                .Select(igr => igr.Key)
                .OrderBy(k => k)
                .ToArray();
            var vel = lst
                .GroupBy(gr => Math.Round(gr.V))
                .Select(igr => igr.Key)
                .OrderBy(k => k)
                .ToArray();
            var alph = lst
                .GroupBy(gr => Math.Round(gr.Alpha))
                .Select(igr => igr.Key)
                .OrderBy(k => k)
                .ToArray();
            var bet = lst
                .GroupBy(gr => Math.Round(gr.Betta))
                .Select(igr => igr.Key)
                .OrderBy(k => k)
                .ToArray();
            var thetta = lst
                .GroupBy(gr => Math.Round(gr.Thetta))
                .Select(igr => igr.Key)
                .OrderBy(k => k)
                .ToArray();

            dems = new double[][] {
                temperat,
                time,
                vel,
                alph,
                bet,
                thetta
            };

            data = new Grammy[
                temperat.Length,
                time.Length,
                vel.Length,
                alph.Length,
                bet.Length,
                thetta.Length];

            dems_dx = new double[dems.Length];
            for (int i = 0; i < dems_dx.Length; i++) {
                dems_dx[i] = dems[i][1] - dems[i][0];
            }
        }

        public MT_pos GoToNextPos(MT_pos fromPos, MT_pos posFromGranny) {
            var vx0 = fromPos.V_x;
            var vz0 = fromPos.V_z;
            var vxz0 = Math.Sqrt(vx0 * vx0 + vz0 * vz0);

            var x_vx0n = vx0 / vxz0;
            var x_vz0n = vz0 / vxz0;

            var z_vx0n = -x_vz0n;
            var z_vz0n = x_vx0n;

            var x1 = fromPos.X + x_vx0n * posFromGranny.X + z_vx0n * posFromGranny.Z;
            var y1 = fromPos.Y + posFromGranny.Y;
            var z1 = fromPos.Z + x_vz0n * posFromGranny.X + z_vz0n * posFromGranny.Z;

            var vx1 = x_vx0n * posFromGranny.V_x + z_vx0n * posFromGranny.V_z;
            var vy1 = posFromGranny.V_y;
            var vz1 = x_vz0n * posFromGranny.V_x + z_vz0n * posFromGranny.V_z;

            return new MT_pos() {
                X = x1,
                Y = y1,
                Z = z1,
                V_x = vx1,
                V_y = vy1,
                V_z = vz1
            };
        }

        public Grammy GrammyInterp(Vector vBegin) {
            var lst = new List<(int ind0, int ind1, double t)>();
            for (int i = 0; i < vBegin.Length; i++) {
                var tp = GetInterp(i, vBegin[i]);
                lst.Add(tp);
            }
            int n = (int)Math.Pow(2, vBegin.Length);
            var n_devs = new int[vBegin.Length];
            n_devs[0] = n / 2;
            for (int i = 1; i < vBegin.Length; i++) {
                n_devs[i] = n_devs[i-1] / 2;
            }
            var vecs = new List<Vector>(n);
            
            for (int i = 0; i < n; i++) {
                var n_inds = new int[vBegin.Length];
                for (int j = 0; j < n_inds.Length; j++) {
                    //Вот тут надо исправить
                    n_inds[j] = (i / n_devs[j])% n_devs[j] == 0
                        ? lst[j].ind0
                        : lst[j].ind1;
                    
                }
                var vec = data[
                    n_inds[0],
                    n_inds[1],
                    n_inds[2],
                    n_inds[3],
                    n_inds[4],
                    n_inds[5]].ToOneVector();
                vecs.Add(vec);
            }
            for (int i = 0; i < n_devs.Length; i++) {
                var vecs_reduce = new List<Vector>(n_devs[i]);
                for (int j = 0; j < n_devs[i]; j++) {
                    var vec_red = vecs[j] * lst[i].t + vecs[j + n_devs[i]] * (1 - lst[i].t);
                    vecs_reduce.Add(vec_red);
                }
                vecs = vecs_reduce;
            }
            var res = new Grammy();
            res.FromOneVector(vecs[0]);
            return res;

        }

        (int ind0, int ind1, double t) GetInterp(int inds_n, double val) {
            var ind0 = (int)((val - dems[inds_n][0]) / dems_dx[inds_n]);
            if (ind0 < 0)
                ind0 = 0;
            var ind1 = ind0 + 1;
            if(ind1 >= dems[inds_n].Length) {
                return (dems[inds_n].Length-2, dems[inds_n].Length-1,1d);
            } else {
                return (ind0, ind1, (val - dems[inds_n][0]) / dems_dx[inds_n] - ind0);
            }
        }
    }
}
