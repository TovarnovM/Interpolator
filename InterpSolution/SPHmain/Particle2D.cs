using SimpleIntegrator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static System.Math;

namespace SPHmain {

    public class Particle2D: Position2D {
        public Position2D Vel;
        public double hmax, xmin, ymin;
        public Particle2D(double hmax,double xmin,double xmax,double ymin,double ymax) {
            this.hmax = hmax;
            this.xmin = xmin;
            this.ymin = ymin;


            Vel = new Position2D();
            Vel.Name = "Vel";
            AddChild(Vel);

            AddDiffVect(Vel);
            Neibs = new List<Particle2D>(30);

            
        }

        public double dX {
            get {
                return Vel.X;
            }

            set {
                Vel.X = value;
            }
        }

        public double dY {
            get {
                return Vel.Y;
            }

            set {
                Vel.Y = value;
            }
        }

        public List<Particle2D> Neibs;

        public double GetDistTo(Particle2D particle) {
            double deltX = X - particle.X;
            double deltY = Y - particle.Y;
            return Sqrt(deltX * deltX + deltY * deltY);
        }

        public int XIndex {
            get {
                return (int)Floor((X - xmin) / hmax);
            }
        }
        public int YIndex {
            get {
                return (int)Floor((Y - ymin) / hmax);
            }
        }
    }
}
