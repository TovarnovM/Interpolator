using Microsoft.Research.Oslo;
using MyRandomGenerator;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MeetingPro {
    public class GramofonLarva {
        public NDemVec nDemVec0;
        public MT_pos mT_Pos0;
        public double delta_t = 1d / 23d;
        public double dt;
        public Mis mis;
        public int n_helm = 9;
        public double delta0 = -15, delta1 = 15;
        public int n_eler = 7;
        public double delta_el0 = -20, delta_el1 = 20;
        public int n_rnd = 100, n_rnd_tst = 100;
        public GramofonLarva(NDemVec nDemVec0, MT_pos mT_Pos0) {
            mis = new Mis();
            dt = 0.0001;// delta_t / 100;
            this.nDemVec0 = new NDemVec(nDemVec0);
            this.mT_Pos0 = new MT_pos(mT_Pos0);
        }
        public void SetMis0(double del1, double del2, double del_el) {
            mis.SetMTpos(mT_Pos0);
            mis.SetNDemVec(nDemVec0);
            mis.delta_i_rad[0] = del1 * Mis.RAD;
            mis.delta_i_rad[1] = del2 * Mis.RAD; 
            mis.delta_i_rad[2] = -mis.delta_i_rad[0];
            mis.delta_i_rad[3] = -mis.delta_i_rad[1];
            mis.delta_eler = del_el * Mis.RAD;
        }

        public (MT_pos pos, NDemVec vec) CalcOneVar(double del1, double del2, double del_el) {
            SetMis0(del1, del2, del_el);
            var v0 = mis.Rebuild(mis.TimeSynch);
            var sol = Ode.RK45(mis.TimeSynch, v0, mis.f, dt).SolveTo(mis.TimeSynch + delta_t);
            foreach (var sp in sol) {

            }
            return (mis.GetMTPos(), mis.GetNDemVec());
        }

        public List<(double del1, double del2, double del_el, double flaggy)> GetPlan() {
            var res = new List<(double del1, double del2, double del_el, double flaggy)>(5+n_helm * n_helm * n_eler + n_rnd + n_rnd_tst);
            double ddelt = (delta1 - delta0) / (n_helm - 1);
            double ddelt_el = (delta_el1 - delta_el0) / (n_eler - 1);
                        
            for (int i = 0; i < n_helm; i++) {
                double del1 = delta0 + ddelt*i;
                for (int j = 0; j < n_helm; j++) {
                    double del2 = delta0 + ddelt * j;
                    for (int k = 0; k < n_eler; k++) {
                        double del_el = delta_el0 + ddelt_el*k;
                        res.Add((del1, del2, del_el, 0));
                    }
                }
            }

            MyRandom rnd = new MyRandom();
            for (int i = 0; i < n_rnd; i++) {
                double del1 = rnd.GetDouble(delta0, delta1);
                double del2 = rnd.GetDouble(delta0, delta1);
                double del_el = rnd.GetDouble(delta_el0, delta_el1);
                res.Add((del1, del2, del_el, 0));
            }
            for (int i = 0; i < n_rnd_tst; i++) {
                double del1 = rnd.GetDouble(delta0, delta1);
                double del2 = rnd.GetDouble(delta0, delta1);
                double del_el = rnd.GetDouble(delta_el0, delta_el1);
                res.Add((del1, del2, del_el, 1));
            }
            return res;
        }

        public List<OneWay> GetSols() {
            var plan = GetPlan();
            var res = new List<OneWay>(plan.Count); 
            foreach (var (del1, del2, del_el,flaggy) in plan) {
                var (pos1, vec1) = CalcOneVar(del1, del2, del_el);
                var ow = new OneWay {
                    Vec0 = new NDemVec(nDemVec0),
                    Pos0 = new MT_pos(mT_Pos0),

                    Del1 = del1,
                    Del2 = del2,
                    Del_el = del_el,
                    Flaggy = flaggy,

                    Vec1 = vec1,
                    Pos1 = pos1
                };
                res.Add(ow);
            }
            return res;
        }

    }
}
