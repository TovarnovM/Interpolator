using Interpolator;
using System;

namespace RocketAero {
    public class Rocket {
        private double sss = Double.NaN;
        public AeroGraphs AeroGr { get; set; } = null;
        public double Aero_v { get; set; } = 1.52E-5;
        public double Aero_a { get; set; } = 340.3;
        private double _alpha = 0.0;
        private double _sinAlpha = 0.0;
        private double _cosAlpha = 1.0;
        public double Alpha {
            get {
                return _alpha;
            }
            set {
                _alpha = value > 90.0 ?
                            90.0 :
                         value < -90.0 ?
                            -90.0 :
                         value;
                _sinAlpha = Math.Sin(_alpha * AeroGraphs.PIdiv180);
                _cosAlpha = Math.Cos(_alpha * AeroGraphs.PIdiv180);
            }

        }
        public double SinAlpha {
            get {
                return _sinAlpha;
            }
        }
        public double CosAlpha {
            get {
                return _cosAlpha;
            }
        }
        /// <summary>
        /// мах
        /// </summary>
        public double M { get; set; } = 0.0;

        public double Cy {
            get {
                return Cy_f + Cy_I * kt_I * S_I_shtr + Cy_II * kt_II * S_II_shtr;
            }
        }
        public double Cy1 {
            get {
                return Cy1_f + Cy1_I * kt_I * S_I_shtr + Cy1_II * kt_II * S_II_shtr;
            }
        }
        public double GetCy(double mach, double alpha, double delta_I = 0.0, double delta_II = 0.0, bool changeState = true) {
            var oldAlpha = Alpha;
            var oldMach = M;
            var oldDelta_I = W_I.Delta;
            var oldDelta_II = W_II.Delta;

            Alpha = alpha;
            M = mach;
            W_I.Delta = delta_I;
            W_II.Delta = delta_II;

            var result = Cy;

            if (!changeState) {
                Alpha = oldAlpha;
                M = oldMach;
                W_I.Delta = oldDelta_I;
                W_II.Delta = oldDelta_II;
            }
            return result;
        }
        public double Cy1a {
            get {
                var cy1a_f = Cy1a_f;
                var cy1a_I = Cy1a_I;
                var cy1a_II = Cy1a_II;
                var sI_sm = S_I_shtr;
                var sII_sm = S_II_shtr;
                return cy1a_f + cy1a_I * kt_I * sI_sm + cy1a_II * kt_II * sII_sm;
            }
        }

        public double Cx0 {
            get {
                var cx0f = Cx0_f;
                var cx0_I = W_I.Wing.Cx01(M_I);
                var cx0_II = W_II.Wing.Cx01(M_II);
                var sI_sm = W_I.Wing.S_k * W_I.N_console_4Cx / Body.S_mid;
                var sII_sm = W_II.Wing.S_k * W_II.N_console_4Cx / Body.S_mid;
                return 1.05 * (cx0f + cx0_I * kt_I * sI_sm + cx0_II * kt_II * sII_sm);
            }
        }
        public double Cxi {
            get {
                var cxi_f = Cxi_f;
                var cxi_I = Cxi_I;
                var cxi_II = Cxi_II;
                var sI_sm = S_I_shtr;
                var sII_sm = S_II_shtr;
                return cxi_f + cxi_I * kt_I * sI_sm + cxi_II * kt_II * sII_sm;
            }
        }
        public double Cx {
            get {
                return Cx0 + Cxi;
            }
        }
        public double GetCx(double mach, double alpha, double delta_I = 0.0, double delta_II = 0.0, bool changeState = true) {
            var oldAlpha = Alpha;
            var oldMach = M;
            var oldDelta_I = W_I.Delta;
            var oldDelta_II = W_II.Delta;

            Alpha = alpha;
            M = mach;
            W_I.Delta = delta_I;
            W_II.Delta = delta_II;

            var result = Cx;

            if (!changeState) {
                Alpha = oldAlpha;
                M = oldMach;
                W_I.Delta = oldDelta_I;
                W_II.Delta = oldDelta_II;
            }
            return result;
        }

        public double X_d {
            get {
                return Cy1 != 0.0 ?
                      (Cy1_f * X_d_f + Cy1_I * X_d_I * S_I_shtr * kt_I + Cy1_II * X_d_II * S_II_shtr * kt_II) / Cy1 :
                      X_Fa;
            }
        }
        public double X_Fa {
            get {
                return (Cy1a_f * X_Fa_f + Cy1a_I * X_Fa_I * S_I_shtr * kt_I + Cy1a_II * X_Fa_II * S_II_shtr * kt_II) / Cy1a;
            }
        }
        public double X_ct { get; set; } = 0.5;

        public double Mz_omz {
            get {
                var bakI = W_I.Wing.B_ak / Body.L;
                var bakII = W_II.Wing.B_ak / Body.L;
                return Mz_omz_f
                     + Mz_omz_I * S_I_shtr * bakI * bakI * Math.Sqrt(kt_I)
                     + Mz_omz_II * S_II_shtr * bakII * bakII * Math.Sqrt(kt_II);
            }
        }

        public double M_I {
            get {
                return M * Math.Sqrt(kt_I);
            }
        }
        public double M_II {
            get {
                return M * Math.Sqrt(kt_II);
            }
        }
        public double kt_I {
            get {
                return Get_kt_I_shtr(M, Alpha);
            }
        }
        public double kt_II {
            get {
                return kt_I * Get_kt_II_shtr(M_I, Alpha);
            }
        }

        public double kaa_I {
            get {
                return Get_kaa(W_I, M_I, Aero_v, Aero_a);
            }
        }
        public double kaa_II {
            get {
                return Get_kaa(W_II, M_II, Aero_v, Aero_a);
            }
        }
        public double Kaa_I {
            get {
                return Get_Kaa(W_I, M_I, Aero_v, Aero_a);
            }
        }
        public double Kaa_II {
            get {
                return Get_Kaa(W_II, M_II, Aero_v, Aero_a);
            }
        }

        public double kd0_I {
            get {
                return Get_kd0(W_I, M_I, Aero_v, Aero_a);
            }
        }
        public double kd0_II {
            get {
                return Get_kd0(W_II, M_II, Aero_v, Aero_a);
            }
        }
        public double Kd0_I {
            get {
                return Get_Kd0(W_I, M_I, Aero_v, Aero_a);
            }
        }
        public double Kd0_II {
            get {
                return Get_Kd0(W_II, M_II, Aero_v, Aero_a);
            }
        }

        public double S_I_shtr {
            get {
                return W_I.Wing.S_k / Body.S_mid;
            }
        }
        public double S_II_shtr {
            get {
                return W_II.Wing.S_k / Body.S_mid;
            }
        }

        public double Cy1a_f {
            get {
                return Body.GetCy1a(M);
            }
        }
        public double Cy1_f {
            get {
                if (Alpha == 0.0)
                    return 0.0;
                var cy1fa = Cy1a_f;
                var hi_a = Hi_a;
                var sMinus = W_I.Wing.B_ak * W_I.Wing.D + W_II.Wing.B_ak * W_II.Wing.D;
                var s_bok = Body.S_bok - sMinus;
                var cx_cyl = Cx_cyl_star;
                var expr1 = AeroGraphs._180divPI * cy1fa * hi_a * SinAlpha * CosAlpha;
                var expr2 = s_bok * cx_cyl * SinAlpha * SinAlpha * Math.Sign(Alpha) / Body.S_mid;
                return expr1 + expr2;
            }
        }
        public double Cy_f {
            get {
                return Cy1_f * CosAlpha - Cx0_f * SinAlpha - SinAlpha * (Cxi_f - Cy1_f * SinAlpha) / CosAlpha;
            }
        }
        public double Cx0_f {
            get {
                return Body.Cx0(M, Body.X_t, Aero_v, Aero_a);
            }
        }
        public double Cxi_f {
            get {
                var m21_lmb = AeroGraphs.M2min1(M) / Body.Lmb_nos;
                var type = Body.Nose is RocketNos_ConePlusCyl ? 0.0
                         : Body.Nose is RocketNos_OzjPlusCyl ? 1.0
                         : Body.Nose is RocketNos_SpherePlusCyl ? 0.5
                         : Body.Nose is RocketNos_Compose && (Body.Nose as RocketNos_Compose).Main is RocketNos_ConePlusCyl ? 0.0//(Nose as RocketNos_Compose).Rshtrih
                         : Body.Nose is RocketNos_Compose && (Body.Nose as RocketNos_Compose).Main is RocketNos_OzjPlusCyl ? 1.0//1 - (Nose as RocketNos_Compose).Rshtrih
                         : 0.5;
                var ksi = AeroGr.GetV("4_40", m21_lmb, type);
                return Cy1_f * SinAlpha + 2.0 * ksi * SinAlpha * SinAlpha * CosAlpha;
            }
        }
        public double X_Fa_f_nosCyl {
            get {
                var x_delt = AeroGr.GetV("5_7", AeroGraphs.M2min1(M) / Body.Lmb_nos, Body.L_cyl / Body.Lmb_nos);
                x_delt *= Body.L_nos;
                return Body.L_nos - Body.W_nos / Body.S_mid + x_delt;
            }
        }
        public double X_Fa_f_korm {
            get {
                if (Body.L_korm == 0.0 || Body.D == Body.D1)
                    return 0.0;
                return Body.L - (Body.S_mid * Body.L_korm - Body.W_korm) / (Body.S_mid - Body.S_dno);

            }
        }
        public double X_Fa_f {
            get {
                var cy1a_f = Cy1a_f;
                var xf_nosCyl = X_Fa_f_nosCyl;
                var cy1a_nosCyl = Body.GetCy1a_nosCyl(M);
                var cy1a_korm = Body.GetCy1a_korm();
                var xf_korm = X_Fa_f_korm;
                return (cy1a_nosCyl * xf_nosCyl + cy1a_korm * xf_korm) / cy1a_f;
            }
        }
        public double X_d_f {
            get {
                var cy1_f = Cy1_f;
                if (cy1_f == 0.0)
                    return X_Fa_f;
                var cy1_af = Cy1a_f;
                var hi_a = Hi_a;
                var x_Fa_f = X_Fa_f;
                var cx_cyl = Cx_cyl_star;

                var ss_I = W_I.Wing.B_b * W_I.Wing.D;
                var ss_II = W_II.Wing.B_b * W_II.Wing.D;
                var xxt_I = W_I.X + 0.5 * W_I.Wing.B_b;
                var xxt_II = W_II.X + 0.5 * W_II.Wing.B_b;

                var s_bok = Body.S_bok - ss_I - ss_II;
                var x_ct = (Body.S_bok * Body.X_ct - ss_I * xxt_I - ss_II * xxt_II) / s_bok;

                var expr1 = AeroGraphs._180divPI * cy1_af * hi_a * SinAlpha * CosAlpha * x_Fa_f;
                var expr2 = s_bok * cx_cyl * SinAlpha * SinAlpha * Math.Sign(Alpha) * x_ct / Body.S_mid;
                return (expr1 + expr2) / cy1_f;
            }
        }
        public double Mz_omz_f {
            get {
                return Get_m_z_omg_z_f(X_ct);
            }
        }

        public double AlphaEff_I {
            get {
                return W_I.Delta != 0.0 ?
                                    kaa_I * Alpha + W_I.Delta * n_I * kd0_I :
                                    kaa_I * Alpha;
            }
        }
        public double A_I {
            get {
                return W_I.Wing.Get_A(M_I);
            }
        }
        public double Cy1a_I {
            get {
                return W_I.Wing.GetCy1a(M_I) * Kaa_I;
            }
        }
        public double CyI_Delta_I {
            get {
                return W_I.Wing.GetCy1a(M_I) * Kd0_I * n_I;
            }
        }
        public double CyII_Delta_I {
            get {
                return -W_II.Wing.GetCy1a(M_II) * Kaa_II * Get_eps_sr_delta();
            }
        }
        public double Cy1_Delta_I {
            get {
                return CyI_Delta_I * W_I.Wing.S_k * kt_I / Body.S_mid +
                       CyII_Delta_I * W_II.Wing.S_k * kt_II / Body.S_mid;
            }
        }
        public double n_I {
            get {
                return Get_n(W_I, M_I);
            }
        }
        public double Cy1_I {
            get {
                var Kaa = Kaa_I;
                var kaa = kaa_I;
                var cn = Cn_I;
                if (W_I.Delta == 0.0)
                    return Kaa * cn / kaa;

                var sinDelta = Math.Sin(W_I.Delta * AeroGraphs.PIdiv180);
                var cosDelta = Math.Cos(W_I.Delta * AeroGraphs.PIdiv180);
                var cx0 = Cx0_I;

                return cosDelta * Kaa * cn / kaa - cx0 * sinDelta;
            }
        }
        public double Cx0_I {
            get {
                return W_I.Wing.Cx01(M_I, W_I.Wing.X_t, Aero_v, Aero_a);
            }
        }
        public double Cx1_I {
            get {
                var cx0 = Cx0_I;
                if (W_I.Delta == 0.0)
                    return cx0;

                var cn = Cn_I;
                var sinDelta = Math.Sin(W_I.Delta * AeroGraphs.PIdiv180);
                var cosDelta = Math.Cos(W_I.Delta * AeroGraphs.PIdiv180);
                return cn * sinDelta + cx0 * cosDelta;
            }
        }
        public double Cn_I {
            get {
                return W_I.Wing.Cn(M_I, AlphaEff_I);
            }
        }
        public double Cy_I {
            get {
                return Cy1_I * CosAlpha - Cx1_I * SinAlpha;
            }
        }
        public double Cxi_I {
            get {
                var Kaa = Kaa_I;
                var kaa = kaa_I;
                var alphaEff = AlphaEff_I;
                var cn = Cn_I;
                var ksi = W_I.Wing.TgHi0 < 0.01
                            ? 0.0
                            : AeroGr.GetV("4_43", AeroGraphs.M2min1(M_I) / W_I.Wing.TgHi0, Math.Abs(alphaEff));
                var lmbd_k = W_I.Wing.Lmb_k;
                var x = lmbd_k * AeroGraphs.M2min1(M_I);
                var y = lmbd_k * W_I.Wing.TgHi0;
                var c_f_shtr = AeroGr.GetV("4_42", x, y);
                c_f_shtr = c_f_shtr / lmbd_k;
                var expr1 = cn * Math.Sin(AeroGraphs.PIdiv180 * (Alpha + W_I.Delta));
                var expr2 = cn * (Kaa - kaa) * SinAlpha * Math.Cos(AeroGraphs.PIdiv180 * W_I.Delta) / kaa;
                var expr3 = cn * ksi * c_f_shtr * Math.Cos(AeroGraphs.PIdiv180 * (Alpha + W_I.Delta));
                return expr1 + expr2 + expr3;
            }
        }
        public double X_Fa_I {
            get {
                return Get_X_Fa_or_delta(W_I, M_I);
            }
        }
        public double X_Fdelta_I {
            get {
                var cy1_delta_I = Cy1_Delta_I;
                var expr1 = CyI_Delta_I * W_I.Wing.S_k * kt_I * Get_X_Fa_or_delta(W_I, M_I, 1) / Body.S_mid;
                var expr2 = CyII_Delta_I * W_II.Wing.S_k * kt_II * Get_X_Fa_or_delta(W_II, M_II, 1) / Body.S_mid;
                return (expr1 + expr2) / cy1_delta_I;
            }
        }
        /// <summary>
        /// ф . 5,63
        /// </summary>
        public double X_F_I {
            get {
                return W_I.Delta == 0.0 ?
                        X_Fa_I :
                        (Kaa_I * Alpha * X_Fa_I + Kd0_I * n_I * W_I.Delta * X_Fdelta_I) / (Kaa_I * Alpha + Kd0_I * n_I * W_I.Delta);
            }
        }
        public double X_d_I {
            get {
                return Get_X_d(1);
            }
        }
        public double Mz_omz_I {
            get {
                return Get_m_z_omg_z_wing(X_ct, 1);
            }
        }


        public double Eps_sr_a {
            get {
                return Get_eps_sr_alpha();
            }
        }
        public double AlphaEff_II {
            get {
                var cn1 = Cn_I;
                var eps_sr_a = Get_eps_sr_alpha();
                //ф. 3,85
                var eps_sr = eps_sr_a * cn1 / (W_I.Wing.GetCy1a(M_I) * kaa_I);

                var alphaEff = kaa_II * (Alpha - eps_sr);
                alphaEff += W_II.Delta != 0.0 ?
                            W_II.Delta * n_II * kd0_II
                          : 0.0;
                return alphaEff;
            }
        }
        public double A_II {
            get {
                return W_II.Wing.Get_A(M_II);
            }
        }
        public double Cy1a_II {
            get {
                return W_II.Wing.GetCy1a(M_II) * Kaa_II * (1.0 - Eps_sr_a);
            }
        }
        public double Cy1_Delta_II {
            get {
                return W_II.Wing.GetCy1a(M_II) * Kd0_II * n_II * W_II.Wing.S_k * kt_II / Body.S_mid;
            }
        }
        public double n_II {
            get {
                return Get_n(W_II, M_II);
            }
        }
        public double X_Fdelta_II {
            get {
                var expr1 = kd0_II * Get_X_Fizkr(W_II, M_II);
                var expr2 = (Kd0_II - kd0_II) * Get_X_Fif(W_II, M_II);
                return (expr1 + expr2) / Kd0_II;
            }
        }
        public double Cy1_II {
            get {
                var Kaa = Kaa_II;
                var kaa = kaa_II;
                var cn = Cn_II;
                if (W_II.Delta == 0.0)
                    return Kaa * cn / kaa;

                var sinDelta = Math.Sin(W_II.Delta * AeroGraphs.PIdiv180);
                var cosDelta = Math.Cos(W_II.Delta * AeroGraphs.PIdiv180);
                var cx0 = W_II.Wing.Cx01(M_II);

                return cosDelta * Kaa * cn / kaa - cx0 * sinDelta;
            }
        }
        public double Cx0_II {
            get {
                return W_II.Wing.Cx01(M_II, W_II.Wing.X_t, Aero_v, Aero_a);
            }
        }
        public double Cx1_II {
            get {
                var cx0 = Cx0_II;
                if (W_II.Delta == 0.0)
                    return cx0;

                var cn = Cn_II;
                var sinDelta = Math.Sin(W_II.Delta * AeroGraphs.PIdiv180);
                var cosDelta = Math.Cos(W_II.Delta * AeroGraphs.PIdiv180);
                return cn * sinDelta + cx0 * cosDelta;
            }
        }
        public double Cn_II {
            get {
                return W_II.Wing.Cn(M_II, AlphaEff_II);
            }
        }
        public double Cy_II {
            get {
                return Cy1_II * CosAlpha - Cx1_II * SinAlpha;
            }
        }
        public double Cxi_II {
            get {
                var Kaa = Kaa_II;
                var kaa = kaa_II;
                var alphaEff = AlphaEff_II;

                var cn = Cn_II;

                var ksi = W_II.Wing.TgHi0 < 0.01
                            ? 0.0
                            : AeroGr.GetV("4_43", AeroGraphs.M2min1(M_II) / W_II.Wing.TgHi0, alphaEff);
                var lmbd_k = W_II.Wing.Lmb_k;
                var x = lmbd_k * AeroGraphs.M2min1(M_II);
                var y = lmbd_k * W_II.Wing.TgHi0;
                var c_f_shtr = AeroGr.GetV("4_42", x, y);
                c_f_shtr = c_f_shtr / lmbd_k;
                var expr1 = cn * Math.Sin(AeroGraphs.PIdiv180 * (Alpha + W_II.Delta));
                var expr2 = cn * (Kaa - kaa) * SinAlpha * Math.Cos(AeroGraphs.PIdiv180 * W_II.Delta) / kaa;
                var expr3 = cn * ksi * c_f_shtr * Math.Cos(AeroGraphs.PIdiv180 * (Alpha + W_II.Delta));
                return expr1 + expr2 + expr3;
            }
        }
        public double X_Fa_II {
            get {
                return Get_X_Fa_or_delta(W_II, M_II);
            }
        }
        public double X_F_II {
            get {
                return W_II.Delta == 0.0 ?
                        X_Fa_II :
                        (Kaa_II * Alpha * X_Fa_II + Kd0_II * n_II * W_II.Delta * X_Fdelta_II) / (Kaa_II * Alpha + Kd0_II * n_II * W_II.Delta);
            }
        }
        public double X_d_II {
            get {
                return Get_X_d(2);
            }
        }
        public double Mz_omz_II {
            get {
                return Get_m_z_omg_z_wing(X_ct, 2);
            }
        }

        public double Hi_a {
            get {
                return Get_hi_a(M, Alpha);
            }
        }
        public double Cx_cyl_star {
            get {
                return AeroGr.GetV("3_32", M * Math.Abs(SinAlpha));
            }
        }

        public WingOrient W_I { get; set; }
        public WingOrient W_II { get; set; }
        public RocketBody Body { get; set; }

        public double Get_Kaa_star(WingOrient wing) {
            double d_shtr = Body.D / wing.Wing.L;
            return 1.0 + 3 * d_shtr - d_shtr * (1 - d_shtr) / wing.Wing.Etta_k;
        }
        public double Get_kaa_star(WingOrient wing) {
            double d_shtr = Body.D / wing.Wing.L;
            return (1 + 0.41 * d_shtr) * (1 + 0.41 * d_shtr) * (1 + 3 * d_shtr - d_shtr * (1 - d_shtr) / wing.Wing.Etta_k) / ((1 + d_shtr) * (1 + d_shtr));
        }

        public double Get_kd0_star(WingOrient wing) {
            double kaa_star = Get_kaa_star(wing);
            double Kaa_star = Get_Kaa_star(wing);
            return kaa_star * kaa_star / Kaa_star;
        }
        public double Get_Kd0_star(WingOrient wing) {
            return Get_kaa_star(wing);
        }

        /// <summary>
        /// 3.57
        /// </summary>
        /// <param name="wing"></param>
        /// <param name="mach"></param>
        /// <param name="v"></param>
        /// <param name="a_m"></param>
        /// <returns></returns>
        public double Get_hi_pc_shtr(WingOrient wing, double mach, double v, double a_m) {
            double d_shtr = wing.Wing.D / wing.Wing.L;
            double l_1 = wing.X + wing.Wing.X_b + 0.5 * wing.Wing.B_b;
            //ф. 3.17
            double delta_star_shtr = 0.093 * l_1 * (1 + 0.4 * mach + 0.147 * mach * mach - 0.006 * mach * mach * mach) / (Math.Pow(mach * a_m * l_1 / (v), 0.2) * wing.Wing.D);

            var ex1 = (1.0 - d_shtr * (1 + delta_star_shtr)) / (1.0 - d_shtr);
            var ex2 = 1.0 - ((wing.Wing.Etta_k - 1) * d_shtr * (1 + delta_star_shtr)) / (wing.Wing.Etta_k + 1 - 2 * d_shtr);
            var ex3 = 1.0 - ((wing.Wing.Etta_k - 1) * d_shtr) / (wing.Wing.Etta_k + 1 - 2 * d_shtr);
            //ф. 3.57
            return ex1 * ex2 / ex3;
        }

        /// <summary>
        /// ФОРМУЛА 3.16
        /// </summary>
        /// <param name="wing">ЧЕ ЗА КРЫЛО</param>
        /// <param name="mach">мах</param>
        /// <param name="v">кинематич коэфф. вязкости воздуха</param>
        /// <param name="a_m">скорость звука</param>
        /// <returns></returns>
        public double Get_hi_pc(WingOrient wing, double mach, double v, double a_m) {
            double d_shtr = wing.Wing.D / wing.Wing.L;
            double l_1 = wing.X + wing.Wing.X_b + 0.5 * wing.Wing.B_b;
            //ф. 3.17
            double delta_star_shtr = 0.093 * l_1 * (1 + 0.4 * mach + 0.147 * mach * mach - 0.006 * mach * mach * mach) / (Math.Pow(mach * a_m * l_1 / (v), 0.2) * Body.D);
            //ф. 3.16
            return (1 - (2 * d_shtr * d_shtr * delta_star_shtr) / (1 - d_shtr * d_shtr)) * (1 - (d_shtr * (wing.Wing.Etta_k - 1) * delta_star_shtr) / ((1 - d_shtr) * (wing.Wing.Etta_k + 1)));
        }
        public double Get_hi_pc(WingOrient wing, double mach) {
            return Get_hi_pc(wing, mach, 1.51E-5, 340.3);
        }
        /// <summary>
        /// 3.13
        /// </summary>
        /// <param name="mach"></param>
        /// <returns></returns>
        public double Get_hi_M(double mach) {
            return AeroGr.GetV("3_13", mach);
        }
        /// <summary>
        /// 3.18
        /// </summary>
        /// <param name="wing"></param>
        /// <returns></returns>
        public double Get_hi_nos(WingOrient wing) {
            double l_1 = wing.X + wing.Wing.X_b + 0.5 * wing.Wing.B_b;
            double l_shtr = l_1 / Body.D;
            return 0.6 + 0.4 * (1 - Math.Exp(-0.5 * l_shtr));
        }
        public double Get_F_Lhv(WingOrient wing, double mach) {
            if (mach < 1.0)
                return 1.0;
            double d_shtr = Body.D / wing.Wing.L;
            double b_b_shtr = wing.Wing.B_b / (0.5 * Math.PI * Body.D * Math.Sqrt(mach * mach - 1));
            double l_hv = Body.L - wing.X - wing.Wing.X_b - wing.Wing.B_b;
            double l_hv_shtr = l_hv / (0.5 * Math.PI * Body.D * Math.Sqrt(mach * mach - 1));
            double c = (4 + 1 / wing.Wing.Etta_k) * (1 + 8 * d_shtr * d_shtr);
            double lapl1 = AeroGr.GetV("gauss", (b_b_shtr + l_hv_shtr) * Math.Sqrt(2 * c));
            double lapl2 = AeroGr.GetV("gauss", l_hv_shtr * Math.Sqrt(2 * c));
            return 1.0 - (Math.Sqrt(Math.PI) * (lapl1 - lapl2)) / (2 * b_b_shtr * Math.Sqrt(c));
        }

        public double Get_Kaa(WingOrient wing, double mach, double v, double a_m) {
            double Kaastar = Get_Kaa_star(wing);
            double kaastar = Get_kaa_star(wing);
            double hipc = Get_hi_pc(wing, mach, v, a_m);
            double him = Get_hi_M(mach);
            double hinos = Get_hi_nos(wing);
            double FLhv = Get_F_Lhv(wing, mach);
            return (kaastar + (Kaastar - kaastar) * FLhv) * hipc * him * hinos;
        }
        public double Get_kaa(WingOrient wing, double mach, double v, double a_m) {
            double kaastar = Get_kaa_star(wing);
            double hipc = Get_hi_pc(wing, mach, v, a_m);
            double him = Get_hi_M(mach);
            double hinos = Get_hi_nos(wing);
            return kaastar * hipc * him * hinos;
        }

        public double Get_kd0(WingOrient wing, double mach, double v, double a_m) {
            double kd0star = Get_kd0_star(wing);
            double hipc = Get_hi_pc_shtr(wing, mach, v, a_m);
            double him = Get_hi_M(mach);
            return kd0star * hipc * him;
        }
        public double Get_Kd0(WingOrient wing, double mach, double v, double a_m) {
            double Kd0star = Get_Kd0_star(wing);
            double kd0star = Get_kd0_star(wing);
            double hipc = Get_hi_pc_shtr(wing, mach, v, a_m);
            double him = Get_hi_M(mach);
            double FLhv = Get_F_Lhv(wing, mach);
            return (kd0star + (Kd0star - kd0star) * FLhv) * hipc * him;
        }

        public double Get_kt_I_shtr(double mach, double alpha) {
            double lmbda_nos;
            if (Body.Nose is RocketNos_ConePlusCyl)
                lmbda_nos = Body.Lmb_nos;
            else {
                double cx_nos = Body.Cx_nose(mach);
                InterpXY lmb_ot_Cx = new InterpXY();
                foreach (var item in (AeroGr.Graphs["4_11"] as Interp2D)._data) {
                    double lmb = item.Key;
                    double cx_tmp = item.Value.GetV(mach);
                    lmb_ot_Cx.Add(cx_tmp, lmb);
                }
                lmbda_nos = lmb_ot_Cx.GetV(cx_nos);
            }
            return AeroGr.GetV("3_21", lmbda_nos, mach);
        }
        public double Get_kt_II_shtr(double mach, double alpha) {
            double b_a_I = W_I.Wing.B_ak;
            double x1 = W_I.X + W_I.Wing.X_ak + b_a_I;
            double x2 = W_II.X + W_II.Wing.X_ak + 0.5 * W_II.Wing.B_ak;
            double x = x2 - x1;
            double x_shtr = x / b_a_I;
            double k_t_star = AeroGr.GetV("3_22", mach, x_shtr);
            double s_I = W_I.Wing.S_k;
            double s_II = W_II.Wing.S_k;
            return (k_t_star + s_II / s_I) / (1 + s_II / s_I);
        }

        public double Get_eps_sr_alpha() {
            var z_v_shtr = W_I.Wing.Z_v_shtr(M_I);
            var d_I = W_I.Wing.D;
            var l_I = W_I.Wing.L;
            var z_v = 0.5 * (d_I + z_v_shtr * (l_I - d_I));

            var x_v = W_I.Wing.GetX(z_v) + W_I.Wing.GetB(z_v) - W_I.Wing.X_b - W_I.Wing.B_b * W_I.X_povor_otn;
            var x_II = W_II.Wing.X_ak + W_II.Wing.B_ak * 0.5 -
                W_I.Wing.X_b + W_I.Wing.B_b * W_I.X_povor_otn + x_v * Math.Cos(W_I.Delta * AeroGraphs.PIdiv180);
            var y_II = 0.0;
            var y_v = Math.Abs(x_II * SinAlpha - x_v * Math.Sin(W_I.Delta * AeroGraphs.PIdiv180) + y_II);

            var _1_div_etta = 1 / W_II.Wing.Etta_k;
            var _2zv_div_lII = 2.0 * z_v / W_II.Wing.L;
            var _2yv_div_lII = 2.0 * y_v / W_II.Wing.L;
            var d_II_shtr = W_II.Wing.D / W_II.Wing.L;
            var i = AeroGr.GetV("3_17", _2zv_div_lII, _2yv_div_lII, d_II_shtr, _1_div_etta);
            var ksi = 1.0;
            return AeroGraphs._180divPI * i * W_I.Wing.L_k * W_I.Wing.GetCy1a(M_I) * kaa_I * ksi / (2 * Math.PI * z_v_shtr * W_II.Wing.L_k * W_I.Wing.Lmb_k * Kaa_II);
        }
        public double Get_eps_sr_delta() {

            return Get_eps_sr_alpha() * kd0_I * n_I / kaa_I;
        }

        public double Get_n(WingOrient wing, double mach) {
            var k_sh = AeroGr.GetV("k_shel", mach);
            return k_sh * Math.Cos(wing.Hi_rulei * AeroGraphs.PIdiv180);
        }

        public double Get_X_Fizkr(WingOrient wing, double mach) {
            var x_ak = wing.X + wing.Wing.X_ak;
            var b_ak = wing.Wing.B_ak;
            var x_graf = AeroGraphs.M2min1(mach) * wing.Wing.Lmb_k;
            var y_graf = wing.Wing.Etta_k;
            var z_graf = wing.Wing.Lmb_k * wing.Wing.GetTgHiM(0.5);
            var x_Fizkr_shtr = AeroGr.GetV("5_8", x_graf, y_graf, z_graf);
            return x_ak + b_ak * x_Fizkr_shtr;
        }
        public double Get_X_Fdelta(WingOrient wing, double mach) {
            var x_Fizkr = Get_X_Fizkr(wing, mach);
            var d_shtr = wing.Wing.D / wing.Wing.L;
            var f_1 = AeroGr.GetV("5_11", d_shtr);
            f_1 *= 0.5 * wing.Wing.L_k;
            return x_Fizkr - f_1 * wing.Wing.GetTgHiM(0.5);
        }
        public double Get_X_Fif(WingOrient wing, double mach) {
            var x_b = wing.X + wing.Wing.X_b;
            var b_b = wing.Wing.B_b;
            var x_Fizkr = Get_X_Fizkr(wing, mach);
            var x_Fb_shtr = x_Fizkr + 0.02 * wing.Wing.Lmb_k * wing.Wing.GetTgHiM(0.5);
            var f_Lhv = Get_F_Lhv(wing, mach);
            var f_1Lhv = Get_F_1Lhv(wing, mach);
            return x_b + b_b * x_Fb_shtr * f_Lhv * f_1Lhv;
        }

        /// <summary>
        /// формула 5.39
        /// </summary>
        /// <param name="wing"></param>
        /// <param name="mach"></param>
        /// <param name="mode">0 - alpha 1- delta</param>
        /// <returns></returns>
        public double Get_X_Fa_or_delta(WingOrient wing, double mach, int mode = 0) {
            var x_ak = wing.X + wing.Wing.X_ak;
            var b_ak = wing.Wing.B_ak;
            var x_graf = AeroGraphs.M2min1(mach) * wing.Wing.Lmb_k;
            var y_graf = wing.Wing.Etta_k;
            var z_graf = wing.Wing.Lmb_k * wing.Wing.GetTgHiM(0.5);
            var x_Fizkr_shtr = AeroGr.GetV("5_8", x_graf, y_graf, z_graf);

            var x_Fizkr = x_ak + b_ak * x_Fizkr_shtr;

            var d_shtr = wing.Wing.D / wing.Wing.L;
            var f_1 = AeroGr.GetV("5_11", d_shtr);
            f_1 *= 0.5 * wing.Wing.L_k;

            var x_Fdelta = x_Fizkr - f_1 * wing.Wing.GetTgHiM(0.5);

            var x_b = wing.X + wing.Wing.X_b;
            var b_b = wing.Wing.B_b;
            var x_Fb_shtr = x_Fizkr_shtr + 0.02 * wing.Wing.Lmb_k * wing.Wing.GetTgHiM(0.5);
            var f_Lhv = Get_F_Lhv(wing, mach);
            var f_1Lhv = Get_F_1Lhv(wing, mach);

            var x_Fif = x_b + b_b * x_Fb_shtr * f_Lhv * f_1Lhv;

            var k = mode == 0 ?
                    Get_kaa(wing, mach, Aero_v, Aero_a) :
                    Get_kd0(wing, mach, Aero_v, Aero_a);
            var K = mode == 0 ?
                    Get_Kaa(wing, mach, Aero_v, Aero_a) :
                    Get_kd0(wing, mach, Aero_v, Aero_a);

            return (x_Fizkr + (k - 1) * x_Fdelta + (K - k) * x_Fif) / K;
        }
        /// <summary>
        /// формула 5.49
        /// </summary>
        /// <param name="wing"></param>
        /// <param name="mach"></param>
        /// <returns></returns>
        public double Get_F_1Lhv(WingOrient wing, double mach) {
            if (mach < 1.0)
                return 1.0;
            double d_shtr = Body.D / wing.Wing.L;
            double b_b_shtr = wing.Wing.B_b / (0.5 * Math.PI * Body.D * Math.Sqrt(mach * mach - 1));
            double l_hv = Body.L - wing.X - wing.Wing.X_b - wing.Wing.B_b;
            double l_hv_shtr = l_hv / (0.5 * Math.PI * Body.D * Math.Sqrt(mach * mach - 1));
            double c = (4 + 1 / wing.Wing.Etta_k) * (1 + 8 * d_shtr * d_shtr);

            var expr1 = (Math.Exp(-c * l_hv_shtr * l_hv_shtr) - Math.Exp(-c * (b_b_shtr + l_hv_shtr) * (b_b_shtr + l_hv_shtr))) / (c * b_b_shtr * b_b_shtr);
            double lapl1 = AeroGr.GetV("gauss", l_hv_shtr * Math.Sqrt(2 * c));
            var expr2 = Math.Sqrt(Math.PI) * lapl1 / (b_b_shtr * Math.Sqrt(c));
            return 1.0 - expr1 + expr2;
        }
        public double Get_hi_a(double mach, double alpha) {
            return 1.0 - 0.45 * (1.0 - Math.Exp(-0.06 * mach * mach)) * (1.0 - Math.Exp(-0.12 * Math.Abs(alpha)));
        }

        public double Get_X_d(int mode) {
            double Kaa, kaa, cn, delta, cx0, cy1_a_izkr, aEff, x_F, A, x_ct, x_Fizkr, x_ov;
            switch (mode) {
                case 2:
                    Kaa = Kaa_II;
                    kaa = kaa_II;
                    cn = Cn_II;
                    delta = W_II.Delta;
                    cx0 = Cx0_II;
                    cy1_a_izkr = W_II.Wing.GetCy1a(M_II);
                    aEff = AlphaEff_II;
                    x_F = X_F_II;
                    A = A_II;
                    x_ct = W_II.X + W_II.Wing.X_ct_k;
                    x_Fizkr = Get_X_Fizkr(W_II, M_II);
                    x_ov = W_II.X + W_II.Wing.X_b + W_II.Wing.B_b * W_II.X_povor_otn;
                    break;
                default:
                    Kaa = Kaa_I;
                    kaa = kaa_I;
                    cn = Cn_I;
                    delta = W_I.Delta;
                    cx0 = Cx0_I;
                    cy1_a_izkr = W_I.Wing.GetCy1a(M_I);
                    aEff = AlphaEff_I;
                    x_F = X_F_I;
                    A = A_I;
                    x_ct = W_I.X + W_I.Wing.X_ct_k;
                    x_Fizkr = Get_X_Fizkr(W_I, M_I);
                    x_ov = W_I.X + W_I.Wing.X_b + W_I.Wing.B_b * W_I.X_povor_otn;
                    break;
            }
            var sinAEff2 = Math.Sin(aEff * AeroGraphs.PIdiv180);
            sinAEff2 *= sinAEff2;
            if (delta == 0.0) {
                if (sinAEff2 == 0.0)
                    return x_F;
                return x_F + (A * sinAEff2 * Math.Sign(aEff) * (x_ct - x_Fizkr) / cn);
            }
            else {
                var cosDelt = Math.Cos(delta * AeroGraphs.PIdiv180);
                var sinDelt = Math.Sin(delta * AeroGraphs.PIdiv180);

                var expr1 = Kaa * cn * cosDelt * x_F / kaa;
                var expr2 = Kaa * A * sinAEff2 * Math.Sign(aEff) * cosDelt * (x_ct - x_Fizkr) / kaa;
                var expr3 = cx0 * sinDelt * x_ov;
                var expr4 = Kaa * cn * cosDelt / kaa - cx0 * sinDelt;

                return (expr1 + expr2 + expr3) / expr4;
            }

        }
        public double Get_m_z_omg_z_f(double x_ct) {
            var expr1 = x_ct / Body.L;
            var lmbF = Body.L / Body.D;
            var lmbN = Body.Lmb_nos;
            var expr2 = (2.0 * lmbF * lmbF - lmbN * lmbN) / (4.0 * lmbF * (lmbF - 2.0 * lmbN / 3.0));
            return -2.0 * (1.0 - expr1 + expr1 * expr1 - expr2);
        }
        public double Get_m_z_omg_z_wing(double x_ct, int mode) {

            double machm1 = mode == 2 ?
                          AeroGraphs.M2min1(M_II) :
                          AeroGraphs.M2min1(M_I);
            double tg05 = mode == 2 ?
                          W_II.Wing.GetTgHiM(0.5) :
                          W_I.Wing.GetTgHiM(0.5);
            double lmbk = mode == 2 ?
                          W_II.Wing.Lmb_k :
                          W_I.Wing.Lmb_k;
            double etta = mode == 2 ?
                          W_II.Wing.Etta_k :
                          W_I.Wing.Etta_k;
            double x_ak = mode == 2 ?
                          W_II.Wing.X_ak + W_II.X :
                          W_I.Wing.X_ak + W_I.X;
            double b_ak = mode == 2 ?
                          W_II.Wing.B_ak :
                          W_I.Wing.B_ak;

            var expr1 = AeroGr.GetV("5_15", lmbk * machm1, lmbk * tg05, etta);
            var b1 = AeroGr.GetV("5_16", lmbk * machm1, lmbk * tg05, etta);
            var x_t_shtr = (x_ct - x_ak) / b_ak;
            //5.74
            var mzCya_izkr = expr1 - b1 * (0.5 - x_t_shtr) - AeroGraphs._180divPI * (0.5 - x_t_shtr) * (0.5 - x_t_shtr);

            var cyaIzkr = mode == 2 ?
                          Cy1a_II :
                          Cy1a_I;
            var Kaa = mode == 2 ?
                        Kaa_II :
                        Kaa_I;
            double delta_mz = 0.0;
            if (mode == 2) {
                //5.82
                var expr11 = (x_ct - (W_I.Wing.X_ak + W_I.X + 0.5 * W_I.Wing.B_ak)) / W_II.Wing.B_ak;
                var expr12 = (x_ct - X_Fa_II) / W_II.Wing.B_ak;
                delta_mz = -AeroGraphs._180divPI * Cy1a_II * Kaa_II * Eps_sr_a * expr11 * expr12;
            }
            return mzCya_izkr * cyaIzkr * Kaa + delta_mz;
        }


        public static Rocket DefaultRocket(AeroGraphs ag) {
            Rocket r = new Rocket();
            r.AeroGr = ag;

            r.Body = new RocketBody(ag) {
                L = 4.18,
                L_nos = 0.356,
                L_korm = 0.09,
                D = 0.31,
                D1 = 0.28,
                //Nose = new RocketNos_ConePlusCyl()
                Nose = new RocketNos_Compose("7_2", 118 * 2 / 310.0)
            };

            r.W_I = new WingOrient(
                new RocketWing(ag) {
                    B0 = 1.022,
                    B1 = 0.327,
                    L = 1.016,
                    Hi0 = 50,
                    C_shtr = 0.07,
                    D = r.Body.D,
                    Profile = new WingProf_6(0.4)
                }) {
                X = 1.82028
            };
            r.W_II = new WingOrient(
                new RocketWing(ag) {
                    B0 = 0.55,
                    B1 = 0.07117,
                    L = 0.7,
                    Hi0 = 50,
                    C_shtr = 0.072,
                    D = r.Body.D,
                    Profile = new WingProf_6(0.4)
                }) {
                X = 3.599999,
                X_povor_otn = 218.0 / 338.0
            };
            return r;
        }
    }
}
