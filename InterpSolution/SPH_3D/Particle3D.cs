using SimpleIntegrator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static System.Math;

namespace SPH_3D {
    public interface IParticle3D : IScnObj {
        /// <summary>
        /// Х координата частицы
        /// </summary>
        double X { get; }

        /// <summary>
        /// Y координата частицы
        /// </summary>
        double Y { get; }

        /// <summary>
        /// Z координата частицы
        /// </summary>
        double Z { get; }

        /// <summary>
        /// Скорость по X
        /// </summary>
        double Vx { get; }

        /// <summary>
        /// Скорость по Y
        /// </summary>
        double Vy { get; }

        /// <summary>
        /// Скорость по Z
        /// </summary>
        double Vz { get; }

        /// <summary>
        /// Скорость
        /// </summary>
        IPosition3D Vel { get; }

        /// <summary>
        /// Соседи частицы
        /// </summary>
        List<IParticle3D> Neibs { get; }

        /// <summary>
        /// получить максимальный радиус сглаживания
        /// </summary>
        /// <returns></returns>
        double GetHmax();

        /// <summary>
        /// Расстояние до частицы
        /// </summary>
        /// <param name="particle">до этой частицы</param>
        /// <returns></returns>
        double GetDistTo(IParticle3D particle);

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
    public abstract class Particle3DBase: Position3D, IParticle3D {
        #region IParticle 3D impl
        public IPosition3D Vel { get; private set; }
        public double Vx {
            get {
                return Vel.X;
            }

            set {
                Vel.X = value;
            }
        }

        public double Vy {
            get {
                return Vel.Y;
            }

            set {
                Vel.Y = value;
            }
        }

        public double Vz {
            get {
                return Vel.Z;
            }

            set {
                Vel.Z = value;
            }
        }

        public List<IParticle3D> Neibs { get; private set; }
        public double GetDistTo(IParticle3D particle) {
            double deltX = X - particle.X;
            double deltY = Y - particle.Y;
            double deltZ = Z - particle.Z;
            return Sqrt(deltX * deltX + deltY * deltY + deltZ * deltZ);
        }
        public double GetHmax() {
            return hmax;
        }
        #endregion

        public double hmax;
        public Particle3DBase(double hmax) {
            this.hmax = hmax;

            Vel = new Position3D();
            Vel.Name = "Vel";
            AddChild(Vel);

            AddDiffVect(Vel);
            Neibs = new List<IParticle3D>(30);

            Name = "Particle";
        }

        #region Abstract
        public abstract int StuffCount { get; }

        public abstract void DoStuff(int stuffIndex);

        

        #endregion

    }

}
