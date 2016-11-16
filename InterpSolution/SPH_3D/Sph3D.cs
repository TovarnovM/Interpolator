using SimpleIntegrator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using static System.Math;

namespace SPH_3D {
    /// <summary>
    /// Интегратор, представляющий двумерную задачу SPH метода
    /// В качестве частицы выступает интерфейс IParticle2D
    /// </summary>
    public class Sph3D: ScnObjDummy, IScnObj {
        #region local classes

        /// <summary>
        /// Класс представляющий плоскую ячейку с частицами (необходим для быстрого и параллельного определения соседей частиц)
        /// </summary>
        class Cell3D {

            /// <summary>
            /// Частицы, находящиеся в ячейке
            /// </summary>
            public List<IParticle3D> Particles = new List<IParticle3D>();


            /// <summary>
            /// Стек с частицами, добавленными в ячейку асинхронными методом. Необходимо перенести их в Particles методом DumpStack()
            /// </summary>
            public ConcurrentStack<IParticle3D> ParticleStack = new ConcurrentStack<IParticle3D>();

            /// <summary>
            /// Индексы ячейки в сетке ячеек
            /// </summary>
            public int XInd, YInd, ZInd;

            /// <summary>
            /// Границы ячейки.
            /// Левая и нижняя границы включены в пространство ячейки,
            /// Правая и верхняя границы не включены в пространство ячейки
            /// </summary>
            public double X0, X1, Y0, Y1, Z0, Z1;

            /// <summary>
            /// Конструктор
            /// </summary>
            /// <param name="XInd">Индекс колонки ячейки в сетке ячеек</param>
            /// <param name="YInd">Индекс строки ячейки в сетке ячеек</param>
            /// <param name="ZInd">Индекс "слоя" ячейки в сетке ячеек</param>
            /// <param name="X0">Левая граница (включена в пространство ячейки)</param>
            /// <param name="X1">Правая граница (включена в пространство ячейки)</param>
            /// <param name="Y0">Нижняя граница (не включена в пространство ячейки)</param>
            /// <param name="Y1">Верхняя граница(не включена в пространство ячейки)</param>
            /// <param name="Z0">Ближняя граница (не включена в пространство ячейки)</param>
            /// <param name="Z1">Дальняя граница(не включена в пространство ячейки)</param>
            public Cell3D(int XInd,int YInd,int ZInd,double X0,double X1,double Y0,double Y1, double Z0, double Z1) {
                this.XInd = XInd;
                this.YInd = YInd;
                this.ZInd = ZInd;
                this.X0 = X0;
                this.X1 = X1;
                this.Y0 = Y0;
                this.Y1 = Y1;
                this.Z0 = Z0;
                this.Z1 = Z1;
            }

            /// <summary>
            /// Подходит ли частица к этой ячейке?
            /// Находится ли частица в пространстве ячейки?
            /// </summary>
            /// <param name="particle"></param>
            /// <returns></returns>
            bool GoodParticle(IParticle3D particle) {
                return !(particle.X < X0 || particle.X >= X1
                       || particle.Y < Y0 || particle.Y >= Y1
                       || particle.Z < Z0 || particle.Z >= Z1);
            }

            /// <summary>
            /// Перенести частицы из стэка в Лист с частицами
            /// </summary>
            public void DumpStack() {
                Particles.AddRange(ParticleStack);
                ParticleStack.Clear();

            }

            /// <summary>
            /// Очистить лист с частицами от неподходящих частиц,
            /// неподходяцие частицы переносятся в ConcurrentStack<IParticle2D> toThis
            /// </summary>
            /// <param name="toThis">Push'ит неподходяцие частицы сюда</param>
            public void DumpBadParticles(ConcurrentStack<IParticle3D> toThis) {
                var badParticles = Particles.Where(p => !GoodParticle(p)).ToArray();
                toThis.PushRange(badParticles);
                Particles = Particles.Except(badParticles).ToList();
            }

            /// <summary>
            /// Заполнить поле Neibs у каждой частицы, находящейся в ячейке
            /// В качестве соседей для частицы выступают ВСЕ частицы, находящиеся в этой и в соседних ячейках (за исключением самой частицы)
            /// </summary>
            /// <param name="cells"></param>
            public void FillAllNeibs(Cell3D[,,] cells) {
                int i0 = XInd - 1 >= 0 ? XInd - 1 : XInd;
                int j0 = YInd - 1 >= 0 ? YInd - 1 : YInd;
                int k0 = ZInd - 1 >= 0 ? ZInd - 1 : ZInd;
                int i1 = XInd + 1 < cells.GetLength(0) ? XInd + 1 : XInd;
                int j1 = YInd + 1 < cells.GetLength(1) ? YInd + 1 : YInd;
                int k1 = ZInd + 1 < cells.GetLength(2) ? ZInd + 1 : ZInd;
                var allPart = Enumerable.Empty<IParticle3D>();

                for(int i = i0; i <= i1; i++) {
                    for(int j = j0; j <= j1; j++) {
                        for(int k = k0; k <= k1; k++) {
                            allPart = allPart.Concat(cells[i,j,k].Particles);
                        }
                    }
                }
                var allPlst = allPart.ToList();
                foreach(var particle in Particles) {
                    particle.Neibs.Clear();
                    particle.Neibs.AddRange(allPlst.Where(p => !p.Equals(particle)));
                }
            }
        }
        #endregion

        #region private fields

        /// <summary>
        /// Собственно матрица ячеек 
        /// [индекс по Х, индекс по Y]
        /// [столбец, строка]
        /// Размерность [Ncols, Nrows]
        /// </summary>
        Cell3D[,,] CellNet;

        /// <summary>
        /// Кол-во строк и столбцов в матрице ячеек
        /// </summary>
        int Nrows, Ncols, Naisles;

        /// <summary>
        /// Они же, но только в списке (для параллельного перебора)
        /// </summary>
        List<Cell3D> CellsList = new List<Cell3D>();

        /// <summary>
        /// Характеристики сетки ячеек
        /// </summary>
        double 
            hmax, //максимальная длина сглаживания, она же является стороной квадратной ячейки
            xmin, //левая минимальная координата сетки (она же минимальная Х координата частиц)
            xmax, //правая максимальная координата сетки (она же максимальная Х координата частиц)
            ymin, //нижняя минимальная координата сетки (она же минимальная Y координата частиц)
            ymax, //верхнияя максимальная координата сетки (она же максимальная Y координата частиц)
            zmin, //нижняя минимальная координата сетки (она же минимальная Y координата частиц)
            zmax; //верхнияя максимальная координата сетки (она же максимальная Y координата частиц)

        /// <summary>
        /// Тут лежат частицы, которым нужно распределение по ячейкам
        /// </summary>
        ConcurrentStack<IParticle3D> LostPatricleStack = new ConcurrentStack<IParticle3D>();
        #endregion

        #region public fields

        /// <summary>
        /// Лист c интегрируемыми частицами (добавляются в Children)
        /// </summary>
        public List<IParticle3D> Particles = new List<IParticle3D>();

        /// <summary>
        /// Лист со всеми частицами 
        /// </summary>
        public List<IParticle3D> AllParticles = new List<IParticle3D>();

        /// <summary>
        /// Лист с частицами, представляющими "стены"
        /// </summary>
        public List<IParticle3D> WallParticles = new List<IParticle3D>();

        public int MaxStuffCount { get; private set; }

        #endregion

        #region private methods
        /// <summary>
        /// "пересобрать ячейки"
        /// </summary>
        void RebuildCells() {
            foreach(var cell in CellsList) {
                cell.Particles.Clear();
            }
            CellNet = null;
            CellsList.Clear();
            if(AllParticles.Count == 0)
                return;
            xmin = AllParticles.Min(p => p.X) - hmax * 0.5;
            ymin = AllParticles.Min(p => p.Y) - hmax * 0.5;
            zmin = AllParticles.Min(p => p.Z) - hmax * 0.5;
            xmax = AllParticles.Max(p => p.X) + hmax * 0.5;
            ymax = AllParticles.Max(p => p.Y) + hmax * 0.5;
            zmax = AllParticles.Max(p => p.Z) + hmax * 0.5;

            Nrows = (int)Ceiling((ymax - ymin) / hmax);
            Ncols = (int)Ceiling((xmax - xmin) / hmax);
            Naisles = (int)Ceiling((zmax - zmin) / hmax);
            CellNet = new Cell3D[Ncols,Nrows,Naisles];
            for(int i = 0; i < Ncols; i++) {
                for(int j = 0; j < Nrows; j++) {
                    for(int k = 0; k < Naisles; k++) {
                        double x0 = xmin + i * hmax;
                        double x1 = x0 + hmax;
                        double y0 = ymin + j * hmax;
                        double y1 = y0 + hmax;
                        double z0 = zmin + j * hmax;
                        double z1 = z0 + hmax;
                        CellNet[i,j,k] = new Cell3D(i,j,k,x0,x1,y0,y1,z0,z1);
                        CellsList.Add(CellNet[i,j,k]);
                    }

                }
            }
        }
      
        /// <summary>
        /// Заполнить ячейки
        /// </summary>
        public void FillCells() {
            //Оставляем только хорошие частицы, остальное в Lost
            Parallel.ForEach(CellsList.Where(c=>c.Particles.Count>0),c => {
                c.DumpBadParticles(LostPatricleStack);
            });

            //Распределяем Lost по ячейкам
            Parallel.ForEach(LostPatricleStack,p => {
                CellNet[GetHashXInd(p),GetHashYInd(p),GetHashZInd(p)].ParticleStack.Push(p);
            });
            LostPatricleStack.Clear();

            //Оставляем только хорошие частицы, остальное в Lost
            Parallel.ForEach(CellsList.Where(c => c.ParticleStack.Count > 0),c => {
                c.DumpStack();
            });
        }

        /// <summary>
        /// Заполняем соседей
        /// </summary>
        public void FillNeibs() {
            Parallel.ForEach(CellsList.Where(c => c.Particles.Count > 0),c => {
                c.FillAllNeibs(CellNet);
            });
        }

        /// <summary>
        /// Вызываем методы DoStuff(1)...DoStuff(n) у ВСЕХ частиц 
        /// </summary>
        void UpdateParticles() {
            //Заполняем d/dt
            for(int i = 0; i < MaxStuffCount; i++) {
                Parallel.ForEach(AllParticles.Where(p => i < p.StuffCount), p => {
                    p.DoStuff(i);
                });
            }
        }

        /// <summary>
        /// ScnObjDummy.SynchMeBefore()
        /// </summary>
        /// <param name="t"></param>
        void SynchBeforeAction(double t) {
            FillCells();
            UpdateParticles();
        }

        /// <summary>
        /// Вычисляет индекс ячейки (колонку) в сетке CellNet, подходящей для частицы
        /// </summary>
        /// <param name="particle"></param>
        /// <returns></returns>
        int GetHashXInd(IParticle3D particle) {
            return (int)Floor((particle.X - xmin) / hmax);
        }

        /// <summary>
        /// Вычисляет индекс ячейки (строку) в сетке CellNet, подходящей для частицы
        /// </summary>
        /// <param name="particle"></param>
        /// <returns></returns>
        int GetHashYInd(IParticle3D particle) {
            return (int)Floor((particle.Y - ymin) / hmax);
        }

        /// <summary>
        /// Вычисляет индекс ячейки (z) в сетке CellNet, подходящей для частицы
        /// </summary>
        /// <param name="particle"></param>
        /// <returns></returns>
        int GetHashZInd(IParticle3D particle) {
            return (int)Floor((particle.Z - zmin) / hmax);
        }
        #endregion

        #region Constructors

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="integrParticles">Интегрируемые частицы</param>
        /// <param name="wall">неинтегрируемые частицы</param>
        public Sph3D(IEnumerable<IParticle3D> integrParticles,IEnumerable<IParticle3D> wall) {
            Particles.AddRange(integrParticles);
            WallParticles.AddRange(wall);
            AllParticles.AddRange(integrParticles.Concat(wall));

            MaxStuffCount = AllParticles.Max(p => p.StuffCount);
            hmax = AllParticles.Max(p => p.GetHmax());

            int i = 0;
            foreach(var p in Particles) {
                p.Name = $"particle{i++}";
                AddChildUnsafe(p);
            }
            i = 0;
            foreach(var w in WallParticles) {
                w.Name = $"wall{i++}";
            }

            LostPatricleStack.PushRange(AllParticles.ToArray());
            RebuildCells();

            SynchMeBefore += SynchBeforeAction;
        }
        #endregion

        #region Static methods

        #endregion
    }
}
