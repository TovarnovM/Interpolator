using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using SerializableGenerics;

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
        public double GetV(params double[] t)
        {
            return Value;
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
                return _data.Values[N].GetV(t[1]);
            else if (t.Length > 2)
            {
                var t_next = new double[t.Length - 1];
                t.CopyTo(t_next, 1);
                return _data.Values[N].GetV(t_next);
            }
            return 0;
        }
        public int AddElement(double t, T element)
        {
            if (_data.ContainsKey(t))
                return AddElement(t + Double.Epsilon, element);
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
            if (t.Length == 1)
                return _data.Values[N].GetV();
            else if (t.Length == 2)
                return _data.Values[N].GetV(t[1]);
            else if (t.Length > 2)
            {
                var t_next = new double[t.Length - 1];
                t.CopyTo(t_next, 1);
                return _data.Values[N].GetV(t_next);
            }
            return 0;   
        }
        public double InerpMethodLine(params double[] t)
        {
            double t1, t2, y1, y2;
            t1 = _data.Keys[N];
            t2 = _data.Keys[N+1];
            y1 = GetVSub(t);
            N = N + 1;
            y2 = GetVSub(t);
            return y1 + (y2 - y1) * (t[0] - t1) / (t2 - t1);
        }
        //Получить значение в точке с координатами t
        public double GetV(params double[] t)
        {
            if (_data.Count == 0 || t.Length == 0)
                return 0;
            SetN(t[0]);
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
    }

    [XmlRoot(nameof(InterpXY))]
    public class InterpXY: Interp<InterpDouble>
    {
        public int Add(double t, double value)
        {
            return AddElement(t, new InterpDouble(value));
        }
    }
    public class InterpXYZ: Interp<InterpXY>
    {
        
    }
    public class InterpXYZR: Interp<InterpXYZ>
    {

    }
}
