using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using Sharp3D.Math.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeetingPro {
    public class VM_traect {

        public PlotModel ModelXY { get; set; }
        public PlotModel ModelZY { get; set; }
        public PlotModel ModelXZ { get; set; }

        public List<PlotModel> allModels;


        public VM_traect() {
            CreatePlotModels();
            CreateAxis();
        }

        int serCount = 0;
        OxyColor[] cols = new OxyColor[] { OxyColors.Blue, OxyColors.BlueViolet, OxyColors.DarkOrange, OxyColors.Gray, OxyColors.Gold, OxyColors.MediumBlue, OxyColors.MediumTurquoise, OxyColors.OrangeRed };

        public void ClearSerries() {
            serCount = 0;
            allModels.ForEach(pm => pm.Series.Clear());
        }

        public void DrawOneTraectory(List<Vector3D> lst, string trname) {
            LineSeries serXY, serXZ, serZY;
            serXY = InitLineSer(lst, "X", "Y", trname, cols[serCount % cols.Length]);
            serXZ = InitLineSer(lst, "X", "Z", trname, cols[serCount % cols.Length]);
            serZY = InitLineSer(lst, "Z", "Y", trname, cols[serCount % cols.Length]);
            serCount++;
            ModelXY.Series.Add(serXY);
            ModelXZ.Series.Add(serXZ);
            ModelZY.Series.Add(serZY);
            allModels.ForEach(pm => pm.InvalidatePlot(true));
        }

        public LineSeries InitLineSer(List<Vector3D> lst, string fieldX, string fieldY, string title, OxyColor color) {
            return new LineSeries() {
                ItemsSource = lst,
                DataFieldX = fieldX,
                DataFieldY = fieldY,
                Title = title,
                Color = color,
                LineStyle = LineStyle.Dash
            };
        }

        //void CreateWheelLineSer(OxyColor color) {
        //    wheelSerListXY = new List<LineSeries>();
        //    wheelSerListXZ = new List<LineSeries>();
        //    wheelSerListZY = new List<LineSeries>();
        //    int n = DrawDummy.wheels.Count;
        //    for (int i = 0; i < n; i++) {
        //        var wheelSerXY = new LineSeries() {
        //            Title = "Wheel № " + i.ToString(),
        //            // StrokeThickness = 2,
        //            Color = cols[i]
        //        };
        //        wheelSerListXY.Add(wheelSerXY);
        //        ModelXY.Series.Add(wheelSerXY);
        //        allLineSer.Add(wheelSerXY);

        //        var wheelSerXZ = new LineSeries() {
        //            Title = "Wheel № " + i.ToString(),
        //            // StrokeThickness = 2,
        //            Color = cols[i]
        //        };
        //        wheelSerListXZ.Add(wheelSerXZ);
        //        ModelXZ.Series.Add(wheelSerXZ);
        //        allLineSer.Add(wheelSerXZ);

        //        var wheelSerZY = new LineSeries() {
        //            Title = "Wheel № " + i.ToString(),
        //            // StrokeThickness = 2,
        //            Color = cols[i]
        //        };
        //        wheelSerListZY.Add(wheelSerZY);
        //        ModelZY.Series.Add(wheelSerZY);
        //        allLineSer.Add(wheelSerZY);
        //    }

        //}

        //void CreateDrawDummy(Func<RobotDynamics> getDummyFunc) {
        //    DrawDummy = getDummyFunc();
        //    DrawDummy.Rebuild();
        //    Model1Rx = new VMPropRx<PlotModel, SolPoint>(
        //        () => {
        //            var Model1 = GetNewModel("params", "X", "Y");

        //            return Model1;

        //        },
        //        (sp, pm) => {
        //            DrawDummy.SynchMeTo(sp);
        //            Draw(sp.T, pm);
        //            return pm;
        //        }
        //    );

        //    SolPointList = new VMPropRx<List<SolPoint>, SolPoint>(
        //        () => new List<SolPoint>(),
        //        (sp, lst) => {
        //            lst.Add(sp);
        //            return lst;
        //        });

        //}
        void CreatePlotModels() {
            ModelXY = new PlotModel() { PlotType = PlotType.Cartesian };
            ModelXZ = new PlotModel();// { PlotType = PlotType.Cartesian };
            ModelZY = new PlotModel();// { PlotType = PlotType.Cartesian };
            allModels = new List<PlotModel>() {
                ModelXY,ModelXZ,ModelZY
            };
        }
        void CreateAxis() {
            var axisX_XY = new LinearAxis() {
                MajorGridlineStyle = LineStyle.Solid,
                MaximumPadding = 0,
                MinimumPadding = 0,
                MinorGridlineStyle = LineStyle.Dot,
                Position = AxisPosition.Bottom//,
                //Title = "X"
            };
            var axisY_XY = new LinearAxis() {
                MajorGridlineStyle = LineStyle.Solid,
                MaximumPadding = 0,
                MinimumPadding = 0,
                MinorGridlineStyle = LineStyle.Dot,
                Position = AxisPosition.Left//,
                //Title = "Y"
            };
            ModelXY.Axes.Add(axisX_XY);
            ModelXY.Axes.Add(axisY_XY);
            var axisX_XZ = new LinearAxis() {
                MajorGridlineStyle = LineStyle.Solid,
                MaximumPadding = 0,
                MinimumPadding = 0,
                MinorGridlineStyle = LineStyle.Dot,
                Position = AxisPosition.Bottom//,
                //Title = "X"
            };
            var axisZ_XZ = new LinearAxis() {
                MajorGridlineStyle = LineStyle.Solid,
                StartPosition = 1,
                EndPosition = 0,
                MaximumPadding = 0,
                MinimumPadding = 0,
                MinorGridlineStyle = LineStyle.Dot,
                Position = AxisPosition.Left//,
                                            // Title = "Z"
            };
            ModelXZ.Axes.Add(axisX_XZ);
            ModelXZ.Axes.Add(axisZ_XZ);



            var axisY_ZY = new LinearAxis() {
                MajorGridlineStyle = LineStyle.Solid,
                MaximumPadding = 0,
                MinimumPadding = 0,
                MinorGridlineStyle = LineStyle.Dot,
                Position = AxisPosition.Left//,
                //Title = "Z"
            };
            var axisZ_ZY = new LinearAxis() {
                MajorGridlineStyle = LineStyle.Solid,
                MaximumPadding = 0,
                MinimumPadding = 0,
                MinorGridlineStyle = LineStyle.Dot,
                Position = AxisPosition.Bottom//,
                //Title = "Y"
            };
            ModelZY.Axes.Add(axisY_ZY);
            ModelZY.Axes.Add(axisZ_ZY);

            bool isInternalChangeX = false;
            axisX_XY.AxisChanged += (s, e) => {
                if (isInternalChangeX) {
                    return;
                }
                isInternalChangeX = true;
                axisX_XZ.Zoom(axisX_XY.ActualMinimum, axisX_XY.ActualMaximum);
                this.ModelXZ.InvalidatePlot(false);
                isInternalChangeX = false;
            };
            axisX_XZ.AxisChanged += (s, e) => {
                if (isInternalChangeX) {
                    return;
                }
                isInternalChangeX = true;
                axisX_XY.Zoom(axisX_XZ.ActualMinimum, axisX_XZ.ActualMaximum);
                this.ModelXY.InvalidatePlot(false);
                isInternalChangeX = false;
            };

            bool isInternalChangeY = false;
            axisY_XY.AxisChanged += (s, e) => {
                if (isInternalChangeY) {
                    return;
                }
                isInternalChangeY = true;
                axisY_ZY.Zoom(axisY_XY.ActualMinimum, axisY_XY.ActualMaximum);
                this.ModelZY.InvalidatePlot(false);
                isInternalChangeY = false;
            };
            axisY_ZY.AxisChanged += (s, e) => {
                if (isInternalChangeY) {
                    return;
                }
                isInternalChangeY = true;
                axisY_XY.Zoom(axisY_ZY.ActualMinimum, axisY_ZY.ActualMaximum);
                this.ModelXY.InvalidatePlot(false);
                isInternalChangeY = false;
            };

            //bool isInternalChangeZ = false;
            //axisZ_ZY.AxisChanged += (s,e) => {
            //    if(isInternalChangeZ) {
            //        return;
            //    }
            //    isInternalChangeZ = true;
            //    axisZ_XZ.Zoom(axisZ_ZY.ActualMinimum,axisZ_ZY.ActualMaximum);
            //    this.ModelXZ.InvalidatePlot(false);
            //    isInternalChangeZ = false;
            //};
            //axisZ_XZ.AxisChanged += (s,e) => {
            //    if(isInternalChangeZ) {
            //        return;
            //    }
            //    isInternalChangeZ = true;
            //    axisZ_ZY.Zoom(axisZ_XZ.ActualMinimum,axisZ_XZ.ActualMaximum);
            //    this.ModelZY.InvalidatePlot(false);
            //    isInternalChangeZ = false;
            //};

        }


        //void DrawOneLineH(Line3D l) {
        //    var xAxisNorm = Vector3D.XAxis;
        //    var yAxisNorm = Vector3D.YAxis;
        //    hSerXY.Points.Add(new DataPoint(double.NaN, double.NaN));
        //    hSerXY.Points.Add(new DataPoint(l.p0 * xAxisNorm, l.p0 * yAxisNorm));
        //    hSerXY.Points.Add(new DataPoint(l.p1 * xAxisNorm, l.p1 * yAxisNorm));

        //    xAxisNorm = Vector3D.XAxis;
        //    yAxisNorm = Vector3D.ZAxis;
        //    hSerXZ.Points.Add(new DataPoint(double.NaN, double.NaN));
        //    hSerXZ.Points.Add(new DataPoint(l.p0 * xAxisNorm, l.p0 * yAxisNorm));
        //    hSerXZ.Points.Add(new DataPoint(l.p1 * xAxisNorm, l.p1 * yAxisNorm));

        //    xAxisNorm = Vector3D.ZAxis;
        //    yAxisNorm = Vector3D.YAxis;
        //    hSerZY.Points.Add(new DataPoint(double.NaN, double.NaN));
        //    hSerZY.Points.Add(new DataPoint(l.p0 * xAxisNorm, l.p0 * yAxisNorm));
        //    hSerZY.Points.Add(new DataPoint(l.p1 * xAxisNorm, l.p1 * yAxisNorm));
        //}



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
