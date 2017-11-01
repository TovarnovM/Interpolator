using Microsoft.Research.Oslo;
using OxyPlot;
using OxyPlot.Series;
using Sharp3D.Math.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MeetingPro {
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public ViewMod Vm { get; set; }
        public MainWindow() {
            Graphs.FilePath = @"C:\Users\User\Documents\data.xml";
            Vm = new ViewMod();
            DataContext = this;
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            var mis = new Mis();
            //mis.delta_eler = -0.5 * Mis.RAD;
            mis.Vel.X = 0;
            var v0 = mis.Rebuild();
            double t0 = 0, t1 = 60, dt = 0.1;
            var res = Ode.RK45(t0, v0, mis.f, 0.001).SolveFromToStep(0, t1, dt);
            var s = mis.GetDiffPrms().Select(dp => dp.FullName).ToList();
            var sss = mis.GetAllParams().Select(dp => dp.FullName).ToList();
            var dct = mis.SaveToDict();

            var dlst = new List<Data4Draw>((int)((t1 - t0) / dt) + 5);

            foreach (var sp in res) {
                dlst.Add(mis.GetData4Draw());
            }

            Vm.DrawDL(dlst);

            //var lst = res.ToList();
            //Vm.Pm.Series.Clear();

            //var serXY = new LineSeries() { Title = "XY" };
            //var serXZ = new LineSeries() { Title = "XZ" };
            //Vm.Pm.Series.Add(serXY);
            //Vm.Pm.Series.Add(serXZ);
            //int xInd = s.IndexOf("Mis.X");
            //int yInd = s.IndexOf("Mis.Y");
            //int zInd = s.IndexOf("Mis.Z");
            //foreach (var sp in lst) {
            //    serXY.Points.Add(new DataPoint(sp.X[xInd], sp.X[yInd]));
            //    serXZ.Points.Add(new DataPoint(sp.X[xInd], sp.X[zInd]));
            //}

            //var sers = s.Select(ss => new LineSeries() { Title = ss }).ToArray();
            //foreach (var sp in lst) {
            //    var t = sp.T;
            //    for (int i = 0; i < sp.X.Length; i++) {
            //        sers[i].Points.Add(new DataPoint(t, sp.X[i]));
            //    }
            //}
            //foreach (var ser in sers) {
            //    Vm.Pm.Series.Add(ser);
            //}
            //Vm.Pm.InvalidatePlot(true);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e) {
            var mis = new Mis();
            var tetta0 = 40 * Mis.RAD;
            var VecOX = new Vector3D(Math.Cos(tetta0), Math.Sin(tetta0), 0);
            mis.RotateOxThenNearOy(VecOX, Vector3D.YAxis);
            mis.Vel.Vec3D = VecOX;
            var v0 = mis.Rebuild();
            var res = Ode.RK45(0, v0, mis.f, 0.005);


            var dlst = new List<Data4Draw>();

            foreach (var sp in res) {
                if(sp.T > 1 && mis.Y <= 0) {
                    break;
                }
                dlst.Add(mis.GetData4Draw());

            }
            var hmax = dlst.Select(d => d.Y).Max();
            btn.Content = hmax.ToString();
            Vm.DrawDL(dlst);

        }
    }
}
