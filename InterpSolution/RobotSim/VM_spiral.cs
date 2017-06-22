using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotSim {
    public class VM_spiral {
        public PlotModel Pm { get; set; }
        public VM_spiral() {
            Pm = GetNewModel("Цель", "Х, м", "Y, м");

        }
        MagnitudeAxis magAx;
        public PlotModel GetNewModel(string title = "", string xname = "", string yname = "") {
            var model = new PlotModel {
                Title = title,
                //PlotMargins = new OxyThickness(40, 80, 40, 40),
                PlotType = PlotType.Polar,
                PlotAreaBorderThickness = new OxyThickness(0)
            };
            int fi_ = 3, ro_ = 10;
            var matrix = new double[fi_, ro_];
            for (int i = 0; i < fi_; i++) {
                for (int j = 0; j < ro_; j++) {
                    matrix[i, j] = 1;
                }
            }
            magAx = new MagnitudeAxis { Minimum = 0, Maximum = 4, MajorStep = 1, MinorStep = 0.2, Title = "Полный импульс, Н с",  };
            
                var aa = new AngleAxis { Minimum = 90, Maximum = 270, MajorStep = 30, MinorStep = 15 };
            model.Axes.Add(aa);
            model.Axes.Add(magAx);
            aa.StartAngle = 90;
            aa.EndAngle = 270;

            var lines1 = new AreaSeries() {
                Smooth = true,
                Title = "Угол возвышения 85 град."
            };
            lines1.Points.Add(new DataPoint(3.8,90));
            lines1.Points.Add(new DataPoint(3.75,120));
            lines1.Points.Add(new DataPoint( 3.65,150));
            lines1.Points.Add(new DataPoint(3.5,180 ));
            lines1.Points.Add(new DataPoint(3.70,210));
            lines1.Points.Add(new DataPoint(3.85, 240));
            lines1.Points.Add(new DataPoint(4, 270));
            lines1.Points2.Clear();
            lines1.Points2.Add(new DataPoint(3.5, 90));
            lines1.Points2.Add(new DataPoint(3.4, 120));
            lines1.Points2.Add(new DataPoint(3.3, 150));
            lines1.Points2.Add(new DataPoint(3.1, 180));
            lines1.Points2.Add(new DataPoint(3.35, 210));
            lines1.Points2.Add(new DataPoint(3.55, 240));
            lines1.Points2.Add(new DataPoint(3.6, 270));
            model.Series.Add(lines1);

            var lines2 = new AreaSeries() {
                Smooth = true,
                Title = "Угол возвышения 60 град."
            };
            lines2.Points.Add(new DataPoint(3.5, 90));
            lines2.Points.Add(new DataPoint(3.4, 120));
            lines2.Points.Add(new DataPoint(3.3, 150));
            lines2.Points.Add(new DataPoint(3.1, 180));
            lines2.Points.Add(new DataPoint(3.35, 210));
            lines2.Points.Add(new DataPoint(3.55, 240));
            lines2.Points.Add(new DataPoint(3.6, 270));
            lines2.Points2.Clear();
            lines2.Points2.Add(new DataPoint(3.3, 90));
            lines2.Points2.Add(new DataPoint(3.15, 120));
            lines2.Points2.Add(new DataPoint(3.0, 150));
            lines2.Points2.Add(new DataPoint(2.9, 180));
            lines2.Points2.Add(new DataPoint(3.1, 210));
            lines2.Points2.Add(new DataPoint(3.2, 240));
            lines2.Points2.Add(new DataPoint(3.4, 270));
            model.Series.Add(lines2);
            var lines3 = new AreaSeries() {
                Smooth = true,
                Title = "Угол возвышения 30 град."
            };
            lines3.Points.Add(new DataPoint(3.3, 90));
            lines3.Points.Add(new DataPoint(3.15, 120));
            lines3.Points.Add(new DataPoint(3.0, 150));
            lines3.Points.Add(new DataPoint(2.9, 180));
            lines3.Points.Add(new DataPoint(3.1, 210));
            lines3.Points.Add(new DataPoint(3.2, 240));
            lines3.Points.Add(new DataPoint(3.4, 270));
            lines3.Points2.Clear();
            lines3.Points2.Add(new DataPoint(2.8, 90));
            lines3.Points2.Add(new DataPoint(2.7, 120));
            lines3.Points2.Add(new DataPoint(2.55, 150));
            lines3.Points2.Add(new DataPoint(2.4, 180));
            lines3.Points2.Add(new DataPoint(2.65, 210));
            lines3.Points2.Add(new DataPoint(2.85, 240));
            lines3.Points2.Add(new DataPoint(3.0, 270));
            model.Series.Add(lines3);
            var lines4 = new AreaSeries() {
                Smooth = true,
                Title = "Угол возвышения 0град."
            };
            lines4.Points.Add(new DataPoint(2.8, 90));
            lines4.Points.Add(new DataPoint(2.7, 120));
            lines4.Points.Add(new DataPoint(2.55, 150));
            lines4.Points.Add(new DataPoint(2.4, 180));
            lines4.Points.Add(new DataPoint(2.65, 210));
            lines4.Points.Add(new DataPoint(2.85, 240));
            lines4.Points.Add(new DataPoint(3.0, 270));
            lines4.Points2.Clear();
            lines4.Points2.Add(new DataPoint(0, 0));
            model.Series.Add(lines4);
            return model;
        }

    }
}
