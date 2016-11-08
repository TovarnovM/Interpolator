using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneDemSPH {
    public class ViewModel {
        private ScatterSeries Ro;
        private ScatterSeries V;
        private ScatterSeries P;
        private ScatterSeries E;

        public ViewModel() {
            Model1 = GetNewModel("params", "X", "p,Ro,V");
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
        }

        public void Draw(OneDemExample curr) {
            Ro.Points.Clear();
            V.Points.Clear();
            P.Points.Clear();
            E.Points.Clear();
            foreach(var p in curr.AllParticles) {
                Ro.Points.Add(new ScatterPoint(p.X,p.Ro));
                V.Points.Add(new ScatterPoint(p.X,p.V));
                P.Points.Add(new ScatterPoint(p.X,p.P));
                E.Points.Add(new ScatterPoint(p.X,p.E));
            }
            Model1.InvalidatePlot(true);
        }

        public PlotModel Model1 { get; set; }
        public PlotModel GetNewModel(string title = "", string xname ="",string yname = "") {
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
