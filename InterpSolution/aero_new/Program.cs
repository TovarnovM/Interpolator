using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AeroApp;

namespace RocketAero {
    class Program {
        static void Main(string[] args) {
            var r = new Rocket();
            var ag = AeroGraphs.Instance;
            r.AeroGr = ag;
            r.X_ct = 2.5;
            r.Body = new RocketBody(ag) {
                L = 4.18,
                L_nos = 0.356,
                L_korm = 0.09,
                D = 0.31,
                D1 = 0.28,
                //Nose = new RocketNos_ConePlusCyl()
                Nose = new RocketNos_Compose("7_2", 118 * 2 / 310.0)
            };

            r.W_I = new WingOrient(
                new RocketWing(ag) {
                    B0 = 1.022,
                    B1 = 0.327,
                    L = 1.016,
                    Hi0 = 50,
                    C_shtr = 0.07,
                    D = r.Body.D,
                    Profile = new WingProf_6(0.4)
                }) {
                X = 1.82028
            };
            r.W_II = new WingOrient(
                new RocketWing(ag) {
                    B0 = 0.55,
                    B1 = 0.07117,
                    L = 0.7,
                    Hi0 = 50,
                    C_shtr = 0.072,
                    D = r.Body.D,
                    Profile = new WingProf_6(0.4)
                }) {
                X = 3.599999,
                X_povor_otn = 218.0 / 338.0
            };

            r.M = 0.5;
            r.Alpha = 10;

            Console.WriteLine($"{r.Cx}");

            r.M = 0.1;
            r.Alpha = 10;

            Console.WriteLine($"{r.Cx}");
            Console.ReadKey();
        }
    }
}
