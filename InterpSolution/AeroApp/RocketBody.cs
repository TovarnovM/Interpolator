using System;

namespace RocketAero {
    /// <summary>
    /// геометрия Фюзеляжа ракеты обобщенныая
    /// </summary>
    public class RocketBody_Geom {

        public double L { get; set; } = 1;
        public double L_nos { get; set; } = 0.2;
        public double L_korm { get; set; } = 0.0;
        public double L_cyl { get { return L - L_nos - L_korm; } }
        public double D { get; set; } = 0.15;
        public double D1 { get; set; } = 0.15;

        public double Lmb_nos {
            get {
                return L_nos / D;
            }
        }
        public double Lmb_cyl {
            get {
                return L_cyl / D;
            }
        }
        public double Lmb_korm {
            get {
                return L_korm / D;
            }
        }
        /// <summary>
        /// Площадь миделя
        /// </summary>
        public double S_mid {
            get {
                return Math.PI * D * D * 0.25;
            }
        }
        public double S_dno {
            get {
                return Math.PI * D1 * D1 * 0.25;
            }
        }
        /// <summary>
        /// Площадь корпуса без дна)
        /// </summary>
        public double S_fuse {
            get {
                return Nose.GetF_nos(D, L_nos) + Math.PI * D * L_cyl + Math.PI * 0.5 * (D + D1) * Math.Sqrt(0.25 * D * D + L_korm * L_korm);
            }
        }
        public double S_bok {
            get {
                return L_cyl * D + 0.5 * L_korm * (D + D1) + 0.5 * L_nos * D;
            }
        }
        public double X_ct {
            get {
                return (L_nos * D * L_nos / 3.0
                     + L_cyl * D * (L_nos + 0.5 * L_cyl)
                     + 0.5 * L_korm * (D + D1) * (L_nos + L_cyl + L_korm * (2.0 * D1 + D) / (3.0 * D + 3.0 * D1)))
                     / S_bok;
            }
        }
        public IRocketBody_nos Nose { get; set; } = new RocketNos_ConePlusCyl();
        public RocketBody_Geom() {

        }
        public double W_nos {
            get {
                return Nose.GetW_nos(D, L_nos);
            }
        }
        public double W_korm {
            get {
                return Math.PI * L_korm * (D * D + D * D1 + D1 * D1) * 0.25 / 3.0;
            }
        }
    }
    public class RocketBody : RocketBody_Geom, IRocket_Coeff {
        public AeroGraphs AeroGr { get; set; } = null;
        public double X_t_abs { get; set; } = 0.5;
        public double X_t {
            get {
                var result = X_t_abs / L;
                return result > 1.0 ? 1.0 : result;
            }
        }

        public virtual double GetCy1a(double mach) {
            if (AeroGr == null)
                return 0.0;
            return GetCy1a_nosCyl(mach) + GetCy1a_korm();
        }
        public double GetCy1a_korm() {
            double cy1a_korm = 0.0;
            if (D1 != D && L_korm != 0) {
                if (D1 > D)
                    cy1a_korm = 0.8 * (D1 * D1 / (D * D) - 1) * 2 * AeroGraphs.PIdiv180 * Math.Cos(2 * Math.Atan(0.5 * (D1 - D) / L_korm));
                else
                    cy1a_korm = -0.2 * (1 - D1 * D1 / (D * D)) * 2 * AeroGraphs.PIdiv180;
            }
            return cy1a_korm;
        }
        public double GetCy1a_nosCyl(double mach) {
            return Nose.GetCy1a_nos(AeroGr, mach, Lmb_nos, Lmb_cyl);
        }
        public RocketBody(AeroGraphs ag) {
            AeroGr = ag;
        }
        /// <summary>
        /// Cx трения 
        /// </summary>
        /// <param name="mach">мах</param>
        /// <param name="x_t">относитальная точка  перехода лам погр.слоя в турбул</param>
        /// <param name="v">кинематическая вязкость среды</param>
        /// <param name="a_m">местная скорость звука</param>
        /// <returns></returns>
        public double Cx_tr(double mach, double x_t, double v, double a_m) {
            var vel = mach * a_m;
            var re = vel * L / v;
            var _2CfM0 = AeroGr.GetV("4_2", re, x_t);
            var etta = AeroGr.GetV("4_3", mach, x_t);
            return 0.5 * _2CfM0 * etta * S_fuse / S_mid;
        }
        public double Cx_tr(double mach) {
            return Cx_tr(mach, 0, 1.51E-5, 340.3);
        }
        public double Cx_nose(double mach) {
            return Nose.GetCx_nos(AeroGr, mach, Lmb_nos);
        }
        public double Cx_korm(double mach) {
            if (D1 == D || L_korm == 0)
                return 0;
            double etta = D1 / D;
            return AeroGr.GetV("4_24", mach, Lmb_korm, etta);
            //double S_dno_shtr = D1 * D1 / (D * D);
            //double expr = D1 < D ?
            //              Math.Sqrt(1 - S_dno_shtr) :
            //              Math.Sqrt(S_dno_shtr * (S_dno_shtr - 1));
            //double sinBetta = Math.Sin(Math.Atan(Math.Abs((D1 - D) * 0.5) / L_korm));
            //double sinBettaSqr = sinBetta* sinBetta;
            //return expr * (2.09 * sinBettaSqr + 0.19 * sinBettaSqr / Math.Sqrt(AeroGraphs.M2min1(mach)));
        }
        public double Cx_dno(double mach) {
            return 0;
        }
        public double Cx0(double mach) {
            return Cx_tr(mach) + Cx_nose(mach) + Cx_korm(mach) + Cx_dno(mach);
        }
        public double Cx0(double mach, double x_t, double v, double a_m) {
            return Cx_tr(mach, x_t, v, a_m) + Cx_nose(mach) + Cx_korm(mach) + Cx_dno(mach);
        }
        //public double Cx_i(double mach, double alpha, WingOrient w_I, WingOrient w_II)
        //{
        //    var sinAlpha = Math.Sin(alpha * AeroGraphs.PIdiv180);
        //    var cosAlpha = Math.Cos(alpha * AeroGraphs.PIdiv180);
        //    var m21_lmb = AeroGraphs.M2min1(mach) / Lmb_nos;
        //    var type = Nose is RocketNos_ConePlusCyl ? 0.0
        //             : Nose is RocketNos_OzjPlusCyl ? 1.0
        //             : Nose is RocketNos_SpherePlusCyl ? 0.5
        //             : Nose is RocketNos_Compose && (Nose as RocketNos_Compose).Main is RocketNos_ConePlusCyl ? 0.0//(Nose as RocketNos_Compose).Rshtrih
        //             : Nose is RocketNos_Compose && (Nose as RocketNos_Compose).Main is RocketNos_OzjPlusCyl ? 1.0//1 - (Nose as RocketNos_Compose).Rshtrih
        //             : 0.5;
        //    var ksi = AeroGr.GetV("4_40", m21_lmb, type);
        //    return Cy1(mach, alpha,w_I, w_II) * sinAlpha + 2.0 * ksi * sinAlpha * sinAlpha * cosAlpha;
        //}
        //public double Cy1(double mach, double alpha, WingOrient w_I, WingOrient w_II)
        //{
        //    var cy1fa = GetCy1a(mach);
        //    var hi_a = Get_hi_a(mach, alpha);
        //    var sMinus = w_I.Wing.B_ak * w_I.Wing.D + w_II.Wing.B_ak * w_II.Wing.D;
        //    var s_bok = S_bok - sMinus;
        //    var sinAlpha = Math.Sin(alpha * AeroGraphs.PIdiv180);
        //    var cosAlpha = Math.Cos(alpha * AeroGraphs.PIdiv180);
        //    var cx_cyl = AeroGr.GetV("3_32", mach * Math.Abs(sinAlpha));
        //    var expr1 = AeroGraphs._180divPI * cy1fa * hi_a * sinAlpha * cosAlpha;
        //    var expr2 = s_bok * cx_cyl * sinAlpha * sinAlpha * Math.Sign(alpha) / S_mid;
        //    return expr1 + expr2;
        //}

    }
}
