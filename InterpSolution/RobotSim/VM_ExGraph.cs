using Interpolator;
using OxyPlot;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotSim {
    public class VM_ExGraph {
        public PlotModel Pm { get; set; }
        public VM_ExGraph() {
            Pm = ViewModel.GetNewModel("Результаты", "t, сек", "...");
            Pm.PlotType = PlotType.XY;

        }

        public ObservableCollection<CheckedListItem<GraffLine>> graphs = new ObservableCollection<CheckedListItem<GraffLine>>();
        public void Rebuild(Experiments_Wall exp) {
            var res = exp.GetResults();
            Pm.Series.Clear();
            graphs.Clear();
            foreach (var r in res) {
                var ser = new LineSeries() {
                    Title = r.Key
                };
                foreach (var point in r.Value.Data) {
                    ser.Points.Add(new DataPoint(point.Key, point.Value.Value));
                }
                Pm.Series.Add(ser);
                var grLine = new GraffLine() {
                    Name = r.Key,
                    LineSer = ser
                };
                var chLstItem = new CheckedListItem<GraffLine>(grLine, true);
                chLstItem.PropertyChanged += grLine.ChLstItem_PropertyChanged;
                graphs.Add(chLstItem);
            }
        }
        public void AddSmoothDict(Dictionary<string,InterpXY> smDict) {
            foreach (var r in smDict) {
                var ser = new LineSeries() {
                    Title = r.Key
                };
                foreach (var point in r.Value.Data) {
                    ser.Points.Add(new DataPoint(point.Key, point.Value.Value));
                }
                Pm.Series.Add(ser);
                var grLine = new GraffLine() {
                    Name = r.Key,
                    LineSer = ser
                };
                var chLstItem = new CheckedListItem<GraffLine>(grLine, true);
                chLstItem.PropertyChanged += grLine.ChLstItem_PropertyChanged;
                graphs.Add(chLstItem);
            }
        }

        public void RebuildAll(List<Experiments_Wall> expList) {

        }
    }
}
