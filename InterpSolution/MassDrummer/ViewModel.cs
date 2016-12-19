using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MassDrummer {
    public class ViewModel {
        private AreaSeries kont;

        public PlotModel Model1 { get; private set; }
        public int DrawState { get; set; } = 1;
        public int WichGraph { get; set; } = 0;



        public ViewModel() {
            Model1 = GetNewModel("Ударники для Натусика)","X, мм","Y, мм");
            Model1.PlotType = PlotType.Cartesian;
            kont = new AreaSeries() {
                Title = "сечение ударника",
            };
            var line = new LineAnnotation();
            Model1.Annotations.Add(line);
            Model1.Series.Add(kont);
        }

        public void Draw(ShapeBase shape, string parName, double parVal) {
            //pm.Axes.Remove(colorAxis);
            kont.Points.Clear();
            kont.Points2.Clear();
            kont.Points.AddRange(shape.GetPoints());
            kont.Points2.AddRange(shape.GetPoints2());
            Model1.Title = $"{parName} = {parVal:0.####}";
            Model1.InvalidatePlot(true);
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
