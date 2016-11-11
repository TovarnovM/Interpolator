using SimpleIntegrator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static System.Math;

namespace SPHmain {
    public interface IParticle2D : IScnObj {
        /// <summary>
        /// Х координата частицы
        /// </summary>
        double X { get; }

        /// <summary>
        /// Y координата частицы
        /// </summary>
        double Y { get; }

        /// <summary>
        /// Скорость по X
        /// </summary>
        double dX { get; }

        /// <summary>
        /// Скорость по Y
        /// </summary>
        double dY { get; }

        /// <summary>
        /// Соседи частицы
        /// </summary>
        List<IParticle2D> Neibs { get; }

        /// <summary>
        /// Расстояние до частицы
        /// </summary>
        /// <param name="particle">до этой частицы</param>
        /// <returns></returns>
        double GetDistTo(IParticle2D particle);

        /// <summary>
        /// Первое действе над частицей при интегрировании
        /// </summary>
        void DoStuff1();

        /// <summary>
        /// Второе действе над частицей при интерировании
        /// </summary>
        void DoStuff2();
    }

    /// <summary>
    /// Абстрактный класс представляющий частицу для SPH 2D
    /// </summary>
    public abstract class Particle2D: Position2D, IParticle2D {
        public Position2D Vel;
        public double hmax;
        public Particle2D(double hmax) {
            this.hmax = hmax;

            Vel = new Position2D();
            Vel.Name = "Vel";
            AddChild(Vel);

            AddDiffVect(Vel);
            Neibs = new List<IParticle2D>(30);

            Name = "Particle";
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

        public List<IParticle2D> Neibs { get; private set; }

        public double GetDistTo(IParticle2D particle) {
            double deltX = X - particle.X;
            double deltY = Y - particle.Y;
            return Sqrt(deltX * deltX + deltY * deltY);
        }


        /// <summary>
        /// Сделать что-то первое (например что-то высчитать)
        /// </summary>
        public abstract void DoStuff1();

        public abstract void DoStuff2();
    }

}
