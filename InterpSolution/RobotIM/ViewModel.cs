﻿using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using ReactiveODE;
using RobotIM.Core;
using RobotIM.Scene;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotIM {
    public class ViewModel {
        protected ScatterSeries P,pdead;
        protected LineSeries SerWalls, SerVision, SerFurs;
        protected HeatMapSeries SerCells;
        protected LinearColorAxis linearColorAxis1;

        public VMPropRx<PlotModel, GameLoop> Model1Rx { get; private set; }


        public ViewModel() {
            Model1Rx = new VMPropRx<PlotModel, GameLoop>(() => {
                var Model1 = GetNewModel("Field", "X, м", "Y, м");
                Model1.PlotType = PlotType.Cartesian;
                P = new ScatterSeries() {
                    Title = "P",
                    MarkerType = MarkerType.Triangle,
                    MarkerSize = 5,
                    MarkerFill = OxyColors.Green,
                    ColorAxisKey = null
                };
                Model1.Series.Add(P);

                pdead = new ScatterSeries() {
                    Title = "dead",
                    MarkerType = MarkerType.Diamond,
                    MarkerSize = 5,
                    MarkerFill = OxyColors.Gray,
                    ColorAxisKey = null
                };
                Model1.Series.Add(pdead);

                SerWalls = new LineSeries() {
                    Color = OxyColors.DarkBlue,
                    StrokeThickness = 3
                };
                Model1.Series.Add(SerWalls);
                SerFurs = new LineSeries() {
                    Color = OxyColors.DarkGreen,
                    StrokeThickness = 3
                };
                Model1.Series.Add(SerFurs);
                SerVision = new LineSeries() {
                    Color = OxyColors.Green,
                    StrokeThickness = 2
                };
                Model1.Series.Add(SerVision);

                SerCells = new HeatMapSeries();
                Model1.Series.Add(SerCells);
                return Model1;

            },
            (gl, pm) => {
                Draw(gl, pm);
                return pm;
            }
            );


        }

        public void DrawRoom(Room room, bool drawCells = false) {
            SerWalls.Points.Clear();
            foreach (var w in room.walls) {
                SerWalls.Points.Add(new DataPoint(Double.NaN, Double.NaN));
                foreach (var p in w.pointsList) {
                    SerWalls.Points.Add(new DataPoint(p.X, p.Y));
                }
            }
            SerFurs.Points.Clear();
            foreach (var f in room.furnitures) {
                SerFurs.Points.Add(new DataPoint(Double.NaN, Double.NaN));
                foreach (var p in f.pointsList) {
                    SerFurs.Points.Add(new DataPoint(p.X, p.Y));
                }
            }

            var gab = room.gabarit;

            SerCells.IsVisible = drawCells;
            if (!drawCells) {
                Model1Rx.Value.InvalidatePlot(true);
                return;
            }
            bool noNoise = !(room.staticNoiseMap != null && room.staticNoiseMap.Length != 0);   
            if (!noNoise) {
                linearColorAxis1.AbsoluteMaximum = room.staticNoiseMap.Max2D();
            }
            SerCells.X0 = gab.p1.X;
            SerCells.X1 = gab.p2.X;
            SerCells.Y0 = gab.p1.Y;
            SerCells.Y1 = gab.p2.Y;
            SerCells.Interpolate = false;
            SerCells.Data = new Double[room.searchGrid.width, room.searchGrid.height];
            for (int i = 0; i < room.searchGrid.width; i++) {
                for (int j = 0; j < room.searchGrid.height; j++) {
                    if(noNoise)
                        SerCells.Data[i, j] = room.searchGrid.IsWalkableAt(i, j) ? 1d : 0d;
                    else {
                        var noise = room.staticNoiseMap[i, j];
                        SerCells.Data[i, j] = noise; // room.searchGrid.IsWalkableAt(i, j) ? noise : 0d;
                    }
                }
            }


            Model1Rx.Value.InvalidatePlot(true);
        }
        public void ReDraw() {
            Model1Rx.Value.InvalidatePlot(true);
        }
        public void Draw(GameLoop t, PlotModel pm) {
            P.Points.Clear();
            SerVision.Points.Clear();
            foreach (var p in t.GetUnitsSpec<UnitWithVision>(false)) {
                var ser = p.Enabled ? P : pdead;
                ser.Points.Add(new ScatterPoint(p.X, p.Y,value:1));
                if (!p.Enabled)
                    continue;
                SerVision.Points.Add(new DataPoint(Double.NaN, Double.NaN));
                SerVision.Points.Add(new DataPoint(p.X, p.Y));
                SerVision.Points.Add(new DataPoint(p.X + p.viewDir.X, p.Y + p.viewDir.Y));
            }
            pm.Title = $"{t.Time:0.###} s";
            pm.InvalidatePlot(true);
        }

        public PlotModel GetNewModel(string title = "", string xname = "", string yname = "") {

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
