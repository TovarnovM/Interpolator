using Microsoft.Research.Oslo;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using ReactiveODE;
using Sharp3D.Math.Core;
using System.Collections.Generic;

namespace RobotSim {
    public class ViewModel {
        private LineSeries bSer;
        private LineSeries Wheels;

        public VMPropRx<PlotModel,SolPoint> Model1Rx { get; private set; }
        public VMPropRx<List<SolPoint>,SolPoint> SolPointList { get; private set; }
        RobotDynamics _curr4Draw;


        public ViewModel() {
            _curr4Draw = new RobotDynamics();
            _curr4Draw.Rebuild();
            Model1Rx = new VMPropRx<PlotModel,SolPoint>(() => {
                var Model1 = GetNewModel("params","X","Y");
                bSer = new LineSeries() {
                    Title = "Body",
                    StrokeThickness = 2,
                    Color = OxyColors.Green
                };
                Model1.Series.Add(bSer);

                return Model1;

            },
            (sp,pm) => {
                _curr4Draw.SynchMeTo(sp);
                Draw(sp.T,pm);
                return pm;
            }
            );

            SolPointList = new VMPropRx<List<SolPoint>,SolPoint>(
                () => new List<SolPoint>(),
                (sp,lst) => {
                    lst.Add(sp);
                    return lst;
                });


        }



        public void Draw(double t,PlotModel pm) {
            bSer.Points.Clear();
            _curr4Draw.SynchMe(t);
            var xAxisNorm = -Vector3D.ZAxis;
            var yAxisNorm = Vector3D.YAxis;
            for(int i = 0; i < 7; i++) {
                bSer.Points.Add(new DataPoint(_curr4Draw.GetUgol(i) * xAxisNorm,_curr4Draw.GetUgol(i) * yAxisNorm));
            }
            

            //foreach(var p in _curr4Draw.AllParticles) {
            //    Ro.Points.Add(new ScatterPoint(p.X,p.Ro));
            //    V.Points.Add(new ScatterPoint(p.X,p.V));
            //    P.Points.Add(new ScatterPoint(p.X,p.P));
            //    E.Points.Add(new ScatterPoint(p.X,p.E));
            //}
            //pm.Title = $"{t:0.###} s,  RoMax = {_curr4Draw.Particles.Max(p => p.Ro):0.###},  Pmax = {_curr4Draw.Particles.Max(p => p.P):0.###}";
            pm.InvalidatePlot(true);
            //Thread.Sleep(1000);
        }

        public PlotModel GetNewModel(string title = "",string xname = "",string yname = "") {

            var m = new PlotModel { Title = title };
            m.PlotType = PlotType.Cartesian;
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