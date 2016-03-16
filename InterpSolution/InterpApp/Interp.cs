using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpolator
{
    public interface IInterpElem
    {
        double GetV(params double[] t);
        
    }
    class InterpDouble : IInterpElem
    {
        public double Value { get; set; }
        double IInterpElem.GetV(params double[] t)
        {
            return Value;
        }
    }

    public class Interp<T>:IInterpElem where T :IInterpElem
    {
        private SortedList<double, T> _data = new SortedList<double, T>() ;

        double IInterpElem.GetV(params double[] t)
        {
            if(t.Length > 0)
            {
                return 0;
                //Короч запилить
            }
        }
    }
}
