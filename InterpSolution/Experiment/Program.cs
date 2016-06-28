using Microsoft.Research.Oslo;
using System;

namespace Experiment {
    class Program {
        static void Main(string[] args) {
            var pos = new Position(1, 2, 3, 1, "pos");
            var pos_ = new Position(1, 0.5, -0.25, 0, "dposDt");
            pos.AddChild(pos_);
            pos.AddDiffVect(pos_, true);

            var solve = Ode.RK45(0, pos.Rebuild(), pos.f, 10);

            foreach (var item in solve.SolveFromToStep(0, 1000, 100)) {
                Console.WriteLine($"t = {item.T},   \tV = {item.X}");
            }

            foreach (var item in pos.Prms) {
                Console.WriteLine($"{item.Name} = {item.GetVal(0d)}");
            }



            //var ww = new ScnPrm("ww", null, 1);
            //ww.SetVal(33);
            //Console.WriteLine($"ww = {ww.GetVal(1)}");

            //var obj = new ScnObjDummy();
            //obj.AddParam(ww);

            //var dww = new ScnPrmConst("_",null,2);

            //obj.AddDiffPropToParam(ww,dww);
            //obj.Rebuild();

            //foreach(var item in obj.DiffArr) {
            //    Console.WriteLine($"{item.MyDiff.Name} = {item.MyDiff.GetVal(0d)}");
            //}

            Console.ReadLine();
        }
    }
}
