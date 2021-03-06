﻿using OxyPlot;
using OxyPlot.Series;
using Sharp3D.Math.Core;
using SimpleIntegrator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotSim {
    public class TestVM {
        public PlotModel ModelTest { get; set; }
        LineSeries r, a;
        public TestVM() {
            ModelTest = ViewModel.GetNewModel("Test","x","y");
            r = new LineSeries() {
                Title = "right"
            };
            ModelTest.Series.Add(r);
            a = new LineSeries() {
                Title = "полученные"
            };
            ModelTest.Series.Add(a);
        }

        internal void Draw(List<double> ts,List<double> rightAnsw,List<double> answrs) {
            r.Points.Clear();
            a.Points.Clear();
            for(int i = 0; i < ts.Count; i++) {
                r.Points.Add(new DataPoint(ts[i],rightAnsw[i]));
                a.Points.Add(new DataPoint(ts[i],answrs[i]));
            }
            ModelTest.InvalidatePlot(true);
        }
    }

    class Majatnik : MaterialObjectNewton {
        public Majatnik() {
            Mass.Value = 10;

            var v0 = new Vector3D(1,0,0);
            
            Vec3D = v0;

            //var fg = Force.GetForceCentered(Mass.Value * 9.8,new Vector3D(0,-1,0));
            //AddForce(fg);

            var p0 = new Vector3D(0,0,0);
            L = (v0 - p0).GetLength();
            var fn = Force.GetForceCentered(new Vector3D(1,1,1));
            fn.SynchMeBefore += t => {
                var ff = Phys3D.GetKMuForce_Step(Vec3D,Vel.Vec3D,p0,Vector3D.Zero,1000,100,0.5);
                fn.Value = ff.GetLength();
                fn.Direction.Vec3D = ff.Norm;
            };
            AddForce(fn);
        }

        public double L { get; private set; }
    }
}
