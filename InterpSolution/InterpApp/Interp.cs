using SerializableGenerics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Xml.Serialization;

namespace Interpolator {
    public interface IInterpElemAbs<T> : ICloneable,IDisposable {
        T GetV(params double[] t);
    }
    public interface IInterpParams {
        ExtrapolType ET_left { get; set; }
        ExtrapolType ET_right { get; set; }
        InterpolType InterpType { get; set; }
    }


    //тип экстраполяции  = 0, = крайн значению, = продолжению метода, вызывает ошибку
    public enum ExtrapolType { etZero, etValue, etMethod_Line, etError,etRepeat };
    public enum InterpolType { itStep, itLine, itSpecial4_3_17 };


    [Serializable, XmlRoot(nameof(InterpElemAbs<T>))]
    public abstract class InterpElemAbs<T> : IInterpElemAbs<T> {

        
        private T _v;
        [XmlAttribute]
        public T Value {
            get { return _v; }
            set { _v = value; }
        }

        /// <summary>
        /// Основной метод) Просто получить значение
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public T GetV(params double[] t) {
            return _v;
        }

        public abstract object Clone();

        public virtual void Dispose() {
        }

        public InterpElemAbs(T value) {
            Value = value;
        }
        public InterpElemAbs() {
            Value = default(T);
        }
    }

    [Serializable]
    public abstract class InterpAbs<T, TRes> : IInterpElemAbs<TRes>, IInterpParams where T : IInterpElemAbs<TRes> {
        //Тут хранятся данные
        public SerializableSortedList<double,T> _data = new SerializableSortedList<double,T>();
        [XmlIgnore]
        public SortedList<double,T> Data {
            get {
                return _data;
            }
        }
        public string Title { get; set; } = "Title";

        //Тип экстраполяции
        public ExtrapolType ET_left { get; set; } = ExtrapolType.etValue;
        public ExtrapolType ET_right { get; set; } = ExtrapolType.etValue;

        //[ThreadStatic]
        private int _n = 0;
        //номер интервала (левого элемента), в который попала искомая точка
        [XmlIgnore]
        public int N {
            get {
                return _n;
            }
            protected set {
                _n = value;
            }
        }
        public int Count {
            get {
                return _data.Count;
            }
        }



        //делегат функции по реализации метода интерполяции
        public delegate TRes InterpolMethod(params double[] t);
        protected InterpolMethod InterpMethodCurr { get; set; }
        protected InterpolType _interpType;
        public InterpolType InterpType {
            get {
                return _interpType;
            }
            set {
                _interpType = value;
                switch(value) {
                    case InterpolType.itStep:
                    InterpMethodCurr = new InterpolMethod(this.InterpMethodStep);
                    break;
                    case InterpolType.itSpecial4_3_17:
                    if(this is PotentGraff3P)
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
        public InterpAbs() {
            InterpType = InterpolType.itLine;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected TRes GetVSub(params double[] t) {
            if(t.Length == 1)
                return _data.Values[_n].GetV();
            else if(t.Length == 2)
                return _data.Values[_n].GetV(t[0]);
            else if(t.Length > 2) {
                var t_next = new double[t.Length - 1];
                Array.Copy(t,t_next,t.Length - 1);
                return _data.Values[_n].GetV(t_next);
            }
            return default(TRes);
        }
        public virtual int AddElement(double t,T element) {

            if(_data.ContainsKey(t))
                throw new Exception("Добавляешь дубликат по t");
            _data.Add(t,element);
            return _data.IndexOfValue(element);
        }
        public virtual void RemoveElement(int elemIndex) {

            if(elemIndex < _data.Count && elemIndex >= 0) {
                _data.RemoveAt(elemIndex);
                _n = 0;
            }
        }
        public void Clear() {
            _data.Clear();
        }
        //функция нахождения интервала, в который попадает запрашиваемая точка + Инициализирует _n
        //-1 - находится левее 0 точки, = length - нахиодтся справа последней
        public virtual int SetN(double t) {
            try {
                if(_data.Count < 1)
                    return 0;
                if(_n < 0) {
                    if(_data.Keys[0] > t)
                        return -1;
                    _n = 0;
                }
                int min, max;
                int lengthM1 = _data.Count - 1;
                if(_data.Keys[_n] <= t) {
                    //проверка "на прошлый вызванный интервал"
                    if(_n == lengthM1 || _data.Keys[_n + 1] > t)
                        return _n;
                    ++_n;
                    //проверка на соседний правый интервал
                    if(_n == lengthM1 || _data.Keys[_n + 1] > t)
                        return _n;
                    //проверка на экстраполяцию справа
                    if(_data.Keys.Last() <= t) {
                        _n = lengthM1;
                        return lengthM1;
                    }
                    min = _n;
                    max = lengthM1;
                } else {
                    //проверка на ближайший левый интервал, относительно прошлого вызванного интервала
                    if(_n == 0 || _data.Keys[_n - 1] <= t)
                        return --_n;
                    //проверка на экстраполяцию слева
                    if(_data.Keys[0] > t) {
                        _n = -1;
                        return _n;
                    }
                    min = 0;
                    max = _n;
                }
                //Бинарный поиск
                while(min != max) {
                    _n = (min + max) / 2;
                    if(_data.Keys[_n] <= t) {
                        if(_data.Keys[_n + 1] > t)
                            return _n;
                        min = _n;
                    } else
                        max = _n;
                }
                _n = min;
                return _n;
                //OLD linear search
                //if (_data.Count < 1)
                //    return 0;
                //if (_n < 0)
                //{
                //    if (_data.Keys[0] > t)
                //        return -1;
                //    _n = 0;
                //}
                //if(_n == _data.Count-1 && _data.Keys[_n] <= t)
                //{
                //    return _n;
                //}
                //if (_data.Count > 1 && _data.Keys[_n] <= t && _data.Keys[_n + 1] > t)
                //    return _n;

                //if (_data.Keys[_n] > t)
                //    for (int i = _n; i >= 0; i--)
                //    {
                //        if (_data.Keys[i] <= t)
                //        {
                //            break;
                //        }
                //        _n = i - 1;
                //    }
                //else
                //    for (int i = _n; i < _data.Count; i++)
                //    {
                //        if (_data.Keys[i] > t)
                //        {
                //            break;
                //        }
                //        _n = i;
                //    }
                //return _n;
            }
            catch(Exception) {
                return 0;
            }
        }
        //Метод интерполяции "ступенька" возр. знач. = ближ левому точке
        public virtual TRes InterpMethodStep(params double[] t) {
            return GetVSub(t);
        }
        public abstract TRes InterpMethodLine(params double[] t);

        public abstract TRes InerpMethodSpecial4_3_17(params double[] t);

        public TRes this[params double[] t] {
            get {
                return GetV(t);
            }
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
        public virtual TRes GetV(params double[] t) {
            if(_data.Count == 0 || t.Length == 0)
                return default(TRes);
            SetN(t.Last());
            //Экстраполяция (пока только 2 типа)
            if(_n < 0 || _n == _data.Count - 1 || _data.Count == 1) {
                ExtrapolType ET_temp = _n < 0 ? ET_left : ET_right;
                _n = _n < 0 ? 0 : _n;
                switch(ET_temp) {
                    case ExtrapolType.etZero:
                    if(_n == _data.Count - 1 && _data.ContainsKey(t[0])) {
                        return GetVSub(t);
                    }
                    return default(TRes);
                    case ExtrapolType.etValue:
                    return GetVSub(t);
                    case ExtrapolType.etMethod_Line: {
                        _n -= _n == _data.Count - 1 ?
                            1 :
                            0;
                        return InterpMethodLine(t);
                    }
                    case ExtrapolType.etRepeat:
                    //Не забыть изменить в InterpXY
                    int l = t.Length;
                    var shiftParams = new double[l];
                    Array.Copy(t,shiftParams,l);
                    RepeatShift(ref shiftParams[l - 1]);
                    return GetV(shiftParams);

                    default:
                    return GetVSub(t);
                }
            }
            return InterpMethodCurr(t);
        }
        public void RepeatShift(ref double t) {
            double first = _data.Keys.First();
            double interval = _data.Keys.Last() - first;
            t = t - Math.Floor((t - first) / interval) * interval;
        }
        public void CopyParamsFrom(IInterpParams parent) {
            this.ET_left = parent.ET_left;
            this.ET_right = parent.ET_right;
            this.InterpType = parent.InterpType;
        }
        public void CopyParamsTo(IInterpParams child) {
            child.ET_left = this.ET_left;
            child.ET_right = this.ET_right;
            child.InterpType = this.InterpType;
        }
        public override string ToString() {
            return Title;
        }
        public void SaveToXmlFile(string fileName) {
            try {
                XmlSerializer serial = new XmlSerializer(this.GetType());
                var sw = new StreamWriter(fileName);
                serial.Serialize(sw,this);
                sw.Close();
            }
            catch(Exception) { }

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
        public static T2 LoadFromXmlString<T2>(string fileStr) where T2 : InterpAbs<T,TRes> {
            try {
                XmlSerializer serial = new XmlSerializer(typeof(T2));
                var sw = new StringReader(fileStr);
                T2 result = (T2)serial.Deserialize(sw);
                sw.Close();
                return result;
            }
            catch(Exception) { }
            return null;
        }
        public static T2 LoadFromXmlFile<T2>(string fileName) where T2 : InterpAbs<T,TRes> {
            try {
                XmlSerializer serial = new XmlSerializer(typeof(T2));
                var sw = new StreamReader(fileName);
                T2 result = (T2)serial.Deserialize(sw);
                sw.Close();
                return result;
            }
            catch(Exception) { }
            return null;
        }

        public object Clone() {

            ConstructorInfo cInfo = this.GetType().GetConstructor(new Type[] { });
            var myClone = cInfo.Invoke(null) as InterpAbs<T,TRes>;
            foreach(var chld in _data) {
                myClone.AddElement(chld.Key,(T)chld.Value.Clone());
            }
            myClone.Title = Title;
            CopyParamsTo(myClone);
            return myClone;
        }

        public virtual void Dispose() {
            foreach(var item in _data) {
                item.Value.Dispose();
            }
            _data.Clear();
            _data = null;

        }
    }



}
