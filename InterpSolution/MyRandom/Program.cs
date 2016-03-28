using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RandPCG;
using System.IO;

namespace MyRandomApp
{
    class Program
    {

        static void Main(string[] args)
        {

            var rnd = new Generator((ulong)Environment.TickCount & Int32.MaxValue);
            var fw = new StreamWriter("randValues.txt");
            for (int i = 0; i < 1000000; i++)
            {
                double tmp = Math.Sqrt(-2.0 * Math.Log(rnd.Next())) * Math.Cos(Math.PI * 2 * rnd.Next());
                fw.WriteLine(tmp);
            }
            fw.Close();

        }
    }
}
