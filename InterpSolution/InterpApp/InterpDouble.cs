using MoreLinq;
using SerializableGenerics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;

namespace Interpolator {
    public interface IInterpElem : IInterpElemAbs<double> { }

    [Serializable, XmlRoot(nameof(InterpDouble))]
    public class InterpDouble : InterpElemAbs<double>, IInterpElem {
        public InterpDouble(double value) : base(value) {

        }
        public InterpDouble() : this(0) {

        }
        public override object Clone() {
            return new InterpDouble(Value);
        }
    }



    [Serializable]
    public class Interp<T> : InterpAbs<T, double>, IInterpElem where T : IInterpElem {

        public override double InterpMethodLine(int n, params double[] t) {
            double t1, t2, y1, y2;
            t1 = _data.Keys[n];
            t2 = _data.Keys[n + 1];
            y1 = GetVSub(n, t);
            n = n + 1;
            y2 = GetVSub(n, t);
            return y1 + (y2 - y1) * (t.Last() - t1) / (t2 - t1);
        }

        public override double InerpMethodSpecial4_3_17(int n, params double[] t) {
            double t1, t2, y1, y2;
            t1 = _data.Keys[n];
            t2 = _data.Keys[n + 1];
            y1 = GetVSub(n, t);
            n++;
            y2 = GetVSub(n, t);
            if (y2 == 0 && y1 != 0) {
                double _2z_l = t[0];
                double _2y_l = t[1];
                double D_2 = t[2];
                double koeff = (_2z_l - D_2) / (1 - D_2);
                n--;
                _2z_l = t1 + koeff * (1 - t1);
                y1 = GetVSub(n, t1 + koeff * (1 - t1), _2y_l, D_2);
                n++;
                _2z_l = t2 + koeff * (1 - t2);
                y2 = GetVSub(n, t2 + koeff * (1 - t2), _2y_l, D_2);
            }

            return y1 + (y2 - y1) * (t.Last() - t1) / (t2 - t1);
        }





    }




    [XmlRoot(nameof(InterpXY))]
    public class InterpXY : Interp<InterpDouble> {
        [XmlIgnore]
        private bool _isSynch = false;
        private double[] _x;
        private double[] _y;
        private double[] _k;
        private double[] _b;
        private int _length;
        public override int AddElement(double t, InterpDouble element) {
            _isSynch = false;
            return base.AddElement(t, element);
        }
        public override void RemoveElement(int elemIndex) {
            _isSynch = false;
            base.RemoveElement(elemIndex);
        }
        public void SynchArrays() {
            _length = _data.Count;
            _x = new double[_length];
            _y = new double[_length];
            _k = new double[_length];
            _b = new double[_length];
            int indx = 0;
            foreach (var item in _data) {
                _x[indx] = item.Key;
                _y[indx] = item.Value.Value;
                ++indx;
            }
            for (int i = 0; i < _length - 1; i++) {
                _k[i] = (_y[i + 1] - _y[i]) / (_x[i + 1] - _x[i]);
                _b[i] = _y[i] - _k[i] * _x[i];
            }
            _isSynch = true;
        }
        public double InterpMethodXYLine(int n, double t) {
            return _k[n] * t + _b[n];
        }
        public override int SetN(double t) {
            try {
                int n = N;
                if (!_isSynch)
                    SynchArrays();
                if (_length < 1)
                    return 0;
                if (n < 0) {
                    if (_x[0] > t)
                        return -1;
                    n = 0;
                }
                int min, max;
                int lengthM1 = _length - 1;
                if (_x[n] <= t) {
                    if (n == lengthM1 || _x[n + 1] > t)
                        return n;
                    ++n;
                    if (n == lengthM1 || _x[n + 1] > t)
                        return n;
                    if (_x.Last() <= t) {
                        n = lengthM1;
                        return lengthM1;
                    }
                    min = n;
                    max = lengthM1;
                } else {
                    if (n == 0 || _x[n - 1] <= t)
                        return --n;
                    if (_x[0] > t) {
                        n = -1;
                        return n;
                    }
                    min = 0;
                    max = n;
                }
                while (min != max) {
                    n = (min + max) / 2;
                    if (_x[n] <= t) {
                        if (_x[n + 1] > t)
                            return n;
                        min = n;
                    } else
                        max = n;
                }
                n = min;
                return n;

            } catch (Exception) {
                return 0;
            }
        }
        public override double GetV(params double[] t) {
            var tt = t[0];
            int n = SetN(tt);

            //Экстраполяция (пока только 2 типа)
            if (n < 0 || n == _length - 1 || _length == 1) {
                ExtrapolType ET_temp = n < 0 ? ET_left : ET_right;
                n = n < 0 ? 0 : n;
                N = n;
                switch (ET_temp) {
                    case ExtrapolType.etZero:
                        if (n == _length - 1 && (tt == _x[0] || tt == _x[_length - 1])) {
                            return _y[n];
                        }
                        return 0;
                    case ExtrapolType.etValue:
                        return _y[n];
                    case ExtrapolType.etMethod_Line: {
                            n -= n == _length - 1 ?
                                1 :
                                0;
                            N = n;
                            return InterpMethodXYLine(n, tt);
                        }
                    case ExtrapolType.etRepeat:
                        //Не забыть изменить в Interp<>
                        RepeatShift(ref tt);
                        return GetV(tt);
                    default:
                        return _y[n];
                }
            }
            if (InterpType == InterpolType.itLine)
                return InterpMethodXYLine(n, tt);
            if (InterpType == InterpolType.itStep)
                return _y[n];
            return InterpMethodXYLine(n, tt);
        }

        public int Add(double t, double value, bool allowDublicates = false) {
            if (!allowDublicates && _data.ContainsKey(t))
                return 0;
            return AddElement(t, new InterpDouble(value));
        }
        public void CopyDataFrom(InterpXY parent, bool delPrevData = false) {
            if (delPrevData)
                _data.Clear();
            _data.Capacity = _data.Capacity > (_data.Count + parent.Data.Count) ?
                                (int)((_data.Count + parent.Data.Count) * 1.5) :
                                _data.Capacity;
            foreach (var item in parent.Data) {
                Add(item.Key, item.Value.Value);
            }
        }
        public InterpXY CopyMe() {
            var result = new InterpXY();
            result.CopyParamsFrom(this);
            result.CopyDataFrom(this);
            return result;
        }
        public void AddData(double[] ts, double[] vals, bool delPrevData = false) {
            if (ts.Length != vals.Length || ts.Length == 0)
                throw new ArgumentException($"Неправильные параметры, походу разные длины векторов");
            if (delPrevData)
                _data.Clear();
            _data.Capacity = _data.Capacity > (_data.Count + ts.Length) ?
                            (int)((_data.Count + ts.Length) * 1.5) :
                            _data.Capacity;
            for (int i = 0; i < ts.Length; i++) {
                Add(ts[i], vals[i]);
            }
        }
        public InterpXY() : base() {

        }
        public InterpXY(double[] ts, double[] vals) : this() {
            this.AddData(ts, vals);
        }
        public InterpXY(InterpXY parent) {
            CopyParamsFrom(this);
            CopyDataFrom(this);
        }
        public static InterpXY LoadFromXmlFile(string fileName) => InterpXY.LoadFromXmlFile<InterpXY>(fileName);
        public static InterpXY LoadFromXmlString(string fileStr) => InterpXY.LoadFromXmlString<InterpXY>(fileStr);
        public override void Dispose() {
            base.Dispose();
            _x = null;
            _y = null;
            _k = null;
            _b = null;
        }

        public IEnumerable<Vector> Points() {
            foreach (var item in _data) {
                yield return new Vector(item.Key, item.Value.Value);
            }
        }

        public override string ToString() {
            var strb = new StringBuilder();
            strb.Append($"{Title} = [ ");
            foreach (var d in _data) {
                strb.Append($"({d.Key:0.###};{d.Value.Value:0.###}) ");
            }
            strb.Append($"]");
            return strb.ToString();
        }
    }

    [XmlRoot(nameof(Interp2D))]
    public class Interp2D : Interp<InterpXY> {
        public Interp2D CopyMe() {
            var result = new Interp2D();
            result.CopyParamsFrom(this);
            foreach (var item in _data) {
                result.AddElement(item.Key, item.Value.CopyMe());
            }
            return result;
        }
        public void SynchArrays() {
            foreach (var ixy in _data.Values) {
                ixy.SynchArrays();
            }
        }
        /// <summary>
        /// На вход подается прямоугольная матрица. первый индекс - "строка", второй - "столбец";
        /// элемент [0,0] не учитывается; 
        /// Нулевая строка (начиная с 1 столбца) представляет собой семейство аргументов t для идентифицикации интерполяторов XY;
        /// Нулевой столбец (начиная с 1 строки) представляет собой семейство аргументов t для идентифицикации
        /// элементов внутри интерполяторов XY;
        /// Т.е. (количество столбцов - 1) = количеству одномерных интерполяторов внутри объекта,
        /// а    (количество строк - 1)    = количеству элементов внутри каждого одномерного интерполятора.
        /// </summary>
        /// <param name="m">матрица c данными</param>
        public void ImportDataFromMatrix(double[,] m, bool copyParams = true) {
            if (m.GetLength(0) < 2 || m.GetLength(1) < 2)
                throw new ArgumentException($"Неправильные параметры, походу разные длины векторов");
            var tsXY = new double[m.GetLength(1) - 1];
            var tsInXY = new double[m.GetLength(0) - 1];
            var vecs = new double[tsXY.Length][];

            for (int i = 0; i < tsXY.Length; i++)
                tsXY[i] = m[0, i + 1];

            for (int i = 0; i < tsInXY.Length; i++)
                tsInXY[i] = m[i + 1, 0];

            for (int i = 0; i < tsXY.Length; i++) {
                vecs[i] = new double[tsInXY.Length];
                for (int j = 0; j < tsInXY.Length; j++)
                    vecs[i][j] = m[j + 1, i + 1];
            }
            ImportDataFromVectors(tsXY, tsInXY, vecs, copyParams);
        }
        /// <summary>
        /// tsXY.Length == vecs.Length == N
        /// tsInXY.Length == vecs[0..N].Length
        /// </summary>
        /// <param name="tsXY">векстор с аргументами t для идентифицикации интерполяторов XY </param>
        /// <param name="tsInXY">векстор с аргументами t для идентифицикации элементов внутри интерполяторов XY</param>
        /// <param name="vecs">массив векторов для идентификации Интерполяторов XY</param>
        public void ImportDataFromVectors(double[] tsXY, double[] tsInXY, double[][] vecs, bool copyParams = true) {
            if (tsXY.Length != vecs.Length || tsXY.Length == 0 || tsInXY.Length == 0)
                throw new ArgumentException($"Неправильные параметры, походу разные длины векторов");
            for (int i = 0; i < vecs.Length; i++)
                if (tsInXY.Length != vecs[i].Length)
                    throw new ArgumentException($"Неправильные параметры, походу разные длины векторов");
            _data.Clear();
            for (int i = 0; i < tsXY.Length; i++) {
                InterpXY tmpInterp = new InterpXY(tsInXY, vecs[i]);
                if (copyParams)
                    tmpInterp.CopyParamsFrom(this);
                AddElement(tsXY[i], tmpInterp);
            }

        }
        public static Interp2D LoadFromXmlFile(string fileName) => Interp2D.LoadFromXmlFile<Interp2D>(fileName);
        public static Interp2D LoadFromXmlString(string fileStr) => Interp2D.LoadFromXmlString<Interp2D>(fileStr);
    }

    [XmlRoot(nameof(Interp3D))]
    public class Interp3D : Interp<Interp2D> {
        public Interp3D CopyMe() {
            var result = new Interp3D();
            result.CopyParamsFrom(this);
            foreach (var item in _data) {
                result.AddElement(item.Key, item.Value.CopyMe());
            }
            return result;
        }
        public static Interp3D LoadFromXmlFile(string fileName) => Interp3D.LoadFromXmlFile<Interp3D>(fileName);
        public static Interp3D LoadFromXmlString(string fileStr) => Interp3D.LoadFromXmlString<Interp3D>(fileStr);
    }

    [XmlRoot(nameof(Interp4D))]
    public class Interp4D : Interp<Interp3D> {
        public Interp4D CopyMe() {
            var result = new Interp4D();
            result.CopyParamsFrom(this);
            foreach (var item in _data) {
                result.AddElement(item.Key, item.Value.CopyMe());
            }
            return result;
        }
        public static Interp4D LoadFromXmlFile(string fileName) => Interp4D.LoadFromXmlFile<Interp4D>(fileName);
        public static Interp4D LoadFromXmlString(string fileStr) => Interp4D.LoadFromXmlString<Interp4D>(fileStr);
    }

    //Далее идет интерполяторы для графиков "линий уровня"
    /// <summary>
    /// Класс полилинии в двумерном пространстве, состоящей из линейных отрезков. Координаты типа Vector
    /// </summary>
    [XmlRoot(nameof(LevelLine))]
    public class LevelLine : InterpDouble {
        public static LevelLine LoadFromXmlFile(string fileName) {
            try {
                XmlSerializer serial = new XmlSerializer(typeof(LevelLine));
                var sw = new StreamReader(fileName);
                LevelLine result = (LevelLine)serial.Deserialize(sw);
                sw.Close();
                return result;
            } catch (Exception) { }
            return null;
        }
        public static LevelLine LoadFromXmlString(string fileStr) {
            try {
                XmlSerializer serial = new XmlSerializer(typeof(LevelLine));
                var sw = new StringReader(fileStr);
                LevelLine result = (LevelLine)serial.Deserialize(sw);
                sw.Close();
                return result;
            } catch (Exception) { }
            return null;
        }
        public List<Vector> pointsList = new List<Vector>();
        public LevelLine(double Value = 0.0) : base(Value) {

        }

        public LevelLine() : base() {

        }
        public int Count { get { return pointsList.Count; } }
        public void AddPoint(double x, double y) {
            pointsList.Add(new Vector(x, y));
        }
        public void AddPoints(double[] x, double[] y) {
            int n = Math.Min(x.Length, y.Length);
            for (int i = 0; i < n; i++) {
                AddPoint(x[i], y[i]);
            }
        }
        public void AddPoints(double[][] xy) {
            foreach (double[] item in xy) {
                if (item.Length > 1)
                    AddPoint(item[0], item[1]);
            }
        }
        public void AddPoint(Vector point) {
            pointsList.Add(point);
        }
        public Vector NearestToPoint(Vector point) {
            if (pointsList.Count == 0)
                return new Vector(0, 0);
            Vector result = new Vector(double.MaxValue, double.MaxValue);
            for (int i = 0; i < pointsList.Count - 1; i++) {
                Vector tmpResult = MinimumDistanceVector(pointsList[i], pointsList[i + 1], point);
                if ((tmpResult - point).LengthSquared < (result - point).LengthSquared)
                    result = tmpResult;
            }
            return result;
        }
        public double GetDistanceTo(Vector point) {
            if (pointsList.Count == 0)
                return -1;
            var nearestP = NearestToPoint(point);
            return (point - nearestP).Length;
        }

        public static Vector MinimumDistanceVector(Vector v, Vector w, Vector p) {
            // Return minimum distance vector between line segment vw and point p
            // i.e. |w-v|^2 -  avoid a sqrt
            double l2 = (w - v).LengthSquared;
            // v == w case
            if (l2 == 0.0)
                return w - p;
            // Consider the line extending the segment, parameterized as v + t (w - v).
            // We find projection of point p onto the line. 
            // It falls where t = [(p-v) . (w-v)] / |w-v|^2
            // We clamp t from [0,1] to handle points outside the segment vw.
            double t = Math.Max(0, Math.Min(1, (p - v) * (w - v) / l2));
            return v + t * (w - v);  // Projection falls on the segment
        }
        public bool IsCrossMe(Vector b0, Vector b1) {
            for (int i = 0; i < pointsList.Count - 1; i++) {
                if (LinesIntersect(pointsList[i], pointsList[i + 1], b0, b1))
                    return true;
            }
            return false;
        }

        public static bool BoundingBoxesIntersect(Vector a0, Vector a1, Vector b0, Vector b1) {
            return Math.Min(a0.X, a1.X) <= Math.Max(b0.X, b1.X)
                    && Math.Max(a0.X, a1.X) >= Math.Min(b0.X, b1.X)
                    && Math.Min(a0.Y, a1.Y) <= Math.Max(b0.Y, b1.Y)
                    && Math.Max(a0.Y, a1.Y) >= Math.Min(b0.Y, b1.Y);
        }
        public static bool IsPointOnLine(Vector a0, Vector a1, Vector p) {
            // Move the image, so that a.first is on (0|0)
            return Math.Abs(CrossProduct(a1 - a0, p - a0)) < EPSILON;
        }
        public static bool IsPointRightOfLine(Vector a0, Vector a1, Vector p) {
            // Move the image, so that a.first is on (0|0)
            return CrossProduct(a1 - a0, p - a0) < 0;
        }
        public static bool LineSegmentTouchesOrCrossesLine(Vector a0, Vector a1, Vector b0, Vector b1) {
            return IsPointOnLine(a0, a1, b0)
                        || IsPointOnLine(a0, a1, b1)
                        || (IsPointRightOfLine(a0, a1, b0) ^
                             IsPointRightOfLine(a0, a1, b1));
        }
        public static bool LinesIntersect(Vector a0, Vector a1, Vector b0, Vector b1) {

            return BoundingBoxesIntersect(a0, a1, b0, b1)
                    && LineSegmentTouchesOrCrossesLine(a0, a1, b0, b1)
                    && LineSegmentTouchesOrCrossesLine(b0, b1, a0, a1);
        }
        public static double CrossProduct(Vector a, Vector b) {
            return a.X * b.Y - b.X * a.Y;
        }
        public static double EPSILON = 0.000001;

        public override object Clone() {
            var myClone = new LevelLine(Value);
            myClone.pointsList.AddRange(pointsList);
            return myClone;
        }

        public override void Dispose() {
            base.Dispose();
            pointsList.Clear();
            pointsList = null;
        }
    }
    /// <summary>
    /// График, составленный из линий уровня
    /// </summary>
    [XmlRoot(nameof(PotentGraff2P))]
    public class PotentGraff2P : Interp<LevelLine> {
        /// <summary>
        /// Воооот тут  t[0] = x
        ///             t[1] = y
        /// return интерполированный параметр Value от соседних LevelLine'ов
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public override double GetV(params double[] t) {
            if (_data.Count == 0 || t.Length < 2)
                return 0.0;
            if (_data.Count == 1)
                return _data.Values[0].Value;

            var p = new Vector(t[0], t[1]);
            Vector[] nPoints = new Vector[_data.Count];
            double[] nL = new double[_data.Count];
            int ni = 0;
            for (int i = 0; i < _data.Count; i++) {
                //ближайшая точка на i-ой кривой от точки p 
                nPoints[i] = _data.Values[i].NearestToPoint(p);
                //рассотяние от точки p до i-ой кривой
                nL[i] = (nPoints[i] - p).Length;
                //запоминаем номер ближайшей кривой в ni
                if (nL[i] < nL[ni])
                    ni = i;
            }
            int nj = -1;
            for (int i = 0; i < _data.Count; i++) {
                //Нет смысла сравнивать один и тот же вектор
                if (i == ni)
                    continue;
                //если вектор кратчайшего расстоняия НЕ пересекает самую ближнюю линию, значит точка лежит между линиями
                if (!_data.Values[ni].IsCrossMe(p, nPoints[i])) {
                    //если дебют, то запоминаем
                    if (nj < 0) {
                        nj = i;
                        continue;
                    }
                    //если он самый короткий из "тупых"), то запоминаем
                    if (nL[i] < nL[nj])
                        nj = i;
                }
            }
            //Интерполировать нечего, мы за пределами. Берем ближайшее значение
            if (nj == -1)
                return _data.Values[ni].Value;
            //линейно интерполируем между соседними линиями
            if (nL[nj] != 0)
                return _data.Values[ni].Value + nL[ni] * (_data.Values[nj].Value - _data.Values[ni].Value) / (nL[nj] + nL[ni]);
            else
                return _data.Values[nj].Value;
        }
        /// <summary>
        /// Проверяет пересечения линий уровня
        /// true - всё хоршо, данные хорошие
        /// false - данные плохие
        /// </summary>
        /// <returns></returns>
        public bool ValidData() {
            for (int i = 0; i < _data.Count - 1; i++) {
                for (int j = 0; j < _data.Values[i].Count - 1; j++) {
                    for (int k = i + 1; k < _data.Count; k++) {
                        for (int j1 = 0; j1 < _data.Values[k].Count - 1; j1++) {
                            if (LevelLine.LinesIntersect(_data.Values[i].pointsList[j], _data.Values[i].pointsList[j + 1],
                                                         _data.Values[k].pointsList[j1], _data.Values[k].pointsList[j1 + 1]))
                                return false;
                        }
                    }

                }
            }
            return true;
        }
        public static PotentGraff2P LoadFromXmlFile(string fileName) => PotentGraff2P.LoadFromXmlFile<PotentGraff2P>(fileName);
        public static PotentGraff2P LoadFromXmlString(string fileStr) => PotentGraff2P.LoadFromXmlString<PotentGraff2P>(fileStr);
    }
    /// <summary>
    /// Семейство графиков, каждый из которых представляет собой семейство графиков линий уровня
    /// </summary>
    [XmlRoot(nameof(PotentGraff3P))]
    public class PotentGraff3P : Interp<PotentGraff2P> {
        public static PotentGraff3P LoadFromXmlFile(string fileName) => PotentGraff3P.LoadFromXmlFile<PotentGraff3P>(fileName);
        public static PotentGraff3P LoadFromXmlString(string fileStr) => PotentGraff3P.LoadFromXmlString<PotentGraff3P>(fileStr);
    }
    /// <summary>
    /// Семейство графиков, каждый из которых представляет собой PotentGraff3D
    /// </summary>
    [XmlRoot(nameof(PotentGraff4P))]
    public class PotentGraff4P : Interp<PotentGraff3P> {
        public static PotentGraff4P LoadFromXmlFile(string fileName) => PotentGraff4P.LoadFromXmlFile<PotentGraff4P>(fileName);
        public static PotentGraff4P LoadFromXmlString(string fileStr) => PotentGraff4P.LoadFromXmlString<PotentGraff4P>(fileStr);
    }

    public static class InterpXY_smoother {
        public static InterpXY GetSmootherByN_uniform(this InterpXY who, int n) {
            var res = new InterpXY() {
                Title = who.Title
            };
            var que = new Queue<double>(n + 1);
            double QueSred()
            {
                if (que.Count == 0)
                    return 0d;
                double sum = 0;
                foreach (var val in que) {
                    sum += val;
                }
                return sum / que.Count;
            }
            foreach (var val in who.Data) {
                if (que.Count >= n) {
                    que.Dequeue();
                }
                if (que.Count < n) {
                    que.Enqueue(val.Value.Value);
                }
                res.Add(val.Key, QueSred());
            }
            return res;
        }
        public static InterpXY GetSmootherByN_line(this InterpXY who, int n) {
            var res = new InterpXY() {
                Title = who.Title
            };
            var que = new Queue<double>(n + 1);
            var koeff = Enumerable.Range(0, n)
                .Select(i => (double)(n - i) / n)
                .ToArray() ;
            var koeffSum = Enumerable.Range(1, n)
                .Select(i => koeff.Take(i).Sum())
                .ToArray();

            double QueSred()
            {
                if (que.Count == 0)
                    return 0d;
                double sum = 0;
                int i = 0;
                foreach (var val in que.Reverse()) {
                    sum += val*koeff[i++];
                }

                return sum / koeffSum[i-1];
            }
            foreach (var val in who.Data) {
                if (que.Count >= n) {
                    que.Dequeue();
                }
                if (que.Count < n) {
                    que.Enqueue(val.Value.Value);
                }
                res.Add(val.Key, QueSred());
            }
            return res;
        }
        public static InterpXY GetSmootherByN_Median(this InterpXY who, int n) {
            var res = new InterpXY() {
                Title = who.Title
            };
            var que = new Queue<double>(n + 1);
            double QueSred()
            {
                if (que.Count == 0)
                    return 0d;
                double sum = 0;
                int i = 0;
                var arr = que.ToArray();
                Array.Sort(arr);
                return arr[(arr.Length - 1) / 2];
            }
            foreach (var val in who.Data) {
                if (que.Count >= n) {
                    que.Dequeue();
                }
                if (que.Count < n) {
                    que.Enqueue(val.Value.Value);
                }
                res.Add(val.Key, QueSred());
            }
            return res;
        }

        struct Elem {
            public double t, val;
            public Elem(double t, double val) {
                this.t = t;
                this.val = val;
            }
        }
        public static InterpXY GetSmootherByT_uniform(this InterpXY who, double dt_back, double dt_front = 0d) {
            var res = new InterpXY() {
                Title = who.Title
            };
            var que_b = new Queue<Elem>();
            var que_f = new Queue<Elem>();
            double Que_b_S()
            {
                if (que_b.Count == 0)
                    return 0d;
                double sum = 0;
                var qList = que_b.ToList();
                for (int i = 1; i < qList.Count; i++) {
                    sum += (qList[i].val + qList[i - 1].val) * (qList[i].t - qList[i - 1].t) / 2;
                }
                var t0 = qList[qList.Count - 1].t - dt_back;
                sum += (qList[0].val + who.GetV(t0)) * (qList[0].t - t0) / 2;
                return sum;
            }

            double Que_f_S()
            {
                if (que_f.Count == 0)
                    return 0d;
                double sum = 0;
                var qList = que_f.ToList();
                for (int i = 1; i < qList.Count; i++) {
                    sum += (qList[i].val + qList[i - 1].val) * (qList[i].t - qList[i - 1].t) / 2;
                }
                var t_last = qList[0].t + dt_front;
                sum += (qList[qList.Count-1].val + who.GetV(t_last)) * (t_last - qList[qList.Count - 1].t) / 2;
                return sum;
            }


            var data = who.Data.ToList();

            for (int i = 0; i < data.Count; i++) {
                que_b.Enqueue(new Elem(data[i].Key, data[i].Value.Value));
                if (data[i].Key - que_b.Peek().t >= dt_back) {
                    que_b.Dequeue();
                }

                for (int j = i + que_f.Count; j < data.Count; j++) {
                    if (data[j].Key < data[i].Key + dt_front) {
                        que_f.Enqueue(new Elem(data[j].Key, data[j].Value.Value));
                    } else {
                        break;
                    }
                }
                if (que_f.Count >0 &&  que_f.Peek().t < data[i].Key) {
                    que_f.Dequeue();
                }
                res.Add(data[i].Key, (Que_b_S() + Que_f_S()) / (dt_back + dt_front));               
            }
            return res;
        }
    }
    public static class InterpXY_finder {
        public static double Get_MaxElem_T(this InterpXY who) {
            return who.Get_MaxElem_T(who.Data.First().Key, who.Data.Last().Key);
        }
        public static double Get_MaxElem_T(this InterpXY who, double t0, double t1) {
            return who.Data
                .SkipWhile(d => d.Key < t0)
                .SkipWhile(d => d.Key <= t1)
                .Max(d => d.Value.Value);
        }
        public static double Get_MinElem_T(this InterpXY who) {
            return who.Get_MinElem_T(who.Data.First().Key, who.Data.Last().Key);
        }
        public static double Get_MinElem_T(this InterpXY who, double t0, double t1) {
            return who.Data
                .SkipWhile(d => d.Key < t0)
                .SkipWhile(d => d.Key <= t1)
                .Min(d => d.Value.Value);
        }
        public static double Get_Integral(this InterpXY who) {
            return who.Get_Integral(who.Data.First().Key, who.Data.Last().Key);
        }
        public static double Get_Integral(this InterpXY who, double t0, double t1) {
            var y0 = who.GetV(t0);
            var y1 = 0d;
            double sum = 0d;
            foreach (var item in who.Data
                .SkipWhile(d => d.Key < t0)
                .TakeWhile(d => d.Key <= t1)) {

                y1 = item.Value.Value;
                sum += (y0 + y1) * (item.Key - t0) * 0.5;
                y0 = y1;
                t0 = item.Key;
            }
            y1 = who.GetV(t1);
            sum += (y0 + y1) * (t1 - t0) * 0.5;
            return sum;

        }

        public static InterpXY GetInterpMultyConst(this InterpXY who, double mnozj) {
            var res = new InterpXY();
            foreach (var d in who.Data) {
                res.Add(d.Key, d.Value.Value * mnozj);
            }
            return res;
        }
    }
}
