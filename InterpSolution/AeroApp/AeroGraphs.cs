using AeroApp.Properties;
using Interpolator;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace RocketAero {
    /// <summary>
    /// Там графики из Л-Ч
    /// </summary>
    public class AeroGraphs : ICloneable {
        private Dictionary<string, IInterpElem> _graphs = new Dictionary<string, IInterpElem>();
        public Dictionary<string, IInterpElem> Graphs { get { return _graphs; } }
        public AeroGraphs(bool loadNew = true) {
            if (loadNew)
                LoadMe();
        }
        public AeroGraphs(AeroGraphs cloneMe) {
            foreach (var graph in cloneMe.Graphs) {
                Graphs.Add(graph.Key, (IInterpElem)graph.Value.Clone());
            }
        }


        protected void LoadMe() {
            var resourses = Resources.ResourceManager.GetResourceSet(System.Globalization.CultureInfo.CurrentUICulture, true, true);
            foreach (DictionaryEntry item in resourses) {
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
            }
        }
        public string CutMyString(string cutThis) {
            string result = cutThis;
            cutThis = cutThis.Trim('_');
            if (cutThis.EndsWith("_4D") ||
                cutThis.EndsWith("_3D") ||
                cutThis.EndsWith("_2D") ||
                cutThis.EndsWith("_1D") ||
                cutThis.EndsWith("_4P"))
                cutThis = cutThis.Remove(cutThis.Length - 3);
            return cutThis;
        }
        public int HowManyParams(string graphNum) {
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
        public bool HasGraph(string graphNum) {
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
        public double GetV(string graphNum, params double[] t) {
            try {
                return _graphs[CutMyString(graphNum)].GetV(t);
            }
            catch (Exception ex) {
                Debug.WriteLine(ex.Message);
                return 0.0;
            }

        }
        public static double PIdiv180 = 0.01745329251994329576923690768489D;
        public static double _180divPI = 57.295779513082320876798154814105D;
        public static double M2min1(double mach) {
            return Math.Sqrt(Math.Abs(mach * mach - 1)) * Math.Sign(mach - 1);
        }

        public object Clone() {
            return new AeroGraphs(this);
        }
    }
}
