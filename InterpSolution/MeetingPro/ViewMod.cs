using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using Sharp3D.Math.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MeetingPro {
    public class ViewMod {
        public PlotModel Pm { get; set; }
        public ViewMod() {
            Pm = GetNewModel("графики", "время, с", "stuff");
        }

        public void DrawDL(List<Data4Draw> dl){
            var names = typeof(Data4Draw)
                .GetProperties(BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.Public)
                .Select(pi => pi.Name)
                .ToList();

            var filter = new List<string>() {
                "T",
               "V",
                "V_x",
                "V_y",
                "V_z",
                "dV_x",
                "dV_y",
                "dV_z",
               "X",
                "Y",
                "Z",
            //    "Alpha",
            //    "P",
                "Om_x",
                "Om_y",
                "Om_z",
            //    "Kren"
            };



            var sers = names
                .Where(n => !filter.Contains(n))
                .Select(n => new LineSeries() {
                    Title = n,
                    ItemsSource = dl,
                    DataFieldX = "T",
                    DataFieldY = n
                })
                .ToList();

            Pm.Series.Clear();
            sers.ForEach(s => Pm.Series.Add(s));

            Pm.InvalidatePlot(true);

        }
        public static PlotModel GetNewModel(string title = "", string xname = "", string yname = "") {

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

            //linearColorAxis1 = new LinearColorAxis();
            //linearColorAxis1.Position = AxisPosition.Right;
            //linearColorAxis1.Palette = OxyPalettes.Jet(100);
            //linearColorAxis1.AbsoluteMaximum = 1;
            //linearColorAxis1.AbsoluteMinimum = 0;

            //m.Axes.Add(linearColorAxis1);

            return m;
        }
        public List<Vector3D> GetBezie(Vector3D fr, Vector3D to, Vector3D fr_1, Vector3D to_1, int n) {
            //Vector3D interpF(Vector3D fr_p, Vector3D to_p, double interp) {
            //    return to_p * interp + (1 - interp) * fr_p;
            //}
            var res = new List<Vector3D>(n+1);
            res.Add(fr);
            double dn = 1d / (n);
            for (int i = 1; i < n; i++) {
                var t = dn * i;
                var p1 = fr_1 * t + (1 - t) * fr;// interpF(fr, fr_1, t);
                var p2 = to_1 * t + (1 - t) * fr_1;// interpF(fr_1, to_1, t);
                var p3 =  to* t +(1 - t) * to_1;//interpF(to_1, to, t);
                var p12 =  p2* t +(1 - t) * p1;//interpF(p1, p2, t);
                var p23 =  p3* t +(1 - t) * p2;//interpF(p2, p3, t);
                var p =  p23* t +(1 - t) * p12;//interpF(p12, p23, t);
                res.Add(p);
            }
            res.Add(to);
            return res;
        }

    }
}
