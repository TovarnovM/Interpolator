using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPHmain {
    public class KernelFunction {
        

        public static class KernelF {
            public static double dWdr(double r_shtr,double h) {
                double q = Math.Abs(r_shtr) / h;
                if(q > 2.0)
                    return 0.0;

                double a = -2.0 / (3.0 * h * h);
                double result = 0;

                if(q < 0.66666)
                    result = 1;
                else if(q >= 0.66666 && q < 1.0)
                    result = 3.0 * q * (4.0 - 3.0 * q) / 4.0;
                else if(q >= 1.0 && q <= 2.0)
                    result = 3.0 * (2.0 - q) * (2.0 - q) / 4.0;

                return result * a;
            }
            public static double W(double r_shtr,double h) {
                double q = Math.Abs(r_shtr) / h;
                if(q > 2.0)
                    return 0.0;
                double a = 2.0 / (3.0 * h);
                double result = 0;

                if(q >= 0 && q <= 1.0)
                    result = 0.25 * (4d - 6 * q * q + 3 * q * q * q);
                else if(q > 1.0 && q <= 2.0)
                    result = 0.25 * (2.0 - q) * (2.0 - q) * (2.0 - q);

                return result * a;
            }
        }

        public static class KernelF1 {
            public static double dWdr(double r_shtr,double h) {
                double r = Math.Abs(r_shtr) / h;
                if(r > 2.0)
                    return 0.0;
                double a = 1d / h;
                double r2 = r * r;
                double r3 = r2 * r;
                //double r4 = r3 * r;
                double result = -2 * v2 * r + 3 * v3 * r2 - 4 * v4 * r3;

                return result * a;
            }
            const double
                v1 = 2d / 3d,
                v2 = 9d / 8d,
                v3 = 19d / 24d,
                v4 = 5d / 32d;
            public static double W(double r_shtr,double h) {
                double r = Math.Abs(r_shtr) / h;
                if(r > 2.0)
                    return 0.0;
                double a = 1d / h;
                double r2 = r * r;
                double r3 = r2 * r;
                double r4 = r3 * r;
                double result = v1 - v2 * r2 + v3 * r3 - v4 * r4;

                return result * a;
            }
        }
    }
}
