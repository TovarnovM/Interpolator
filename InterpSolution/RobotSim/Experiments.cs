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
using SimpleIntegrator;
using static System.Math;
using System.Globalization;

namespace RobotSim {
    public class Experiments_Wall_params {
        public int id { get; set; } = 0;
        public string ResultIndex { get; set; } = "";
        public string Name { get; set; } = "Experiment1";
        /// <summary>
        /// в градусах
        /// </summary>
        public double Angle { get; set; } = 0;
        public double WheelMoment { get; set; } = 0;//4;
        public double OmegaMax { get; set; } = 4;
        public double Magnetic_h { get; set; } = 0.07;
        public double Magnetic_Fmax { get; set; } = 1.3;
        public double K_trenya { get; set; } = 0.9;
        public double SurfProp_K { get; set; } = 10000;
        public double SurfProp_mu { get; set; } = 100;
        public Vector3D GetCenterBoxOtnCM() {
            return new Vector3D(CenterBoxOtnCM_X, -CenterBoxOtnCM_Y, CenterBoxOtnCM_Z);
        }
        public double CenterBoxOtnCM_X { get; set; } = 0.00433;
        public double CenterBoxOtnCM_Y { get; set; } = 0.00555;
        public double CenterBoxOtnCM_Z { get; set; } = 0;
        public double Mass { get; set; } = 2;
        public double l { get; set; } = 0.187;
        public double h { get; set; } = 0.03;
        public double w { get; set; } = 0.155;

        public double pawAngleSpeed { get; set; } = 3;
        public double Mz { get; set; } = 0;
        public double TimeMax { get; set; } = 10;
        public virtual Experiments_Wall_params GetCopy() {
            return (Experiments_Wall_params)MemberwiseClone();
        }
        public Dictionary<string,string> GetDict() {
            return this.GetType()
                .GetProperties()
                .ToDictionary(
                    p => p.Name,
                    p => p.GetValue(this).ToString()
                 );
        }
    }

    public class Experiments_Wall {
        public Experiments_Wall_params Prs { get; set; } = new Experiments_Wall_params();
        public virtual RobotDynamics GetRD() {
            var sol = new RobotDynamics(Prs.Mass, Prs.l, Prs.h, Prs.w, Prs.GetCenterBoxOtnCM(), Prs.Name);
            sol.Body.AddMoment(Force.GetMoment(Prs.Mz, Vector3D.ZAxis, sol.Body));
            //sol.Body.Vec3D = new Vector3D(0.3, 0.1, 0);
            sol.Body.SynchQandM();
            sol.Body.RotateOXtoVec(new Vector3D(1, 0, 0));
            sol.Body.SynchQandM();
            sol.Body.SetPosition_LocalPoint_LocalMoveToIt_LocalFixed(Vector3D.XAxis, -Vector3D.YAxis, -Vector3D.ZAxis, Vector3D.ZAxis);
            sol.Body.Vec3D = Prs.GetCenterBoxOtnCM();
            sol.Body.SynchQandM();
            var sinAlpha = Sin(Prs.Angle * PI / 180);
            var cosAlpha = Cos(Prs.Angle * PI / 180);
            var vecOX = Vector3D.XAxis * cosAlpha + Vector3D.ZAxis * sinAlpha;
            sol.Body.SetPosition_LocalPoint_LocalMoveToIt_LocalFixed(Vector3D.XAxis, vecOX, -Vector3D.YAxis,Vector3D.YAxis);

            sol.pawAngleFunc0 = PawFunc0;
            sol.pawAngleFunc3 = PawFunc0;
            sol.Create4GUS(Prs.WheelMoment, Prs.OmegaMax);
            var mostLeftPoint = sol.TracksAll
                .SelectMany(tr => tr.ConnP.Select(cp => tr.WorldTransform * cp))
                .MinBy(p => p.X);


            MagneticForce.Clear();
            MagneticForce.Add(0, Prs.Magnetic_Fmax);
            MagneticForce.Add(0.33* Prs.Magnetic_h, Prs.Magnetic_Fmax*0.4);
            MagneticForce.Add(0.66 * Prs.Magnetic_h, Prs.Magnetic_Fmax * 0.2);
            MagneticForce.Add(Prs.Magnetic_h, 0);
            surfPoint = new Vector3D(mostLeftPoint.X, 0, 0);
            surf = new FlatSurf(Prs.SurfProp_K, Prs.SurfProp_mu, surfPoint, new Vector3D(1, 0, 0));
            sol.AddSurf_magnetic(surf, Prs.K_trenya, MagForceFunct);

            sol.AddGForcesToAll();

            CommandsDependsOnCurrPOs(sol);
            return sol;
        }
        InterpXY MagneticForce = new InterpXY();

        double MagForceFunct(double h) {
            return MagneticForce.GetV(h);
        }
        public void CommandsDependsOnCurrPOs(RobotDynamics solution, bool blockWheels = false) {
            solution.Body.SynchQandM();
            solution.wheels.ForEach(w => w.SynchQandM());
            solution.BlockedWheels = blockWheels;
        }

        public double PawFunc0(double t) {
            return t * Prs.pawAngleSpeed;
        }

        public Dictionary<string, InterpXY> GetResults() => Results;  
        protected Dictionary<string, InterpXY> Results = new Dictionary<string, InterpXY>();
        protected List<SolPoint> SolPoints = new List<SolPoint>();
        private Vector3D surfPoint;
        private FlatSurf surf;
        protected List<int> logIds;

        public virtual void PrepDict(RobotDynamics rd) {
            Results.Clear();
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

        public virtual void FillResults(RobotDynamics rd) {
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
        
        public virtual Dictionary<string, double> GetObrabotkaSmoth(double dt_b, double dt_f) {
            var res = new Dictionary<string, double>();
            var smooth = GetSmooth(dt_b, dt_f,"");
            double t0 = 0.2;
            double t1 = smooth["Скорость Y, м/с"].Data
                .SkipUntil(d => d.Key < t0)
                .TakeWhile(d => d.Value.Value > 0)
                .Select(d => d.Key)
                .LastOrDefault();
            bool isGood = t1 > t0;
            double isGoodD = isGood ? 1 : 0;
            res.Add("Норм вариант?", isGoodD);
            res.Add("Макс сила на переднем модуле, Н", 0d);
            res.Add("Скорость движения, см/с", 0d);
            res.Add("Угол передних лап, гр", 0d);
            res.Add("Суммарная удерживающая сила, Н", 0d);
            if(t1 > t0) {
                var intFm = new InterpXY();
                foreach (var item in smooth["Макс нагрузка лапы 0, Н"].Data) {
                    intFm.Add(item.Key, Min(item.Value.Value, smooth["Макс нагрузка лапы 3, Н"].GetV(item.Key)));
                }
                double tpawmax = intFm.Get_MaxElem_T(t0, t1);
                res["Макс сила на переднем модуле, Н"] = intFm.GetV(tpawmax);
                res["Скорость движения, см/с"] = smooth["Скорость Y, м/с"].GetV(tpawmax)*100;
                res["Угол передних лап, гр"] = Results["Угол передних лап, гр"].GetV(tpawmax);
                res["Суммарная удерживающая сила, Н"] = smooth.Where(s => s.Key.StartsWith("Суммарная нагрузка лапы")).Sum(s => s.Value.GetV(tpawmax)) * Prs.K_trenya;
            }
            return res;
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
        char separator = ';';
        protected void SaveResultsToFile(string exFilePath = @"C:\Users\User\Desktop\ExperLog.txt", string solFilePath = @"C:\Users\User\Desktop\ExperLog_sol.xml") {
            using (var f = new StreamWriter(exFilePath)) {
                f.WriteLine(JsonConvert.SerializeObject(Prs));
                var sb = new StringBuilder();
                //sb.Append("info : {{");
                //foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(this)) {
                //    string name = descriptor.Name;
                //    object value = descriptor.GetValue(this);
                //    sb.Append($"\"{name}\":{value},\n");
                //}
                //sb.Append("}}\n");
                f.WriteLine("=============");
                
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

            var sw = new StreamWriter(solFilePath);
            DummyIOHelper.SerializeManySP(sw, SolPoints);
            sw.Close();
        }

        public void AnalyzeSolPoint() {
            var sol = GetRD();
            PrepDict(sol);
            foreach (var sp in SolPoints) {
                sol.f(sp.T, sp.X);
                FillResults(sol);
            }
        }

        public void LoadSolPoints(string solFilePath = @"C:\Users\User\Desktop\ExperLog_sol.xml") {
            SolPoints.Clear();
            var sr = new StreamReader(solFilePath);
            DummyIOHelper.DeserializeManySP(sr, SolPoints);
            sr.Close();
        }

        public void LoadResultsFromFile(string exFilePath = @"C:\Users\User\Desktop\ExperLog.txt") {
            using (var f = new StreamReader(exFilePath)) {
                var sb = new StringBuilder();
                string line = f.ReadLine();
                while (line != "=============") {
                    sb.Append(line);
                    line = f.ReadLine();
                }
                Prs = JsonConvert.DeserializeObject<Experiments_Wall_params>(sb.ToString());
                line = f.ReadLine();
                var hds = line.Split(separator).Select(h => h.Trim()).ToList();
                Results.Clear();


                foreach (var h in hds) {
                    Results.Add(h, new InterpXY());
                }

                while (!f.EndOfStream) {
                    line = f.ReadLine();
                    var vals = line.Split(separator).Select(s => GetDouble(s.Trim())).ToList();
                    var t = vals[0];
                    for (int i = 1; i < hds.Count; i++) {
                        Results[hds[i]].Add(t, vals[i]);
                    }
                }
            }
        }

        public void FillDictFromSP() {
            var pr = GetRD();
            var v0 = pr.Rebuild(pr.TimeSynch);
            PrepDict(pr);
            foreach (var sp in SolPoints) {
                pr.SynchMeTo(sp);
                FillResults(pr);
                if (StopFunc(pr) != "") {
                    Prs.ResultIndex = StopFunc(pr);
                    break;
                }
            }

        }

        protected const string defexFilePath = @"C:\Users\User\Desktop\ExperLog.txt";
        protected const string defsolFilePath = @"C:\Users\User\Desktop\ExperLog_sol.xml";
        protected double _dt_ = 0.00001, _dt_out_ = 0.001, _tstartRecord = -1d;
        public virtual void Start(string exFilePath = defexFilePath, string solFilePath = defsolFilePath) {
            try {
                var pr = GetRD();
                var v0 = pr.Rebuild(pr.TimeSynch);
                PrepDict(pr);
                var names = pr.GetDiffPrms().Select(dp => dp.FullName).ToList();
                var dt = _dt_;
                
                var solutions = Ode.MidPoint(pr.TimeSynch, v0, pr.f, dt).WithStep(_dt_out_);
                foreach (var sol in solutions) {
                    if(sol.T >= _tstartRecord) {
                        FillResults(pr);
                        SolPoints.Add(sol);
                    }



                    if (StopFunc(pr) != "") {
                        Prs.ResultIndex = StopFunc(pr);
                        break;
                    }
                }
                SaveResultsToFile(exFilePath, solFilePath);

            } finally {

            }

        }


        public Task StartAsync() {

            return Task.Factory.StartNew( _ => Start(), TaskCreationOptions.LongRunning);
        }

        public virtual string StopFunc(RobotDynamics rd) {
            if (rd.TimeSynch >Prs.TimeMax)//* Prs.pawAngleSpeed > 90)
                return "время интегрирования";
            if (rd.Body.Y < -0.5)
                return "сполз";
            foreach (var w in rd.wheels) {
                if (surf.GetDistance(w.Vec3D) < w.R_max * 1.5)
                    return "";
            }
            return "отвалился";
        }

        public static double GetDouble(string value, double defaultValue = 0d) {

            //Try parsing in the current culture
            if (!double.TryParse(value, System.Globalization.NumberStyles.Any, CultureInfo.CurrentCulture, out double result) &&
                //Then try in US english
                !double.TryParse(value, System.Globalization.NumberStyles.Any, CultureInfo.GetCultureInfo("en-US"), out result) &&
                //Then in neutral language
                !double.TryParse(value, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out result)) {
                result = defaultValue;
            }
            
            return result;
        }

        public Dictionary<string, InterpXY> GetSmooth(double dt_param_b, double dt_param_f, string nameDop = "_smooth") {
            var smoothDict = new Dictionary<string, InterpXY>();
            foreach (var dictElem in Results) {
                smoothDict.Add(dictElem.Key + nameDop, dictElem.Value.GetSmootherByT_uniform(dt_param_b, dt_param_f).GetSmootherByT_uniform(dt_param_b, dt_param_f));
            }
            return smoothDict;
        }
    }


}
