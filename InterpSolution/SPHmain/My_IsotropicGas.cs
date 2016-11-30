using System;
using System.Collections.Generic;
using Sharp3D.Math.Core;

using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPH_2D
{
    public class FileReader
    {
        private static List<My_IsotropicGas> ConstructBountPart(List<Tuple<Vector2D, Vector2D>> boundaries,
            double boundaryDensity)
        {
            List<My_IsotropicGas> WallParticles = new List<My_IsotropicGas>();
            foreach (var side in boundaries)
            {
                Vector2D vector = side.Item2 - side.Item1;
                int partN = Convert.ToInt32((Math.Abs(vector.GetLength())) * boundaryDensity);
                double xstep = vector.X / partN;
                double ystep = vector.Y / partN;
                for (int i = 0; i < partN; ++i)
                {
                    My_IsotropicGas a = new My_IsotropicGas(1, 1, true);
                    a.X = side.Item1.X + i * xstep;
                    a.Y = side.Item1.Y + i * ystep;
                    WallParticles.Add(a);
                }
            }
            return WallParticles;
        }
        //
        static List<Tuple<Vector2D, Vector2D>> CreateBoundaries(string pathToBoundaries)
        {
            System.IO.StreamReader file =
                 new System.IO.StreamReader(pathToBoundaries);
            List<Tuple<Vector2D, Vector2D>> boundaries_ = new List<Tuple<Vector2D, Vector2D>>();

            string line;
            while ((line = file.ReadLine()) != null)
            {
                string[] numbers = line.Split(',');
                double first = Convert.ToDouble(numbers[0]);
                double second = Convert.ToDouble(numbers[1]);
                Vector2D firstP = new Vector2D(first, second);
                first = Convert.ToDouble(numbers[2]);
                second = Convert.ToDouble(numbers[3]);
                Vector2D secondP = new Vector2D(first, second);
                Tuple<Vector2D, Vector2D> side = new Tuple<Vector2D, Vector2D>(firstP, secondP);
                boundaries_.Add(side);
            }
            return boundaries_;
        }

        public static List<My_IsotropicGas> ConstructBountPart(string pathToBound, List<Tuple<Vector2D, Vector2D>> boundaries)
        {
            boundaries = CreateBoundaries(pathToBound);
            List<My_IsotropicGas> WallParticles = ConstructBountPart(boundaries, 1.0);
            return WallParticles;
        }

        public static List<My_IsotropicGas> FillRegion(List<Vector2D> region, bool shape, int N)
        {
            //плотность давление масса скорость (x,y)
            double[] cond = { 1, 1, 1, 0, 1 };
            double X = Math.Abs(region[0].X - region[3].X);
            double Y = Math.Abs(region[0].Y - region[1].Y);
            // double XYratio = X / Y;
            int per_y = (int)Math.Sqrt(N);
            int per_x = per_y;//number / per_y;
            double xstep = X / per_x;
            double ystep = Y / per_y;
            //

            List<My_IsotropicGas> particles = new List<My_IsotropicGas>();
            for (int i = 0; i < N; ++i)
            {
                particles.Add(new My_IsotropicGas(1, 1, false, cond));
            }

            for (int i = 0; i < per_y; ++i)
            {
                for (int j = 0; j < per_x; ++j)
                {
                    double curX = region[0].X + j * xstep;
                    double curY = region[0].Y + i * ystep;
                    particles[per_x * i + j].X = curX;
                    particles[per_x * i + j].Y = curY;
                }
            }

            if (!shape)
            {
                List<My_IsotropicGas> particlesSphere = new List<My_IsotropicGas>();
                double center_x = (region[2].X + region[0].X) / 2;
                double center_y = (region[2].Y + region[0].Y) / 2;
                double r = (region[2].X - region[0].X) / 2;
                for (int i = 0; i < N; ++i)
                {
                    double curR = Math.Sqrt((center_x - particles[i].X) * (center_x - particles[i].X) +
                        (center_y - particles[i].Y) * (center_y - particles[i].Y));
                    if (curR <= r)
                        particlesSphere.Add(particles[i]);
                }
                return particlesSphere;
            }
            return particles;
        }

        public static List<My_IsotropicGas> ConstructRealPart(string pathToReal)
        {
            //rectangle
            System.IO.StreamReader file =
                 new System.IO.StreamReader(pathToReal);
            string line;
            line = file.ReadLine();
            List<bool> shapes = new List<bool>();//true rect false sphere
            if (line == "rectangle")
            {
                shapes.Add(true);
            }
            else
            {
                shapes.Add(false);
            }

            List<List<Vector2D>> regions = new List<List<Vector2D>>();
            List<Vector2D> region = new List<Vector2D>();
            while ((line = file.ReadLine()) != null)
            {
                if (line != "rectangle" && line != "circle")
                {
                    string[] numbers = line.Split(',');
                    double first = Convert.ToDouble(numbers[0]);
                    double second = Convert.ToDouble(numbers[1]);
                    Vector2D point = new Vector2D(first, second);
                    region.Add(point);
                }
                else
                {
                    regions.Add(region);
                    if (line == "rectangle") shapes.Add(true);
                    else shapes.Add(false);
                    region = new List<Vector2D>();
                }
            }
            regions.Add(region);
            //
            List<My_IsotropicGas> particles = new List<My_IsotropicGas>();

            var regionsAndShapes = regions.Zip(shapes, (n, w) => new { Reg = n, Shape = w });

            foreach (var regShape in regionsAndShapes)
            {
                particles.AddRange(FillRegion(regShape.Reg, regShape.Shape, 100));
            }
            //
            return particles;
        }
    }

    public class My_IsotropicGas : IsotropicGasParticle
    {
        bool isboundary;

        public My_IsotropicGas(double d, double hmax, bool boundarytype = false, double[] cond = null) : base(d, hmax)
        {
            //cond -  плотность давление масса скорость (x,y)
            isboundary = boundarytype;
            if (cond != null)
            {
                Ro = cond[0];
                P = cond[1];
                M = cond[2];
                Vel.X = cond[3];
                Vel.Y = cond[4];
                E = P / (0.4 * Ro);
            }
        }

        Tuple<double, double, double, double> calcDistanse(Tuple<Vector2D, Vector2D> side)
        {
            double A = side.Item1.Y - side.Item2.Y;
            double B = side.Item2.X - side.Item1.X;
            double C = side.Item1.X * side.Item2.Y - side.Item2.X * side.Item1.Y;
            double distance = Math.Abs((A * X + B * Y + C) / Math.Sqrt(A * A + B * B));
            return new Tuple<double, double, double, double>(A, B, C, distance);
        }
        //принимает границы, находит ближайшую к точке линию и возвращает координаты симетричной точки
        public Tuple<double, double> createMirrorPart(List<Tuple<Vector2D, Vector2D>> boundaries)
        {
            Tuple<double, double, double, double> result = calcDistanse(boundaries[0]);
            foreach (var side in boundaries)
            {
                Tuple<double, double, double, double> current = calcDistanse(side);
                if (current.Item4 < result.Item4)
                {
                    result = current;
                }
            }
            double A = result.Item1;
            double B = result.Item2;
            double C = result.Item3;
            double D = -(A * Y - B * X);
            double Y1 = -(D * A + C * B) / (B * B + A * A);
            //if (Double.IsInfinity(Y1)) Y1 = 0;
            double X1 = (A * Y1 + D) / B;
            //if (Double.IsInfinity(X1)) X1 = 0;
            double X2 = 2 * X1 - X;
            double Y2 = 2 * Y1 - Y;
            return new Tuple<double, double>(X2, Y2);
        }
        //
        public double C()
        {
            return (k * (k - 1) * E);
        }
        //
        public override void FillDts()
        {
            Ro = 0;
            foreach (var neib in Neibs.Where(n => GetDistTo(n) < hmax).Cast<My_IsotropicGas>())
            {
                Vector2D Rji = neib.Vec2D - Vec2D;
                Vector2D Rji_norm = Rji.Norm;
                if (!neib.isboundary)
                {
                    double mj = neib.M;
                    double deltax = X - neib.X;
                    double deltay = Y - neib.Y;
                    double deltaVx = Vel.X - neib.Vel.X;
                    double deltaVy = Vel.Y - neib.Vel.Y;
                    double r = Math.Sqrt(deltax * deltax + deltay * deltay);

                    //
                    double H = 0;
                    if ((deltaVx * deltax + deltaVy * deltay) >= 0)
                    {
                        double alpha_ = 1.1;
                        double beta_ = 1.1;
                        double V = Math.Sqrt(deltaVx * deltaVx + deltaVy * deltaVy);
                        double phi = hmax * r * V / (Math.Pow(r, 2) + Math.Pow(0.1 * hmax, 2));
                        H = 2 * (-alpha_ * 0.5 * (C() + neib.C()) * phi + beta_ * Math.Pow(phi, 2)) / (Ro + neib.Ro);
                    }

                    //
                    double brackets = mj * (P / (Ro * Ro) + neib.P / (neib.Ro * neib.Ro) + H);
                    Vector2D dW = new Vector2D((deltax / r) * dW_func(r, hmax), (deltay / r) * dW_func(r, hmax));
                    //плотность
                    Ro += mj * W_func(r, hmax);
                    //скорости
                    dV.X += brackets * dW.X;
                    dV.Y += brackets * dW.Y;
                    //энергия
                    dE += 0.5 * brackets * Vel.Vec2D * dW;

                }
                else
                {
                    double r0 = 1;
                    double rij = Rji.GetLength();
                    if (r0 / rij <= 1)
                    {
                        double n1 = 12;
                        double n2 = 4;
                        double D = (Vel.Vec2D.GetLength() > neib.Vel.Vec2D.GetLength()) ?
                            Vel.Vec2D.GetLength() * Vel.Vec2D.GetLength() :
                            neib.Vel.Vec2D.GetLength() * neib.Vel.Vec2D.GetLength();
                        double brackets = D * (Math.Pow(r0 / rij, n1) - Math.Pow(r0 / rij, n2)) / (rij * rij);
                        double PDijx = brackets * (X - neib.X);
                        double PDijy = brackets * (Y - neib.Y);
                        //
                        dV.X += PDijx / M;
                        dV.Y += PDijy / M;
                    }
                }
            }

        }

    }

    public class My_Sph2D : Sph2D
    {

        public static List<Tuple<Vector2D, Vector2D>> boundaries;

        public My_Sph2D(string pathToreal, string pathToBound) :
            base(FileReader.ConstructRealPart(pathToreal), FileReader.ConstructBountPart(pathToBound, boundaries))
        {
            int i = 0;
            i++;
        }

    }
}

//string pathToBoundaries = "D:\\diploma\\case1.txt";
//boundaries = CreateBoundaries(pathToBoundaries);

//Particles.AddRange(integrParticles);
//            if (wall != null)
//                constructBountPart