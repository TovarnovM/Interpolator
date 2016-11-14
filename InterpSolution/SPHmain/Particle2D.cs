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
        /// Скорость
        /// </summary>
        IPosition2D Vel { get; }

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
        /// Сколько зависимых операций необходимо частице на каждом шаге интегрирования
        /// </summary>
        int StuffCount { get; }

        /// <summary>
        /// Произвести зависимую операцию под индексом stuffIndex
        /// </summary>
        /// <param name="stuffIndex"></param>
        void DoStuff(int stuffIndex);
    }

    /// <summary>
    /// Абстрактный класс представляющий частицу для SPH 2D
    /// </summary>
    public abstract class Particle2D: Position2D, IParticle2D {
        #region IParticle 2D impl
        public IPosition2D Vel { get; private set; }
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
        #endregion
        
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

        #region Abstract
        public abstract int StuffCount { get; }

        public abstract void DoStuff(int stuffIndex);
        #endregion

    }

}
