using Microsoft.Research.Oslo;
using Sharp3D.Math.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MeetingPro {
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
                                    //if(i0 == 3 && i1 == 0 && i2 == 2 && i3 == 4 && i4 ==3 && i5 ==2) {
                                    //    break;
                                    //}
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
                    n_inds[j] = (i %(2* n_devs[j]))/ n_devs[j] == 0
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
                    var vec_red = vecs[j] * (1-lst[i].t) + vecs[j + n_devs[i]] * lst[i].t;
                    vecs_reduce.Add(vec_red);
                }
                vecs = vecs_reduce;
            }

            var res = new Grammy();
            res.FromOneVector(vecs[0]);
            return res;

        }

        (int ind0, int ind1, double t) GetInterp(int inds_n, double val) {
            double ind0d = (val - dems[inds_n][0]) / dems_dx[inds_n];
            var ind0 = (int)(ind0d);
            if (ind0d < 0) {
                return (0, 1, 0);
            }               
            var ind1 = ind0 + 1;
            if(ind1 >= dems[inds_n].Length) {
                return (dems[inds_n].Length-2, dems[inds_n].Length-1,1d);
            } else {
                return (ind0, ind1, (val - dems[inds_n][0]) / dems_dx[inds_n] - ind0);
            }
        }

        public void GrammyStep_ray(Vector3D dist_ray, ref MT_pos currPos, ref Vector currVec) {           
            var v0 = currPos.GetVel0();
            var x0 = new Vector3D(v0.X, 0, v0.Z).Norm;
            var y0 = new Vector3D(0, 1, 0);
            var z0 = new Vector3D(-x0.Z, 0, x0.X);

            var ray_local = new Vector3D(dist_ray * x0, dist_ray * y0, dist_ray * z0);

            var grcurr = GrammyInterp(currVec);
            var nextVec = grcurr.PolygonsIntercept(new Vector3D(0, 0, 0), ray_local);
            var posFromGrammy = Grammy.PosFromVec(nextVec);
            var nextPos = GoToNextPos(currPos, posFromGrammy);

            currPos = nextPos;
            currVec = Grammy.BeginVecFromVec(nextVec);
        }

        public double GrammyStep_toPoint(Vector3D to_point, ref MT_pos currPos, ref Vector currVec) {
            var rayDist = (to_point - currPos.GetPos0()).Norm;
            GrammyStep_ray(rayDist, ref currPos, ref currVec);
            return (to_point - currPos.GetPos0()).GetLength();
        }

        public List<Vector3D> GetTstList(Vector3D p0, Vector3D p_trg, double temperat, int n = 23 * 30) {
            var (pos0, vec0, _) = InitConditions.GetInitCondition(p0, p_trg, temperat);

            var res = new List<Vector3D>(n + 2);
            res.Add(pos0.GetPos0());
            for (int i = 0; i < n; i++) {
                var dist = GrammyStep_toPoint(p_trg, ref pos0, ref vec0);
                if (dist < 100) {
                    break;
                }
                res.Add(pos0.GetPos0());
            }
            return res;
        }
        public List<Vector3D> GetTstList2(Vector3D p0, Vector3D p_dir, double temperat, int n = 23 * 30) {
            var (pos0, vec0, _) = InitConditions.GetInitCondition(p0, p0 + p_dir*10, temperat);

            var res = new List<Vector3D>(n + 2);
            res.Add(pos0.GetPos0());
            for (int i = 0; i < n; i++) {
                GrammyStep_ray(p_dir, ref pos0, ref vec0);

                res.Add(pos0.GetPos0());
            }
            return res;
        }
    }
}
