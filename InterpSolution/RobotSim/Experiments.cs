using Interpolator;
using Microsoft.Research.Oslo;
using MoreLinq;
using Newtonsoft.Json;
using Sharp3D.Math.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Math;

namespace RobotSim {
    public class Experiments_Wall {
        public int id { get; set; } = 0;
        public string ResultIndex { get; set; }
        public string Name { get; set; } = "Experiment1";
        /// <summary>
        /// в градусах
        /// </summary>
        public double Angle { get; set; } = 0;
        public double WheelMoment { get; set; } = 0.5;
        public double OmegaMax { get; set; } = 5;
        public double Magnetic_h { get; set; } = 0.07;
        public double Magnetic_Fmax { get; set; } = 1.3;
        public double K_trenya { get; set; } = 0.9;
        public double SurfProp_K { get; set; } = 10000;
        public double SurfProp_mu { get; set; } = 100;
        public Vector3D CenterBoxOtnCM = new Vector3D(0.00433, -0.00555, 0);

        public double Mass { get; set; } = 2;
        public double l { get; set; } = 0.187;
        public double h { get; set; } = 0.03;
        public double w { get; set; } = 0.155;

        public double pawAngleSpeed = 3;
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
            surf = new FlatSurf(SurfProp_K, SurfProp_mu, surfPoint, new Vector3D(1, 0, 0));
            sol.AddSurf_magnetic(surf, K_trenya, MagForceFunct);

            sol.AddGForcesToAll();

            CommandsDependsOnCurrPOs(sol);
            return sol;
        }
        InterpXY MagneticForce = new InterpXY();

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

        public Dictionary<string, InterpXY> GetResults() => Results;  
        Dictionary<string, InterpXY> Results = new Dictionary<string, InterpXY>();
        private Vector3D surfPoint;
        private FlatSurf surf;
        List<int> logIds;

        void PrepDict(RobotDynamics rd) {
            Results.Add("Скорость Y, м/с", new InterpXY());
            Results.Add("Y, м", new InterpXY());
            Results.Add("Угол передних лап, гр", new InterpXY());
            for (int i = 0; i < rd.wheels.Count; i++) {
                Results.Add($"Скорость вращения колеса {i}, об/мин", new InterpXY());
            }
            logIds = rd.TracksAll.Select(tr => tr.logId).Distinct().ToList();
            foreach (var tr_id in logIds) {
                Results.Add($"Макс нагрузка лапы {tr_id}, Н", new InterpXY());
                Results.Add($"Мин нагрузка лапы {tr_id}, Н", new InterpXY());
                Results.Add($"Средняя нагрузка лапы {tr_id}, Н", new InterpXY());
                Results.Add($"Суммарная нагрузка лапы {tr_id}, Н", new InterpXY());
                Results.Add($"Длина пятна лапы {tr_id}, мм", new InterpXY());
            }
        }

        void FillResults(RobotDynamics rd) {
            Results["Скорость Y, м/с"].Add(rd.TimeSynch, rd.Body.Vel.Y);
            Results["Y, м"].Add(rd.TimeSynch, rd.Body.Vec3D.Y);
            Results["Угол передних лап, гр"].Add(rd.TimeSynch, PawFunc0(rd.TimeSynch));
            for (int i = 0; i < rd.wheels.Count; i++) {
                Results[$"Скорость вращения колеса {i}, об/мин"].Add(rd.TimeSynch,rd.wheels[i].Omega.Vec3D.GetLength()/2/PI*60);
            }
            foreach (var trid in logIds) {
                FillPawSpot(trid,rd);
            }

        }

        private void FillPawSpot(int trid, RobotDynamics rd) {
            var spot = surf.LogFromStep.Where(tup => tup.id == trid).ToList();
            double max = 0, min = 0, aver = 0, summ = 0,maxLength=0;
            if (spot.Count > 0) {
                min = spot[0].value;
                foreach (var tup in spot) {
                    if (tup.value > max)
                        max = tup.value;
                    if (tup.value < min)
                        min = tup.value;
                    summ += tup.value;
                }
                aver = summ / spot.Count;
                maxLength = spot.Select(tup => tup.p).Max(p => spot.Select(tup => (tup.p - p).GetLength()).Max());
            }

            Results[$"Макс нагрузка лапы {trid}, Н"].Add(rd.TimeSynch, max);
            Results[$"Мин нагрузка лапы {trid}, Н"].Add(rd.TimeSynch, min);
            Results[$"Средняя нагрузка лапы {trid}, Н"].Add(rd.TimeSynch, aver);
            Results[$"Суммарная нагрузка лапы {trid}, Н"].Add(rd.TimeSynch, summ);
            Results[$"Длина пятна лапы {trid}, мм"].Add(rd.TimeSynch, maxLength*1000);
        }

        void SaveResults() {
            using (var f = new StreamWriter(@"C:\Users\User\Desktop\ExperLog.txt")) {
                f.WriteLine(JsonConvert.SerializeObject(this));
                var sb = new StringBuilder();
                //sb.Append("info : {{");
                //foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(this)) {
                //    string name = descriptor.Name;
                //    object value = descriptor.GetValue(this);
                //    sb.Append($"\"{name}\":{value},\n");
                //}
                //sb.Append("}}\n");
                f.WriteLine("=============");
                var separator = ";";
                f.Write(sb.ToString());
                sb.Clear();
                sb.Append("t, sec;");
                foreach (var h in Results.Keys) {
                    sb.Append(h + separator);
                }
                sb.Remove(sb.Length - 1, 1);
                f.WriteLine(sb.ToString());
                int n = Results.Values.First().Count;
                for (int i = 0; i < n; i++) {
                    sb.Clear();
                    var t = Results.Values.First().Data.Keys[i];
                    sb.Append($"{t}" + separator);
                    foreach (var h in Results.Keys) {
                        sb.Append($"{Results[h].GetV(t)}" + separator);
                    }
                    sb.Remove(sb.Length - 1, 1);
                    f.WriteLine(sb.ToString());
                }
                f.Close();
            }
        }

        public void Start() {
            var pr = GetRD();
            var v0 = pr.Rebuild(pr.TimeSynch);
            PrepDict(pr);
            var names = pr.GetDiffPrms().Select(dp => dp.FullName).ToList();
            var dt = 0.00001;

            var solutions = Ode.MidPoint(pr.TimeSynch, v0, pr.f, dt).WithStep(0.001);
            foreach (var sol in solutions) {
                FillResults(pr);
                if (StopFunc(pr) != "") {
                    ResultIndex = StopFunc(pr);
                    break;
                }
                    
            }
            SaveResults();
        }

        public Task StartAsync() {
            return Task.Factory.StartNew(Start, TaskCreationOptions.LongRunning);
        }

        public string StopFunc(RobotDynamics rd) {
            if (rd.TimeSynch * pawAngleSpeed > 90)
                return "долго считает";
            if (rd.Body.Y < -0.5)
                return "сполз";
            foreach (var w in rd.wheels) {
                if (surf.GetDistance(w.Vec3D) < w.R_max * 1.5)
                    return "";
            }
            return "отвалился";
        }
    }
}
