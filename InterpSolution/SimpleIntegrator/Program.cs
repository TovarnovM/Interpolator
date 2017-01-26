using Interpolator;
using Microsoft.Research.Oslo;
using Sharp3D.Math.Core;
using System;
using System.Diagnostics;

namespace SimpleIntegrator {
    class Program {
        static void Main(string[] args) {
            var mp = new MaterialObjectNewton() { Name = "ball" };
            mp.Mass.Ix = 10;
            mp.Mass.Iy = 10;
            mp.Mass.Iz = 10;

            //mp.AddForce(new ForceCenter(10,new Position3D(0,-1,0,"Dir"),null,"G"));
            //mp.AddForce(new Force(1,new Position3D(0,1,0,"Dir"),new Position3D(1,0,0,"FPoint"),null));
            //mp.AddForce(new Force(1,new Position3D(0,-1,0,"Dir"),new Position3D(-1,0,0,"FPoint"),null));
            //mp.AddMoment(new ForceCenter(2,new Position3D(1,0,0,"Dir"),null,"Moment"));
            //mp.Vel.X = 2;
            mp.Vel.Y = 100;
            //mp.Vel.Z = -0.1;


            var x0 = mp.Rebuild();

            var solve = Ode.RK45(0,mp.Rebuild(),mp.f);

            var res = mp.GetAllParamsValues(0,x0);
            for(int i = 0; i < res.Length; i++) {
                Console.WriteLine($"{mp.AllParamsNames[i]} = \t{res[i]}");
            }
            SolPoint sp = new SolPoint();
            string diffNames = "    V =  [ ";
            foreach(var diffpar in mp.DiffArr) {
                diffNames += $"{diffpar.FullName}, ";
            }
            diffNames += " ];";
            Console.WriteLine(diffNames);
            foreach(var item in solve.SolveFromToStep(0,21,1)) {
                Console.WriteLine($"t = {item.T}, {item.X.ToString("G3"):-7}");
                sp = item;
            }

            res = mp.GetAllParamsValues(sp);
            for(int i = 0; i < res.Length; i++) {
                Console.WriteLine($"{mp.AllParamsNames[i]} = \t{res[i]}");
            }
            //==============================================================================
            //var mp = new MaterialPoint() { Name = "ball" };
            //mp.AddForce(new ForceCenter(10,new Position3D(0,-1,0,"Dir"),null));
            //mp.Vel.X = 2;
            //mp.Vel.Y = 100;
            //mp.Vel.Z = -0.1;


            //var x0 = mp.Rebuild();

            //var solve = Ode.RK45(0,mp.Rebuild(),mp.f);

            //var res = mp.GetAllParamsValues(0,x0);
            //for(int i = 0; i < res.Length; i++) {
            //    Console.WriteLine($"{mp.AllParamsNames[i]} = \t{res[i]}");
            //}
            //SolPoint sp = new SolPoint();
            //string diffNames = "    V =  [ ";
            //foreach(var diffpar in mp.DiffArr) {
            //    diffNames += $"{diffpar.FullName}, ";
            //}
            //diffNames += " ];";
            //Console.WriteLine(  diffNames);
            //foreach(var item in solve.SolveFromToStep(0,21,1)) {
            //    Console.WriteLine($"t = {item.T},   \tV = {item.X}");
            //    sp = item;
            //}

            //res = mp.GetAllParamsValues(sp);
            //for(int i = 0; i < res.Length; i++) {
            //    Console.WriteLine($"{mp.AllParamsNames[i]} = \t{res[i]}");
            //}



            //var dm = new ScnObjDummy() { Name = "Rocket" };
            //dm.AddChild(new Position3D(new Vector3D(3,77,0)));
            //dm.AddChild(new MassPoint(10));
            //dm.AddChild(new Force3D(new Vector3D(0,-8,4)));
            //dm.AddLaw(new NewtonLaw4MatPoint3D());

            //var x0 = dm.Rebuild();

            //var solve = Ode.RK45(0,dm.Rebuild(),dm.f);

            //var res = dm.GetAllParamsValues(0,x0);
            //for(int i = 0; i < res.Length; i++) {
            //    Console.WriteLine($"{dm.AllParamsNames[i]} = \t{res[i]}");
            //}

            //SolPoint sp = new SolPoint();
            //foreach(var item in solve.SolveFromToStep(0,100,1)) {
            //    Console.WriteLine($"t = {item.T},   \tV = {item.X}");
            //    sp = item;
            //}

            //res = dm.GetAllParamsValues(sp);
            //for(int i = 0; i < res.Length; i++) {
            //    Console.WriteLine($"{dm.AllParamsNames[i]} = \t{res[i]}");
            //}
            //==========================
            //var sw = new Stopwatch();

            //var pos = new Position3D(0,0,0,"pos");

            //var pos_ = new Position3D(12,17,-22,"dposDt");
            //pos.AddChild(pos_);
            //pos.AddDiffVect(pos_);

            //var interpX = new InterpXY();
            //interpX.Add(0,0);
            //interpX.Add(2,1);
            //interpX.Add(6,-1);
            //interpX.Add(8,0);
            //pos_.pX.SealInterp(interpX);



            //var solve = Ode.RK45(0,pos.Rebuild(),pos.f,0.01);

            //SolPoint sp = new SolPoint();
            //sw.Start();
            //foreach(var item in solve.SolveFromToStep(0,10000,1000)) {
            //    Console.WriteLine($"t = {item.T},   \tV = {item.X}");
            //    sp = item;
            //}
            //sw.Stop();
            //Console.WriteLine($"{sw.ElapsedMilliseconds}");    
            //var res = pos.GetAllParamsValues(sp);
            //for(int i = 0; i < res.Length; i++) {
            //    Console.WriteLine($"{pos.AllParamsNames[i]} = \t{res[i]}");
            //}




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
