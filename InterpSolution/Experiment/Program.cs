using System;

namespace Experiment {
    class Program {
        static void Main(string[] args) {
            var ww = new ScnPrmConst("ww", null, 1);
            ww.SetVal(33);
            Console.WriteLine($"{ww.GetVal(1)}");
            Console.ReadLine();
        }
    }
}
