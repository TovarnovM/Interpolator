using Sharp3D.Math.Core;
using SimpleIntegrator;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static System.Math;

namespace SPH_2D {
    
    /// <summary>
    /// отрезок
    /// </summary>
    public class Segment {
        public Vector2D p1, p2;
        public double A, B, C;
        public Segment(double x1, double y1, double x2, double y2) {
            p1 = new Vector2D(x1,y1);
            p2 = new Vector2D(x2,y2);
            CalcABC();
        }
        public Segment(Vector2D p1, Vector2D p2) {
            this.p1 = p1;
            this.p2 = p2;
            CalcABC();
        }
        public void CalcABC() {
            A = p1.Y - p2.Y;
            B = p2.X - p1.X;
            C = p1.X * p2.Y - p2.X * p1.Y;
        }

        /// <summary>
        /// Возвращает вектор, перпендикулярный прямой, начало которого в точке fromMe.Vec2D, а конец на прямой. 
        /// </summary>
        /// <param name="fromMe"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2D GetNormalToMe(Vector2D fromMe) {
            double A_ = -B , B_ = A, C_ = B*fromMe.X-A*fromMe.Y;
            double znam = A * B_ - A_*B;
            if(Math.Abs(znam) > 1E-12) {
                double X = -(C * B_ - C_ * B) / znam - fromMe.X;
                double Y = -(A * C_ - A_ * C) / znam - fromMe.Y;
                return new Vector2D(X,Y);
            } 
            return new Vector2D(fromMe.X,fromMe.Y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2D GetNormalToMe(IParticle2D fromMe) {
            return GetNormalToMe(fromMe.Vec2D);
        }

        /// <summary>
        /// Показывает, находится ли точка в окрестности отрезка
        /// </summary>
        /// <param name="particle"></param>
        /// <param name="h"></param>
        /// <returns></returns>
        public bool CloseToMe(IParticle2D particle, double h) {
            var H = GetNormalToMe(particle);
            if(H.GetLength() > h)
                return false;

            var normalGlobal =  H + particle.Vec2D;
            var vHloc = normalGlobal - p1;
            var p2loc = p2 - p1;
            var vdelta= p2loc.Norm * h;
            vHloc += vdelta;
            p2loc += 2 * vdelta;
            return p2loc * vHloc > 0 && p2loc.GetLengthSquared() > vHloc.GetLengthSquared();

        }

       
    }



    public interface IGasParticleVer3 : IParticle2D {
        Vector2D VelVec2D { get; set; }
        bool isboundary { get; }
        double M { get; set; }
        double Ro { get; set; }
        double E { get; set; }
        double P { get; set; }
        double C { get; }
    }

    public class GasParticleVer3: Particle2DBase, IGasParticleVer3 {
        public bool isboundary { get; } = false;
        public List<Segment> Boundaries { get; private set; }
        public double h;
        public double M { get; set; }

        public double Ro { get; set; }
        public IScnPrm pRo { get; set; }

        public double dRo { get; set; }
        public IScnPrm pdRo { get; set; }

        public double E { get; set; }
        public IScnPrm pE { get; set; }

        public double dE { get; set; }
        public IScnPrm pdE { get; set; }

        public double P { get; set; }
        public IScnPrm pP { get; set; }

        public double k = 1.4;

        public Position2D Vel { get; private set; }
        public Position2D dVdt { get; set; }

        public Vector2D VelVec2D {
            get {
                return Vel.Vec2D;
            }
            set {
                Vel.Vec2D = value;
            }
        }

        public Vector2D dVdtVec2D {
            get {
                return dVdt.Vec2D;
            }
            set {
                dVdt.Vec2D = value;
            }
        }

        public ConcurrentStack<IParticle2D> MiracleStack = new ConcurrentStack<IParticle2D>();
        public List<IGasParticleVer3> BestNeibs;
        
        public GasParticleVer3(double h,double hmax, List<Segment> boundaries = null) : base(hmax) {
            Boundaries = boundaries ?? new List<Segment>();
            this.h = h;

            Vel = new Position2D();
            Vel.Name = "Vel";
            AddChild(Vel);

            AddDiffVect(Vel);

            dVdt = new Position2D();
            dVdt.Name = "dV";
            AddChild(dVdt);
            Vel.AddDiffVect(dVdt,false);


            // AddDiffPropToParam(pRo,pdRo);
            AddDiffPropToParam(pE,pdE);
        }
        
        public override int StuffCount {
            get {
                return 4;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void DoStuff(int stuffIndex) {
            switch(stuffIndex) {
                case 0: {
                    SetP();
                    break;
                }
                case 1: {
                    MiracleMe();
                    break;
                }
                case 2: {
                    FillBestNeibsPlusRO();
                    break;
                }
                case 3: {                   
                    FillDts();
                    break;
                }

                default:
                break;
            }
        }
        public virtual void SetP() {

            //Ro = Neibs.Cast<IGasParticleVer3>().Sum(n => {
            //    double w = W_func(GetDistTo(n),h);
            //    return n.M * w;

            //});
            P = (k - 1d) * Ro * E;
            dRo = 0d;
            dE = 0d;
            dVdt.Vec2D = Vector2D.Zero;
            MiracleStack.Clear();
        }
        private void MiracleMe() {
            foreach(var bound in Boundaries) {
                if(bound.CloseToMe(this,hmax)) {
                    var mirrorClone = CreateMiracleDummy(bound);
                    foreach(var neib in Neibs) {
                        if(neib is GasParticleVer3) {
                            (neib as GasParticleVer3).MiracleStack.Push(mirrorClone);
                        }
                    }
                }
            }
        }

        private void FillBestNeibsPlusRO() {
            BestNeibs = Neibs.Where(n => !ReferenceEquals(n,this)).Concat(MiracleStack).Where(n => GetDistTo(n) < hmax).Cast<IGasParticleVer3>().ToList();
            
            Ro = BestNeibs.Sum(n => {
                double w = W_func(GetDistTo(n),h);
                return n.M * w;
            });
        }

        public void FillDts() {

            foreach(var neib in BestNeibs)
            {
                Vector2D deltaV = Vel.Vec2D - neib.VelVec2D;
                Vector2D deltaR = Vec2D - neib.Vec2D;
                if(!neib.isboundary) {

                    double mj = neib.M;
                    double r = deltaR.GetLength();
                    //
                    double H = 0;
                    double scalar = deltaV * deltaR;
                    if(scalar < 0) {
                        double alpha_ = 0.9;
                        double beta_ = 0.9;
                        double phi_ = 0.1;
                        double phi = hmax * scalar / (Math.Pow(r,2) + Math.Pow(phi_ * hmax,2));
                        double Ro_ = (Ro + neib.Ro) / 2;
                        double C_ = (C + neib.C) / 2;

                        H = (-alpha_ * C_ * phi + beta_ * Math.Pow(phi,2)) / Ro_;

                    }
                    //
                    double brackets = mj * (P / (Ro * Ro) + neib.P / (neib.Ro * neib.Ro) + 0);
                    if(H > 1) {
                        double res = brackets / H;
                        int a = 0;
                    }
                    Vector2D dW = new Vector2D((deltaR.X / r) * dW_func(r,hmax),(deltaR.Y / r) * dW_func(r,hmax));
                    //плотность
                    //Ro += mj * W_func(r, hmax);
                    //скорости
                    dVdt.X += -brackets * dW.X;
                    dVdt.Y += -brackets * dW.Y;
                    //энергия
                    dE += 0.5 * brackets * deltaV * dW;
                } else {
                    double r0 = hmax/5;
                    double rij = deltaR.GetLength();
                    if(r0 / rij <= 1) {
                        double n1 = 12;
                        double n2 = 4;
                        double D = (Vel.Vec2D.GetLength() > neib.VelVec2D.GetLength()) ?
                            Vel.Vec2D.GetLength() * Vel.Vec2D.GetLength() :
                            neib.VelVec2D.GetLength() * neib.VelVec2D.GetLength();
                        D = D * 2;
                        double brackets = -D * (Math.Pow(r0 / rij,n1) - Math.Pow(r0 / rij,n2)) / (rij * rij);
                        double PDijx = brackets * deltaR.X;
                        double PDijy = brackets * deltaR.Y;
                        //
                        dVdt.X += PDijx / M;
                        dVdt.Y += PDijy / M;
                    }
                }
            }
        }



        /// <summary>
        /// Получить скорость звука
        /// </summary>
        /// <returns></returns>
        public double C {
            get {
                return (k * (k - 1) * E);
            }
        }
        

        public GasParticleVer3Dummy CreateMiracleDummy(Segment mirrorSegment) {
            var res = new GasParticleVer3Dummy(hmax);
            res.Name = "MirrorOf" + Name;
            res.Owner = this;
            res.M = M;
            res.Ro = Ro;
            res.E = E;
            res.P = P;
            res.C = C;
            res.Vec2D = Vec2D + 2 * mirrorSegment.GetNormalToMe(this);
            res.VelVec2D = VelVec2D + 2 * mirrorSegment.GetNormalToMe(VelVec2D + mirrorSegment.p1);

            return res;
        }
    }

    public class GasParticleVer3Dummy: Particle2DDummyBase, IGasParticleVer3 {
        public GasParticleVer3Dummy(double hmax) : base(hmax) {
        }

        public bool isboundary { get; set; } = false;
        public double M { get; set; }

        public double Ro { get; set; }

        public double E { get; set; }

        public double P { get; set; }

        public Vector2D VelVec2D { get; set; }

        public double C { get; set; }
    }


    public static class SPH2D_Ver3  {
        public static double h_default = 1d / 300;

        public static Sph2D TestTruba(params double[] initcond) {
            double lt = initcond[0]; //длина трубы
            double ht = initcond[1]; //высота трубы
            double x0t = initcond[2];//отностиельная граница по Икс
            double p1t = initcond[3]; //Давление слева
            double ro1t = initcond[4]; //Плотность слева
            double p2t = initcond[5]; //Давление справа
            double ro2t = initcond[6]; //Плотность справа

            var bounds = new List<Segment>(4);
            bounds.Add(new Segment(0,0,0,ht));
            bounds.Add(new Segment(0,ht,lt,ht));
            bounds.Add(new Segment(lt,ht,lt,0));
            bounds.Add(new Segment(lt,0,0,0));

            int Nx = (int)Math.Round(lt / h_default);
            double dx = lt / Nx;

            int Ny = (int)Math.Round(ht / h_default);
            double dy = ht / Ny;

            double delta = Math.Min(dx,dy);
            double m1 = GetMass(delta,h_default,ro1t);
            double m2 = GetMass(delta,h_default,ro2t);

            var particles = new List<GasParticleVer3>(Nx * Ny+10);

            double x = 0.5 * delta;
            do {
                double y = 0.5 * delta;
                do {
                    var p = new GasParticleVer3(h_default,2 * h_default,bounds);
                    p.X = x;
                    p.Y = y;
                    p.P = x < lt * x0t ? p1t : p2t;
                    p.Ro = x < lt * x0t ? ro1t : ro2t;
                    p.M = x < lt * x0t ? m1 : m2;
                    particles.Add(p);

                    y += delta;
                } while(y < ht);
                x += delta;

            } while(x<lt);
            
            particles.ForEach(p => {
                p.E = p.P /((p.k - 1d) * p.Ro) ;
            });
            return new Sph2D(particles,null);

        }

        struct W_helper {
            public double x, y, w;
            public W_helper(double x, double y, double w) {
                this.x = x;
                this.y = y;
                this.w = w;
            }
        }
        public static double GetMass(double delta, double h, double ro) {
            double x = 0;
            double y = 0;
            double f;
            var lst = new List<W_helper>();
            
            do {
                do {
                    f = Particle2DBase.W_func(Sqrt(x*x+y*y),h);
                    lst.Add(new W_helper(x,y,f));
                    x += delta;
                } while(f > 0d);
                y += delta;
                x = 0;
                f = Particle2DBase.W_func(Sqrt(x * x + y * y),h);
            } while(f > 0d);

            double sum = 0;
            sum += lst.Where(wh => wh.x == 0 ^ wh.y == 0).Sum(wh => wh.w * 2);
            sum += lst.Where(wh => wh.x == 0 && wh.y == 0).Sum(wh => wh.w);
            sum += lst.Where(wh => wh.x != 0 && wh.y != 0).Sum(wh => wh.w * 4);

            return ro / sum;
        }

    }
}
