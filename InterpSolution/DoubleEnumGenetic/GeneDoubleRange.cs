using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Randomizations;
using Sharp3D.Math.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Math;

namespace DoubleEnumGenetic {
    public class GeneDoubleRange : IGeneDE {
        public double eps = 0.000000001;
        public string Name { get; set; }
        public double GetRandValue() {
            return RandomizationProvider.Current.GetDouble(Left,Right);
        }
        private double _min = 0d;
        public double Left {
            get { return _min; }
            set {
                if(value <= Right)
                    _min = value;
                else
                    _min = Right;
            }
        }
        private double _max = 0d;
        public double Right {
            get { return _max; }
            set {
                if(value >= Left)
                    _max = value;
                else
                    _max = Left;
            }
        }
        public GeneDoubleRange(string name,double min,double max) {
            Name = name;
            _min = min < max ? min : max;
            _max = max > min ? max : min;
        }
        public double Gauss(double x,double xm,double sko) {
            return Exp(-0.5 * (x - xm) * (x - xm) / (sko * sko)) / (2.506628274631000502415765284811 * sko);
        }
        public double GetRandValue_Norm(double xm,double sko) {
            if(sko < eps)
                return xm >= Left && xm <= Right ? xm : GetRandValue();
            const double SIGMARULE = 3.5;
            double x1 = Max(_min,xm - SIGMARULE * sko);
            double x2 = Min(_max,xm + SIGMARULE * sko);
            if(x1 >= x2)
                return xm <= _min ? _min : _max;

            double amplit = xm >= x1 && xm <= x2 ?
                Gauss(xm,xm,sko) :
                Max(Gauss(x1,xm,sko),Gauss(x2,xm,sko));
            double x, h, f;
            do {
                x = RandomizationProvider.Current.GetDouble(x1,x2);
                h = RandomizationProvider.Current.GetDouble();
                f = Gauss(x,xm,sko) / amplit;
            } while(f <= h);
            return x;
        }
        public double GetRandValue_Uniform(double x1,double x2) {
            double lft = Min(x1,x2);
            double rgt = Max(x1,x2);
            if(rgt < Left)
                return Left;
            if(lft > Right)
                return Right;
            lft = Max(lft,Left);
            rgt = Min(rgt,Right);
            return RandomizationProvider.Current.GetDouble(lft,rgt);
        }

        public bool ValidateValue(double value) {
            //if(!(value is double))
            //    throw new ArgumentException("Нужно проверять Double, а не ",value.GetType().ToString());
            return Left <= (double)value && (double)value <= Right;
        }

        public GeneDoubleRange Copy() {
            return new GeneDoubleRange(Name,Left,Right);
        }


    }
}
