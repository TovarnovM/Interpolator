using System;

namespace RocketAero {
    /// <summary>
    /// Интерфейс для определения Cy1a
    /// </summary>
    public interface IRocket_Coeff {
        double GetCy1a(double mach);
    }

    /// <summary>
    /// Интерфейс для головной части
    /// </summary>
    public interface IRocketBody_nos {
        double GetCy1a_nos(AeroGraphs ag, double mach, double lmb_nos, double lmb_cyl);
        /// <summary>
        /// Площадь носовой части
        /// </summary>
        /// <param name="d"></param>
        /// <param name="l"></param>
        /// <returns></returns>
        double GetF_nos(double d, double l);
        double GetCx_nos(AeroGraphs ag, double mach, double lmb_nos);
        double GetW_nos(double d, double l);
    }

    /// <summary>
    /// Конус+цилиндр
    /// </summary>
    public class RocketNos_ConePlusCyl : IRocketBody_nos {
        public double GetCx_nos(AeroGraphs ag, double mach, double lmb_nos) {
            return ag.GetV("4_11", mach, lmb_nos);
        }

        public double GetCy1a_nos(AeroGraphs ag, double mach, double Lmb_nos, double Lmb_cyl) {
            return ag.GetV("3_2", AeroGraphs.M2min1(mach) / Lmb_nos, Lmb_cyl / Lmb_nos);
        }

        public double GetF_nos(double d, double l) {
            return Math.PI * 0.5 * d * Math.Sqrt(0.25 * d * d + l * l);
        }

        public double GetW_nos(double d, double l) {
            return 0.25 * Math.PI * d * d * l / 3.0;
        }
    }
    /// <summary>
    /// оживало+цилиндр
    /// </summary>
    public class RocketNos_OzjPlusCyl : IRocketBody_nos {
        public double GetCx_nos(AeroGraphs ag, double mach, double lmb_nos) {
            return ag.GetV("4_12", mach, lmb_nos);
        }

        public double GetCy1a_nos(AeroGraphs ag, double mach, double Lmb_nos, double Lmb_cyl) {
            return ag.GetV("3_3", AeroGraphs.M2min1(mach) / Lmb_nos, Lmb_cyl / Lmb_nos);
        }

        public double GetF_nos(double d, double l) {
            return Math.PI * 0.5 * d * Math.Sqrt(0.25 * d * d + l * l) * 1.25;
        }

        public double GetW_nos(double d, double l) {
            return 0.533 * 0.25 * Math.PI * d * d * l;
        }
    }

    /// <summary>
    /// Торец или полусфера
    /// </summary>
    public class RocketNos_SpherePlusCyl : IRocketBody_nos {
        private static string grNum = "3_4";
        //1 - сфера 0 - тупой конец)
        private double _type = 1.0;
        public double Type {
            get { return _type; }
            set {
                _type = value < 0.0 ? 0.0 : value > 1.0 ? 1.0 : value;
            }
        }
        public double GetCy1a_nos(AeroGraphs ag, double mach, double lmb_nos, double lmb_cyl) {
            return ag.GetV(grNum, AeroGraphs.M2min1(mach) / lmb_cyl, _type);
        }

        public double GetF_nos(double d, double l) {
            return Math.PI * d * d * (0.25 + 0.25 * _type);
        }

        public double GetCx_nos(AeroGraphs ag, double mach, double lmb_nos) {
            var lmb_forgr = 0.5 * _type;
            return ag.GetV("4_13", mach, lmb_forgr);
        }

        public double GetW_nos(double d, double l) {
            var r = 0.5 * d;
            return (2.0 * Math.PI * r * r * r / 3.0) * Type;
            ;
        }

        public RocketNos_SpherePlusCyl() { }
        /// <summary>
        /// 1 - сфера
        /// 0 - торец
        /// </summary>
        /// <param name="type"></param>
        public RocketNos_SpherePlusCyl(double type) {
            Type = type;
        }
    }

    /// <summary>
    /// Составной
    /// </summary>
    public class RocketNos_Compose : IRocketBody_nos {
        public IRocketBody_nos Main { get; set; }
        public IRocketBody_nos Little { get; set; }
        public double Rshtrih { get; set; } //2r/D
        public double GetTetta(double lmb_nos) {
            var tetta = 0.0;
            double type = 0.0;
            if (Little is RocketNos_SpherePlusCyl) {
                type = (Little as RocketNos_SpherePlusCyl).Type;
            }

            if (type == 0.0) {
                tetta = Math.Atan((1.0 - Rshtrih) / (2.0 * lmb_nos));
            }
            else {
                var expr1 = Math.Sqrt((2 * lmb_nos - Rshtrih * type) * (2 * lmb_nos - Rshtrih * type) + 1.0);
                var alp = Math.Asin(Rshtrih / expr1);
                var btta = Math.Atan(2.0 * lmb_nos - Rshtrih * type);
                tetta = Math.PI * 0.5 - alp - btta;
            }

            return tetta;
        }
        public double GetLmbdShtr(double lmb_nos) {
            double lmb_shtr = lmb_nos;
            double type = 0.0;
            if (Little is RocketNos_SpherePlusCyl) {
                type = (Little as RocketNos_SpherePlusCyl).Type;
            }

            if (Main is RocketNos_ConePlusCyl) {
                double tetta = GetTetta(lmb_nos);
                lmb_shtr = 0.5 / Math.Tan(tetta);
            }

            if (Main is RocketNos_OzjPlusCyl) {
                lmb_shtr = (lmb_nos - Rshtrih * 0.5 * type) / Math.Sqrt(1 - Rshtrih);
            }
            return lmb_shtr;
        }

        public RocketNos_Compose(IRocketBody_nos main, IRocketBody_nos little, double r) {
            Main = main;
            Little = little;
            Rshtrih = r;
        }
        /// <summary>
        /// Альтернативный конструктор 
        /// mode = "7_1" - конус + сфера табл 3.1 №7
        /// mode = "7_2" - оживало + сфера табл 3.1 №7
        /// mode = "8_1" - конус + торец табл 3.1 №7
        /// mode = "8_2" - оживало + торец табл 3.1 №7
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="r_shtr"></param>
        public RocketNos_Compose(string mode, double r_shtr) {
            Rshtrih = r_shtr;
            if (mode == "7_2") {
                Main = new RocketNos_OzjPlusCyl();
                Little = new RocketNos_SpherePlusCyl(1);
                return;
            }
            if (mode == "8_1") {
                Main = new RocketNos_ConePlusCyl();
                Little = new RocketNos_SpherePlusCyl(0);
                return;
            }
            if (mode == "8_2") {
                Main = new RocketNos_OzjPlusCyl();
                Little = new RocketNos_SpherePlusCyl(0);
                return;
            }
            Main = new RocketNos_ConePlusCyl();
            Little = new RocketNos_SpherePlusCyl(1);
        }

        public double GetCy1a_nos(AeroGraphs ag, double mach, double lmb_nos, double lmb_cyl) {
            double lmb_shtr = GetLmbdShtr(lmb_nos);
            return Main.GetCy1a_nos(ag, mach, lmb_shtr, lmb_cyl) * (1 - Rshtrih * Rshtrih) +
                    Little.GetCy1a_nos(ag, mach, lmb_nos, lmb_cyl) * Rshtrih * Rshtrih;
        }

        public double GetF_nos(double d, double l) {
            double l_shtr = GetLmbdShtr(l / d) * d;
            double type = 0.0;
            if (Little is RocketNos_SpherePlusCyl)
                type = (Little as RocketNos_SpherePlusCyl).Type;
            var tetta = GetTetta(l / d);
            var expr1 = Main.GetF_nos(d, l_shtr);
            var expr2 = Main.GetF_nos(d * Rshtrih * Math.Cos(tetta), l_shtr - l + Rshtrih * d * 0.5 * type - Rshtrih * d * 0.5 * Math.Sin(tetta));
            var expr3 = Little.GetF_nos(d * Rshtrih, Rshtrih * d * 0.5 * type);
            return expr1 - expr2 + expr3;
        }

        public double GetCx_nos(AeroGraphs ag, double mach, double lmb_nos) {
            double lmb_shtr = GetLmbdShtr(lmb_nos);
            double type = 0;
            double cosTetta = Math.Cos(Math.Atan(0.5 / lmb_shtr));
            double main_expr = Rshtrih * Rshtrih;
            if (Little is RocketNos_SpherePlusCyl) {
                type = (Little as RocketNos_SpherePlusCyl).Type;
            }

            if (Main is RocketNos_ConePlusCyl) {
                cosTetta = Math.Cos(Math.Atan(0.5 / lmb_shtr));
                main_expr = Rshtrih * Rshtrih * (type * (cosTetta - 1) + 1);
            }

            if (Main is RocketNos_OzjPlusCyl) {
                cosTetta = Math.Cos(Math.Atan(Math.Sqrt(1 - Rshtrih) / lmb_shtr));
                double cosTettaExpr = type * (cosTetta - 1) + 1;
                double r_strSqr = Rshtrih * Rshtrih;
                main_expr = r_strSqr * cosTettaExpr * cosTettaExpr * (3.1 - 1.4 * cosTettaExpr * Rshtrih - 0.7 * cosTettaExpr * cosTettaExpr * r_strSqr);
            }
            return Main.GetCx_nos(ag, mach, lmb_shtr) * (1 - main_expr) + Little.GetCx_nos(ag, mach, lmb_nos) * Rshtrih * Rshtrih;
        }

        public double GetW_nos(double d, double l) {
            var lmbShtr = GetLmbdShtr(l / d);
            var l_dop = (Little is RocketNos_SpherePlusCyl) ? (Little as RocketNos_SpherePlusCyl).Type * d * 0.5 * Rshtrih : 0.0;

            return Little.GetW_nos(d * Rshtrih, l)
                    + Main.GetW_nos(d, d * lmbShtr)
                    - Main.GetW_nos(d * Rshtrih, d * lmbShtr - l + l_dop);
        }
    }
}
