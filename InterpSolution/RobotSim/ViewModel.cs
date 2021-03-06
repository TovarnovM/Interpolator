﻿using Microsoft.Research.Oslo;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Series;
using ReactiveODE;
using Sharp3D.Math.Core;
using System.Collections.Generic;
using System;
using SimpleIntegrator;

namespace RobotSim {
    public class ViewModel {
        private LineSeries bSerXY, bSerXZ, bSerZY, t1SerXY, t1SerXZ, t1SerZY, hSerXY, hSerXZ, hSerZY; 
        List<LineSeries> wheelSerListXY, wheelSerListXZ, wheelSerListZY;
        List<LineSeries> allLineSer = new List<LineSeries>();
        ScatterSeries massCentersXY, massCentersXZ, massCentersZY;
        List<ScatterSeries> allScattSer = new List<ScatterSeries>();
        //private List<ArrowAnnotation> fAnnotXY, fAnnotXZ, fAnnotZY;

        public PlotModel ModelXY { get; set; }
        public PlotModel ModelZY { get; set; }
        public PlotModel ModelXZ { get; set; }

        public double ForceMashtab { get; set; } = 1d;

        public VMPropRx<PlotModel,SolPoint> Model1Rx { get; private set; }
        public VMPropRx<List<SolPoint>,SolPoint> SolPointList { get; private set; }
        RobotDynamics DrawDummy;


        public ViewModel(Func<RobotDynamics> getDummyFunc) {
            CreateDrawDummy(getDummyFunc);
            //=============================================
            CreatePlotModels();
            CreateAxis();
            CreateBodyLineSer(OxyColors.Green);
            CreateTrackLineSer(OxyColors.CadetBlue);
            CreateWheelLineSer(OxyColors.Blue);
            CreateHeightLineSer(OxyColors.Red);
        }

        void SettingH(LineSeries ls,OxyColor color) {
            ls.Color = color;
            ls.LineStyle = LineStyle.Dash;
            ls.MarkerFill = color;
            ls.MarkerSize = 3;
            ls.MarkerStroke = OxyColors.White;
            ls.MarkerStrokeThickness = 1.5;
            ls.MarkerType = MarkerType.Circle;
            ls.StrokeThickness = 1.5;
           
        }

        private void CreateHeightLineSer(OxyColor color) {


            hSerXY = new LineSeries() {
                Title = "h",
                // StrokeThickness = 2,
                Color = color
            };
            SettingH(hSerXY,color);
            ModelXY.Series.Add(hSerXY);
            hSerXZ = new LineSeries() {
                Title = "h",
                //LabelFormatString = "{2}",
                // StrokeThickness = 2,
                Color = color
            };
            SettingH(hSerXZ,color);
            ModelXZ.Series.Add(hSerXZ);
            hSerZY = new LineSeries() {
                Title = "h",
                // StrokeThickness = 2,
                Color = color
            };
            SettingH(hSerZY,color);
            ModelZY.Series.Add(hSerZY);
            allLineSer.Add(hSerXY);
            allLineSer.Add(hSerXZ);
            allLineSer.Add(hSerZY);
        }

        void CreateWheelLineSer(OxyColor color) {
            wheelSerListXY = new List<LineSeries>();
            wheelSerListXZ = new List<LineSeries>();
            wheelSerListZY = new List<LineSeries>();
            OxyColor[] cols = new OxyColor[] { OxyColors.Blue,OxyColors.BlueViolet,OxyColors.DarkOrange,OxyColors.Gray,OxyColors.Gold,OxyColors.MediumBlue,OxyColors.MediumTurquoise,OxyColors.OrangeRed };
            int n = DrawDummy.wheels.Count;
            for(int i = 0; i < n; i++) {
                var wheelSerXY = new LineSeries() {
                    Title = "Wheel № " + i.ToString(),
                    // StrokeThickness = 2,
                    Color = cols[i]                
                };
                wheelSerListXY.Add(wheelSerXY);
                ModelXY.Series.Add(wheelSerXY);
                allLineSer.Add(wheelSerXY);

                var wheelSerXZ = new LineSeries() {
                    Title = "Wheel № " + i.ToString(),
                    // StrokeThickness = 2,
                    Color = cols[i]
                };
                wheelSerListXZ.Add(wheelSerXZ);
                ModelXZ.Series.Add(wheelSerXZ);
                allLineSer.Add(wheelSerXZ);

                var wheelSerZY = new LineSeries() {
                    Title = "Wheel № " + i.ToString(),
                    // StrokeThickness = 2,
                    Color = cols[i]
                };
                wheelSerListZY.Add(wheelSerZY);
                ModelZY.Series.Add(wheelSerZY);
                allLineSer.Add(wheelSerZY);
            }

        }

        void CreateDrawDummy(Func<RobotDynamics> getDummyFunc) {
            DrawDummy = getDummyFunc();
            DrawDummy.Rebuild();
            Model1Rx = new VMPropRx<PlotModel,SolPoint>(
                () => {
                    var Model1 = GetNewModel("params","X","Y");

                    return Model1;

                },
                (sp,pm) => {
                    DrawDummy.SynchMeTo(sp);
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
        void CreatePlotModels() {
            ModelXY = new PlotModel() { PlotType = PlotType.Cartesian };
            ModelXZ = new PlotModel();// { PlotType = PlotType.Cartesian };
            ModelZY = new PlotModel();// { PlotType = PlotType.Cartesian };
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
            axisX_XY.AxisChanged += (s,e) => {
                if(isInternalChangeX) {
                    return;
                }
                isInternalChangeX = true;
                axisX_XZ.Zoom(axisX_XY.ActualMinimum,axisX_XY.ActualMaximum);
                this.ModelXZ.InvalidatePlot(false);
                isInternalChangeX = false;
            };
            axisX_XZ.AxisChanged += (s,e) => {
                if(isInternalChangeX) {
                    return;
                }
                isInternalChangeX = true;
                axisX_XY.Zoom(axisX_XZ.ActualMinimum,axisX_XZ.ActualMaximum);
                this.ModelXY.InvalidatePlot(false);
                isInternalChangeX = false;
            };

            bool isInternalChangeY = false;
            axisY_XY.AxisChanged += (s,e) => {
                if(isInternalChangeY) {
                    return;
                }
                isInternalChangeY = true;
                axisY_ZY.Zoom(axisY_XY.ActualMinimum,axisY_XY.ActualMaximum);
                this.ModelZY.InvalidatePlot(false);
                isInternalChangeY = false;
            };
            axisY_ZY.AxisChanged += (s,e) => {
                if(isInternalChangeY) {
                    return;
                }
                isInternalChangeY = true;
                axisY_XY.Zoom(axisY_ZY.ActualMinimum,axisY_ZY.ActualMaximum);
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
        void CreateBodyLineSer(OxyColor color) {
            bSerXY = new LineSeries() {
                Title = "Body",
                // StrokeThickness = 2,
                Color = color
            };
            ModelXY.Series.Add(bSerXY);
            bSerXZ = new LineSeries() {
                Title = "Body",
                //LabelFormatString = "{2}",
                // StrokeThickness = 2,
                Color = color
            };
            ModelXZ.Series.Add(bSerXZ);
            bSerZY = new LineSeries() {
                Title = "Body",
                // StrokeThickness = 2,
                Color = color
            };
            ModelZY.Series.Add(bSerZY);
            allLineSer.Add(bSerXY);
            allLineSer.Add(bSerXZ);
            allLineSer.Add(bSerZY);

            massCentersXY = new ScatterSeries() {
                Title = "MassCenter",
                MarkerType = MarkerType.Circle,
                MarkerSize = 2,
                MarkerFill = color
            };
            ModelXY.Series.Add(massCentersXY);
            massCentersXZ = new ScatterSeries() {
                Title = "MassCenter",
                MarkerType = MarkerType.Circle,
                MarkerSize = 2,
                MarkerFill = color
            };
            ModelXZ.Series.Add(massCentersXZ);
            massCentersZY = new ScatterSeries() {
                Title = "MassCenter",
                MarkerType = MarkerType.Circle,
                MarkerSize = 2,
                MarkerFill = color
            };
            ModelZY.Series.Add(massCentersZY);
            allScattSer.Add(massCentersXY);
            allScattSer.Add(massCentersXZ);
            allScattSer.Add(massCentersZY);
        }
        void CreateTrackLineSer(OxyColor color) {
            t1SerXY = new LineSeries() {
                Title = "Track1",
                // StrokeThickness = 2,
                Color = color
            };
            ModelXY.Series.Add(t1SerXY);
            t1SerXZ = new LineSeries() {
                Title = "Track1",
                // StrokeThickness = 2,
                Color = color
            };
            ModelXZ.Series.Add(t1SerXZ);
            t1SerZY = new LineSeries() {
                Title = "Track1",
                // StrokeThickness = 2,
                Color = color
            };
            ModelZY.Series.Add(t1SerZY);

            allLineSer.Add(t1SerXY);
            allLineSer.Add(t1SerXZ);
            allLineSer.Add(t1SerZY);
        }

        public void Draw(double t,PlotModel pm) {
            foreach(var ls in allLineSer) {
                ls.Points.Clear();
            }
            foreach (var ss in allScattSer) {
                ss.Points.Clear();
            }
            ModelXY.Annotations.Clear();
            ModelXZ.Annotations.Clear();
            ModelZY.Annotations.Clear();

            ModelXY.Title = $"t = {t} s";
            ModelXZ.Title = $"t = {t} s";
            ModelZY.Title = $"t = {t} s";

            DrawDummy.SynchMe(t);


            DrawBody();
            // DrawBodyForces();

            DrawTrack1();

            DrawWheels();

            DrawHs();

            ModelXY.InvalidatePlot(true);
            ModelXZ.InvalidatePlot(true);
            ModelZY.InvalidatePlot(true);
            //Thread.Sleep(1000);
        }

        private void DrawHs() {
            //var l = FlatSurf.GetH(
            //    DrawDummy.Body.Vec3D,
            //  //  -Vector3D.YAxis,
            //    DrawDummy.surfs
            //    );
            //DrawOneLineH(l);
            foreach(var w in DrawDummy.wheels) {
                var l = FlatSurf.GetH(w.Vec3D,DrawDummy.surfs);
                DrawOneLineH(l);
            }
        }

        void DrawOneLineH(Line3D l) {
            var xAxisNorm = Vector3D.XAxis;
            var yAxisNorm = Vector3D.YAxis;
            hSerXY.Points.Add(new DataPoint(double.NaN,double.NaN));
            hSerXY.Points.Add(new DataPoint(l.p0 * xAxisNorm,l.p0 * yAxisNorm));
            hSerXY.Points.Add(new DataPoint(l.p1 * xAxisNorm,l.p1 * yAxisNorm));

            xAxisNorm = Vector3D.XAxis;
            yAxisNorm = Vector3D.ZAxis;
            hSerXZ.Points.Add(new DataPoint(double.NaN,double.NaN));
            hSerXZ.Points.Add(new DataPoint(l.p0 * xAxisNorm,l.p0 * yAxisNorm));
            hSerXZ.Points.Add(new DataPoint(l.p1 * xAxisNorm,l.p1 * yAxisNorm));

            xAxisNorm = Vector3D.ZAxis;
            yAxisNorm = Vector3D.YAxis;
            hSerZY.Points.Add(new DataPoint(double.NaN,double.NaN));
            hSerZY.Points.Add(new DataPoint(l.p0 * xAxisNorm,l.p0 * yAxisNorm));
            hSerZY.Points.Add(new DataPoint(l.p1 * xAxisNorm,l.p1 * yAxisNorm));
        }

        private void DrawWheels() {
            for(int i = 0; i < DrawDummy.wheels.Count; i++) {
                DrawOneWheel(DrawDummy.wheels[i],i);
            }
            //foreach(var w in DrawDummy.wheels) {
            //    DrawOneWheel(w);
            //}
        }

        private void DrawOneWheel(RbWheel w, int ind) {
            for(int i = 0; i < w.Zubya.Length; i++) {
                var z = w.Zubya[i];
                DrawOneZub(w,z, ind, i==0 ? 1d : 0.3);
            }
        }

        private void DrawOneZub(RbWheel w,Vector3D z, int ind, double k = 1d) {
            var xAxisNorm = Vector3D.XAxis;
            var yAxisNorm = Vector3D.YAxis;
            var z0 = w.Vec3D;
            var z1 = w.WorldTransform * z;

            
            z0 += (z1 - z0) * (1 - k);

            wheelSerListXY[ind].Points.Add(new DataPoint(double.NaN,double.NaN));                  
            wheelSerListXY[ind].Points.Add(new DataPoint(z0 * xAxisNorm,z0 * yAxisNorm));
            wheelSerListXY[ind].Points.Add(new DataPoint(z1 * xAxisNorm,z1 * yAxisNorm));


            xAxisNorm = Vector3D.XAxis;
            yAxisNorm = Vector3D.ZAxis;
            wheelSerListXZ[ind].Points.Add(new DataPoint(double.NaN,double.NaN));
            wheelSerListXZ[ind].Points.Add(new DataPoint(z0 * xAxisNorm,z0 * yAxisNorm));
            wheelSerListXZ[ind].Points.Add(new DataPoint(z1 * xAxisNorm,z1 * yAxisNorm));

            xAxisNorm = Vector3D.ZAxis;
            yAxisNorm = Vector3D.YAxis;
            wheelSerListZY[ind].Points.Add(new DataPoint(double.NaN,double.NaN));
            wheelSerListZY[ind].Points.Add(new DataPoint(z0 * xAxisNorm,z0 * yAxisNorm));
            wheelSerListZY[ind].Points.Add(new DataPoint(z1 * xAxisNorm,z1 * yAxisNorm));
        }

        private void DrawTrack1() {
            foreach(var track in DrawDummy.TracksAll) {
                DrawOneTrack(track);
            }
            //foreach(var track in DrawDummy.Tracks1) {
            //    DrawOneTrack(track);
            //}
            //foreach(var track in DrawDummy.Tracks2) {
            //    DrawOneTrack(track);
            //}
            //foreach(var track in DrawDummy.tDummy.Tracks) {
            //    DrawOneTrack(track);
            //}
        }

        private void DrawOneTrack(RbTrack track) {
            var xAxisNorm = Vector3D.XAxis;
            var yAxisNorm = Vector3D.YAxis;
                      
            t1SerXY.Points.Add(new DataPoint(double.NaN,double.NaN));
            var inds = new int[] { 0,1,3,2,0 };
            foreach(var i in inds) {
                t1SerXY.Points.Add(new DataPoint(track.GetConnPWorld(i) * xAxisNorm,track.GetConnPWorld(i) * yAxisNorm));
            }


            xAxisNorm = Vector3D.XAxis;
            yAxisNorm = Vector3D.ZAxis;
            t1SerXZ.Points.Add(new DataPoint(double.NaN,double.NaN));
            foreach(var i in inds) {
                t1SerXZ.Points.Add(new DataPoint(track.GetConnPWorld(i) * xAxisNorm,track.GetConnPWorld(i) * yAxisNorm));
            }

            xAxisNorm = Vector3D.ZAxis;
            yAxisNorm = Vector3D.YAxis;
            t1SerZY.Points.Add(new DataPoint(double.NaN,double.NaN));
            foreach(var i in inds) {
                t1SerZY.Points.Add(new DataPoint(track.GetConnPWorld(i) * xAxisNorm,track.GetConnPWorld(i) * yAxisNorm));
            }
        }

        private void DrawBodyForces() {
            foreach(var force in DrawDummy.Body.Forces) {
                DrawForce(force, DrawDummy.Body, Vector3D.XAxis, Vector3D.YAxis,ModelXY, OxyColors.Red);
                DrawForce(force,DrawDummy.Body,Vector3D.XAxis,Vector3D.ZAxis,ModelXZ,OxyColors.Red);
                DrawForce(force,DrawDummy.Body,Vector3D.ZAxis,Vector3D.YAxis,ModelZY,OxyColors.Red);
            }

            foreach(var moment in DrawDummy.Body.Moments) {
                DrawForce(moment,DrawDummy.Body,Vector3D.XAxis,Vector3D.YAxis,ModelXY,OxyColors.Blue);
                DrawForce(moment,DrawDummy.Body,Vector3D.XAxis,Vector3D.ZAxis,ModelXZ,OxyColors.Blue);
                DrawForce(moment,DrawDummy.Body,Vector3D.ZAxis,Vector3D.YAxis,ModelZY,OxyColors.Blue);
            }

            foreach(var force in DrawDummy.Body.ForcesNegative) {
                DrawForce(force,DrawDummy.Body,Vector3D.XAxis,Vector3D.YAxis,ModelXY,OxyColors.Red,-1);
                DrawForce(force,DrawDummy.Body,Vector3D.XAxis,Vector3D.ZAxis,ModelXZ,OxyColors.Red,-1);
                DrawForce(force,DrawDummy.Body,Vector3D.ZAxis,Vector3D.YAxis,ModelZY,OxyColors.Red,-1);
            }

            foreach(var moment in DrawDummy.Body.MomentsNegative) {
                DrawForce(moment,DrawDummy.Body,Vector3D.XAxis,Vector3D.YAxis,ModelXY,OxyColors.Blue,-1);
                DrawForce(moment,DrawDummy.Body,Vector3D.XAxis,Vector3D.ZAxis,ModelXZ,OxyColors.Blue,-1);
                DrawForce(moment,DrawDummy.Body,Vector3D.ZAxis,Vector3D.YAxis,ModelZY,OxyColors.Blue,-1);
            }
        }



        private void DrawForce(Force force, IOrient3D toBody,Vector3D xAxisNorm,Vector3D yAxisNorm, PlotModel m, OxyColor color, int neg = 1) {          
            double x1 = force.AppPoint == null
                ? toBody.Vec3D * xAxisNorm
                : force.AppPoint.Vec3D_World * xAxisNorm * neg;
            double y1 = force.AppPoint == null
                ? toBody.Vec3D * yAxisNorm
                : force.AppPoint.Vec3D_World * yAxisNorm * neg;

            double x2 = force.Vec3D_Dir_World * xAxisNorm * ForceMashtab + x1;
            double y2 = force.Vec3D_Dir_World * yAxisNorm * ForceMashtab + y1;

            DrawForce(m,x1,y1,x2,y2,force.Value.ToString(), color);
        }

        private void DrawForce(PlotModel pm, double x1, double y1, double x2, double y2, string title,OxyColor color) {
            var a = new ArrowAnnotation() {
                Color = color,
                Text = title,
                StartPoint = new DataPoint(x1,y1),
                EndPoint = new DataPoint(x2,y2)
            };
            pm.Annotations.Add(a);
            
        }

        void DrawLineBody(int i0, int i1) {
            var xAxisNorm = Vector3D.XAxis;
            var yAxisNorm = Vector3D.YAxis;
            bSerXY.Points.Add(new DataPoint(double.NaN,double.NaN));
            bSerXY.Points.Add(new DataPoint(DrawDummy.GetUgol(i0) * xAxisNorm,DrawDummy.GetUgol(i0) * yAxisNorm));
            bSerXY.Points.Add(new DataPoint(DrawDummy.GetUgol(i1) * xAxisNorm,DrawDummy.GetUgol(i1) * yAxisNorm));

            xAxisNorm = Vector3D.XAxis;
            yAxisNorm = Vector3D.ZAxis;
            bSerXZ.Points.Add(new DataPoint(double.NaN,double.NaN));
            bSerXZ.Points.Add(new DataPoint(DrawDummy.GetUgol(i0) * xAxisNorm,DrawDummy.GetUgol(i0) * yAxisNorm));
            bSerXZ.Points.Add(new DataPoint(DrawDummy.GetUgol(i1) * xAxisNorm,DrawDummy.GetUgol(i1) * yAxisNorm));

            xAxisNorm = Vector3D.ZAxis;
            yAxisNorm = Vector3D.YAxis;
            bSerZY.Points.Add(new DataPoint(double.NaN,double.NaN));
            bSerZY.Points.Add(new DataPoint(DrawDummy.GetUgol(i0) * xAxisNorm,DrawDummy.GetUgol(i0) * yAxisNorm));
            bSerZY.Points.Add(new DataPoint(DrawDummy.GetUgol(i1) * xAxisNorm,DrawDummy.GetUgol(i1) * yAxisNorm));
        }
        void DrawBody() {
            
            DrawLineBody(0,1);
            DrawLineBody(1,2);
            DrawLineBody(2,3);
            DrawLineBody(3,0);
            DrawLineBody(4,5);
            DrawLineBody(5,6);
            DrawLineBody(6,7);
            DrawLineBody(7,4);
            DrawLineBody(0,4);
            DrawLineBody(1,5);
            DrawLineBody(2,6);
            DrawLineBody(3,7);

            var vec = DrawDummy.Body.Vec3D;
            var xAxisNorm = Vector3D.XAxis;
            var yAxisNorm = Vector3D.YAxis;
            massCentersXY.Points.Add(new ScatterPoint(vec * xAxisNorm, vec * yAxisNorm));

            xAxisNorm = Vector3D.XAxis;
            yAxisNorm = Vector3D.ZAxis;
            massCentersXZ.Points.Add(new ScatterPoint(vec * xAxisNorm, vec * yAxisNorm));

            xAxisNorm = Vector3D.ZAxis;
            yAxisNorm = Vector3D.YAxis;
            massCentersZY.Points.Add(new ScatterPoint(vec * xAxisNorm, vec * yAxisNorm));
        }

        public static PlotModel GetNewModel(string title = "",string xname = "",string yname = "") {

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