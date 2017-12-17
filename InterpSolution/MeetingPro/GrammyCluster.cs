using Microsoft.Research.Oslo;
using Sharp3D.Math.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
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

        public static MT_pos GoToNextPos(MT_pos fromPos, MT_pos posFromGranny) {
            var xl = new Vector3D(fromPos.V_x, 0, fromPos.V_z).Norm;
            var yl = new Vector3D(0, 1, 0);
            var zl = xl & yl;

            var xg = new Vector3D(Vector3D.XAxis * xl, 0, Vector3D.XAxis * zl);
            var yg = new Vector3D(0, 1, 0);
            var zg = new Vector3D(Vector3D.ZAxis * xl, 0, Vector3D.ZAxis * zl);

            var posl = posFromGranny.GetPos0();
            var pos1 = fromPos.GetPos0() + new Vector3D(posl * xg, posl * yg, posl * zg);

            var vell = posFromGranny.GetVel0();
            var vel1 = new Vector3D(vell * xg, vell * yg, vell * zg);

            return new MT_pos() {
                X = pos1.X,
                Y = pos1.Y,
                Z = pos1.Z,
                V_x = vel1.X,
                V_y = vel1.Y,
                V_z = vel1.Z
            };
            //var vx0 = fromPos.V_x;
            //var vz0 = fromPos.V_z;
            //var vxz0 = Math.Sqrt(vx0 * vx0 + vz0 * vz0);

            //var x_vx0n = vx0 / vxz0;
            //var x_vz0n = vz0 / vxz0;

            //var z_vx0n = -x_vz0n;
            //var z_vz0n = x_vx0n;

            //var x1 = fromPos.X + x_vx0n * posFromGranny.X + z_vx0n * posFromGranny.Z;
            //var y1 = fromPos.Y + posFromGranny.Y;
            //var z1 = fromPos.Z + x_vz0n * posFromGranny.X + z_vz0n * posFromGranny.Z;

            //var vx1 = x_vx0n * posFromGranny.V_x + z_vx0n * posFromGranny.V_z;
            //var vy1 = posFromGranny.V_y;
            //var vz1 = x_vz0n * posFromGranny.V_x + z_vz0n * posFromGranny.V_z;

            //return new MT_pos() {
            //    X = x1,
            //    Y = y1,
            //    Z = z1,
            //    V_x = vx1,
            //    V_y = vy1,
            //    V_z = vz1
            //};
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

        public void GrammyStep_ray(Vector3D dist_ray, ref MT_pos currPos, ref Vector currVec, out double grammyL, out Grammy gr_curr) {           
            var v0 = currPos.GetVel0();
            var x0 = new Vector3D(v0.X, 0, v0.Z).Norm;
            var y0 = new Vector3D(0, 1, 0);
            var z0 = x0 & y0;

            var ray_local = new Vector3D(dist_ray * x0, dist_ray * y0, dist_ray * z0);

            gr_curr = GrammyInterp(currVec);
            var nextVec = gr_curr.PolygonsIntercept(new Vector3D(0, 0, 0), ray_local);
            var posFromGrammy = Grammy.PosFromVec(nextVec);
            var nextPos = GoToNextPos(currPos, posFromGrammy);

            currPos = nextPos;
            currVec = Grammy.BeginVecFromVec(nextVec);
            
            grammyL = Grammy.PosFromVec(nextVec).GetPos0().GetLength();
        }

        public double GrammyStep_toPoint(Vector3D to_point, ref MT_pos currPos, ref Vector currVec, out double grammyL, out Grammy gr_curr) {
            var rayDist = (to_point - currPos.GetPos0()).Norm;
            GrammyStep_ray(rayDist, ref currPos, ref currVec,out grammyL, out gr_curr);
            return (to_point - currPos.GetPos0()).GetLength();
        }


        public List<(MT_pos pos,Grammy gr)> TraectToPoint(Vector3D p0, Vector3D p_trg, double temperat) {
            var (pos0, vec0, tend) = InitConditions.GetInitCondition(p0,new Vector3D(1,0,0), p_trg, temperat);
            int n = (int)(tend * 23) + 2;
            var res = new List<(MT_pos, Grammy)>(n);
            double coneL = 0d;
            double t = vec0[1];
            while(t<tend) {                
                var dist = GrammyStep_toPoint(p_trg, ref pos0, ref vec0, out coneL, out Grammy gr_curr);
                t += 1d/23d;
                gr_curr.vBegin[1] = t;
                res.Add((new MT_pos(pos0), gr_curr));
                if (dist < coneL*1.1) {
                    break;
                }               
            }
            return res;
        }
        public List<(MT_pos pos, Grammy gr)> TraectToDir(Vector3D p0, Vector3D p_dir, double temperat) {
            var (pos0, vec0, tend) = InitConditions.GetInitCondition(p0, new Vector3D(1, 0, 0), p0 + p_dir*1000, temperat);
            int n = (int)(tend * 23) + 2;
            var res = new List<(MT_pos, Grammy)>(n);
            double coneL = 0d;
            double t = vec0[1];
            while (t < tend) {
                GrammyStep_ray(p_dir, ref pos0, ref vec0, out coneL, out Grammy gr_curr);
                t += 1d / 23d;
                gr_curr.vBegin[1] = t;
                res.Add((new MT_pos(pos0), gr_curr));
            }
            return res;
        }
        public List<(MT_pos pos, Grammy gr)> TraectToPoint_inHorizSurf(Vector3D p0, Vector3D v_dir_0, Vector3D p_trg, double temperat) {
            
            var (pos0, vec0, tend) = InitConditions.GetInitCondition(p0, v_dir_0, p_trg, temperat);
            int n = (int)(tend * 23) + 2;
            var res = new List<(MT_pos, Grammy)>(n);
            double t = vec0[1];

            bool isHit = HitFromThatPos(pos0, vec0, p_trg, tend, res);

            return res;
        }

        public static Vector3D GetSurfTarget_toPoint(Vector3D n, Vector3D p_surf, Vector3D vel, Vector3D pos, Vector3D toPoint, double gamma = 30, double dt = 1d / 23d, double a = 13) {
            var p_loc1 = pos - p_surf;
            var pos_s = pos - (p_loc1 * n) * n;
            var y_s = (pos - pos_s).Norm;
            var vel_ys = (y_s * vel) * y_s;
            var vel_xs = vel - vel_ys;
            var x_s = vel_xs.Norm;
            var z_s = x_s & y_s;

            var l_s = vel.GetLength() * dt * a;

            var toPoint_s = toPoint - pos_s;
            var toPoint_s_1 = toPoint_s.Norm;

            if (l_s > (pos - toPoint).GetLength()) {
                return toPoint;
            }

            var gamma_toPoint = Math.Atan2(toPoint_s_1 * z_s, toPoint_s_1 * x_s)*180/Math.PI;
            if (Math.Abs(gamma_toPoint) > gamma) {
                gamma = Math.Abs(gamma) * Math.Sign(gamma_toPoint);
            } else {
                gamma = gamma_toPoint;
            }

            var r_s = x_s * Math.Cos(gamma * Math.PI / 180) + z_s * Math.Sin(gamma * Math.PI / 180);
            
            var p_dist_s = pos_s + r_s * l_s;
            return p_dist_s;
        }

        public bool GoodVec(ref Vector vec) {
            return !(dems[2][0] > vec[2] || vec[2] > dems[2][dems[2].Length - 1] //Vel
                //|| dems[3][0] > vec[3] || vec[3] > dems[3][dems[3].Length - 1] //alph
                //|| dems[4][0] > vec[4] || vec[4] > dems[4][dems[4].Length - 1] //bet
                || dems[5][0] > vec[5] || vec[5] > dems[5][dems[5].Length - 1] //Thetta
                );
        }

        public bool HitFromThatPos(MT_pos pos0, Vector vec0, Vector3D p_dist, Vector3D surf_n, Vector3D surf_p, double t_max, List<(MT_pos pos, Grammy gr)> lst, double dt = 1d/23d, double h0 = -20, double gst_dist = 1800) {
            bool hit = false;
            double coneL = 0d;
            double t = vec0[1];
            while (t < t_max && GoodVec(ref vec0)) {
                var curr_mt_pos = new MT_pos(pos0);
                double dist = (curr_mt_pos.GetPos0() - p_dist).GetLength();
                Vector3D currTrgP;
                if (dist > gst_dist)
                    currTrgP = GetSurfTarget_toPoint(surf_n, surf_p, pos0.GetVel0(), pos0.GetPos0(), p_dist);
                else
                    currTrgP = p_dist;
                GrammyStep_toPoint(currTrgP, ref pos0, ref vec0, out coneL, out Grammy gr_curr);
                t += dt;
                gr_curr.vBegin[1] = t;
                lst.Add((curr_mt_pos, gr_curr));
                
                if (dist < coneL * 3) {
                    hit = true;
                    break;
                }
                if (curr_mt_pos.GetPos0()[1] < h0) {
                    break;
                }
            }
            return hit;
        }

        public bool HitFromThatPos(MT_pos pos0, Vector vec0, Vector3D p_dist, double t_max, List<(MT_pos pos, Grammy gr)> lst, double dt = 1d / 23d, double h0 = -20, double gst_dist = 1800) {
            var p_in_surf = pos0.GetPos0();
            var p_in_surf2 = p_in_surf + pos0.GetVel0();

            var surf_n = ((p_in_surf2 - p_in_surf).Norm & (p_dist - p_in_surf).Norm).Norm;
            return HitFromThatPos(pos0, vec0, p_dist, surf_n, p_in_surf, t_max, lst, dt, h0, gst_dist);
        }
        
        public List<(MT_pos pos, Grammy gr)> GetExtrimeTraect(MT_pos pos0, Vector vec0, Vector3D p_trg, Vector3D p_trg_extrime_dir, double tMax, double tFast, double h0 = -20) {
            double tFast_base = tFast * 0.85; 
            int nfast = (int)(tFast_base * 23) + 2;
            int nmax = (int)(tMax * 23) + 2;
            var base_traect = new List<(MT_pos, Grammy)>(nfast);
            var extrime_traect = new List<(MT_pos, Grammy)>(nmax);

            var p_extrime_trg = p_trg + p_trg_extrime_dir.Norm * 30000;

            HitFromThatPos(pos0, vec0, p_extrime_trg, tFast_base, base_traect);
            int i_base_max = base_traect.Count - 1;
            int i_base_curr = i_base_max / 2;
            int i_base_min = 0;
            int extrime_ind = 0;

            while (i_base_max- i_base_min>13) {
                var tup = base_traect[i_base_curr];
                var pos1 = new MT_pos(tup.Item1);
                var vec1 = tup.Item2.vBegin.Clone();

                var traect_curr = new List<(MT_pos, Grammy)>(nmax);

                var hit = HitFromThatPos(pos1, vec1, p_trg, tMax, traect_curr, h0:h0);
                if (hit) {
                    extrime_traect = traect_curr;
                    extrime_ind = i_base_curr;
                    i_base_min = i_base_curr;
                    i_base_curr = (i_base_max + i_base_min) / 2;
                } else {
                    i_base_max = i_base_curr;
                    i_base_curr = (i_base_max + i_base_min) / 2;
                }
            }

            if (extrime_ind == 0) {
                return extrime_traect;
            }

            var res = new List<(MT_pos, Grammy)>(nmax);
            for (int i = 0; i <= extrime_ind; i++) {
                res.Add(base_traect[i]);
            }
            for (int i = 0; i < extrime_traect.Count; i++) {
                res.Add(extrime_traect[i]);
            }
            return res;
        }

        public Dictionary<string, List<(MT_pos pos, Grammy gr)>> getSuperDict(Vector3D p0, Vector3D v_dir_0, Vector3D p_trg, double temperat) {
            var (pos0, vec0, tend) = InitConditions.GetInitCondition(p0, v_dir_0, p_trg, temperat);
            int n = (int)(tend * 23) + 2;
            var fastest = new List<(MT_pos pos, Grammy gr)>(n);
            double t = vec0[1];

            bool isHit = HitFromThatPos(pos0, vec0, p_trg, tend, fastest);

            var bunch_dict = new Dictionary<string, List<(MT_pos pos, Grammy gr)>>();
            bunch_dict.Add("наибыстрейшая", fastest);
            if (!isHit) {
                return bunch_dict;
            }
            var extrime_up = GetExtrimeTraect(fastest[0].pos, fastest[0].gr.vBegin, p_trg, new Vector3D(0, 1, 0),tend, tend);
            if(extrime_up.Count != 0) {
                bunch_dict.Add("экстремальная верхняя", extrime_up);
            }
            var extrime_down = GetExtrimeTraect(fastest[0].pos, fastest[0].gr.vBegin, p_trg, new Vector3D(0, -1, 0), tend, tend);
            if (extrime_down.Count != 0) {
                bunch_dict.Add("экстремальная нижняя", extrime_down);
            }
            var extrime_left = GetExtrimeTraect(fastest[0].pos, fastest[0].gr.vBegin, p_trg, new Vector3D(0, 0, -1), tend, tend);
            if (extrime_left.Count != 0) {
                bunch_dict.Add("экстремальная левая", extrime_left);
            }
            var extrime_right = GetExtrimeTraect(fastest[0].pos, fastest[0].gr.vBegin, p_trg, new Vector3D(0, 0, 1), tend, tend);
            if (extrime_right.Count != 0) {
                bunch_dict.Add("экстремальная правая", extrime_right);
            }
            return bunch_dict;
        }

        public void CreateCPPFuncFile(string filepath) {
            using(var sw = new StreamWriter(filepath)) {
                var separator = ",";
                sw.WriteLine(@"#include ""Funcs.h""");
                sw.WriteLine(@"TFloat * get_dems_dx()");
                sw.WriteLine(@"{");
                sw.WriteLine(@"     TFloat * res = new TFloat[6]{");
                sw.Write    (@"         ");
                var sb = new StringBuilder();
                foreach (var d in dems_dx) {                 
                    sb.Append(d.ToString(CultureInfo.GetCultureInfo("en-GB")));
                    sb.Append(separator);                  
                }
                sb.Length--;
                sw.WriteLine(sb.ToString());
                sw.WriteLine(@"     };");
                sw.WriteLine(@"     return res;");
                sw.WriteLine(@"}");
                sw.WriteLine(@" ");
                sw.WriteLine(@"TFloat ** get_dems()");
                sw.WriteLine(@"{");
                sw.WriteLine(@"     TFloat** res = new TFloat*[6]{");
                sb.Clear();
                foreach (var dd in dems) {                  
                    sb.Append($"         new TFloat[{dd.Length}]{{");
                    foreach (var d in dd) {
                        sb.Append(d.ToString(CultureInfo.GetCultureInfo("en-GB")));
                        sb.Append(separator);
                    }
                    sb.Length--;
                    sb.Append("},\n");                    
                }
                sb.Length -= 2;
                sw.WriteLine(sb.ToString());
                sw.WriteLine(@"     };");
                sw.WriteLine(@"     return res;");
                sw.WriteLine(@"}");
                sw.WriteLine(@" ");
                
            }
        }

        public void SaveDataToCSV(string filepath) {
            using (var sw = new StreamWriter(filepath)) {
                var separator = " ";                
                for (int i0 = 0; i0 < data.GetLength(0); i0++) { //
                    for (int i1 = 0; i1 < data.GetLength(1); i1++) {
                        for (int i2 = 0; i2 < data.GetLength(2); i2++) {
                            for (int i3 = 0; i3 < data.GetLength(3); i3++) {
                                for (int i4 = 0; i4 < data.GetLength(4); i4++) {
                                    for (int i5 = 0; i5 < data.GetLength(5); i5++) {
                                        var vec = data[i0, i1, i2, i3, i4, i5].ToOneVector();
                                        for (int j = 0; j < vec.Length; j++) {
                                            sw.Write(((float)vec[j]).ToString("E4", CultureInfo.GetCultureInfo("en-GB")));
                                            if (j < vec.Length - 1)
                                                sw.Write(separator);
                                        }
                                        sw.WriteLine();
                                    }
                                }
                            }
                        }
                    }
                }
                sw.Close();
                
            }
        }
    }
}
/*
sw.WriteLine(@"TFloat ******* get_data()");
sw.WriteLine(@"{");
sw.WriteLine($"     TFloat******* res = new TFloat******[{data.GetLength(0)}]{{"); //
sb.Clear();
for (int i0 = 0; i0 < data.GetLength(0); i0++) { //
    sw.WriteLine($"     new TFloat*****[{data.GetLength(1)}] {{");//

    for (int i1 = 0; i1 < data.GetLength(1); i1++) {
        sw.Write("     ");
        sw.WriteLine($"     new TFloat****[{data.GetLength(2)}] {{");

        for (int i2 = 0; i2 < data.GetLength(2); i2++) {
            sw.Write("          ");
            sw.WriteLine($"     new TFloat***[{data.GetLength(3)}] {{");

            for (int i3 = 0; i3 < data.GetLength(3); i3++) {
                sw.Write("               ");
                sw.WriteLine($"     new TFloat**[{data.GetLength(4)}] {{");

                for (int i4 = 0; i4 < data.GetLength(4); i4++) {
                    sw.Write("                    ");
                    sw.WriteLine($"     new TFloat*[{data.GetLength(5)}] {{");

                    for (int i5 = 0; i5 < data.GetLength(5); i5++) {
                        sw.Write("                         ");
                        var vec = data[i0, i1, i2, i3, i4, i5].ToOneVector();
                        sw.Write($"     new TFloat[{vec.Length}] {{");
                        for (int j = 0; j < vec.Length; j++) {
                            sw.Write(((float)vec[j]).ToString("E4", CultureInfo.GetCultureInfo("en-GB")));
                            if (j < vec.Length - 1)
                                sw.Write(separator);
                        }
                        sw.Write("}");
                        if (i5 < data.GetLength(5) - 1)
                            sw.WriteLine(separator);
                        else
                            sw.WriteLine();
                    }
                    sw.Write("}");
                    if (i4 < data.GetLength(4) - 1)
                        sw.WriteLine(separator);
                    else
                        sw.WriteLine();
                }
                sw.Write("}");
                if (i3 < data.GetLength(3) - 1)
                    sw.WriteLine(separator);
                else
                    sw.WriteLine();
            }
            sw.Write("}");
            if (i2 < data.GetLength(2) - 1)
                sw.WriteLine(separator);
            else
                sw.WriteLine();
        }
        sw.Write("}");
        if (i1 < data.GetLength(1) - 1)
            sw.WriteLine(separator);
        else
            sw.WriteLine();
    }
    sw.Write("}");
    if (i0 < data.GetLength(0) - 1)
        sw.WriteLine(separator);
    else
        sw.WriteLine();
}
sw.WriteLine(@"     };");
sw.WriteLine(@"     return res;");
sw.WriteLine(@"}");

sw.Close();
*/
