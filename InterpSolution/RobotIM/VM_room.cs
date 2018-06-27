using OxyPlot;
using OxyPlot.Series;
using RobotIM.Scene;
using Sharp3D.Math.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotIM {
    public class VM_room: ViewModel {
        public Room room { get; set; }
        public ScatterSeries ss_noises;
        public ObservableCollection<StaticNoisePoint> NoiseList = new ObservableCollection<StaticNoisePoint>();
        public VM_room() {
            ss_noises = new ScatterSeries() {
                Title = "Шумы",
                LabelFormatString = "{FullName}"
            };
            ss_noises.DataFieldValue = "noiseDB";
            ss_noises.DataFieldX = "X";
            ss_noises.DataFieldY = "Y";
            ss_noises.DataFieldTag = "FullName";
            ss_noises.ItemsSource = NoiseList;
            Model1Rx.Value.Series.Add(ss_noises);
            MouseEvents4SS(ss_noises, Model1Rx.Value);

        }
        internal void GenerateNewRoom(RoomGeneratorSettings settings, double cellSize) {
            room = new Room();
            var tp = RoomGenerator.GetWallsAndFurs(settings);
            room.walls = tp.walls;
            room.furnitures = tp.furs;
            room.cellsize = cellSize;
            room.CreateScene();
            DrawRoom(room);

        }
        public void AddNoisePoint() {
            var center = (room.gabarit.p2 - room.gabarit.p1)*0.5 + room.gabarit.p1;
            AddNoisePoint(center.X, center.Y);

        }
        public void AddNoisePoint(double x, double y) {
            var np = new StaticNoisePoint(new Vector2D(x, y), 30);
            NoiseList.Add(np);
            Model1Rx.Value.InvalidatePlot(true);
        }

        void MouseEvents4SS(ScatterSeries s1, PlotModel model) {
            int indexOfPointToMove = -1;

            // Subscribe to the mouse down event on the line series
            s1.MouseDown += (s, e) => {
                // only handle the left mouse button (right button can still be used to pan)
                if (e.ChangedButton == OxyMouseButton.Left) {
                    int indexOfNearestPoint = (int)Math.Round(e.HitTestResult.Index);
                    var nearestPoint = s1.Transform(NoiseList[indexOfNearestPoint].X, NoiseList[indexOfNearestPoint].Y);

                    // Check if we are near a point
                    if ((nearestPoint - e.Position).Length < 10) {
                        // Start editing this point
                        indexOfPointToMove = indexOfNearestPoint;
                    } 

                    // Remember to refresh/invalidate of the plot
                    model.InvalidatePlot(true);

                    // Set the event arguments to handled - no other handlers will be called.
                    e.Handled = true;
                }
            };

            s1.MouseMove += (s, e) =>
            {
                if (indexOfPointToMove >= 0) {
                    // Move the point being edited.

                    NoiseList[indexOfPointToMove].X = s1.InverseTransform(e.Position).X;
                    NoiseList[indexOfPointToMove].Y = s1.InverseTransform(e.Position).Y;
                    model.InvalidatePlot(true);
                    e.Handled = true;
                }
            };

            s1.MouseUp += (s, e) => {
                // Stop editing
                indexOfPointToMove = -1;
                model.InvalidatePlot(false);
                e.Handled = true;
            };

        }

        public void CalcField() {
            room.staticNoisesList.Clear();
            room.staticNoisesList.AddRange(NoiseList);
            room.InitNoiseMap();
        }
    }
}
