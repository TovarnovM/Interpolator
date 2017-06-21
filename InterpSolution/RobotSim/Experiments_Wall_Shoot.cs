using Interpolator;
using Microsoft.Research.Oslo;
using Sharp3D.Math.Core;
using SimpleIntegrator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Math;

namespace RobotSim {
    public class Experiments_WallShoot_params : Experiments_Wall_params {
        public Experiments_WallShoot_params() {
            pawAngleSpeed = 0;
            WheelMoment = 0;
            TimeMax = 1;
        }
        public Vector3D GetCenterImpulse() {
            return new Vector3D(CenterImpulse_X, CenterImpulse_Y, CenterImpulse_Z);
        }
        public double CenterImpulse_X { get; set; } = 0.015;
        public double CenterImpulse_Y { get; set; } = 0.035;
        public double CenterImpulse_Z { get; set; } = 0;
        public double Tetta { get; set; } = 90d;
        public double Alpha { get; set; } = 0;
        public string ImpulseType { get; set; } = "fire";
        public double ImpulseT { get; set; } = 0.00035;
        public double ImpulseT0 { get; set; } = 0.2;
        public double Impulse { get; set; } = 2;
        public Vector3D GetShootDir() {
            var az1 = -Vector3D.XAxis * Cos(Tetta * PI / 180) + Vector3D.ZAxis * Sin(Tetta * PI / 180);
            return az1* Cos(Alpha * PI / 180) + Vector3D.YAxis * Sin(Alpha * PI / 180);
        }
        public Experiments_WallShoot_params GetCopy1() {
            return (Experiments_WallShoot_params)MemberwiseClone();
        }
    }

    public class Experiments_Wall_Shoot : Experiments_Wall {
        public Experiments_WallShoot_params PrsShoot { get {
                return (Experiments_WallShoot_params)Prs;
            }
            set {
                Prs = value;
            } }
        public Experiments_Wall_Shoot() {
            Prs = new Experiments_WallShoot_params();
            
        }
        public override RobotDynamics GetRD() {
            var sol = base.GetRD();

            if (!(Prs is Experiments_WallShoot_params)) {
                throw new Exception("Даешь нормальные параметры");
            }
            
            CommandsDependsOnCurrPOs(sol, true);
            ClipImpulse(sol.Body);

            return sol;
        }

        void ClipImpulse(MaterialObjectNewton body) {
            var fr = Force.GetForce(1, -PrsShoot.GetShootDir(), body, PrsShoot.GetCenterImpulse(), body);
            var imp = GetImpulse();
            fr.SynchMeBefore += t => {
                fr.Value = imp.GetV(t);
            };

            body.AddForce(fr);
        }

        InterpXY GetImpulse() {
            var impT = PrsShoot.ImpulseT;
            var impT0 = PrsShoot.ImpulseT0;
            var imp = new InterpXY();
            if (PrsShoot.ImpulseType == "fire") {
                
                imp.Add(impT0+0, 0);
                imp.Add(impT0 + impT *0.1, 0.1);
                imp.Add(impT0 + impT * 0.2, 0.3);
                imp.Add(impT0 + impT * 0.25, 0.6);
                imp.Add(impT0 + impT * 0.3, 0.95);
                imp.Add(impT0 + impT * 0.4, 1);
                imp.Add(impT0 + impT * 0.5, 0.9);
                imp.Add(impT0 + impT * 0.6, 0.5);
                imp.Add(impT0 + impT * 0.8, 0.1);
                imp.Add(impT0 + impT * 1, 0);
                
            }
            var integr = imp.Get_Integral(impT0, impT0 + impT);
            var mnozj = PrsShoot.Impulse / integr;
            return imp.GetInterpMultyConst(mnozj);
        }

        Vector3D centerMass0, OXAxis0;
        public override void PrepDict(RobotDynamics rd) {
            Results.Clear();
            Results.Add("Скорость Y, см/с", new InterpXY());
            Results.Add("Положение цм от изн X, мм", new InterpXY());
            Results.Add("Положение цм от изн Y, мм", new InterpXY());
            Results.Add("Положение цм от изн Z, мм", new InterpXY());
            Results.Add("Положение цм от изн всего, мм", new InterpXY());
            Results.Add("Перегрузка цм X, g", new InterpXY());
            Results.Add("Перегрузка цм Y, g", new InterpXY());
            Results.Add("Перегрузка цм Z, g", new InterpXY());
            Results.Add("Перегрузка цм всего, g", new InterpXY());
            Results.Add("Ось ОХ X", new InterpXY());
            Results.Add("Ось ОХ Y", new InterpXY());
            Results.Add("Ось ОХ Z", new InterpXY());
            Results.Add("Отклонение от изн положения, гр", new InterpXY());
            centerMass0 = rd.Body.Vec3D;
            OXAxis0 = rd.Body.WorldTransformRot * Vector3D.XAxis;
        }

        public override void FillResults(RobotDynamics rd) {
            //if(Abs(rd.TimeSynch - _tstartRecord)<2*_dt_) {
            //    centerMass0 = rd.Body.Vec3D;
            //    OXAxis0 = rd.Body.WorldTransformRot * Vector3D.XAxis;
            //}
            Results["Скорость Y, см/с"].Add(rd.TimeSynch, rd.Body.Vel.Y*100);
            Results["Положение цм от изн X, мм"].Add(rd.TimeSynch, (rd.Body.X - centerMass0.X)/100);
            Results["Положение цм от изн Y, мм"].Add(rd.TimeSynch, (rd.Body.Y - centerMass0.Y)/100);
            Results["Положение цм от изн Z, мм"].Add(rd.TimeSynch, (rd.Body.Z - centerMass0.Z)/100);
            Results["Положение цм от изн всего, мм"].Add(rd.TimeSynch, (rd.Body.Vec3D - centerMass0).GetLength()/100);
            Results["Перегрузка цм X, g"].Add(rd.TimeSynch, rd.Body.Acc.X/9.81);
            Results["Перегрузка цм Y, g"].Add(rd.TimeSynch, rd.Body.Acc.Y / 9.81);
            Results["Перегрузка цм Z, g"].Add(rd.TimeSynch, rd.Body.Acc.Z / 9.81);
            Results["Перегрузка цм всего, g"].Add(rd.TimeSynch, rd.Body.Acc.Vec3D.GetLength() / 9.81);
            var xaxis = rd.Body.WorldTransformRot * Vector3D.XAxis;
            Results["Ось ОХ X"].Add(rd.TimeSynch, xaxis.X);
            Results["Ось ОХ Y"].Add(rd.TimeSynch, xaxis.Y);
            Results["Ось ОХ Z"].Add(rd.TimeSynch, xaxis.Z);
            Results["Отклонение от изн положения, гр"].Add(rd.TimeSynch, Acos(OXAxis0* xaxis)*180/PI);

        }


        public override void Start(string exFilePath = defexFilePath, string solFilePath = defsolFilePath) {
            try {
                var pr = GetRD();
                var v0 = pr.Rebuild(pr.TimeSynch);
                var dt = _dt_;
                var v00 = Ode.MidPoint(pr.TimeSynch, v0, pr.f, dt).SolveTo(PrsShoot.ImpulseT0-100* 0.000001).Last();

                PrepDict(pr);
                var solutions = Ode.MidPoint(v00.T, v00.X, pr.f, 0.000001).SolveTo(PrsShoot.ImpulseT0+0.03).WithStep(0.00001);
                foreach (var sol in solutions) {
                        FillResults(pr);
                        SolPoints.Add(sol);
                    
                    if (StopFunc(pr) != "") {
                        Prs.ResultIndex = StopFunc(pr);
                        break;
                    }
                }
                if(StopFunc(pr) == "") {
                    var v000 = SolPoints.Last();
                    solutions = Ode.MidPoint(v000.T, v000.X, pr.f, dt).WithStep(_dt_out_);
                    foreach (var sol in solutions) {
                        FillResults(pr);
                        SolPoints.Add(sol);

                        if (StopFunc(pr) != "") {
                            Prs.ResultIndex = StopFunc(pr);
                            break;
                        }
                    }
                }

                SaveResultsToFile(exFilePath, solFilePath);

            } finally {

            }

        }
    }

     
}
