using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpolator
{
    class Program
    {
        static void Main(string[] args)
        {
            var dd = new double[0];
            double t = 1.5;
            int N = 2;
            double[] tstArr = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            
            if (tstArr[N] > t)
                for (int i = N; i >= 0; i--)
                {
                    if (tstArr[i] <= t)
                    {
                        break;
                    }
                    N = i-1;
                }
            else
                for (int i = N; i < tstArr.Length; i++)
                {
                    if (tstArr[i] > t)
                    {
                        break;
                    }
                    N = i;
                }

            Console.WriteLine($"t = {t}   N = {N}");

            var sl = new SortedList<double, double>( new Dictionary<double,double>
                { [0] = 0,
                  [5] = 5,
                  [3] = 3  }  );
            sl.Add(2, 2);
            foreach (var tmp in sl)
                Console.Write($"{tmp}\t");
            Console.ReadLine();
        }
    }
}
