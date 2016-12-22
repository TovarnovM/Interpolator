using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NikTestConsole {
    public static class dlltest {
        
    }
    class Program {
        [DllImport("Nik.dll",EntryPoint = "UniBallS",CharSet = CharSet.Auto)]
        public static extern void UniBallS0(ref float Lcone,ref float dout,ref float Lpiston,ref float m1,ref float m2,ref float Vd,ref float pmax);
        static void Main(string[] args) {
            float l = 0.5f, d = 0.1f, lp = 0.5f, m1 = 7f, m2 = 7f, Vd = 0f, pmax = 0f;

            UniBallS0(ref l,ref d,ref lp,ref m1,ref m2,ref Vd,ref pmax);
            Console.WriteLine(Vd);
            Console.WriteLine(pmax);
            Console.ReadLine();
        }
    }
}
