
using Microsoft.Research.Oslo;
using SimpleIntegrator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneDemSPH {
    public class Particle : ScnObjDummy {
        public int MInd { get; set; }

        public double M { get; set; }
        public IScnPrm pM { get; set; }

        public double P { get; set; }
        public IScnPrm pP { get; set; }

        public double Ro { get; set; }
        public IScnPrm pRo { get; set; }

        public double E { get; set; }
        public IScnPrm pE { get; set; }

        public double V { get; set; }
        public IScnPrm pV { get; set; }

        public double X { get; set; }
        public IScnPrm pX { get; set; }

        public double dE { get; set; }
        public IScnPrm pdE { get; set; }

        public double dV { get; set; }
        public IScnPrm pdV { get; set; }

        public double dRo { get; set; }
        public IScnPrm pdRo { get; set; }

        public double GetRtoPart(Particle toMe) {
            return Math.Abs(X - toMe.X);
        }

        OneDemExample owner2;
        public void SetDts() {
            if(!(Owner is OneDemExample))
                throw new Exception("Sad");

            owner2 = Owner as OneDemExample;

            AddDiffPropToParam(pX,pV);
            AddDiffPropToParam(pV,pdV);
            AddDiffPropToParam(pE,pdE);
            //AddDiffPropToParam(pRo,pdRo);
            //double dro= 0, de = 0, dv = 0;

            ////SetDiff(pRo,t => {

            ////    return dro;
            ////});



            //SynchMeAfter += t => {







            //    var matdW = owner2.matr_dW;
            //    var matV = owner2.matr_ViminVj;
            //    var matSignX = owner2.matr_SignXiminXj;
            //    var matII = owner2.matr_II;
            //    int i = MInd;
            //    dro = 0d;
            //    de = 0d;
            //    dv = 0d;
            //    bool neib_bool = false;
            //    for(int j = 0; j < matdW.ColumnDimension; j++) {
            //        if(matdW[i,j] == 0d) {
            //            //if(neib_bool)
            //            //    break;
            //            continue;
            //        }

            //        if(i == j)
            //            continue;

            //        //neib_bool = true;
            //        var neib = owner2.AllParticles[j];
            //        dro += neib.M * matV[i,j] * matSignX[i,j] * matdW[i,j];
            //        de += 0.5*neib.M * matV[i,j] * matSignX[i,j] * matdW[i,j]
            //                            * (neib.P / (neib.Ro * neib.Ro) + P / (Ro * Ro) + matII[i,j]);
            //        dv -= neib.M * matSignX[i,j] * matdW[i,j]
            //        * (neib.P / (neib.Ro * neib.Ro) + P / (Ro * Ro) +  matII[i,j]);
            //    }
            //};


        }
        public bool IsWall = false;

        public List<Particle> neib_lst = new List<Particle>(33);

        public void Fillneib() {
            neib_lst.Clear();
            var h = owner2.h;
            const int faraway = 50;
            int toInd = Math.Min(owner2.AllParticles.Count,MInd + faraway);
            for(int j = MInd + 1; j < toInd; j++) {
                if(GetRtoPart(owner2.AllParticles[j]) > 2 * h)
                    break;
                neib_lst.Add(owner2.AllParticles[j]);
            }
            for(int j = MInd - 1; j > 0; j--) {
                if(GetRtoPart(owner2.AllParticles[j]) > 2 * h)
                    break;
                neib_lst.Add(owner2.AllParticles[j]);
            }
        }

        public void FillRos() {
            var h = owner2.h;
            Ro = neib_lst.Sum(n => n.M * KernelF.W(X - n.X,h)) + M*KernelF.W(0,h);
        }

        public void FillDs() {
            
            dE = 0;
            dV = 0;
            //dRo = 0;
            var h = owner2.h;
            foreach(var neib in neib_lst) {

                var Vij = V - neib.V;
                var ximinxj = X - neib.X;
                var dw = KernelF.dWdr(ximinxj,h);


                
                var signXij = Math.Sign(ximinxj);

                double II = 0d;
                if(Vij * signXij < 0) {
                    var mu_ij = (h * Vij * ximinxj) / (ximinxj * ximinxj + h * h * owner2.e2);
                    II = 2  *((-mu_ij * owner2.alpha * 0.5 * (Math.Sqrt(owner2.gamma * P / Ro) + Math.Sqrt(owner2.gamma * neib.P / neib.Ro)) + owner2.betta * mu_ij * mu_ij) / (Ro + neib.Ro));
                    
                }



                //dRo += neib.M * Vij * signXij * dw;

                dE += 0.5 * neib.M * Vij * signXij * dw
                                    * (neib.P / (neib.Ro * neib.Ro) + P / (Ro * Ro) + II);
                dV -= neib.M * signXij * dw
                * (neib.P / (neib.Ro * neib.Ro) + P / (Ro * Ro) + II);


            }
        }
    }

    public static class KernelF {
        public static double dWdr(double r_shtr,double h) {
            double q = Math.Abs(r_shtr) / h;
            if(q > 2.0)
                return 0.0;

            double a = -2.0 / (3.0 * h * h);
            double result = 0;

            if(q < 0.66666)
                result = 1;
            else if(q >= 0.66666 && q < 1.0)
                result = 3.0 * q * (4.0 - 3.0 * q) / 4.0;
            else if(q >= 1.0 && q <= 2.0)
                result = 3.0 * (2.0 - q) * (2.0 - q) / 4.0;

            return result * a;
        }
        public static double W(double r_shtr,double h) {
            double q = Math.Abs(r_shtr) / h;
            if(q > 2.0)
                return 0.0;
            double a = 2.0 / (3.0 * h);
            double result = 0;

            if(q >= 0 && q <= 1.0)
                result = 0.25 * (4d - 6 * q * q + 3 * q * q * q);
            else if(q > 1.0 && q <= 2.0)
                result = 0.25 * (2.0 - q) * (2.0 - q) * (2.0 - q);

            return result * a;
        }
    }

    public static class KernelF1 {
        public static double dWdr(double r_shtr,double h) {
            double r = Math.Abs(r_shtr) / h;
            if(r > 2.0)
                return 0.0;
            double a = 1d / h;
            double r2 = r * r;
            double r3 = r2 * r;
            //double r4 = r3 * r;
            double result = - 2*v2 * r + 3*v3 * r2 - 4*v4 * r3;

            return result * a;
        }
        const double 
            v1 = 2d / 3d, 
            v2 = 9d / 8d, 
            v3 = 19d / 24d, 
            v4 = 5d / 32d;
        public static double W(double r_shtr,double h) {
            double r = Math.Abs(r_shtr) / h;
            if(r > 2.0)
                return 0.0;
            double a = 1d / h;
            double r2 = r * r;
            double r3 = r2 * r;
            double r4 = r3 * r;
            double result = v1 - v2 * r2 + v3 * r3 - v4 * r4;

            return result * a;
        }
    }

    public class OneDemExample : ScnObjDummy {
        public Matrix matr_dW, matr_II, matr_ViminVj, matr_SignXiminXj;

        public List<Particle> Particles { get; set; } = new List<Particle>();
        public List<Particle> Wall { get; set; } = new List<Particle>();
        public List<Particle> AllParticles { get; set; } = new List<Particle>();

        const int scaler = 10;
        const double boardL = 0.6;
        const int perc = 80;

        public int Np { get; set; } = 100*scaler;
        public int Np_wall { get; set; } = 20 * scaler;
        public double dt { get; set; } = 0.001;
        public double h { get; set; } = boardL / ((100-perc)*scaler) * 1;
        public double gamma { get; set; } = 1.4;
        public double alpha { get; set; } = 1d;
        public double betta { get; set; } = 2d;
        public double e2 { get; set; } = 0.01;
        public double dx_granica { get; set; }

        public OneDemExample(double dx_gr = 0d) : base() {
            dx_granica = dx_gr;
            int n_pm = perc * scaler;
            

            var dx1 = boardL/n_pm;
            for(int i = 0; i < n_pm; i++) {
                Particles.Add(new Particle() {
                    Name = i.ToString(),
                    X = -boardL + i * dx1,
                    Ro = 1,
                    P = 1,
                    E = 2.5,
                    V = 0
                });
            }
            var dx2 = boardL/(Np - n_pm);
            for(int i = n_pm; i < Np; i++) {
                
                Particles.Add(new Particle() {
                    Name = i.ToString(),
                    X = (i-n_pm) * dx2+ dx_granica,
                    Ro = 0.25,
                    P = 0.1795,
                    E = 1.795,
                    V = 0
                });
            }

            for(int i = -1; i > -Np_wall - 1; i--) {
                Wall.Add(new Particle() {
                    Name = (i).ToString(),
                    X = -boardL + (i) * dx1,
                    Ro = 1,
                    P = 1,
                    E = 2.5,
                    V = 0,
                    IsWall = true
                });
            }
            for(int i = Np + 1; i <= Np + Np_wall; i++) {
                Wall.Add(new Particle() {
                    Name = (i).ToString(),
                    X =  (i-n_pm-1) * dx2 + dx_granica,
                    Ro = 0.25,
                    P = 0.1795,
                    E = 1.795,
                    V = 0,
                    IsWall = true
                });
            }

            var all = Particles.Concat(Wall).OrderBy(p => p.X);
            AllParticles.AddRange(all);

            AllParticles.ForEach(p => p.M = 0.6/n_pm);

            for(int i = 0; i < AllParticles.Count; i++) {
                AllParticles[i].MInd = i;
            }

            var xs = AllParticles.Select(p => p.X).ToArray();
            var walls = AllParticles.Select(p => p.IsWall).ToArray();

            SynchMeBefore += SynchMeAfterAct;

            foreach(var p in Particles) {
                AddChild(p);
            }
            Particles.ForEach(p => p.SetDts());



            var mhi = Particles.Where(p => p.P > 0.3).Sum(p => p.M);
            var mlo = Particles.Where(p => p.P < 0.3).Sum(p => p.M);
            var mall = Particles.Sum(p => p.M);

            foreach(var part in AllParticles) {
                part.Ro = AllParticles.Sum(p => p.M * KernelF.W(part.X - p.X,h));
            }
            foreach(var p in Particles) {
                p.P = GetP(p);
            }

            SynchMeAfter += SynchMeAfterAct;
            //SynchMeForNext += t => {
            //    foreach(var p in Particles) {
            //        p.P = GetP(p);
            //    }
            //};

            particles_par = Particles.AsParallel();
        }

        ParallelQuery<Particle> particles_par;


        public double GetP(Particle p) {
            return (gamma - 1) * p.Ro * p.E;
        }
        public bool FarParticles(Particle p1, Particle p2) {
            var r = p1.GetRtoPart(p2);
            if(Double.IsNaN(r))
                return true;
            return r > 2 * h;
        }

        
        public void SynchMeAfterAct(double t) {
            Parallel.ForEach(Particles,p => p.Fillneib());
            Parallel.ForEach(Particles,p => p.FillRos());
            Parallel.ForEach(Particles,p => p.P = GetP(p));
            Parallel.ForEach(Particles,p => p.FillDs());
            //particles_par.ForAll(p => p.FillDs());
            
            //Particles.ForEach(p => p.FillDs());


            //int N_All = AllParticles.Count;
            //matr_dW = new Matrix(N_All,N_All);
            //matr_II = new Matrix(N_All,N_All);
            //matr_ViminVj = new Matrix(N_All,N_All);
            //matr_SignXiminXj = new Matrix(N_All,N_All);

            //foreach(var p in Particles) {
            //    p.P = GetP(p);
            //}

            //foreach(var part in AllParticles) {
            //    part.Ro = AllParticles.Sum(p => p.M * KernelF.W(part.X - p.X,h));
            //}

            //for(int i = 0; i < N_All; i++) {
            //    var pi = AllParticles[i];
            //    bool neib = false;

            //    for(int j = i+1; j < N_All; j++) {
            //        var pj = AllParticles[j];
            //        if((pi.IsWall && pj.IsWall) || FarParticles(pi,pj)) {
            //            if(neib)
            //                break;
            //            continue;
            //        }
            //        neib = true;    


            //        matr_dW[i,j] = KernelF.dWdr(pi.X - pj.X,h);
            //        matr_dW[j,i] = matr_dW[i,j];

            //        matr_ViminVj[i,j] = pi.V - pj.V;
            //        matr_ViminVj[j,i] = -matr_ViminVj[i,j];

            //        var ximinxj = pi.X - pj.X;

            //        matr_SignXiminXj[i,j] = Math.Sign(ximinxj);
            //        matr_SignXiminXj[j,i] = -matr_SignXiminXj[i,j];

            //        if(matr_ViminVj[i,j]* matr_SignXiminXj[i,j] < 0) {
            //            var mu_ij = (h * matr_ViminVj[i,j] * ximinxj) / (ximinxj * ximinxj + h * h * e2);
            //            matr_II[i,j] = 2 * ((-mu_ij*alpha*0.5*(Math.Sqrt(gamma*pi.P/pi.Ro)+Math.Sqrt(gamma*pj.P/pj.Ro))+betta*mu_ij*mu_ij) / (pi.Ro+pj.Ro));
            //            matr_II[j,i] = matr_II[i,j];
            //        }

            //    }
            //}






        }
    }
}
