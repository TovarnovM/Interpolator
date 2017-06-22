using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Series;
using RobotIM.IM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotIM {
    public class vmTrg {
        public PlotModel Pm { get; set; }
        public vmTrg() {
            Pm = GetNewModel("Цель", "Х, м", "Y, м");

        }

        public void DrawAim(PlotModel pm, Target f, string mess = "", int nShow = 0, int nVsego = 0, double p = 0) {
            pm.Title = $"mess \"{mess}\", N = {nVsego} выстрелов (показано {nShow}), вероятность поражения P = {p}";


            pm.Series.Clear();
            var ss = new ScatterSeries() {
                MarkerType = MarkerType.Star,
                MarkerSize = 4,
                MarkerFill = OxyColors.Red,
                MarkerStroke = OxyColors.Red,
                MarkerStrokeThickness = 1,


            };
            foreach (var point in f.Hits.Take(nShow)) {
                var sp = new ScatterPoint(point.Item2.X, point.Item2.Y, value: point.Item1, tag: "t = " + point.Item1.ToString("G1"));

                ss.Points.Add(sp);
            }
            pm.Series.Add(ss);

            pm.Annotations.Clear();

            foreach (var box in f.AimSurf.Boxes) {
                var ra = new RectangleAnnotation() {
                    MinimumX = box.xmin,
                    MaximumX = box.xmax,
                    MinimumY = box.ymin,
                    MaximumY = box.ymax,
                    Fill = OxyColors.Blue.ChangeSaturation(box.damage),
                    Layer = AnnotationLayer.BelowSeries
                };
                pm.Annotations.Add(ra);
            }

            var ap = new PointAnnotation() {
                X = f.AimSurf.AimPoint.X,
                Y = f.AimSurf.AimPoint.Y,
                Shape = MarkerType.Plus,
                Stroke = OxyColors.Green,
                StrokeThickness = 3,
                Size = 7,
                Text = $"({f.AimSurf.AimPoint})",
                Layer = AnnotationLayer.AboveSeries
            };

            pm.Annotations.Add(ap);


            pm.InvalidatePlot(true);



        }
        public static PlotModel GetNewModel(string title = "", string xname = "", string yname = "") {

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
