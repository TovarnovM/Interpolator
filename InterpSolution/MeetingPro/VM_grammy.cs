using Microsoft.Research.Oslo;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeetingPro {
    public class VM_grammy {
        protected LinearColorAxis linearColorAxis1;
        public PlotModel Pm { get; set; }
        public Dictionary<string, Func<Vector, double>> dict;
        public VM_grammy() {
            Pm = GetNewModel("Грамофон", "x, м", "y, м");
            dict = new Dictionary<string, Func<Vector, double>>() {
                ["Temperature"] = v => v[0],
                ["T"] = v => v[1],
                ["V"] = v => v[2],
                ["Alpha"] = v => v[3],
                ["Betta"] = v => v[4],
                ["Thetta"] = v => v[5],
                ["x"] = v => v.Length > 6 ? v[6] : 0,
                ["y"] = v => v.Length > 6 ? v[7] : 0,
                ["z"] = v => v.Length > 6 ? v[8] : 0,
                ["Vx"] = v => v.Length > 6 ? v[9] : 0,
                ["Vy"] = v => v.Length > 6 ? v[10] : 0,
                ["Vz"] = v => v.Length > 6 ? v[11] : 0
            };
        }
        
        public void DrawGrammy(Grammy gr, string dispName, double min, double max) {
            Pm.Series.Clear();
            
            Pm.Series.Add(GetNewSS(gr.vBegin, dispName, "begin"));
            //int i = 0;
            //foreach (var pl in gr.polygons) {
            //    Pm.Series.Add(GetNewSS(pl.v1, dispName, $"polygon {i}"));
            //    Pm.Series.Add(GetNewSS(pl.v2, dispName, $"polygon {i}"));
            //    Pm.Series.Add(GetNewSS(pl.v3, dispName, $"polygon {i}"));
            //    i++;
            //}

            Pm.Series.Add(GetNewSS(gr.vCenter, dispName, "vCenter"));
            Pm.Series.Add(GetNewSS(gr.vLeft, dispName, "vLeft"));
            Pm.Series.Add(GetNewSS(gr.vRight, dispName, "vRight"));
            Pm.Series.Add(GetNewSS(gr.vUp, dispName, "vUp"));
            Pm.Series.Add(GetNewSS(gr.vDown, dispName, "vDown"));

            linearColorAxis1.AbsoluteMinimum = min;
            linearColorAxis1.AbsoluteMaximum = max;
            linearColorAxis1.Minimum = min;
            linearColorAxis1.Maximum = max;

            Pm.Title = dispName;
            Pm.InvalidatePlot(true);
        }

        ScatterSeries GetNewSS(Vector vec, string dispName, string title) {
            var p = new {
                X = dict["z"](vec),
                Y = dict["y"](vec),
                Val = dict[dispName](vec)
            };
            var ps = new object[] { p };

            var ss = new ScatterSeries() {
//                Title = title,
                ItemsSource = ps,
                DataFieldX = "X",
                DataFieldY = "Y",
                DataFieldValue = "Val",
                LabelFormatString = title + " {Val:0.##}",
                ColorAxisKey = linearColorAxis1.Key
            };

            return ss;
        }

        public PlotModel GetNewModel(string title = "", string xname = "", string yname = "") {

            var m = new PlotModel { Title = title };
           // m.PlotType = PlotType.Cartesian;
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

            linearColorAxis1 = new LinearColorAxis();
            linearColorAxis1.Position = AxisPosition.Right;
            linearColorAxis1.Palette = OxyPalettes.Jet(100);
            linearColorAxis1.AbsoluteMaximum = 1;
            linearColorAxis1.AbsoluteMinimum = 0;

            m.Axes.Add(linearColorAxis1);
            return m;
        }
    }
}
