using Microsoft.Research.Oslo;
using OxyPlot;
using OxyPlot.Series;
using Sharp3D.Math.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MeetingPro {
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public ViewMod Vm { get; set; }
        public ViewMod Vm3 { get; set; }
        public VM_grammy Vm_gr { get; set; }
        public VM_traect Vm_traect { get; set; }
        public MainWindow() {
            //Graphs.FilePath = @"C:\Users\User\Documents\data.xml";
            Vm = new ViewMod();
            Vm3 = new ViewMod();
            Vm_traect = new VM_traect();
            Vm_gr = new VM_grammy();
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
            
            var tetta0 = 0 * Mis.RAD;
            var VecOX = new Vector3D(Math.Cos(tetta0), Math.Sin(tetta0), 0);
            mis.RotateOxThenNearOy(VecOX, Vector3D.YAxis);
            mis.Vel.Vec3D = VecOX;

            mis.delta_i_rad[0] = 15d * 3.14/180d;
            mis.delta_i_rad[1] = 5d * 3.14 / 180d;
            mis.delta_i_rad[2] = -mis.delta_i_rad[0];
            mis.delta_i_rad[3] = -mis.delta_i_rad[1];
            mis.delta_eler = 1 * 3.14 / 180;
           mis.Y = 1000;
            mis.SynchQandM();

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
                    var pts = lst.AddCoord().Select(tp => new { X = tp.pos.X, Y = tp.pos.Y, B = tp.ow.Vec1.Betta, A = tp.ow.Vec1.Alpha, B0 = tp.ow.Vec0.Betta, A0 = tp.ow.Vec0.Alpha  }).ToList();

                    Vm.Pm.Series.Clear();
                    Vm.Pm.Series.Add(new ScatterSeries() {
                        DataFieldX = "X",
                        DataFieldY = "Y",
                        DataFieldValue = "Val",
                        ItemsSource = pts,
                        LabelFormatString = "A:{A:0.0} B:{B:0.0}"//"{Val:0.0}"//"{Del1:0.0} : {Del2:0.0}"

                    });

                    var pts2 = lst.AddCoord().Uniquest().Select(tp => new { X = tp.pos.X, Y = tp.pos.Y, B = tp.ow.Vec1.Betta, A = tp.ow.Vec1.Alpha, B0 = tp.ow.Vec0.Betta, A0 = tp.ow.Vec0.Alpha }).ToList();
                    Vm.Pm.Series.Add(new ScatterSeries() {
                        DataFieldX = "X",
                        DataFieldY = "Y",
                        DataFieldValue = "Val",
                        ItemsSource = pts2,
                        LabelFormatString = "A:{A:0.0} B:{B:0.0} A0:{A0:0.0} B0:{B0:0.0}",
                        LabelMargin = -16

                    });
                    Vm.Pm.InvalidatePlot(true);
                }


            } finally {

            }

            int f = 44;
        }



        private void btn_plan_Click(object sender, RoutedEventArgs e) {
            var sd = new Microsoft.Win32.SaveFileDialog() {
                Filter = "plan Files|*.pln",
                FileName = "plan"
            };
            if (sd.ShowDialog() != true) {
                return;
            }


            int thn = 11;
            double th0 = -85, th1 = -th0, dth = (th1 - th0) / (thn - 1);

            int alphn = 7;
            double alph0 = -9, alph1 = -alph0, dalph = (alph1 - alph0) / (alphn - 1);

            int betn = 7;
            double bet0 = -9, bet1 = -bet0, dbet = (bet1 - bet0) / (betn - 1);

            int vn = 4;
            double v0 = 50, v1 = 125, dv = (v1 - v0) / (vn - 1);

            int tempn = 5;
            double temp0 = -50, temp1 = 50, dtemp = (temp1 - temp0) / (tempn - 1);

            int tn = 5;
            double t0 = 5, t1 = 45, dt = (t1 - t0) / (tn - 1);

            int n = thn * alphn * betn * vn * tempn * tn;

            var lst = new List<OneWay>(n);
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

            lst.SaveToFile(sd.FileName);
        }

        GrammyExecutor2 ge2;
        private void btn_plan_run_Click(object sender, RoutedEventArgs e) {
            btn_plan_run.IsEnabled = false;
            var sd = new Microsoft.Win32.OpenFileDialog() {
                Filter = "plan Files|*.pln",
                FileName = "plan"
            };
            if (sd.ShowDialog() == true) {
                var plan = GramSLoader.LoadFromFile(sd.FileName);
                ge2 = new GrammyExecutor2(plan);
                using (var dialog = new System.Windows.Forms.FolderBrowserDialog()) {
                    System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                    if (result == System.Windows.Forms.DialogResult.OK) {
                        var sp = dialog.SelectedPath;
                        ge2.saveFPath = sp+ "\\";

                    } else {
                        btn_plan_run.IsEnabled = true;
                        return;
                    }
                } 
                
               
            } else {
                btn_plan_run.IsEnabled = true;
                return;
            }
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
                        var sd = new Microsoft.Win32.SaveFileDialog() {
                            Filter = "GrammyCluster Files|*.grm",
                            FileName = "grammy"
                        };
                        if (sd.ShowDialog() != true) {
                            return;
                        }

                        var sp = dialog.SelectedPath;
                        btn_grammyLoadFolder.Content = "Loading from Folder";
                        var lst = await GramSLoader.LoadGrammyFromFolderAsync(sp);
                        lst.SaveToFile(sd.FileName);
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
                
            }
        }


        GrammyCluster grammyCluster_1_23 = null;
        private void Button_Click_3(object sender, RoutedEventArgs e) {
            try {
                btn_load_MVP.IsEnabled = false;
                Mouse.OverrideCursor = Cursors.Wait;
                var sd = new Microsoft.Win32.OpenFileDialog() {
                    Filter = "GrammyCluster Files|*.grm",
                    FileName = "grammy"
                };
                if (sd.ShowDialog() == true) {
                    var lst = GramSLoader.LoadGrammyFromFile(sd.FileName);
                    //var baad = lst.Where(gr => gr.Thetta > 5)
                    //    .Where(gr => {
                    //        var vecs = new Microsoft.Research.Oslo.Vector[] { gr.vDown, gr.vLeft, gr.vRight, gr.vUp };
                    //        var tehhas = vecs.Select(v => v[5]).Min();
                    //        return tehhas < -3;
                    //    })
                    //    .ToList();
                    grammyCluster_1_23 = new GrammyCluster(lst);
                }

            } catch {
                MessageBox.Show("Ошибка при загрузке файла");
            } finally {
                btn_load_MVP.IsEnabled = true;
                Mouse.OverrideCursor = Cursors.Arrow;
            }

        }

        private void Button_Click_5(object sender, RoutedEventArgs e) {
            var temps = new double[] { -50, -40, -30, -20, -10, 0, 10, 20, 30, 40, 50 };
            //var lst = temps
            //    .Select(temperat => {
            //        var tpl = InitConditions.GetOneSol(temperat);
            //        return new {
            //            Vel = tpl.vel,
            //            T = temperat,
            //            X = tpl.x,
            //            time = tpl.t

            //        };
            //    })
            //    .ToList();

            //Vm.Pm.Series.Clear();
            //Vm.Pm.Series.Add(new ScatterSeries() {
            //    ItemsSource = lst,
            //    DataFieldX = "T",
            //    DataFieldY = "Vel"
            //});


            var sers = temps
                .Select(temp => {
                    var mis = new Mis();
                    mis.Temperature = temp;
                    var interp = mis.gr.r_md.actT.Data;
                    //var interp = mis.gr.m.Data.Values[2].Data;
                    var ser = new LineSeries() {
                        Title = $"{temp}"
                    };

                    foreach (var kv in interp) {
                        ser.Points.Add(new DataPoint(kv.Key, kv.Value.Value));
                    }

                    return ser;

                }).ToList();

            Vm.Pm.Series.Clear();
            sers.ForEach(s => Vm.Pm.Series.Add(s));
            Vm.Pm.InvalidatePlot(true);
        }


        Dictionary<string,List<(MT_pos pos, Grammy gr)>> bunch_dict;

        private void Button_Click_6(object sender, RoutedEventArgs e) {
            if(grammyCluster_1_23 == null) {
                MessageBox.Show("Сначала загрузите файл МВП");
                return;
            }

            var x0 = GramSLoader.GetDouble(tb_x.Text, 0);
            tb_x.Text = x0.ToString();
            var y0 = GramSLoader.GetDouble(tb_y.Text, 300);
            tb_y.Text = y0.ToString();
            var z0 = GramSLoader.GetDouble(tb_z.Text, 0);
            tb_z.Text = z0.ToString();

            var x0_trg = GramSLoader.GetDouble(tb_x_trg.Text, 0);
            tb_x_trg.Text = x0_trg.ToString();
            var y0_trg = GramSLoader.GetDouble(tb_y_trg.Text, 300);
            tb_y_trg.Text = y0_trg.ToString();
            var z0_trg = GramSLoader.GetDouble(tb_z_trg.Text, 0);
            tb_z_trg.Text = z0_trg.ToString();

            var x0_vel = GramSLoader.GetDouble(tb_x_vel.Text, 0);
            tb_x_vel.Text = x0_vel.ToString();
            var y0_vel = GramSLoader.GetDouble(tb_y_vel.Text, 300);
            tb_y_vel.Text = y0_vel.ToString();
            var z0_vel = GramSLoader.GetDouble(tb_z_vel.Text, 0);
            tb_z_vel.Text = z0_vel.ToString();

            var temperature = GramSLoader.GetDouble(tb_temper.Text, 0);
            tb_temper.Text = temperature.ToString();

            var p_trg = new Vector3D(x0_trg, y0_trg, z0_trg);
            var pos0 = new Vector3D(x0, y0, z0);
            var v0_dir = new Vector3D(x0_vel, y0_vel, z0_vel);
            bunch_dict = grammyCluster_1_23.getSuperDict(pos0, v0_dir, p_trg, temperature);
            DrawBunch(bunch_dict);

            var fastest = bunch_dict["наибыстрейшая"];

            Vm3.Pm.Series.Clear();
            Vm3.Pm.Series.Add(new LineSeries() {
                ItemsSource = fastest
                    .Select(tp => new { time = tp.gr.T, val = tp.gr.Alpha })
                    .ToList(),
                DataFieldX = "time",
                DataFieldY = "val",
                Title = "Alpha"
            });
            Vm3.Pm.Series.Add(new LineSeries() {
                ItemsSource = fastest
                    .Select(tp => new { time = tp.gr.T, val = tp.gr.Betta })
                    .ToList(),
                DataFieldX = "time",
                DataFieldY = "val",
                Title = "Betta",
                MarkerType = MarkerType.Cross
            });
            Vm3.Pm.Series.Add(new ScatterSeries() {
                ItemsSource = fastest
                    .Select(tp => new { time = tp.gr.T, val = tp.gr.Thetta })
                    .ToList(),
                DataFieldX = "time",
                DataFieldY = "val",
                Title = "Thetta"
            });
            Vm3.Pm.Series.Add(new LineSeries() {
                ItemsSource = fastest
                    .Select(tp => new { time = tp.gr.T, val = tp.gr.V })
                    .ToList(),
                DataFieldX = "time",
                DataFieldY = "val",
                Title = "Vel",
                MarkerType = MarkerType.Cross
            });
            Vm3.Pm.InvalidatePlot(true);


            //sl.Minimum = 0;
            //sl.Maximum = fastest.Count-1;
        }

        void DrawBunch(Dictionary<string, List<(MT_pos pos, Grammy gr)>> dct) {
            Vm_traect.ClearSerries();
            foreach (var tv in dct) {
                Vm_traect.DrawOneTraectory(tv.Value.Select(tr => tr.pos.GetPos0()).ToList(), tv.Key);
            }
            Vm_traect.ModelXY.ResetAllAxes();
            
        }
        //void DrawGraphs()


        private void sl_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            if (bunch_dict == null) {
                return;
            }
            int ind = (int)e.NewValue;
            var gr = bunch_dict.First().Value[ind].gr;
            Vm_gr.DrawGrammy(gr, "Thetta", -5,+5);
        }

        private void Button_Click_2(object sender, RoutedEventArgs e) {
            if (grammyCluster_1_23 == null) {
                MessageBox.Show("Сначала загрузите файл МВП");
                return;
            }
            var sd = new Microsoft.Win32.SaveFileDialog() {
                Filter = "cpp Files|*.cpp",
                FileName = "Funcs"
            };
            if (sd.ShowDialog() == true) {
                grammyCluster_1_23.CreateCPPFuncFile(sd.FileName);
            }

            
        }

        private void Button_Click_4(object sender, RoutedEventArgs e) {
            if (grammyCluster_1_23 == null) {
                MessageBox.Show("Сначала загрузите файл МВП");
                return;
            }
            var sd = new Microsoft.Win32.SaveFileDialog() {
                Filter = "csv Files|*.csv",
                FileName = "mdata"
            };
            if (sd.ShowDialog() == true) {
                grammyCluster_1_23.SaveDataToCSV(sd.FileName);
            }
            
        }


    }
}
