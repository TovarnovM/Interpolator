﻿using System;
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
        public double Cx_tr(double mach, double x_t, double v, double a_m)
        {
            var vel = mach * a_m;
            var re = vel * L / v;
            var _2CfM0 = AeroGr.GetV("4_2", re, x_t);
            var etta = AeroGr.GetV("4_3", mach, x_t);

            var _2Cf = _2CfM0 * etta;

            var etta_c = AeroGr.GetV("4_28", C_shtr, x_t);
            return _2Cf * etta_c;
        }
        public double Cx_tr(double mach)
        {
            return Cx_tr(mach, 0, 1.51E-5, 340.3);
        }
        public double Cx_v(double mach)
        {
            double lmM21 = Lmb_k * AeroGraphs.M2min1(mach);
            double lmbC3 = Lmb_k * Math.Pow(C_shtr, 1.0 / 3.0);
            double lmbTgHi = Lmb_k * GetTgHiM(Profile.B_c_shtr);
            double cx_romb = AeroGr.GetV("4_30", lmM21, lmbC3, lmbTgHi, Etta_k);
            double fi = AeroGr.GetV("4_32", AeroGraphs.M2min1(mach) - GetTgHiM(Profile.B_c_shtr));
            return cx_romb * (1 + fi * (Profile.K - 1));
        }
        public double Cx01(double mach)
        {
            return Cx_tr(mach) + Cx_v(mach);
        }
        public double Cx01(double mach, double x_t, double v, double a_m)
        {
            return Cx_tr(mach, x_t, v, a_m) + Cx_v(mach);
        }
        public double Z_v_shtr(double mach)
        {
            var p1 = Lmb_k * AeroGraphs.M2min1(mach);
            var p2 = Lmb_k * GetTgHiM(0.5);
            var p3 = Etta_k;
            return AeroGr.GetV("3_16", p1, p2, p3);
        }

    }

    public interface IWingProfile
    {
        double K { get; }
        /// <summary>
        /// B_c_shtr - относитальная длина расположения линии макс толщин
        /// </summary>
        double B_c_shtr { get; }
    }
    /// <summary>
    /// Таблица 4.2 ромб
    /// </summary>
    public class WingProf_romb : IWingProfile
    {
        public double B_c_shtr
        {
            get
            {
                return 0.5;
            }
        }

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
        public double B_c_shtr
        {
            get
            {
                return _xc_str;
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
        public new double B_c_shtr
        {
            get
            {
                return 0.5 - 0.5 * Xc_shtr;
            }
        }
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
        public double B_c_shtr
        {
            get
            {
                return 0.5;
            }
        }
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
        public double B_c_shtr
        {
            get
            {
                return 0.5;
            }
        }

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
        public double B_c_shtr
        {
            get
            {
                return 1.0;
            }
        }

        public double K
        {
            get
            {
                return 0.25;
            }
        }
    }

    public class WingOrient
    {
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
        public double Hi_rulei { get; set; } = 90.0;
        public WingOrient(RocketWing wing)
        {
            Wing = wing;
        }
    }

    class Rocket
    {
        private bool _isSynch = false;
        public AeroGraphs AeroGr { get; set; } = null;
        public double Aero_v { get; set; } = 1.51E-5;
        public double Aero_a { get; set; } = 340.3;
        private double _alpha = 0.0;
        private double _sinAlpha = 0.0;
        private double _cosAlpha = 1.0;
        public double Alpha
        {
            get
            {
                return _alpha;
            }
            set
            {
                _alpha = value > 90.0 ? 
                            90.0 : 
                         value < -90.0 ? 
                            -90.0 : 
                         value;
                _sinAlpha = Math.Sin(_alpha * AeroGraphs.PIdiv180);
                _cosAlpha = Math.Cos(_alpha * AeroGraphs.PIdiv180);
            }

        }  
        public double SinAlpha
        {
            get
            {
                return _sinAlpha;
            }
        }
        public double CosAlpha
        {
            get
            {
                return _cosAlpha;
            }
        }
        /// <summary>
        /// мах
        /// </summary>
        public double M { get; set; } = 0.0;

        public double M_I
        {
            get
            {
                return M * Math.Sqrt(kt_I);
            }
        }
        public double M_II
        {
            get
            {
                return M * Math.Sqrt(kt_II);
            }
        }
        public double kt_I
        {
            get
            {
                return Get_kt_I_shtr(M, Alpha);
            }
        }
        public double kt_II
        {
            get
            {
                return kt_I * Get_kt_II_shtr(M_I, Alpha);
            }
        }

        public double kaa_I
        {
            get
            {
                return Get_kaa(W_I, M_I, Aero_v, Aero_a);
            }
        }
        public double kaa_II
        {
            get
            {
                return Get_kaa(W_II, M_II, Aero_v, Aero_a);
            }
        }
        public double Kaa_I
        {
            get
            {
                return Get_Kaa(W_I, M_I, Aero_v, Aero_a);
            }
        }
        public double Kaa_II
        {
            get
            {
                return Get_Kaa(W_II, M_II, Aero_v, Aero_a);
            }
        }

        public double kd0_I
        {
            get
            {
                return Get_kd0(W_I, M_I, Aero_v, Aero_a);
            }
        }
        public double kd0_II
        {
            get
            {
                return Get_kd0(W_II, M_II, Aero_v, Aero_a);
            }
        }
        public double Kd0_I
        {
            get
            {
                return Get_Kd0(W_I, M_I, Aero_v, Aero_a);
            }
        }
        public double Kd0_II
        {
            get
            {
                return Get_Kd0(W_II, M_II, Aero_v, Aero_a);
            }
        }

        public WingOrient W_I { get; set; }
        public WingOrient W_II { get; set; }
        public RocketBody Body { get; set; }

        public double Get_Kaa_star(WingOrient wing)
        {
            double d_shtr = Body.D / wing.Wing.L;
            return 1.0 + 3 * d_shtr - d_shtr * (1 - d_shtr) / wing.Wing.Etta_k;
        }
        public double Get_kaa_star(WingOrient wing)
        {
            double d_shtr = Body.D / wing.Wing.L;
            return (1 + 0.41 * d_shtr) * (1 + 0.41 * d_shtr) * (1 + 3 * d_shtr - d_shtr * (1 - d_shtr) / wing.Wing.Etta_k) / ((1 + d_shtr) * (1 + d_shtr));
        }

        public double Get_kd0_star(WingOrient wing)
        {
            double kaa_star = Get_kaa_star(wing);
            double Kaa_star = Get_Kaa_star(wing);
            return kaa_star * kaa_star / Kaa_star;
        }
        public double Get_Kd0_star(WingOrient wing)
        {
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
        public double Get_hi_pc_shtr(WingOrient wing, double mach, double v, double a_m)
        {
            double d_shtr = wing.Wing.D / wing.Wing.L;
            double l_1 = wing.X + wing.Wing.Xb + 0.5 * wing.Wing.Bb;
            //ф. 3.17
            double delta_star_shtr = 0.093 * l_1 * (1 + 0.4 * mach + 0.147 * mach * mach - 0.006 * mach * mach * mach) / (Math.Pow(mach * l_1 / (v * a_m), 0.2) * Body.D);

            var ex1 = (1.0 - d_shtr * (1 + delta_star_shtr))/(1.0-d_shtr);
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
        public double Get_hi_pc(WingOrient wing, double mach, double v, double a_m)
        {
            double d_shtr = wing.Wing.D / wing.Wing.L;
            double l_1 = wing.X + wing.Wing.Xb + 0.5 * wing.Wing.Bb;
            //ф. 3.17
            double delta_star_shtr = 0.093 * l_1 * (1 + 0.4 * mach + 0.147 * mach * mach - 0.006 * mach * mach * mach) / (Math.Pow(mach * l_1 / (v * a_m), 0.2) * Body.D);
            //ф. 3.16
            return (1 - (2 * d_shtr * d_shtr * delta_star_shtr) / (1 - d_shtr * d_shtr)) * (1 - (d_shtr * (wing.Wing.Etta_k - 1) * delta_star_shtr) / ((1 - d_shtr) * (wing.Wing.Etta_k + 1)));
        }
        public double Get_hi_pc(WingOrient wing, double mach)
        {
            return Get_hi_pc(wing, mach, 1.51E-5, 340.3);
        }
        /// <summary>
        /// 3.13
        /// </summary>
        /// <param name="mach"></param>
        /// <returns></returns>
        public double Get_hi_M(double mach)
        {
            return AeroGr.GetV("3_13", mach);
        }
        /// <summary>
        /// 3.18
        /// </summary>
        /// <param name="wing"></param>
        /// <returns></returns>
        public double Get_hi_nos(WingOrient wing)
        {            
            double l_1 = wing.X + wing.Wing.Xb + 0.5 * wing.Wing.Bb;
            double l_shtr = l_1 / Body.D;
            return 0.6 + 0.4 * (1 - Math.Exp(-0.5 * l_shtr));
        }
        public double Get_F_Lhv(WingOrient wing, double mach)
        {
            if (mach < 1.0)
                return 1.0;
            double d_shtr = Body.D / wing.Wing.L;
            double b_b_shtr = wing.Wing.Bb / (0.5 * Math.PI * Body.D * Math.Sqrt(mach * mach - 1));
            double l_hv = Body.L - wing.X + wing.Wing.Xb + wing.Wing.Bb;
            double l_hv_shtr = l_hv / (0.5 * Math.PI * Body.D * Math.Sqrt(mach * mach - 1));
            double c = (4 + 1 / wing.Wing.Etta_k) * (1 + 8 * d_shtr * d_shtr);
            double lapl1 = AeroGr.GetV("gauss", (b_b_shtr + l_hv_shtr) * Math.Sqrt(2 * c));
            double lapl2 = AeroGr.GetV("gauss", l_hv_shtr * Math.Sqrt(2 * c));
            return 1.0 - (Math.Sqrt(Math.PI) * (lapl1 - lapl2)) / (2 * b_b_shtr * Math.Sqrt(c));
        }

        public double Get_Kaa(WingOrient wing, double mach, double v, double a_m)
        {
            double Kaastar = Get_Kaa_star(wing);
            double kaastar = Get_kaa_star(wing);
            double hipc = Get_hi_pc(wing,mach,v,a_m);
            double him = Get_hi_M(mach);
            double hinos = Get_hi_nos(wing);
            double FLhv = Get_F_Lhv(wing, mach);
            return (kaastar + (Kaastar - kaastar) * FLhv) * hipc * him * hinos;
        }
        public double Get_kaa(WingOrient wing, double mach, double v, double a_m)
        {
            double kaastar = Get_kaa_star(wing);
            double hipc = Get_hi_pc(wing, mach, v, a_m);
            double him = Get_hi_M(mach);
            double hinos = Get_hi_nos(wing);
            return kaastar * hipc * him * hinos;
        }

        public double Get_kd0(WingOrient wing, double mach, double v, double a_m)
        {
            double kd0star = Get_kd0_star(wing);
            double hipc = Get_hi_pc_shtr(wing, mach, v, a_m);
            double him = Get_hi_M(mach);
            return kd0star * hipc * him;
        }
        public double Get_Kd0(WingOrient wing, double mach, double v, double a_m)
        {
            double Kd0star = Get_Kaa_star(wing);
            double kd0star = Get_kaa_star(wing);
            double hipc = Get_hi_pc_shtr(wing, mach, v, a_m);
            double him = Get_hi_M(mach);
            double FLhv = Get_F_Lhv(wing, mach);
            return (kd0star + (Kd0star - kd0star) * FLhv) * hipc * him;
        }

        public double Get_kt_I_shtr(double mach, double alpha)
        {
            double lmbda_nos;
            if (Body.Nose is RocketNos_ConePlusCyl)
                lmbda_nos = Body.Lmb_nos;
            else
            {
                double cx_nos = Body.Cx_nose(mach);
                InterpXY lmb_ot_Cx = new InterpXY();
                foreach (var item in (AeroGr.Graphs["4_11"] as Interp2D)._data)
                {
                    double lmb = item.Key;
                    double cx_tmp = item.Value.GetV(mach);
                    lmb_ot_Cx.Add(cx_tmp, lmb);
                }
                lmbda_nos = lmb_ot_Cx.GetV(cx_nos);
            }
            return AeroGr.GetV("3_21", lmbda_nos, mach);
        }
        public double Get_kt_II_shtr(double mach, double alpha)
        {
            double b_a_I = W_I.Wing.Ba_k;
            double x1 = W_I.X + W_I.Wing.Xa_k + b_a_I;
            double x2 = W_II.X + W_II.Wing.Xa_k + 0.5 * W_II.Wing.Ba_k;
            double x = x2 - x1;
            double x_shtr = x / b_a_I;
            double k_t_star = AeroGr.GetV("3_22", mach, x_shtr);
            double s_I = W_I.Wing.S * W_I.N_console_4Cx;
            double s_II = W_II.Wing.S * W_II.N_console_4Cx;
            return (k_t_star + s_II / s_I) / (1 + s_II / s_I);
        }

        public double Get_eps_sr_alpha(double mach, double alpha)
        {
            var z_v_shtr = W_I.Wing.Z_v_shtr(mach);
            var d_I = W_I.Wing.D;
            var l_I = W_I.Wing.L;
            var z_v = 0.5 * (d_I + z_v_shtr * (l_I - d_I));

            var x_v = W_I.Wing.GetX(z_v) + W_I.Wing.GetB(z_v) - W_I.Wing.Xb - W_I.Wing.Bb* W_I.X_povor_otn;
            var x_II =  W_II.Wing.Xa_k + W_II.Wing.Ba_k*0.5 -
                W_I.Wing.Xb + W_I.Wing.Bb * W_I.X_povor_otn + x_v*Math.Cos(W_I.Delta*AeroGraphs.PIdiv180);
            var y_II = 0.0;
            var y_v = x_II * Math.Sin(alpha * AeroGraphs.PIdiv180) - x_v * Math.Sin(W_I.Delta * AeroGraphs.PIdiv180) + y_II;

            var _1_div_etta = 1 / W_II.Wing.Etta_k;
            var _2zv_div_lII = 2.0 * z_v / W_II.Wing.L;
            var _2yv_div_lII = 2.0 * y_v / W_II.Wing.L;
            var d_II_shtr = W_II.Wing.D / W_II.Wing.L;
            var i = AeroGr.GetV("3_17", _2zv_div_lII, _2yv_div_lII, d_II_shtr, _1_div_etta);
            var ksi = 1.0;
            return AeroGraphs._180divPI * i * W_II.Wing.L_k * W_I.Wing.GetCy1a(mach) * kaa_I * ksi / (2 * Math.PI * z_v_shtr * W_II.Wing.L_k * W_I.Wing.Lmb_k * Kaa_II);
        }
        public double Get_eps_sr_delta(double mach, double alpha)
        {
            var z_v_shtr = W_I.Wing.Z_v_shtr(mach);
            var d_I = W_I.Wing.D;
            var l_I = W_I.Wing.L;
            var z_v = 0.5 * (d_I + z_v_shtr * (l_I - d_I));

            var x_v = W_I.Wing.GetX(z_v) + W_I.Wing.GetB(z_v) - W_I.Wing.Xb - W_I.Wing.Bb * W_I.X_povor_otn;
            var x_II = W_II.Wing.Xa_k + W_II.Wing.Ba_k * 0.5 -
                W_I.Wing.Xb + W_I.Wing.Bb * W_I.X_povor_otn + x_v * Math.Cos(W_I.Delta * AeroGraphs.PIdiv180);
            var y_II = 0.0;
            var y_v = x_II * Math.Sin(alpha * AeroGraphs.PIdiv180) - x_v * Math.Sin(W_I.Delta * AeroGraphs.PIdiv180) + y_II;

            var _1_div_etta = 1 / W_II.Wing.Etta_k;
            var _2zv_div_lII = 2.0 * z_v / W_II.Wing.L;
            var _2yv_div_lII = 2.0 * y_v / W_II.Wing.L;
            var d_II_shtr = W_II.Wing.D / W_II.Wing.L;
            var i = AeroGr.GetV("3_17", _2zv_div_lII, _2yv_div_lII, d_II_shtr, _1_div_etta);
            var ksi = 1.0;
            return AeroGraphs._180divPI * i * W_II.Wing.L_k * W_I.Wing.GetCy1a(mach) * kaa_I * ksi / (2 * Math.PI * z_v_shtr * W_II.Wing.L_k * W_I.Wing.Lmb_k * Kaa_II);
        }

        public double Get_n(WingOrient wing, double mach)
        {
            var k_sh = AeroGr.GetV("k_shel", mach);
            return k_sh * Math.Cos(wing.Hi_rulei * AeroGraphs.PIdiv180);
        }
    }
}
