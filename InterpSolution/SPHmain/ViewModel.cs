using Microsoft.Research.Oslo;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using ReactiveODE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPH_2D {
    public class ViewModel {
        private ScatterSeries Ro;
        private ScatterSeries V;
        private ScatterSeries P;
        private ScatterSeries E;
        double Ro0,  P0, E0;

        public VMPropRx<PlotModel,SolPoint> Model1Rx { get; private set; }
        Sph2D _curr4Draw;

        public ViewModel(Sph2D curr) {
            _curr4Draw = MainWindow.GetTest();
            _curr4Draw.Rebuild();

            Fill0s(curr);

            Model1Rx = new VMPropRx<PlotModel,SolPoint>(
                () => {
                    var Model1 = GetNewModel("params","X","p,Ro,V");
                    P = new ScatterSeries() {
                        Title = "P",
                        MarkerType = MarkerType.Triangle,
                        MarkerSize = 2,
                        MarkerFill = OxyColors.Green
                    };
                    Model1.Series.Add(P);

                    Ro = new ScatterSeries() {
                        Title = "Ro",
                        MarkerType = MarkerType.Diamond,
                        MarkerSize = 2,
                        MarkerFill = OxyColors.DarkOrange
                    };
                    Model1.Series.Add(Ro);

                    V = new ScatterSeries() {
                        Title = "V",
                        MarkerType = MarkerType.Circle,
                        MarkerSize = 2,
                        MarkerFill = OxyColors.Blue
                    };
                    Model1.Series.Add(V);

                    E = new ScatterSeries() {
                        Title = "E",
                        MarkerType = MarkerType.Circle,
                        MarkerSize = 2,
                        MarkerFill = OxyColors.Red
                    };
                    Model1.Series.Add(E);
                    return Model1;
                },
                (sp,pm) => {
                    _curr4Draw.SynchMeTo(sp);
                    Draw(sp.T,pm);
                    return pm;
                });


        }

        public void Fill0s(Sph2D curr) {
            Ro0 = curr.AllParticles.Cast<IsotropicGasParticle>().Max(p => p.Ro);
            P0 = curr.AllParticles.Cast<IsotropicGasParticle>().Max(p => p.P);
            E0 = curr.AllParticles.Cast<IsotropicGasParticle>().Max(p => p.E);
        }

        public void Draw(double t, PlotModel pm) {
            Ro.Points.Clear();
            V.Points.Clear();
            P.Points.Clear();
            E.Points.Clear();
            foreach(var p in _curr4Draw.AllParticles.Cast<IsotropicGasParticle>()) {
                Ro.Points.Add(new ScatterPoint(p.X,p.Ro/Ro0));
                V.Points.Add(new ScatterPoint(p.X,p.Vel.X/p.GetCl()));
                P.Points.Add(new ScatterPoint(p.X,p.P/P0));
                E.Points.Add(new ScatterPoint(p.X,p.E/E0));
            }
            pm.Title = $"{t:0.##########} s,  RoMax = {_curr4Draw.Particles.Cast<IsotropicGasParticle>().Max(p => p.Ro):0.###},  Pmax = {_curr4Draw.Particles.Cast<IsotropicGasParticle>().Max(p => p.P):0.###}";
            pm.InvalidatePlot(true);
        }
        
        public PlotModel GetNewModel(string title = "",string xname = "",string yname = "") {
            var m = new PlotModel { Title = title };
            var linearAxis1 = new LinearAxis();
            linearAxis1.MajorGridlineStyle = LineStyle.Solid;
            linearAxis1.MaximumPadding = 0;
            linearAxis1.MinimumPadding = 0;
            linearAxis1.MinorGridlineStyle = LineStyle.Dot;
            linearAxis1.Position = AxisPosition.Bottom;
            linearAxis1.Title = xname;
            m.Axes.Add(linearAxis1);
            var linearAxis2 = new LinearAxis();
            linearAxis2.MajorGridlineStyle = LineStyle.Solid;
            linearAxis2.MaximumPadding = 0;
            linearAxis2.MinimumPadding = 0;
            linearAxis2.MinorGridlineStyle = LineStyle.Dot;
            linearAxis2.Title = yname;
            m.Axes.Add(linearAxis2);
            return m;
        }
    }
}
