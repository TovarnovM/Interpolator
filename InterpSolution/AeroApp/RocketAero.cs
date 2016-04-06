using System;
using System.Collections.Generic;
using System.Text;
using Interpolator;
using AeroApp.Properties;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Globalization;
using System.Collections;
using System.Diagnostics;
using System.Xml.Serialization;

namespace RocketAero
{
    /// <summary>
    /// Там графики из Л-Ч
    /// </summary>
    public class AeroGraphs
    {
        private Dictionary<string, IInterpElem> _graphs = new Dictionary<string, IInterpElem>();
        public  Dictionary<string, IInterpElem> Graphs { get { return _graphs; } }
        public AeroGraphs()
        {
            var resourses = Resources.ResourceManager.GetResourceSet(System.Globalization.CultureInfo.CurrentUICulture, true, true);
            foreach (DictionaryEntry item in resourses)
            {
                string key = item.Key.ToString();
                if (key.EndsWith("4D"))
                    _graphs.Add(CutMyString(key), 
                                Interp4D.LoadFromXmlString(item.Value.ToString()));
                else if (key.EndsWith("3D"))
                    _graphs.Add(CutMyString(key), 
                                Interp3D.LoadFromXmlString(item.Value.ToString()));
                else if (key.EndsWith("1D"))
                    _graphs.Add(CutMyString(key), 
                                InterpXY.LoadFromXmlString(item.Value.ToString()));
                else if (key.EndsWith("4P"))
                    _graphs.Add(CutMyString(key),
                                PotentGraff4P.LoadFromXmlString(item.Value.ToString()));
                else
                    _graphs.Add(CutMyString(key), 
                                Interp2D.LoadFromXmlString(item.Value.ToString()));

                //XmlDocument doc = new XmlDocument();
                //doc.LoadXml(item.Value.ToString());
                //var tmpList = new List<string>();
                //foreach (XmlNode elem in doc.DocumentElement.SelectNodes("paramList/string"))
                //{
                //    string par = elem?.InnerText;
                //    if(par != null)
                //        tmpList.Add(par);
                //}
                //if (tmpList.Count == 0)
                //    tmpList.Add("I dont know");
                //_params.Add(CutMyString(key), tmpList);
            }                
        }
        public string CutMyString(string cutThis)
        {
            string result = cutThis;
            cutThis = cutThis.Trim('_');
            if (cutThis.EndsWith("_4D")||
                cutThis.EndsWith("_3D") ||
                cutThis.EndsWith("_2D") ||
                cutThis.EndsWith("_1D") ||
                cutThis.EndsWith("_4P")   )
                cutThis = cutThis.Remove(cutThis.Length - 3);
            return cutThis;
        }
        public int HowManyParams(string graphNum)
        {
            if (!HasGraph(graphNum))
                return -1;
            if (_graphs[CutMyString(graphNum)] is Interp2D)
                return 2;
            if (_graphs[CutMyString(graphNum)] is Interp3D)
                return 3;
            if (_graphs[CutMyString(graphNum)] is Interp4D)
                return 4;
            if (_graphs[CutMyString(graphNum)] is InterpXY)
                return 1;
            if (_graphs[CutMyString(graphNum)] is PotentGraff4P)
                return 4;
            return 0;
        }
        public bool HasGraph(string graphNum)
        {
            return _graphs.ContainsKey(graphNum) || _graphs.ContainsKey(CutMyString(graphNum));
        }
        /// <summary>
        /// Основная функция запроса значений из графиков
        /// имя графика задается в виде "4_11" - график 4.11 из Лебедева "Динамика полета"
        /// количество параметров можно взять в функции HowManyParams(graphNum)
        /// </summary>
        /// <param name="graphNum">имя графика ... типа "5_11"</param>
        /// <param name="t"></param>
        /// <returns></returns>
        public double GetV(string graphNum, params double[] t)
        {
            try
            {
                return _graphs[CutMyString(graphNum)].GetV(t);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                throw ex;
            }

        }
        public static double PIdiv180 = 0.01745329251994329576923690768489D;
        public static double _180divPI = 57.295779513082320876798154814105D;
        public static double M2min1(double mach)
        {
            return Math.Sqrt(Math.Abs(mach * mach - 1)) * Math.Sign(mach - 1);
        }
    }
 
    /// <summary>
    /// Интерфейс для определения Cy1a
    /// </summary>
    public interface IRocket_Coeff
    {
        double GetCy1a(double mach);   
    }    
  
    /// <summary>
    /// Интерфейс для головной части
    /// </summary>
    public interface IRocketBody_nos
    {
        double GetCy1a_nos(AeroGraphs ag, double mach, double lmb_nos, double lmb_cyl);
        /// <summary>
        /// Площадь носовой части
        /// </summary>
        /// <param name="d"></param>
        /// <param name="l"></param>
        /// <returns></returns>
        double GetF_nos(double d, double l);
        double GetCx_nos(AeroGraphs ag, double mach, double lmb_nos);
    }

    /// <summary>
    /// Конус+цилиндр
    /// </summary>
    public class RocketNos_ConePlusCyl : IRocketBody_nos
    {
        public double GetCx_nos(AeroGraphs ag, double mach, double lmb_nos)
        {
            return ag.GetV("4_11", mach, lmb_nos);
        }

        public double GetCy1a_nos(AeroGraphs ag, double mach, double Lmb_nos, double Lmb_cyl)
        {
            return ag.GetV("3_2", AeroGraphs.M2min1(mach) / Lmb_nos, Lmb_cyl / Lmb_nos);
        }

        public double GetF_nos(double d, double l)
        {
            return Math.PI * 0.5 * d * Math.Sqrt(0.25 * d * d + l * l);
        }
    }
    /// <summary>
    /// оживало+цилиндр
    /// </summary>
    public class RocketNos_OzjPlusCyl : IRocketBody_nos
    {
        public double GetCx_nos(AeroGraphs ag, double mach, double lmb_nos)
        {
            return ag.GetV("4_12", mach, lmb_nos);
        }

        public  double GetCy1a_nos(AeroGraphs ag, double mach, double Lmb_nos, double Lmb_cyl)
        {
            return ag.GetV("3_3", AeroGraphs.M2min1(mach) / Lmb_nos, Lmb_cyl / Lmb_nos);
        }

        public double GetF_nos(double d, double l)
        {
            return Math.PI * 0.5 * d * Math.Sqrt(0.25 * d * d + l * l);
        }
    }

    /// <summary>
    /// Торец или полусфера
    /// </summary>
    public class RocketNos_SpherePlusCyl : IRocketBody_nos
    {
        private static string grNum = "3_4";
        //1 - сфера 0 - тупой конец)
        private double _type = 1.0; 
        public double Type
        {
            get { return _type; }
            set
            {
                _type = value < 0.0 ? 0.0 : value > 1.0 ? 1.0 : value;
            }
        }
        public double GetCy1a_nos(AeroGraphs ag, double mach, double lmb_nos, double lmb_cyl)
        {
            return ag.GetV(grNum, AeroGraphs.M2min1(mach) / lmb_cyl, _type);
        }

        public double GetF_nos(double d, double l)
        {
            return Math.PI * d * d * (0.25 + 0.25 * _type);
        }

        public double GetCx_nos(AeroGraphs ag, double mach, double lmb_nos)
        {
            var lmb_forgr = 0.5 * _type;
            return ag.GetV("4_13", mach, lmb_forgr);
        }

        public RocketNos_SpherePlusCyl() { }
        /// <summary>
        /// 1 - сфера
        /// 0 - торец
        /// </summary>
        /// <param name="type"></param>
        public RocketNos_SpherePlusCyl(double type)
        {
            Type = type;
        }
    }

    /// <summary>
    /// Составной
    /// </summary>
    public class RocketNos_Compose : IRocketBody_nos
    {
        public IRocketBody_nos Main { get; set; }
        public IRocketBody_nos Little{ get; set; }
        public double Rshtrih{ get; set; } //2r/D
        
        public RocketNos_Compose(IRocketBody_nos main, IRocketBody_nos little, double r)
        {
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
        /// <param name="r"></param>
        public RocketNos_Compose(string mode, double r)
        {       
            if (mode == "7_2")
            {
                Main = new RocketNos_ConePlusCyl();
                Little = new RocketNos_SpherePlusCyl(0);
            }
            if (mode == "8_1")
            {
                Main = new RocketNos_OzjPlusCyl();
                Little = new RocketNos_SpherePlusCyl(1);
            }
            if (mode == "8_2")
            {
                Main = new RocketNos_OzjPlusCyl();
                Little = new RocketNos_SpherePlusCyl(0);
            }

            Main = new RocketNos_ConePlusCyl();
            Little = new RocketNos_SpherePlusCyl(1);
            Rshtrih = r;
        }

        public double GetCy1a_nos(AeroGraphs ag, double mach, double lmb_nos, double lmb_cyl)
        {
            double lmb_shtr = lmb_nos;
            double type = 0;
            double cosTetta = Math.Cos(Math.Atan(0.5 / lmb_shtr));
            double main_expr = Rshtrih * Rshtrih;
            if (Little is RocketNos_SpherePlusCyl)
            {
                type = (Little as RocketNos_SpherePlusCyl).Type;
            }

            if (Main is RocketNos_ConePlusCyl)
            {
                lmb_shtr = lmb_nos / (1 - Rshtrih);
            }

            if (Main is RocketNos_OzjPlusCyl)
            {
                lmb_shtr = (lmb_nos - Rshtrih * 0.5 * type) / Math.Sqrt(1 - Rshtrih);
            }
            return  Main.GetCy1a_nos(ag, mach, lmb_shtr, lmb_cyl) * (1 - Rshtrih * Rshtrih) + 
                    Little.GetCy1a_nos(ag, mach, lmb_nos, lmb_cyl)* Rshtrih * Rshtrih;
        }

        public double GetF_nos(double d, double l)
        {
            double l_shtr = l / (1 - Rshtrih);
            if (Main is RocketNos_OzjPlusCyl)
                l_shtr = 0.5 * d / ((1 - Rshtrih) / (l / d - 0.5 * Rshtrih));
            double type= 0.0;
            if (Little is RocketNos_SpherePlusCyl)
                type = (Little as RocketNos_SpherePlusCyl).Type;    

            return Main.GetF_nos(d, l_shtr) 
                - Main.GetF_nos(d * Rshtrih, l_shtr - l - Rshtrih * d * 0.5* type) 
                + Little.GetF_nos(d * Rshtrih, Rshtrih * d * 0.5* type);
        }

        public double GetCx_nos(AeroGraphs ag, double mach, double lmb_nos)
        {
            double lmb_shtr = lmb_nos;
            double type = 0;
            double cosTetta = Math.Cos(Math.Atan(0.5 / lmb_shtr));
            double main_expr = Rshtrih * Rshtrih;
            if (Little is RocketNos_SpherePlusCyl)
            {
                type = (Little as RocketNos_SpherePlusCyl).Type;
            }
                
            if (Main is RocketNos_ConePlusCyl)
            {
                lmb_shtr = lmb_nos / (1 - Rshtrih);
                cosTetta = Math.Cos(Math.Atan(0.5 / lmb_shtr));
                main_expr = Rshtrih * Rshtrih * (type * (cosTetta - 1) + 1);
            }
                
            if (Main is RocketNos_OzjPlusCyl)
            {
                lmb_shtr = (lmb_nos - Rshtrih*0.5*type)/ Math.Sqrt(1 - Rshtrih);
                cosTetta = Math.Cos(Math.Atan(Math.Sqrt(1 - Rshtrih) / lmb_shtr));
                double cosTettaExpr = type * (cosTetta - 1) + 1;
                double r_strSqr = Rshtrih * Rshtrih;
                main_expr = r_strSqr * cosTettaExpr * (3.1 - 1.4 * cosTettaExpr * Rshtrih + 0.7 * cosTettaExpr * cosTettaExpr * r_strSqr);
            }
            return Main.GetCx_nos(ag, mach, lmb_shtr) * (1 - main_expr) + Little.GetCx_nos(ag, mach, lmb_nos) * Rshtrih * Rshtrih;
        }
    }

    /// <summary>
    /// геометрия Фюзеляжа ракеты обобщенныая
    /// </summary>
    public class RocketBody_Geom
    {
        
        public double L { get; set; } = 1;
        public double L_nos { get; set; } = 0.2;
        public double L_korm { get; set; } = 0.0;
        public double L_cyl { get { return L - L_nos - L_korm; } } 
        public double D { get; set; } = 0.15;
        public double D1 { get; set; } = 0.15;

        public double Lmb_nos
        {
            get
            {
                return L_nos / D;
            }
        }
        public double Lmb_nos_shtrih
        {
            get
            {
                if (Nose is RocketNos_Compose)
                {
                    double r = (Nose as RocketNos_Compose).Rshtrih * D;
                    if ((Nose as RocketNos_Compose).Little is RocketNos_SpherePlusCyl)
                    {
                        //double tetta_shtrih = ((Nose as RocketNos_Compose).Little as RocketNos_SpherePlusCyl).Type < 0.001 ?
                        //                        Math.Atan((0.5*D-r)/(L_nos)) :

                        //                        Math.PI*0.5 -
                        //                        Math.Asin(r / (Math.Sqrt((L_nos - r) * (L_nos - r) + 0.25 * D * D))) -
                        //                        Math.Atan(2 * (L_nos - r) * (L_nos - r) / D);
                        double tetta_shtrih = Math.Atan((0.5 * D - r) / (L_nos));
                        return  0.5 / Math.Tan(tetta_shtrih);
                    }
                }
                return Lmb_nos;
            }                              
        }
        public double Lmb_cyl
        {
            get
            {
                return L_cyl / D;
            }
        }
        public double Lmb_korm
        {
            get
            {
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
        /// <summary>
        /// Площадь корпуса без дна)
        /// </summary>
        public double S_fuse
        {
            get
            {
                return Nose.GetF_nos(D,L_nos)+Math.PI*D*L_cyl+Math.PI*0.5*(D+D1) * Math.Sqrt(0.25 * D* D+ L_korm* L_korm);
            }
        }
        public IRocketBody_nos Nose { get; set; } = new RocketNos_ConePlusCyl();
        public RocketBody_Geom()
        {
            
        }
    }

    public class RocketBody: RocketBody_Geom, IRocket_Coeff
    {
        public AeroGraphs AeroGr { get; set; } = null;
        public virtual double GetCy1a(double mach)
        {
            if (AeroGr == null)
                return 0.0;
            double cy1a_korm = 0.0;
            if (D1 != D && L_korm != 0)
            {
                if (D1 > D)
                    cy1a_korm = 0.8 * (D1 * D1 / (D * D) - 1) * 2 * AeroGraphs.PIdiv180 * Math.Cos(2 * Math.Atan(0.5 * (D1 - D) / L_korm));
                else
                    cy1a_korm = -0.2 * (1 - D1 * D1 / (D * D)) * 2 * AeroGraphs.PIdiv180;
            }
            return Nose.GetCy1a_nos(AeroGr, mach, Lmb_nos_shtrih, Lmb_cyl) + cy1a_korm;
        }
        public RocketBody(AeroGraphs ag)
        {
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
        public double Cx_tr(double mach, double x_t, double v, double a_m)
        {
            var vel = mach * a_m;
            var re = vel * L / v;
            var _2CfM0 = AeroGr.GetV("4_2", re, x_t);
            var etta = AeroGr.GetV("4_3", mach, x_t);
            return 0.5 * _2CfM0 * etta * S_fuse / S_mid;
        }
        public double Cx_tr(double mach)
        {
            return Cx_tr(mach, 0, 1.51E-5, 340.3);
        }
        public double Cx_nose(double mach)
        {
            return Nose.GetCx_nos(AeroGr, mach, Lmb_nos);
        }
        public double Cx_korm(double mach)
        {
            if (D1 == D || L_korm == 0) return 0;
            double etta = D1 / D ;
            return AeroGr.GetV("4_24", mach, Lmb_korm, etta);
            //double S_dno_shtr = D1 * D1 / (D * D);
            //double expr = D1 < D ?
            //              Math.Sqrt(1 - S_dno_shtr) :
            //              Math.Sqrt(S_dno_shtr * (S_dno_shtr - 1));
            //double sinBetta = Math.Sin(Math.Atan(Math.Abs((D1 - D) * 0.5) / L_korm));
            //double sinBettaSqr = sinBetta* sinBetta;
            //return expr * (2.09 * sinBettaSqr + 0.19 * sinBettaSqr / Math.Sqrt(AeroGraphs.M2min1(mach)));
        }
        public double Cx_dno(double mach)
        {
            return 0;
        }
        public double Cx0f(double mach)
        {
            return Cx_tr(mach) + Cx_nose(mach) + Cx_korm(mach) + Cx_dno(mach);
        }
        public double Cx0f(double mach, double x_t, double v, double a_m)
        {
            return Cx_tr(mach, x_t, v, a_m) + Cx_nose(mach) + Cx_korm(mach) + Cx_dno(mach);
        }

    }

    public class RocketWing_Geom
    {
        public double B0 { get; set; } = 0.2;
        public double B1 { get; set; } = 0.05;
        public double L { get; set; } = 0.4;
        public double D { get; set; } = 0.2;
        public double C_shtr { get; set; } = 0.01;
        private double hi0 = 45;
        public double Hi0
        {
            get { return hi0; }
            set
            {
                hi0 = value;
                tgHi0 = Math.Tan(AeroGraphs.PIdiv180 * hi0);
            }
        }
        private double tgHi0 = 0.5;
        public double TgHi0 {
            get { return tgHi0; }
            set
            {
                tgHi0 = value;
                hi0 = Math.Atan(tgHi0) * AeroGraphs._180divPI;
            }
        }

        public double S { get { return 0.5 * L * (B1 + B0); } }
        public double Lmb { get { return L * L / S; } }
        public double Etta
        {
            get
            {
                return B1 < 0.001 || B0/B1 > 1000 ? 1000 : B0 / B1;
            }
        }
        //Консоли
        public double Bb { get { return GetB(0.5 * D); } }
        public double L_k { get { return L - D; } }
        public double S_k { get { return 0.5 * L_k * (B1 + Bb); } }
        public double Lmb_k { get { return L_k * L_k / S_k; } }
        public double Etta_k
        {
            get
            {
                return B1 < 0.001 || Bb / B1 > 1000 ? 1000 : Bb / B1;
            }
        }
        public double Xb
        {
            get
            {
                return GetX(D*0.5);
            }
        }
        //САХ
        public double Za
        {
            get
            {
                return L * (0.5 * B0 + B1) / (3 * (B0 + B1));
            }
        }
        public double Ba
        {
            get
            {
                return GetB(Za);
            }
        }
        public double Xa
        {
            get
            {
                return GetX(Za);
            }
        }
        //САХ консолей
        public double Za_k
        {
            get
            {
                return 0.5*D+L_k * (0.5 * Bb + B1) / (3 * (Bb + B1));
            }
        }
        public double Ba_k
        {
            get
            {
                return GetB(Za_k);
            }
        }
        public double Xa_k
        {
            get
            {
                return GetX(Za_k);
            }
        }

        public double GetB(double z) => B0 + (B1 - B0) * z * 2 / L;        
        public double GetTgHiM(double m) => tgHi0 - 4 * m * (Etta - 1) / (Lmb * (1 + Etta));        
        public double GetX(double z) => z * tgHi0;

        public IWingProfile Profile { get; set; } = new WingProf_romb();
        
    }
    public class RocketWing : RocketWing_Geom, IRocket_Coeff
    {
        public AeroGraphs AeroGr { get; set; } = null;
        public RocketWing(AeroGraphs ag)
        {
            AeroGr = ag;
        }
        public double GetCy1a(double mach)
        {
            if (AeroGr == null)
                return 0.0;
            return AeroGr.GetV("3_5", Lmb_k * AeroGraphs.M2min1(mach), Lmb_k * Math.Pow(C_shtr, 1.0 / 3.0), Lmb_k * GetTgHiM(0.5));
        }

    }
    public interface IWingProfile
    {
        double K { get; }
    }
    /// <summary>
    /// Таблица 4.2 ромб
    /// </summary>
    public class WingProf_romb : IWingProfile
    {
        public double K
        {
            get
            {
                return 1.0;
            }
        }
    }
    /// <summary>
    /// Таблица 4.2 четырехугольник
    /// </summary>
    public class WingProf_4 : IWingProfile
    {
        public double K
        {
            get
            {
                return 0.25/(Xc_shtr*(1- Xc_shtr));
            }
        }
        private double _xc_str;
        public double Xc_shtr
        {
            get
            {
                return _xc_str;
            }
            set
            {
                _xc_str = value < 0 ? 0 :
                          value > 1 ? 1 :
                          value;
            }
        }
        /// <summary>
        /// Таблица 4.2 четырехугольник
        /// x_c_shtr = b / x_c
        /// </summary>
        /// <param name="x_c_shtr">b / x_c</param>
        public WingProf_4(double x_c_shtr)
        {
            Xc_shtr = x_c_shtr;
        }
    }
    /// <summary>
    /// Таблица 4.2 шестиугольный
    /// </summary>
    public class WingProf_6 : WingProf_4
    {
        public new double K
        {
            get
            {
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
    public class WingProf_sin : IWingProfile
    {
        /// <summary>
        /// PI^2 / 8
        /// </summary>
        public double K
        {
            get
            {
                return 1.23370055; //PI^2 / 8
            }
        }
    }
    /// <summary>
    /// Таблица 4.2 окружности или параболы
    /// </summary>
    public class WingProf_okr : IWingProfile
    {
        /// <summary>
        /// 4 / 3
        /// </summary>
        public double K
        {
            get
            {
                return 4.0 / 3.0;
            }
        }
    }
    /// <summary>
    /// Таблица 4.2 клиновидный
    /// </summary>
    public class WingProf_klin : IWingProfile
    {
        public double K
        {
            get
            {
                return 0.25;
            }
        }
    }
    class RocketAero1
    {

      
    }
}
