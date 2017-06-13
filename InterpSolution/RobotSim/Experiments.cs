using Interpolator;
using Microsoft.Research.Oslo;
using MoreLinq;
using Sharp3D.Math.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Math;

namespace RobotSim {
    public class Experiments_Wall {
        public int id = 0;
        public string Name { get; set; } = "Experiment1";
        /// <summary>
        /// в градусах
        /// </summary>
        public double Angle { get; set; } = 0;
        public double WheelMoment { get; set; } = 1;
        public double OmegaMax { get; set; } = 5;
        public double Magnetic_h { get; set; } = 0.07;
        public double Magnetic_Fmax { get; set; } = 1.3;
        public double K_trenya { get; set; } = 0.9;
        public (double k, double mu) SurfProp = (10000, 100);
        public Vector3D CenterBoxOtnCM = new Vector3D(0.00433, -0.00555, 0);

        public double Mass { get; set; } = 2;
        public double l = 0.187, h = 0.03, w = 0.155;

        public double pawAngleSpeed = 30;
        public  RobotDynamics GetRD() {
            var sol = new RobotDynamics(Mass,l,h,w,CenterBoxOtnCM, Name);
            //sol.Body.Vec3D = new Vector3D(0.3, 0.1, 0);
            sol.Body.SynchQandM();
            sol.Body.RotateOXtoVec(new Vector3D(1, 0, 0));
            sol.Body.SynchQandM();
            sol.Body.SetPosition_LocalPoint_LocalMoveToIt_LocalFixed(Vector3D.XAxis, -Vector3D.YAxis, -Vector3D.ZAxis, Vector3D.ZAxis);
            sol.Body.Vec3D = CenterBoxOtnCM;
            sol.Body.SynchQandM();
            var sinAlpha = Sin(Angle * PI / 180);
            var cosAlpha = Cos(Angle * PI / 180);
            var vecOX = Vector3D.XAxis * cosAlpha + Vector3D.ZAxis * sinAlpha;
            sol.Body.SetPosition_LocalPoint_LocalMoveToIt_LocalFixed(Vector3D.XAxis, vecOX, -Vector3D.YAxis,Vector3D.YAxis);

            sol.pawAngleFunc0 = PawFunc0;
            sol.pawAngleFunc3 = PawFunc0;
            sol.Create4GUS(WheelMoment, OmegaMax);
            var mostLeftPoint = sol.TracksAll
                .SelectMany(tr => tr.ConnP.Select(cp => tr.WorldTransform * cp))
                .MinBy(p => p.X);


            MagneticForce.Clear();
            MagneticForce.Add(0, Magnetic_Fmax);
            MagneticForce.Add(Magnetic_h, 0);
            surfPoint = new Vector3D(mostLeftPoint.X, 0, 0);
            surf = new FlatSurf(SurfProp.k, SurfProp.mu, surfPoint, new Vector3D(1, 0, 0));
            sol.AddSurf_magnetic(surf, K_trenya, MagForceFunct);

            sol.AddGForcesToAll();

            CommandsDependsOnCurrPOs(sol);
            return sol;
        }
        public InterpXY MagneticForce = new InterpXY();

        double MagForceFunct(double h) {
            return MagneticForce.GetV(h);
        }
        public static void CommandsDependsOnCurrPOs(RobotDynamics solution) {
            solution.Body.SynchQandM();
            solution.wheels.ForEach(w => w.SynchQandM());
            solution.BlockedWheels = false;
        }

        double PawFunc0(double t) {
            return t * pawAngleSpeed;
        }

        public Dictionary<string, InterpXY> Results = new Dictionary<string, InterpXY>();
        private Vector3D surfPoint;
        private FlatSurf surf;

        void PrepDict(RobotDynamics rd) {
            Results.Add("Скорость цм, м/с", new InterpXY());
            Results.Add("Угол передних лап, гр", new InterpXY());
            for (int i = 0; i < rd.wheels.Count; i++) {
                Results.Add($"Скорость вращения колеса {i}, град/с", new InterpXY());
            }
            

        }

        void FillResults(RobotDynamics rd) {
            Results["Скорость цм, м/с"].Add(rd.TimeSynch, rd.Body.Vel.Vec3D.GetLength());
            Results["Угол передних лап, гр"].Add(rd.TimeSynch, PawFunc0(rd.TimeSynch));
            for (int i = 0; i < rd.wheels.Count; i++) {
                Results[$"Скорость вращения колеса {i}, град/с"].Add(rd.TimeSynch,rd.wheels[i].Omega.Vec3D.GetLength()*180/PI);
            }

        }

        public void Start() {
            var pr = GetRD();
            var v0 = pr.Rebuild(pr.TimeSynch);
            PrepDict(pr);
            var names = pr.GetDiffPrms().Select(dp => dp.FullName).ToList();
            var dt = 0.00001;

            var solutions = Ode.RK45(pr.TimeSynch, v0, pr.f, dt).WithStep(0.01);
            foreach (var sol in solutions) {
                FillResults(pr);
                if (StopFunc(pr))
                    break;
            }
        }

        public bool StopFunc(RobotDynamics rd) {
            if (rd.TimeSynch * pawAngleSpeed > 120)
                return true;
            foreach (var w in rd.wheels) {
                if (surf.GetDistance(w.Vec3D) < w.R_max * 1.5)
                    return false;
            }
            return true;
        }
    }
}
