using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using SerializableGenerics;
using System.IO;
using System.Windows;

namespace Interpolator
{
    public interface IInterpElem
    {
        double GetV(params double[] t);
    }
    public interface IInterpParams
    {
        ExtrapolType ET_left { get; set; }
        ExtrapolType ET_right { get; set; }
        InterpolType InterpType { get; set; }
    }

    [XmlRoot(nameof(InterpDouble))]
    public class InterpDouble : IInterpElem
    {
        [XmlAttribute]
        public double Value { get; set; }
        /// <summary>
        /// Основной метод) Просто получить значение
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public double GetV(params double[] t)
        {
            return Value;            
        }

        public InterpDouble CopyMe()
        {
            return new InterpDouble(Value);
        }

        public InterpDouble(double value)
        {
            Value = value;
        }
        public InterpDouble()
        {
            Value = 0.0;
        }
    }
    //тип экстраполяции  = 0, = крайн значению, = продолжению метода, вызывает ошибку
    public enum ExtrapolType { etZero, etValue, etMethod_Line, etError };
    public enum InterpolType { itStep, itLine, itSpecial4_3_17 };

    [Serializable]
    public class Interp<T> : IInterpElem, IInterpParams where T : IInterpElem
    {
        //Тут хранятся данные
        public SerializableSortedList<double, T> _data = new SerializableSortedList<double, T>();
        [XmlIgnore]
        public SortedList<double, T> Data
        {
            get
            {
                return _data;
            }
        }
        public string Title { get; set; } = "Title";

        //Тип экстраполяции
        public ExtrapolType ET_left { get; set; } = ExtrapolType.etValue;
        public ExtrapolType ET_right { get; set; } = ExtrapolType.etValue;
        //номер интервала (левого элемента), в который попала искомая точка
        [XmlIgnore]
        public int N { get; private set; } = 0;
        public int Count {
            get
            {
                return _data.Count;
            }
        }



        //делегат функции по реализации метода интерполяции
        public delegate double InterpolMethod(params double[] t);
        private InterpolMethod InterpMethodCurr { get; set; }
        private InterpolType _interpType;
        public InterpolType InterpType
        {
            get
            {
                return _interpType;
            }
            set
            {
                _interpType = value;
                switch (value)
                {
                    case InterpolType.itStep:
                        InterpMethodCurr = new InterpolMethod(this.InterpMethodStep);
                        break;
                    case InterpolType.itSpecial4_3_17:
                        if (this is PotentGraff3P)
                            InterpMethodCurr = new InterpolMethod(this.InerpMethodSpecial4_3_17);
                        else
                            InterpType = InterpolType.itLine;
                        break;
                    default:
                        InterpMethodCurr = new InterpolMethod(this.InterpMethodLine);
                        break;
                }
            }
        }
        public double MinT() => _data.Keys.Min();
        public double MaxT() => _data.Keys.Max();
        public Interp()
        {
            InterpType = InterpolType.itLine;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private double GetVSub(params double[] t)
        {
            if (t.Length == 1)
                return _data.Values[N].GetV();
            else if (t.Length == 2)
                return _data.Values[N].GetV(t[0]);
            else if (t.Length > 2)
            {
                var t_next = new double[t.Length - 1];
                Array.Copy(t, t_next, t.Length - 1);
                return _data.Values[N].GetV(t_next);
            }
            return 0;
        }
        public int AddElement(double t, T element)
        {
            if (_data.ContainsKey(t))
                throw new Exception("Добавляешь дубликат по t");
            _data.Add(t, element);
            return _data.IndexOfValue(element);
        }
        public void RemoveElement(int elemIndex)
        {
            if (elemIndex < _data.Count && elemIndex >= 0)
            {
                _data.RemoveAt(elemIndex);
                N = 0;
            }
        }
        public void Clear()
        {
            _data.Clear();
        }
        //функция нахождения интервала, в который попадает запрашиваемая точка + Инициализирует N
        //-1 - находится левее 0 точки, = length - нахиодтся справа последней
        public int SetN(double t)
        {
            try
            {
                if (_data.Count < 1)
                    return 0;
                if (N < 0)
                {
                    if (_data.Keys[0] > t)
                        return -1;
                    N = 0;
                }
                if(N == _data.Count-1 && _data.Keys[N] <= t)
                {
                    return N;
                }
                if (_data.Count > 1 && _data.Keys[N] <= t && _data.Keys[N + 1] > t)
                    return N;
                   
                if (_data.Keys[N] > t)
                    for (int i = N; i >= 0; i--)
                    {
                        if (_data.Keys[i] <= t)
                        {
                            break;
                        }
                        N = i - 1;
                    }
                else
                    for (int i = N; i < _data.Count; i++)
                    {
                        if (_data.Keys[i] > t)
                        {
                            break;
                        }
                        N = i;
                    }
                return N;
            }
            catch (Exception)
            {
                return 0;
            }
        }
        //Метод интерполяции "ступенька" возр. знач. = ближ левому точке
        public double InterpMethodStep(params double[] t)
        {
            return GetVSub(t);   
        }
        public double InterpMethodLine(params double[] t)
        {
            double t1, t2, y1, y2;
            t1 = _data.Keys[N];
            t2 = _data.Keys[N+1];
            y1 = GetVSub(t);
            N = N + 1;
            y2 = GetVSub(t);
            return y1 + (y2 - y1) * (t.Last() - t1) / (t2 - t1);
        }

        public double InerpMethodSpecial4_3_17(params double[] t)
        {
            double t1, t2, y1, y2;
            t1 = _data.Keys[N];
            t2 = _data.Keys[N + 1];
            y1 = GetVSub(t);
            N = N + 1;
            y2 = GetVSub(t);
            if(y2==0 && y1 != 0)
            {
                double _2z_l = t[0];
                double _2y_l = t[1];
                double D_2 = t[2];
                double koeff = (_2z_l - D_2) / (1 - D_2);
                N = N - 1;
                _2z_l = t1 + koeff * (1 - t1);
                y1 = GetVSub(t1 + koeff * (1 - t1), _2y_l, D_2);
                N = N + 1;
                _2z_l = t2 + koeff * (1 - t2);
                y2 = GetVSub(t2 + koeff * (1 - t2), _2y_l, D_2);
            }

            return y1 + (y2 - y1) * (t.Last() - t1) / (t2 - t1);
        }

        /// <summary>
        /// Получить значение в точке с координатами t;
        /// t[0] - координата самого низкого порядка, для интерполяции вложенных одномерных интерполяторов
        /// t[1] - координата для интерполяции между значениями, полученных при помощи одномерных интерполяторов
        /// t[2] - -----//-----
        /// Пример: Одномерный интерполятор InterpXY.GetV(t) => y = f(t)
        ///         Двумерный интерполятор  InterpXY.GetV(t,X) => y = f(t,X)
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public virtual double GetV(params double[] t)
        {
            if (_data.Count == 0 || t.Length == 0)
                return 0;
            SetN(t.Last());
            //Экстраполяция (пока только 2 типа)
            if (N < 0 || N == _data.Count - 1 || _data.Count == 1)
            {
                ExtrapolType ET_temp = N < 0 ? ET_left : ET_right;
                N = N < 0 ? 0 : N;
                switch (ET_temp)
                {
                    case ExtrapolType.etZero:
                        if(N == _data.Count - 1 && _data.ContainsKey(t[0]))
                        {
                            return GetVSub(t);
                        }
                        return 0;
                    case ExtrapolType.etValue:
                        return GetVSub(t);
                    case ExtrapolType.etMethod_Line:
                        {
                            N -= N == _data.Count - 1 ?
                                1 :
                                0;
                            return InterpMethodLine(t);
                        }
                        
                    default:
                        return GetVSub(t);
                } 
            }
            return InterpMethodCurr(t);    
        }
        public void CopyParamsFrom(IInterpParams parent)
        {
            this.ET_left = parent.ET_left;
            this.ET_right = parent.ET_right;
            this.InterpType = parent.InterpType;
        }
        public void CopyParamsTo(IInterpParams child)
        {
            child.ET_left = this.ET_left;
            child.ET_right = this.ET_right;
            child.InterpType = this.InterpType;
        }
        public override string ToString()
        {
            return Title;
        }
        public void SaveToXmlFile(string fileName)
        {
            try
            {
                XmlSerializer serial = new XmlSerializer(this.GetType());
                var sw = new StreamWriter(fileName);
                serial.Serialize(sw, this);
                sw.Close();
            }
            catch (Exception)
            {      }

        }
        //public Interp<T> LoadFromXmlFile(string fileName)
        //{
        //    try
        //    {
        //        XmlSerializer serial = new XmlSerializer(this.GetType());
        //        var sw = new StreamReader(fileName);
        //        Interp<T> result = (serial.Deserialize(sw) as Interp<T>);
        //        sw.Close();
        //        return result;
        //    }
        //    catch (Exception)
        //    { }
        //    return null;
        //}
        public static T2 LoadFromXmlString<T2>(string fileStr) where T2 : Interp<T>
        {
            try
            {
                XmlSerializer serial = new XmlSerializer(typeof(T2));
                var sw = new StringReader(fileStr);
                T2 result = (T2)serial.Deserialize(sw) ;
                sw.Close();
                return result;
            }
            catch (Exception)
            { }
            return null;
        }
        public static T2 LoadFromXmlFile<T2>(string fileName) where T2 : Interp<T>
        {
            try
            {
                XmlSerializer serial = new XmlSerializer(typeof(T2));
                var sw = new StreamReader(fileName);
                T2 result =(T2)serial.Deserialize(sw);
                sw.Close();
                return result;
            }
            catch (Exception)
            { }
            return null;
        }
    }

    [XmlRoot(nameof(InterpXY))]
    public class InterpXY : Interp<InterpDouble>
    {
        public int Add(double t, double value, bool allowDublicates = false)
        {
            if (!allowDublicates && _data.ContainsKey(t))
                return 0;
            return AddElement(t, new InterpDouble(value));
        }
        public void CopyDataFrom(InterpXY parent, bool delPrevData = false)
        {
            if (delPrevData)
                _data.Clear();
            _data.Capacity =    _data.Capacity > (_data.Count + parent.Data.Count) ?
                                (int)((_data.Count + parent.Data.Count) * 1.5) :
                                _data.Capacity;
            foreach (var item in parent.Data)
            {
                Add(item.Key, item.Value.Value);
            }
        }
        public InterpXY CopyMe()
        {
            var result = new InterpXY();
            result.CopyParamsFrom(this);
            result.CopyDataFrom(this);
            return result;
        }
        public void AddData(double[] ts, double[] vals, bool delPrevData = false)
        {
            if (ts.Length != vals.Length || ts.Length == 0)
                throw new ArgumentException($"Неправильные параметры, походу разные длины векторов");
            if (delPrevData)
                _data.Clear();
            _data.Capacity = _data.Capacity > (_data.Count + ts.Length) ?
                            (int)((_data.Count + ts.Length) * 1.5 ):
                            _data.Capacity;
            for (int i = 0; i < ts.Length; i++)
            {
                Add(ts[i], vals[i]);
            }
        }
        public InterpXY() : base()
        {

        }
        public InterpXY(double[] ts, double[] vals):this()
        {
            this.AddData(ts, vals);
        }
        public InterpXY(InterpXY parent)
        {
            CopyParamsFrom(this);
            CopyDataFrom(this);
        }
        public static InterpXY LoadFromXmlFile(string fileName)  => InterpXY.LoadFromXmlFile<InterpXY>(fileName);
        public static InterpXY LoadFromXmlString(string fileStr) => InterpXY.LoadFromXmlString<InterpXY>(fileStr); 
    }

    [XmlRoot(nameof(Interp2D))]
    public class Interp2D: Interp<InterpXY>
    {
        public Interp2D CopyMe()
        {
            var result = new Interp2D();
            result.CopyParamsFrom(this);
            foreach (var item in _data)
            {
                result.AddElement(item.Key, item.Value.CopyMe());
            }
            return result;
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
        public void ImportDataFromMatrix(double[,] m, bool copyParams = true)
        {
            if(m.GetLength(0) < 2 || m.GetLength(1) < 2)
                throw new ArgumentException($"Неправильные параметры, походу разные длины векторов");
            var tsXY   = new double[m.GetLength(1) - 1];
            var tsInXY = new double[m.GetLength(0) - 1];
            var vecs = new double[tsXY.Length][];

            for (int i = 0; i < tsXY.Length; i++)
                tsXY[i] = m[0, i + 1];

            for (int i = 0; i < tsInXY.Length; i++)
                tsInXY[i] = m[i + 1, 0];

            for (int i = 0; i < tsXY.Length; i++)
            {
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
        public void ImportDataFromVectors(double[] tsXY, double[] tsInXY, double[][] vecs, bool copyParams = true)
        {
            if(tsXY.Length != vecs.Length || tsXY.Length == 0 || tsInXY.Length == 0)
                throw new ArgumentException($"Неправильные параметры, походу разные длины векторов");
            for (int i = 0; i < vecs.Length; i++)
                if(tsInXY.Length != vecs[i].Length)
                    throw new ArgumentException($"Неправильные параметры, походу разные длины векторов");
            _data.Clear();
            for (int i = 0; i < tsXY.Length; i++)
            {
                InterpXY tmpInterp = new InterpXY(tsInXY, vecs[i]);
                if (copyParams)
                    tmpInterp.CopyParamsFrom(this);
                AddElement(tsXY[i], tmpInterp);
            }
                
        }
        public static Interp2D LoadFromXmlFile(string fileName)  => Interp2D.LoadFromXmlFile<Interp2D>(fileName);
        public static Interp2D LoadFromXmlString(string fileStr) => Interp2D.LoadFromXmlString<Interp2D>(fileStr);
    }

    [XmlRoot(nameof(Interp3D))]
    public class Interp3D: Interp<Interp2D>
    {
        public Interp3D CopyMe()
        {
            var result = new Interp3D();
            result.CopyParamsFrom(this);
            foreach (var item in _data)
            {
                result.AddElement(item.Key, item.Value.CopyMe());
            }
            return result;
        }
        public static Interp3D LoadFromXmlFile(string fileName) => Interp3D.LoadFromXmlFile<Interp3D>(fileName);
        public static Interp3D LoadFromXmlString(string fileStr) => Interp3D.LoadFromXmlString<Interp3D>(fileStr);
    }

    [XmlRoot(nameof(Interp4D))]
    public class Interp4D : Interp<Interp3D>
    {
        public Interp4D CopyMe()
        {
            var result = new Interp4D();
            result.CopyParamsFrom(this);
            foreach (var item in _data)
            {
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
    public class LevelLine : InterpDouble
    {
        public static LevelLine LoadFromXmlFile(string fileName)
        {
            try
            {
                XmlSerializer serial = new XmlSerializer(typeof(LevelLine));
                var sw = new StreamReader(fileName);
                LevelLine result = (LevelLine)serial.Deserialize(sw);
                sw.Close();
                return result;
            }
            catch (Exception)
            { }
            return null;
        }
        public static LevelLine LoadFromXmlString(string fileStr)
        {
            try
            {
                XmlSerializer serial = new XmlSerializer(typeof(LevelLine));
                var sw = new StringReader(fileStr);
                LevelLine result = (LevelLine)serial.Deserialize(sw);
                sw.Close();
                return result;
            }
            catch (Exception)
            { }
            return null;
        }
        public List<Vector> pointsList = new List<Vector>();
        public LevelLine(double Value = 0.0): base(Value)
        {

        }

        public LevelLine() : base()
        {

        }
        public int Count { get { return pointsList.Count; } }
        public void AddPoint(double x, double y)
        {
            pointsList.Add(new Vector(x, y));
        }
        public void AddPoints(double[] x, double[] y)
        {
            int n = Math.Min(x.Length, y.Length);
            for (int i = 0; i < n; i++)
            {
                AddPoint(x[i], y[i]);
            }
        }
        public void AddPoints(double[][] xy )
        {
            foreach (double[] item in xy)
            {
                if (item.Length > 1)
                    AddPoint(item[0], item[1]);
            }
        }
        public void AddPoint(Vector point)
        {
            pointsList.Add(point);
        }
        public Vector NearestToPoint(Vector point)
        {
            if(pointsList.Count == 0)
                return new Vector(0, 0);
            Vector result = new Vector(double.MaxValue, double.MaxValue);
            for (int i = 0; i < pointsList.Count-1; i++)
            {
                Vector tmpResult = MinimumDistanceVector(pointsList[i], pointsList[i + 1], point);
                if ((tmpResult - point).LengthSquared < (result - point).LengthSquared)
                    result = tmpResult;
            }
            return result;
        }
        public static Vector MinimumDistanceVector(Vector v, Vector w, Vector p)
        {
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
            double t = Math.Max(0, Math.Min(1, (p - v)*(w - v) / l2));
            return v + t * (w - v);  // Projection falls on the segment
        }
        public bool IsCrossMe(Vector b0, Vector b1)
        {
            for (int i = 0; i < pointsList.Count-1; i++)
            {
                if (LinesIntersect(pointsList[i], pointsList[i + 1], b0, b1))
                    return true;
            }
            return false;
        }
        
        public static bool BoundingBoxesIntersect(Vector a0, Vector a1, Vector b0, Vector b1)
        {
            return     Math.Min(a0.X, a1.X) <= Math.Max(b0.X, b1.X)
                    && Math.Max(a0.X, a1.X) >= Math.Min(b0.X, b1.X)
                    && Math.Min(a0.Y, a1.Y) <= Math.Max(b0.Y, b1.Y)
                    && Math.Max(a0.Y, a1.Y) >= Math.Min(b0.Y, b1.Y);
        }
        public static bool IsPointOnLine(Vector a0, Vector a1, Vector p)
        {
            // Move the image, so that a.first is on (0|0)
            return Math.Abs(CrossProduct(a1 - a0, p - a0)) < EPSILON;
        }
        public static bool IsPointRightOfLine(Vector a0, Vector a1, Vector p)
        {
            // Move the image, so that a.first is on (0|0)
            return CrossProduct(a1 - a0, p - a0) < 0;
        }
        public static bool LineSegmentTouchesOrCrossesLine(Vector a0, Vector a1, Vector b0, Vector b1)
        {
            return      IsPointOnLine(a0, a1, b0)
                        || IsPointOnLine(a0, a1, b1)
                        || ( IsPointRightOfLine(a0, a1, b0)^
                             IsPointRightOfLine(a0, a1, b1)   )  ;
        }
        public static bool LinesIntersect(Vector a0, Vector a1, Vector b0, Vector b1)
        {

            return BoundingBoxesIntersect(a0, a1, b0, b1)
                    && LineSegmentTouchesOrCrossesLine(a0, a1, b0, b1)
                    && LineSegmentTouchesOrCrossesLine(b0, b1, a0, a1);
        }
        public static double CrossProduct(Vector a, Vector b)
        {
            return a.X * b.Y - b.X * a.Y;
        }
        public static double EPSILON = 0.000001;
    }
    /// <summary>
    /// График, составленный из линий уровня
    /// </summary>
    [XmlRoot(nameof(PotentGraff2P))]
    public class PotentGraff2P : Interp<LevelLine>
    {
        /// <summary>
        /// Воооот тут  t[0] = x
        ///             t[1] = y
        /// return интерполированный параметр Value от соседних LevelLine'ов
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public override double GetV(params double[] t)
        {
            if (_data.Count == 0 || t.Length < 2)
                return 0.0;
            if (_data.Count == 1)
                return _data.Values[0].Value;

            var p = new Vector(t[0], t[1]);
            Vector[] nPoints = new Vector[_data.Count];
            double[] nL = new double[_data.Count];
            int ni = 0;
            for (int i = 0; i < _data.Count; i++)
            {
                //ближайшая точка на i-ой кривой от точки p 
                nPoints[i] = _data.Values[i].NearestToPoint(p);
                //рассотяние от точки p до i-ой кривой
                nL[i] = (nPoints[i]- p).Length;
                //запоминаем номер ближайшей кривой в ni
                if (nL[i] < nL[ni])
                    ni = i;
            }
            int nj = -1;
            for (int i = 0; i < _data.Count; i++)
            {
                //Нет смысла сравнивать один и тот же вектор
                if (i == ni)
                    continue;
                //если вектор кратчайшего расстоняия НЕ пересекает самую ближнюю линию, значит точка лежит между линиями
                if(!_data.Values[ni].IsCrossMe(p,nPoints[i]))
                {
                    //если дебют, то запоминаем
                    if (nj < 0)
                    {
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
        public bool ValidData()
        {
            for (int i = 0; i < _data.Count-1; i++)
            {
                for (int j = 0 ; j < _data.Values[i].Count-1; j++)
                {
                    for (int k = i+1; k < _data.Count; k++)
                    {
                        for (int j1 = 0; j1 < _data.Values[k].Count - 1; j1++)
                        {                       
                            if (LevelLine.LinesIntersect(_data.Values[i].pointsList[j] , _data.Values[i].pointsList[j + 1],
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
    public class PotentGraff3P: Interp<PotentGraff2P>
    {
        public static PotentGraff3P LoadFromXmlFile(string fileName) => PotentGraff3P.LoadFromXmlFile<PotentGraff3P>(fileName);
        public static PotentGraff3P LoadFromXmlString(string fileStr) => PotentGraff3P.LoadFromXmlString<PotentGraff3P>(fileStr);
    }
    /// <summary>
    /// Семейство графиков, каждый из которых представляет собой PotentGraff3D
    /// </summary>
    [XmlRoot(nameof(PotentGraff4P))]
    public class PotentGraff4P : Interp<PotentGraff3P>
    {
        public static PotentGraff4P LoadFromXmlFile(string fileName) => PotentGraff4P.LoadFromXmlFile<PotentGraff4P>(fileName);
        public static PotentGraff4P LoadFromXmlString(string fileStr) => PotentGraff4P.LoadFromXmlString<PotentGraff4P>(fileStr);
    }
}
