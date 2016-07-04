using Interpolator;
using Microsoft.Research.Oslo;
using System;

namespace Experiment {
    class Program {
        static void Main(string[] args) {
            var pos = new Position(0, 0, 0, 1, "pos");
            var pos_ = new Position(0, 0, 0, 1, "dposDt");


            pos.AddChild(pos_);
            pos.AddDiffVect(pos_);

            var interpX = new InterpXY();
            interpX.Add(0,0);
            interpX.Add(2,1);
            interpX.Add(6,-1);
            interpX.Add(8,0);
            pos_.pX.SealInterp(interpX);



            var solve = Ode.RK45(0,pos.Rebuild(),pos.f);

            SolPoint sp = new SolPoint();
            foreach(var item in solve.SolveFromToStep(0,2,0.1)) {
                Console.WriteLine($"t = {item.T},   \tV = {item.X}");
                sp = item;
            }

            var res = pos.GetAllParamsValues(sp);
            for(int i = 0; i < res.Length; i++) {
                Console.WriteLine($"{pos.AllParamsNames[i]} = \t{res[i]}");
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
