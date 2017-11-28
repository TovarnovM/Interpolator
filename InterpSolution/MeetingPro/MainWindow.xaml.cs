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
            mis.Vel.X = 10;
            mis.Temperature = -30;
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

        List<OneWay> tstRun() {
            var m = new Mis();
            m.Vec3D = new Vector3D(0, 0, 0);
            m.Vel.Vec3D = new Vector3D(1, 0, 0);
            m.Omega.Y = 0;
            m.Omega.Z = 0;
            m.SetTimeSynch(0);
            m.SynchQandM();

            var v0 = m.Rebuild();
            var sp0 = Ode.RK45(0, v0, m.f, 0.001).SolveTo(m.gr.r_rd.actT.Data.Last().Key + 0.5).Last();

            var ndv = m.GetNDemVec();
            var pos = m.GetMTPos();


            ndv.Kren = 0;
            ndv.Om_x = 0;
            ndv.Om_y = 0;
            ndv.Om_z = 0;
            ndv.Alpha = 10;
            ndv.Betta = 0;


            var gr = new GramofonLarva(ndv, pos);
            var sols = gr.GetSols();
            return sols;
        }
        Task<List<OneWay>> tstRunAsync() {
            return Task.Factory.StartNew<List<OneWay>>(tstRun);
        }
        private async void btn_Copy_Click(object sender, RoutedEventArgs e) {
            btn_Copy.IsEnabled = false;
            var sol = await tstRunAsync();
            sol.SaveToFile(@"C:\Users\User\Desktop\www.csv");
            int i = 90;
            btn_Copy.IsEnabled = true;
            //var fr = new Vector3D(0, 0, 0);
            //var to = new Vector3D(10, 20, 30);
            //var fr1 = new Vector3D(10, 10, 10);
            //var to1 = new Vector3D(20, 30, 30);
            //Vm.Pm.Series.Clear();
            //for (int n = 2; n < 5; n+=1) {
            //    var lst = Vm.GetBezie(fr, to, fr1, to1, n);

            //    Vm.Pm.Series.Add(new LineSeries() {
            //        ItemsSource = lst,
            //        DataFieldX = "X",
            //        DataFieldY = "Y",
            //        Smooth = true,
            //        Title = n.ToString()
            //    });
            //}

            //Vm.Pm.InvalidatePlot(true);
        }

        private void btn_Copy1_Click(object sender, RoutedEventArgs e) {
            var lst = GramSLoader.LoadFromFile(@"C:\Users\User\Desktop\www.csv");
            var pts = lst.AddCoord().Select(tp => new { X = tp.pos.X, Y = tp.pos.Y, Val = tp.ow.Vec1.Kren }).ToList();

            Vm.Pm.Series.Clear();
            Vm.Pm.Series.Add(new ScatterSeries() {
                DataFieldX = "X",
                DataFieldY = "Y",
                DataFieldValue = "Val",
                ItemsSource = pts,
                LabelFormatString = "{Val:0.0}"

            });

            var pts2 = lst.AddCoord().Uniquest().Select(tp => new { X = tp.pos.X, Y = tp.pos.Y, Val = tp.ow.Vec1.Kren }).ToList();
            Vm.Pm.Series.Add(new ScatterSeries() {
                DataFieldX = "X",
                DataFieldY = "Y",
                DataFieldValue = "Val",
                ItemsSource = pts2,
                LabelFormatString = "{Val:0.0}",
                LabelMargin = -16

            });
            Vm.Pm.InvalidatePlot(true);
            int f = 44;
        }
    }
}
