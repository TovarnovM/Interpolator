using Microsoft.Research.Oslo;
using MyRandomGenerator;
using Sharp3D.Math.Core;
using SimpleIntegrator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static System.Math;

namespace RobotIM.IM {
    public class Experim1Info {
        public int id { get; set; } = 0;
        public int I_ind { get; set; } = 0;
        public int J_ind { get; set; } = 0;
        public string ResultIndex { get; set; } = "";
        public string Name { get; set; } = "Round1";
        public double Prob { get; set; } = 0d;
        public double RobotH { get; set; } = 3d;
        /// <summary>
        /// в градусах
        /// </summary>
        public double TrgTetta0 { get; set; } = 60d;
        public double TrgR0 { get; set; } = 30d;
        public string TrgType { get; set; } = "fire";
        /// <summary>
        /// в минутах
        /// </summary>
        public double Delta_Tetta { get; set; } = 60;
        /// <summary>
        /// в минутах
        /// </summary>
        public double Delta_Alpha { get; set; } = 60;
        /// <summary>
        /// кг
        /// </summary>
        public double Mass { get; set; } = 0.01;
        /// <summary>
        /// в процентах
        /// </summary>
        public double Mass_delta { get; set; } = 5;
        public int ZalpCount { get; set; } = 1;
        public int ZalpCount_delta { get; set; } = 0;
        /// <summary>
        /// м^3/кг
        /// </summary>
        public double BallCoeff { get; set; } = 10d/10000d;
        /// <summary>
        /// в процентах
        /// </summary>
        public double BallCoeff_delta { get; set; } = 20;
        /// <summary>
        /// м/c
        /// </summary>
        public double V0 { get; set; } = 100;
        /// <summary>
        /// в процентах
        /// </summary>
        public double V0_delta { get; set; } = 20;
        public int N_rounds { get; set; } = 300;
        public double Integr_timeMax { get; set; } = 2;
        public double Integr_dt { get; set; } = 1E-4;
        public Experim1Info():this(1) {

        }
        public Experim1Info(int index = 1) {
            if(index == 1) {
                Name = "ДРО";
                Mass = 0.015;
                V0 = 280;
                V0_delta = 10;
                BallCoeff = 10d / 1000d;
                BallCoeff_delta = 1;
            }
            if (index == 2) {
                Name = "СП4";
                Mass = 0.0095;
                V0 = 200;
                V0_delta = 5;
                BallCoeff = 6.6d / 1000d;
                BallCoeff_delta = 1;
                TrgType = "2";
            }
            if (index == 3) {
                Name = "ПББС";
                Mass = 0.006;
                V0 = 290;
                V0_delta = 5;
                BallCoeff = 12.5d / 1000d;
                BallCoeff_delta = 1;
                TrgType = "3";
            }
        }
    }
    public class Experim1 {
        public Experim1Info Info { get; set; } = new Experim1Info();
        public Target Targ { get; set; }
        public bool flag1_foo { get; private set; } = false;
        public bool flag2_foo { get; private set; } = false;

        public Vector3D surf_p00, surf_n0, robot_p0, robot_V0_dir;
        protected Matrix4D worldTransform_1 = Matrix4D.Identity, worldTransform = Matrix4D.Identity;

        public void PrepAll() {
            Targ = Target.Factory(Info.TrgType);

            surf_p00 = (Vector3D.XAxis * Cos(Info.TrgTetta0 * PI / 180) - Vector3D.ZAxis * Sin(Info.TrgTetta0 * PI / 180))*Info.TrgR0;
            surf_n0 = -surf_p00.Norm;

            var q = QuaternionD.FromTwoVectors(Vector3D.ZAxis, surf_n0);
            if((Vector3D.ZAxis & surf_n0).GetLengthSquared() < 1E-11) {
                q = QuaternionD.Identity;
            }
            q.Normalize();
            worldTransform = QuaternionD.QuaternionToMatrix(q);
            worldTransform.Col4 = surf_p00;
            worldTransform_1 = Matrix4D.Inverse(worldTransform);

            robot_p0 = new Vector3D(0, Info.RobotH, 0);
            var aimP3 = worldTransform * (new Vector3D(Targ.AimSurf.AimPoint.X, Targ.AimSurf.AimPoint.Y, 0));
            robot_V0_dir = (aimP3 - robot_p0).Norm;
        }
        Vector3D GetV0Rnd() {
            var tetta_vec = (robot_V0_dir - (robot_V0_dir * Vector3D.ZAxis)* Vector3D.ZAxis).Norm;
            var tetta_grad = Acos(tetta_vec * Vector3D.YAxis)*180/PI;
            var tetta_smooth = _rnd.GetNorm(tetta_grad, Info.Delta_Tetta / 3 / 60);
            var q1 = QuaternionD.FromAxisAngle(-Vector3D.ZAxis, tetta_smooth * PI / 180);
            var tetta_vec_smooth = (q1 * Vector3D.YAxis).Norm;

            var alpha_grad = Acos(tetta_vec * robot_V0_dir) * 180 / PI;
            var alpha_smooth = _rnd.GetNorm(alpha_grad, Info.Delta_Alpha / 3 / 60);
            var ttt = tetta_vec & robot_V0_dir;
            var q2 = QuaternionD.FromAxisAngle((tetta_vec & robot_V0_dir).Norm, alpha_smooth * PI / 180);
            var V0_dir_smooth = (q2 * tetta_vec_smooth).Norm;
            if(V0_dir_smooth*surf_n0 > 0) {
                V0_dir_smooth = robot_V0_dir;
                V0_dir_smooth.X = GetRand(V0_dir_smooth.X, 10);
                V0_dir_smooth.Y = GetRand(V0_dir_smooth.Y, 10);
                V0_dir_smooth.Z = GetRand(V0_dir_smooth.Z, 10);
                V0_dir_smooth.Normalize();
                flag1_foo = true;
            }
            var v0_mod_smooth = GetRand(Info.V0, Info.V0_delta);
            return V0_dir_smooth* v0_mod_smooth;
        }
        bool CrossSurf(Vector3D p1, Vector3D p2, out Vector2D cross_p) {
            var p10 = p1 - surf_p00;
            var p20 = p2 - surf_p00;
            cross_p = Vector2D.Zero;
            if ((surf_n0*p10) *(surf_n0*p20) > 0) {
                return false;
            }           
            var crossP3 = GetIntersect(new Line3D(p1, p2));
            var crossP2 = worldTransform_1 * crossP3;
            cross_p = new Vector2D(crossP2.X, crossP2.Y);
            return true;

        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        Vector3D GetIntersect(Line3D line) {
            var u = line.u;
            var dot = surf_n0 * u;
            if (Math.Abs(dot) > 1E-8) {
                var w = line.p0 - surf_p00;
                var fac = -(surf_n0 * w) / dot;
                return line.p0 + (u * fac);
            }
            return Vector3D.Zero;

        }
        public void Start() {
            PrepAll();
            for (int i = 0; i < Info.N_rounds; i++) {
                var zalpC = GetRand(Info.ZalpCount, Info.ZalpCount_delta);
                var prob_one = 0d;
                for (int j = 0; j < zalpC; j++) {
                    prob_one += GetOneRound();
                }
                Info.Prob += prob_one;
            }
            Info.Prob /= Info.N_rounds;
            string s = flag1_foo ? "_Вектора(_" : "";
            s += flag2_foo ? "_Время(_" : "";
            Info.ResultIndex = "Всё ок" + s;
        }
        public double GetOneRound() {
            var fa = new FlyAway();
            fa.Mass.Value = GetRand(Info.Mass, Info.Mass_delta);
            fa.ballKoeff = GetRand(Info.BallCoeff, Info.BallCoeff_delta);
            fa.Y = Info.RobotH;
            fa.Vel.Vec3D = GetV0Rnd();
            var sol = Ode.RK45(0, fa.Rebuild(), fa.f, Info.Integr_dt).SolveTo(Info.Integr_timeMax);
            var p1 = fa.Vec3D;
            foreach (var sp in sol) {
                var p2 = fa.Vec3D;
                if(CrossSurf(p1,p2,out Vector2D cp)){
                    var prob = Targ.AimSurf.getDamage(cp);
                    Targ.Hits.Add((prob, cp));
                    return prob;
                }
            }
            flag2_foo = true;
            return 0;
            
        }
        MyRandom _rnd = new MyRandom();
        double GetRand(double mo, double deltaPerc) {
            return _rnd.GetNorm(mo, mo * deltaPerc / 300);
        }
        class Line3D {
            public Vector3D p1, p0;
            public Vector3D u {
                get {
                    return (p1 - p0).Norm;
                }
            }
            public Line3D(Vector3D p0, Vector3D p1) {
                this.p1 = p1;
                this.p0 = p0;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool Equal(Line3D l) {
                var cr1 = u & l.u;
                if (cr1.GetLengthSquared() > 1E-9)
                    return false;
                var cr2 = (p0 - l.p0) & u;
                if (cr2.GetLengthSquared() > 1E-9)
                    return false;
                return true;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public double GetLength() {
                return (p0 - p1).GetLength();
            }
        }
        class FlyAway: MaterialPointNewton {
            public double ballKoeff { get; set; }
            public double ro_vozd { get; set; } = 1.204;
            Force F_Cx;
            public FlyAway() {
                Name = "FlyAway";
                F_Cx = Force.GetForceCentered(0, Vector3D.XAxis);
                F_Cx.SynchMeBefore += CalcCx;
                AddForce(F_Cx);
                AddGForce();
            }
            void CalcCx(double t) {
                var dirF = -Vel.Vec3D.Norm;
                var V2 = Vel.Vec3D.GetLengthSquared();
                var modulF = ro_vozd *V2 *0.5 * ballKoeff * Mass.Value;
                F_Cx.Value = modulF;
                F_Cx.Direction.Vec3D = dirF;
            }
        }
    }
}
