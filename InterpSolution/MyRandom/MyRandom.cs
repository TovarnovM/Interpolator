using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using RandPCG;
using GeneticSharp.Domain.Randomizations;
using System.Threading;

namespace MyRandomGenerator
{

    public class MyRandom : BasicRandomization {
        public double GetNorm(double mo,double sko,bool _3SigmaRule = true) {
            return mo + sko * GetNorm(_3SigmaRule);
        }
        public double GetNorm(bool _3SigmaRule = true) {
            double answ;
            do {
                answ = Math.Sqrt(-2.0 * Math.Log(GetDouble())) * Math.Cos(Math.PI * 2 * GetDouble());
            } while(_3SigmaRule && Math.Abs(answ) > 3d);
            return answ;
        }
    }

}

