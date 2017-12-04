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
            //Graphs.FilePath = @"C:\Users\User\Documents\data.xml";
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


            var gr = GramofonLarva.Default();
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
            try {
                var sd = new Microsoft.Win32.OpenFileDialog() {
                    Filter = "room Files|*.csv",
                    FileName = "room"
                };
                if (sd.ShowDialog() == true) {
                    var lst = GramSLoader.LoadFromFile(sd.FileName);
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
                }


            } finally {

            }

            int f = 44;
        }

        GrammyExecutor ge;
        private void Button_Click_2(object sender, RoutedEventArgs e) {
            btn_run.IsEnabled = false;
            
            var gl = GramofonLarva.Default(GramSLoader.GetDouble(tb_temperature.Text));
            int id = (int)GramSLoader.GetDouble(tb_id.Text);
            ge = new GrammyExecutor(gl, id);
            Graphs.FilePath = ge.datapath;
            //ge.saveFPath = @"C:\Users\User\Desktop\wwww1";
            //ge.callback = Exc_ExecutDoneNew;
            ge.Run();
        }

        private void Exc_ExecutDoneNew() {
            btn_run.Content = $"{ge.done} / {ge.inqueue}";
        }

        private void btn_run_Copy_Click(object sender, RoutedEventArgs e) {
            btn_run_Copy.Content = $"{ge.done} / {ge.inqueue}";
        }

        private void btn_plan_Click(object sender, RoutedEventArgs e) {
            int thn = 7;
            double th0 = -75, th1 = -th0, dth = (th1 - th0) / (thn - 1);

            int alphn = 7;
            double alph0 = -15, alph1 = -alph0, dalph = (alph1 - alph0) / (alphn - 1);

            int betn = 7;
            double bet0 = -15, bet1 = -bet0, dbet = (bet1 - bet0) / (betn - 1);

            int vn = 7;
            double v0 = 150, v1 = 300, dv = (v1 - v0) / (vn - 1);

            int tempn = 5;
            double temp0 = -50, temp1 = 50, dtemp = (temp1 - temp0) / (tempn - 1);

            int tn = 5;
            double t0 = 5, t1 = 45, dt = (t1 - t0) / (tn - 1);

            var lst = new List<OneWay>(thn * alphn * betn * vn * tempn * tn);
            double id = 0d;

            for (int i = 0; i < thn; i++) {
                double th = th0 + dth * i;

                for (int j = 0; j < alphn; j++) {
                    double alph = alph0 + dalph * j;

                    for (int k = 0; k < betn; k++) {
                        double bet = bet0 + dbet * k;

                        for (int i1 = 0; i1 < vn; i1++) {
                            double v = v0 + dv * i1;

                            for (int j1 = 0; j1 < tempn; j1++) {
                                double temp = temp0 + dtemp * j1;

                                for (int k1 = 0; k1 < tn; k1++) {
                                    double t = t0 + dt * k1;

                                    var ow = new OneWay();
                                    ow.Pos0 = new MT_pos(v, th);
                                    ow.Vec0 = new NDemVec() {
                                        V = v,
                                        T = t,
                                        Temperature = temp,
                                        Thetta = th,
                                        Alpha = alph,
                                        Betta = bet,
                                        Kren = 0,
                                        Om_x = 0,
                                        Om_y = 0,
                                        Om_z = 0

                                    };
                                    ow.Pos1 = new MT_pos();
                                    ow.Vec1 = new NDemVec();
                                    ow.Id = id;
                                    id += 1d;

                                    lst.Add(ow);
                                }
                            }
                        }
                    }
                }

            }

            lst.SaveToFile(@"C:\Users\User\Desktop\wwww1\plan.csv");
        }

        GrammyExecutor2 ge2;
        private void btn_plan_run_Click(object sender, RoutedEventArgs e) {
            btn_plan_run.IsEnabled = false;
            var plan = GramSLoader.LoadFromFile("plan.csv");
            ge2 = new GrammyExecutor2(plan);
            Graphs.FilePath = ge2.datapath;
            //ge.saveFPath = @"C:\Users\User\Desktop\wwww1";
            //ge.callback = Exc_ExecutDoneNew;
            ge2.Run();
        }

        private void btn_plan_status_Click(object sender, RoutedEventArgs e) {
            btn_plan_status.Content = $"{ge2.done} / {ge2.inqueue}";
        }

        private async void btn_grammyLoadFolder_Click(object sender, RoutedEventArgs e) {
            try {
                btn_grammyLoadFolder.IsEnabled = false;
                using (var dialog = new System.Windows.Forms.FolderBrowserDialog()) {
                    System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                    if (result == System.Windows.Forms.DialogResult.OK) {
                        var sp = dialog.SelectedPath;
                        btn_grammyLoadFolder.Content = "Loading from Folder";
                        var lst = await GramSLoader.LoadGrammyFromFolderAsync(sp);
                        lst.SaveToFile("grammy.csv");
                        btn_grammyLoadFolder.Content = "Creating cluster";
                        var grClust = await GrammyCluster.CreateAsync(lst);

                    }
                }
            } catch (Exception ex) {
                btn_grammyLoadFolder.Content = "Error";
                MessageBox.Show(ex.Message);
            }
            btn_grammyLoadFolder.IsEnabled = true;
            btn_grammyLoadFolder.Content = "Load From Folder";
        }

        private void btn_grammyLoadFile_Click(object sender, RoutedEventArgs e) {
            var sd = new Microsoft.Win32.OpenFileDialog() {
                Filter = "room Files|*.csv",
                FileName = "room"
            };
            if (sd.ShowDialog() == true) {
                var lst = GramSLoader.LoadGrammyFromFile(sd.FileName);
                var grClust = new GrammyCluster(lst);
                var interp = grClust.GrammyInterp(new Microsoft.Research.Oslo.Vector(-30, 40, 310, 0, 7, -50));
            }
        }
    }
}
