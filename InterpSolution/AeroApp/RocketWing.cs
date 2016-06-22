using System;

namespace RocketAero {
    public class RocketWing_Geom {
        public double B0 { get; set; } = 0.2;
        public double B1 { get; set; } = 0.05;
        public double L { get; set; } = 0.4;
        public double D { get; set; } = 0.2;
        public double C_shtr { get; set; } = 0.01;
        private double hi0 = 45;
        public double Hi0 {
            get { return hi0; }
            set {
                hi0 = value;
                tgHi0 = Math.Tan(AeroGraphs.PIdiv180 * hi0);
            }
        }
        private double tgHi0 = 0.5;
        public double TgHi0 {
            get { return tgHi0; }
            set {
                tgHi0 = value;
                hi0 = Math.Atan(tgHi0) * AeroGraphs._180divPI;
            }
        }

        public double S { get { return 0.5 * L * (B1 + B0); } }
        public double Lmb { get { return L * L / S; } }
        public double Etta {
            get {
                return B1 < 0.001 || B0 / B1 > 1000 ? 1000 : B0 / B1;
            }
        }

        public double X_ct {
            get {
                var x1 = GetX(L * 0.5);
                return ((B1 + 2 * x1) * B1 + (B1 + B0 + x1) * B0) / (3.0 * B0 + 3.0 * B1);
            }
        }
        //Консоли
        public double B_b { get { return GetB(0.5 * D); } }
        public double L_k { get { return L - D; } }
        public double S_k { get { return 0.5 * L_k * (B1 + B_b); } }
        public double Lmb_k { get { return L_k * L_k / S_k; } }
        public double Etta_k {
            get {
                return B1 < 0.001 || B_b / B1 > 1000 ? 1000 : B_b / B1;
            }
        }
        public double X_b {
            get {
                return GetX(D * 0.5);
            }
        }
        public double X_ct_k {
            get {
                var x1 = GetX(L * 0.5) - X_b;
                return X_b + ((B1 + 2 * x1) * B1 + (B1 + B_b + x1) * B_b) / (3.0 * B_b + 3.0 * B1);
            }
        }
        //САХ
        public double Z_a {
            get {
                return L * (0.5 * B0 + B1) / (3 * (B0 + B1));
            }
        }
        public double B_a {
            get {
                return GetB(Z_a);
            }
        }
        public double X_a {
            get {
                return GetX(Z_a);
            }
        }
        //САХ консолей
        public double Z_ak {
            get {
                return 0.5 * D + L_k * (0.5 * B_b + B1) / (3 * (B_b + B1));
            }
        }
        public double B_ak {
            get {
                return GetB(Z_ak);
            }
        }
        public double X_ak {
            get {
                return GetX(Z_ak);
            }
        }

        public double GetB(double z) => B0 + (B1 - B0) * z * 2 / L;
        public double GetTgHiM(double m) => tgHi0 - 4 * m * (Etta - 1) / (Lmb * (1 + Etta));
        public double GetX(double z) => z * tgHi0;

        public IWingProfile Profile { get; set; } = new WingProf_romb();

    }
    public class RocketWing : RocketWing_Geom, IRocket_Coeff {
        public AeroGraphs AeroGr { get; set; } = null;
        public double X_t { get; set; } = 0.382;
        public RocketWing(AeroGraphs ag) {
            AeroGr = ag;
        }
        public double GetCy1a(double mach) {
            if (AeroGr == null)
                return 0.0;
            return Lmb_k * AeroGr.GetV("3_5", Lmb_k * AeroGraphs.M2min1(mach), Lmb_k * Math.Pow(C_shtr, 1.0 / 3.0), Lmb_k * GetTgHiM(0.5));
        }
        public double Cx_tr(double mach, double x_t, double v, double a_m) {
            var vel = mach * a_m;
            var re = vel * B_a / v;
            var _2CfM0 = AeroGr.GetV("4_2", re, x_t);
            var etta = AeroGr.GetV("4_3", mach, x_t);

            var _2Cf = _2CfM0 * etta;

            var etta_c = AeroGr.GetV("4_28", C_shtr, x_t);
            return _2Cf * etta_c;
        }
        public double Cx_tr(double mach) {
            return Cx_tr(mach, 0, 1.51E-5, 340.3);
        }
        public double Cx_v(double mach) {
            if (mach <= 1.0)
                return 0.0;
            double lmM21 = Lmb_k * AeroGraphs.M2min1(mach);
            double lmbC3 = Lmb_k * Math.Pow(C_shtr, 1.0 / 3.0);
            double lmbTgHi = Lmb_k * GetTgHiM(Profile.B_c_shtr);
            double cx_romb = AeroGr.GetV("4_30", lmM21, lmbC3, lmbTgHi, Etta_k) * Lmb_k * C_shtr * C_shtr;
            double fi = AeroGr.GetV("4_32", AeroGraphs.M2min1(mach) - GetTgHiM(Profile.B_c_shtr));
            return cx_romb * (1 + fi * (Profile.K - 1));
        }
        public double Cx01(double mach) {
            return Cx_tr(mach) + Cx_v(mach);
        }
        public double Cx01(double mach, double x_t, double v, double a_m) {
            return Cx_tr(mach, x_t, v, a_m) + Cx_v(mach);
        }
        public double Z_v_shtr(double mach) {
            var p1 = Lmb_k * AeroGraphs.M2min1(mach);
            var p2 = Lmb_k * GetTgHiM(0.5);
            var p3 = Etta_k;
            return AeroGr.GetV("3_16", p1, p2, p3);
        }
        public double Cn(double mach, double alphaEff) {
            var cy1a = GetCy1a(mach);
            var sinAlpha = Math.Sin(alphaEff * AeroGraphs.PIdiv180);
            var cosAlpha = Math.Cos(alphaEff * AeroGraphs.PIdiv180);
            double A = Get_A(mach);
            return AeroGraphs._180divPI * cy1a * sinAlpha * cosAlpha + A * sinAlpha * sinAlpha * Math.Sign(alphaEff);
        }
        public double Get_A(double mach) {
            var cy1a = GetCy1a(mach);
            double A;
            if (mach <= 1.1) {
                A = AeroGr.GetV("3_35_M1", cy1a);
            }
            else if (mach >= 2.0) {
                A = AeroGr.GetV("3_35_M2", cy1a, 1.0 / Etta_k);
            }
            else {
                var A1 = AeroGr.GetV("3_35_M1", cy1a);
                var A2 = AeroGr.GetV("3_35_M2", cy1a, 1.0 / Etta_k);
                A = A1 + (mach - 1.1) * (A2 - A1) / (2.0 - 1.1);
            }
            return A;
        }

    }

    public interface IWingProfile {
        double K { get; }
        /// <summary>
        /// B_c_shtr - относитальная длина расположения линии макс толщин
        /// </summary>
        double B_c_shtr { get; }
    }
    /// <summary>
    /// Таблица 4.2 ромб
    /// </summary>
    public class WingProf_romb : IWingProfile {
        public double B_c_shtr {
            get {
                return 0.5;
            }
        }

        public double K {
            get {
                return 1.0;
            }
        }
    }
    /// <summary>
    /// Таблица 4.2 четырехугольник
    /// </summary>
    public class WingProf_4 : IWingProfile {
        public double K {
            get {
                return 0.25 / (Xc_shtr * (1 - Xc_shtr));
            }
        }
        private double _xc_str;
        public double Xc_shtr {
            get {
                return _xc_str;
            }
            set {
                _xc_str = value < 0 ? 0 :
                            value > 1 ? 1 :
                            value;
            }
        }
        public double B_c_shtr {
            get {
                return _xc_str;
            }
        }
        /// <summary>
        /// Таблица 4.2 четырехугольник
        /// x_c_shtr = b / x_c
        /// </summary>
        /// <param name="x_c_shtr">b / x_c</param>
        public WingProf_4(double x_c_shtr) {
            Xc_shtr = x_c_shtr;
        }
    }
    /// <summary>
    /// Таблица 4.2 шестиугольный
    /// </summary>
    public class WingProf_6 : WingProf_4 {
        public new double B_c_shtr {
            get {
                return 0.5 - 0.5 * Xc_shtr;
            }
        }
        public new double K {
            get {
                return 1.0 / (1 - Xc_shtr);
            }
        }
        /// <summary>
        /// x_c_shtr = a/b
        /// </summary>
        /// <param name="x_c_shtr"></param>
        public WingProf_6(double x_c_shtr) : base(x_c_shtr) { }
    }
    /// <summary>
    /// Таблица 4.2 синусоидальный
    /// </summary>
    public class WingProf_sin : IWingProfile {
        public double B_c_shtr {
            get {
                return 0.5;
            }
        }
        /// <summary>
        /// PI^2 / 8
        /// </summary>
        public double K {
            get {
                return 1.23370055; //PI^2 / 8
            }
        }
    }
    /// <summary>
    /// Таблица 4.2 окружности или параболы
    /// </summary>
    public class WingProf_okr : IWingProfile {
        public double B_c_shtr {
            get {
                return 0.5;
            }
        }

        /// <summary>
        /// 4 / 3
        /// </summary>
        public double K {
            get {
                return 4.0 / 3.0;
            }
        }
    }
    /// <summary>
    /// Таблица 4.2 клиновидный
    /// </summary>
    public class WingProf_klin : IWingProfile {
        public double B_c_shtr {
            get {
                return 1.0;
            }
        }

        public double K {
            get {
                return 0.25;
            }
        }
    }

    public class WingOrient {
        public double Gamma { get; set; } = 0.0;
        public double Delta { get; set; } = 0.0;
        /// <summary>
        /// Количество консолей для поределения Cx
        /// 2 - для варианта + или X
        /// 1 - для варианта -O-
        /// </summary>
        public int N_console_4Cx { get; set; } = 2;
        public RocketWing Wing { get; set; }
        public double X { get; set; } = 0.0;
        public double X_povor_otn { get; set; } = 0.5;
        /// <summary>
        /// рис 3.23
        /// </summary>
        public double Hi_rulei { get; set; } = 0.0;
        public WingOrient(RocketWing wing) {
            Wing = wing;
        }
    }
}
