using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using SerializableGenerics;
using System.IO;

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
    public enum ExtrapolType { etZero, etValue, etMethod, etError };
    public enum InterpolType { itStep, itLine };

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
        public int Count()
        {
            return _data.Count;
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
                        InterpMethodCurr = new InterpolMethod(this.InerpMethodStep);
                        break;
                    default:
                        InterpMethodCurr = new InterpolMethod(this.InerpMethodLine);
                        break;
                }
            }
        }
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
                return AddElement(t*1.000001, element);
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
                    N = 0;
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
        public double InerpMethodStep(params double[] t)
        {
            return GetVSub(t);   
        }
        public double InerpMethodLine(params double[] t)
        {
            double t1, t2, y1, y2;
            t1 = _data.Keys[N];
            t2 = _data.Keys[N+1];
            y1 = GetVSub(t);
            N = N + 1;
            y2 = GetVSub(t);
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
        public double GetV(params double[] t)
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
        protected static T2 LoadFromXmlString<T2>(string fileStr) where T2 : Interp<T>
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
        protected static T2 LoadFromXmlFile<T2>(string fileName) where T2 : Interp<T>
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
        public int Add(double t, double value)
        {
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
}
