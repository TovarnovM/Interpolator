using Microsoft.Research.Oslo;
using Microsoft.Win32;
using ReactiveODE;
using Sharp3D.Math.Core;
using SimpleIntegrator;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Serialization;
using MoreLinq;
using static SimpleIntegrator.DummyIOHelper;
using System.Drawing;
using OxyPlot;
using OxyPlot.Wpf;
using System.Windows.Threading;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using System.Windows.Forms;
using System.Threading;
using System.Collections.Concurrent;
using System.Data;

namespace RobotSim {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private RobotDynamics pr;
        IEquasionController controller;
        private Microsoft.Research.Oslo.Vector v0;
        private Experiments_Wall ex;
        TaskScheduler ts_ui;

        public ViewModel vm { get; set; }
        public int MyProperty { get; set; }
        public TestVM TstVm { get; set; }
        public VM_ExGraph vm_ex { get; set; }
        public VM_results vm_res { get; set; }

        public ObservableCollection<CheckedListItem<GraffLine>> ExList { get; set; }

        public static void CommandsDependsOnCurrPOs(RobotDynamics solution) {
            solution.Body.SynchQandM();
            solution.wheels.ForEach(w => w.SynchQandM());
            solution.BlockedWheels = true;
        }

        public RobotDynamics GetNewRD() {
            
            var sol1 =  ex.GetRD();
            //CommandsDependsOnCurrPOs(sol1);
            return sol1;

            var sol = new RobotDynamics();
            //sol.Body.Mass.Value = 100;
            //sol.SynchMassGeometry();
            //sol.CreateWheelsSample(false);
            sol.Body.Vec3D = new Vector3D(0.3,0.1,0);
            sol.Body.SynchQandM();
            sol.Body.RotateOXtoVec(sol.Body.WorldTransformRot * new Vector3D(10,-1,4));
            //sol.Body.SetPosition_LocalPoint_LocalMoveToIt_LocalFixed(Vector3D.XAxis,-Vector3D.YAxis,-Vector3D.ZAxis,Vector3D.ZAxis);
            //sol.Body.SetPosition_LocalPoint_LocalMoveToIt_LocalFixed(Vector3D.XAxis,Vector3D.XAxis + Vector3D.ZAxis,-Vector3D.YAxis,Vector3D.YAxis);
            sol.Body.SynchQandM();

            sol.Create4GUS(2,5);

            var mostLeftPoint = sol.TracksAll
                .SelectMany(tr => tr.ConnP.Select(cp => tr.WorldTransform * cp))
                .MinBy(p => p.X);

            //sol.AddSurf_magnetic_standart(new FlatSurf(10000,100,new Vector3D(mostLeftPoint.X,0,0), new Vector3D(1,0,0)),0.9);

            sol.AddSurf(new FlatSurf(10000,100,new Vector3D(1,0,1)));
            ////sol.AddSurf_magnetic_standart(new RbSurfFloor(10000,100,new Vector3D(1,0,1)),100);
            //sol.AddSurf_magnetic_standart(new FlatSurf(10000,100,new Vector3D(0,0,0), new Vector3D(1,1,0)),2);
            //sol.AddSurf_magnetic_standart(new FlatSurf(10000,100,new Vector3D(-0.3,0,0),new Vector3D(1,0,0)),2);
            sol.AddGForcesToAll();
            //sol.wheels[1].AddForce(
            //    Force.GetForce(new Vector3D(-5,0,0),null,new Vector3D(0,0,0),sol.wheels[1]));

            CommandsDependsOnCurrPOs(sol);
            return sol;
        }


        public MainWindow() {
            ts_ui = TaskScheduler.Current;
            ex = new Experiments_Wall_Shoot();
            ex.Prs.Angle = 0;

            vm = new ViewModel(GetNewRD);
            vm_ex = new VM_ExGraph();
            vm_res = new VM_results();
            DataContext = this;
            InitializeComponent();

            var sol = GetNewRD();
            
            //sol.Body.AddForce(new Force(0.1,new Position3D(0,1,0),new Position3D(1,1,1),null));
            //sol.Body.AddForce(new Force(0.1,new Position3D(0,-1,0),new Position3D(0,0,0),null));
            //sol.Body.AddForce(new ForceCenter(1,new Position3D(0,-1,0),null));
            initObs(sol);//(0.001875+0.0075) * 0.5


            var trackbarch = Observable.FromEventPattern<RoutedPropertyChangedEventArgs<double>>(slider,"ValueChanged").Select(i => {
                int newVal = (int)i.EventArgs.NewValue;
                int index = newVal < vm.SolPointList.Value.Count ? newVal : vm.SolPointList.Value.Count - 1;
                return index;
            }).Publish();

            trackbarch.Subscribe(i => {
                if(i < 0)
                    return;
                vm.Model1Rx.Update(vm.SolPointList.Value[i]);
                Title = vm.SolPointList.Value[i].T.ToString();
            });
            trackbarch.Connect();


            TstVm = new TestVM();

            
            ExList = new ObservableCollection<CheckedListItem<GraffLine>>();
            lb1.ItemsSource = ExList;
            ExList.Add(new CheckedListItem<GraffLine>(new GraffLine() { Name = "ssad1" }));
            ExList.Add(new CheckedListItem<GraffLine>(new GraffLine() { Name = "ssad12" }));
            ExList.Add(new CheckedListItem<GraffLine>(new GraffLine() { Name = "3" }));          
            ExList.Add(new CheckedListItem<GraffLine>(new GraffLine() { Name = "4" }));

            //lb1.ItemsSource = null;


        }

        private void initObs(RobotDynamics calc) {
            pr = calc;
            v0 = pr.Rebuild(pr.TimeSynch);
            var names = pr.GetDiffPrms().Select(dp => dp.FullName).ToList();
            var dt = 0.00001;

            var sol = Ode.MidPoint(pr.TimeSynch,v0,pr.f,dt).WithStepRx(0.0001,out controller).StartWith(new SolPoint(pr.TimeSynch,v0)).Publish();
            controller.Pause();

            sol.ObserveOnDispatcher().Subscribe(sp => {
                vm.SolPointList.Update(sp);
                slider.Maximum = (double)(vm.SolPointList.Value.Count > 0 ? vm.SolPointList.Value.Count : 0);
                slider2.Maximum = slider.Maximum;
            });
            sol.Connect();
        }



        private void button_Click_1(object sender,RoutedEventArgs e) {
            controller.Paused = !controller.Paused;
            string txt = controller.Paused ? "Paused" : "Playing";
            button.Content = txt;

        }

        private void button_Save_Click_1(object sender,RoutedEventArgs e) {
            controller.Pause();
            button.Content = "Paused";
            var unit4save = GetNewRD();
            unit4save.Rebuild();

            int newVal = (int)slider.Value;
            int index = newVal < vm.SolPointList.Value.Count ? newVal : vm.SolPointList.Value.Count - 1;
            if(index < 0)
                return;
            unit4save.SynchMeTo(vm.SolPointList.Value[index]);
            var sd = new Microsoft.Win32.SaveFileDialog() {
                Filter = "XML Files|*.xml",
                FileName = "sph1D"
            };
            if(sd.ShowDialog() == true) {
                var sw = new StreamWriter(sd.FileName);
                unit4save.Serialize(sw);
                sw.Close();
            }


        }

        private void button_Copy1_Click_1(object sender,RoutedEventArgs e) {
            controller.Pause();
            button.Content = "Paused";
            var unit4load = GetNewRD();
            unit4load.Rebuild();
            var sd = new Microsoft.Win32.OpenFileDialog() {
                Filter = "XML Files|*.xml",
                FileName = "sph1D"
            };
            if(sd.ShowDialog() == true) {
                var sr = new StreamReader(sd.FileName);
                unit4load.Deserialize(sr);
                sr.Close();

                controller.Cancel();
                vm.SolPointList.Value.Clear();
                initObs(unit4load);



            }

        }

        private async void button1_Click(object sender,RoutedEventArgs e) {
            var m = new Majatnik();
            var v0 = m.Rebuild();
            double dt = 0.01, t0 = 0, t1 = 30;
            double T = 2 * 3.14159 * Math.Sqrt(m.L / 9.8);
double omega = 2 * 3.14159 / T;
            double A = m.X;
            double tetta0 = 3.14159 / 2;


            var s = Ode.RK45(0,v0,m.f,0.0001).SolveFromToStep(t0,t1,dt);
            var l = await getSol(s);
            var ts = l.Select(ee => ee.T).ToList();

            
            var rightAnsw = ts.Select(t => A * Math.Sin(tetta0 + omega * t)).ToList();

            var answrs = l.Select(sp => {
                m.SynchMeTo(sp);
                return m.X;
            })
            .ToList();

            TstVm.Draw(ts,rightAnsw,answrs);


        }

         Task<List<SolPoint>> getSol(IEnumerable<SolPoint> s) {
            return Task.Factory.StartNew<List<SolPoint>>(() => {
                var answ = s.ToList();
                return answ;
            });
         }

        private void button_Copy_Click(object sender,RoutedEventArgs e) {
            RobotDynamics prT = GetNewRD();
            v0 = prT.Rebuild(prT.TimeSynch);
            var names = prT.GetDiffPrms().Select(dp => dp.FullName).ToList();
            var dt = 0.0001;


            var s = Ode.RK45(prT.TimeSynch,v0,prT.f,dt).SolveFromTo(0,0.01);
            foreach(var ss in s) {
                int f = 11;

            }

        }

        private void button_Save_Copy_Click(object sender,RoutedEventArgs e) {
            controller.Pause();
            button.Content = "Paused";
            var unit4save = GetNewRD();
            unit4save.Rebuild();
            
            var sd = new Microsoft.Win32.SaveFileDialog() {
                Filter = "XML Files|*.xml",
                FileName = "manySP"
            };
            if(sd.ShowDialog() == true) {
                var sw = new StreamWriter(sd.FileName);
                SerializeManySP(sw,vm.SolPointList.Value);
                sw.Close();
            }
        }

        private void button_Save_Copy1_Click(object sender,RoutedEventArgs e) {
            controller.Pause();
            button.Content = "Paused";
            var unit4load = GetNewRD();
            unit4load.Rebuild();
            var sd = new Microsoft.Win32.OpenFileDialog() {
                Filter = "XML Files|*.xml",
                FileName = "sph1D"
            };
            if(sd.ShowDialog() == true) {
                var sr = new StreamReader(sd.FileName);
                controller.Cancel();
                vm.SolPointList.Value.Clear();
                DeserializeManySP(sr,vm.SolPointList.Value);
                sr.Close();

                unit4load.SynchMeTo(vm.SolPointList.Value.Last());


                CommandsDependsOnCurrPOs(unit4load);
                initObs(unit4load);



            }
        }

        void saveGif(string fp) {
            GifBitmapEncoder gEnc = new GifBitmapEncoder();
            int fi = (int)slider2.Value;
            int ti = (int)slider.Value;
            foreach (var bmpImage in GetFrames(fi, ti)) {
                //var bmp = bmpImage.GetHbitmap();
                //var src = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                //    bmp,
                //    IntPtr.Zero,
                //    Int32Rect.Empty,
                //    BitmapSizeOptions.FromEmptyOptions());
                gEnc.Frames.Add(BitmapFrame.Create(bmpImage));
                //DeleteObject(bmp); // recommended, handle memory leak
            }
            using (FileStream fs = new FileStream(fp, FileMode.Create)) {
                gEnc.Save(fs);
            }
        }

        IEnumerable<BitmapSource> GetFrames(int fromInd, int toInd) {
            for (int i = fromInd; i <= toInd; i++) {
                slider.Value = i;
               // DoEvents();
                var pngExporter = new PngExporter();
                yield return pngExporter.ExportToBitmap(vm.ModelXY);
                
            }
        }
        public static void DoEvents() {
            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Background,
                                                  new Action(delegate { }));
        }

        private void button_Save_CGif_Click(object sender, RoutedEventArgs e) {
            try {
                controller.Pause();
                button.Content = "Paused";
                button_Save_CGif.IsEnabled = false;

                var sd = new Microsoft.Win32.SaveFileDialog() {
                    Filter = "GIF Files|*.gif",
                    FileName = "XY"
                };
                if (sd.ShowDialog() == true) {
                    saveGif(sd.FileName);
                }
            } finally {
                button_Save_CGif.IsEnabled = true;
            }

        }

        private async void button_Save_CGif_Copy_Click(object sender, RoutedEventArgs e) {
            try {
                button_Save_CGif_Copy.IsEnabled = false;
                ex = new Experiments_Wall_Shoot();
                //ex.Start();
                await ex.StartAsync();
                System.Windows.MessageBox.Show("good");
            } finally {
                button_Save_CGif_Copy.IsEnabled = true;
            }

        }

        private void button1_Click_1(object sender, RoutedEventArgs e) {
            var sd = new Microsoft.Win32.OpenFileDialog() {
                Filter = "Res Files|*.txt"
            };
            if (sd.ShowDialog() == true) {
                ex.LoadResultsFromFile(sd.FileName);
                vm_ex.Rebuild(ex);
                ExList.Clear();
                foreach (var item in vm_ex.graphs) {
                    ExList.Add(item);
                }
                vm_ex.Pm.InvalidatePlot(true);
                button1.Content = sd.FileName;
            }
                
         


        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            var sd = new Microsoft.Win32.SaveFileDialog() {
                Filter = "Джейсон Files|*.json",
                FileName = "Exper_variants"
            };
            if (sd.ShowDialog() == true) {
                var pAdam = new Experiments_Wall_params() {
                    Name = "Ex_res",
                    pawAngleSpeed = 3,
                    Mz = 0
                };
                int mom_count = 7;
                double mom0 = 0.2, mom1 = 1.6, mom_shag = (mom1 - mom0) / mom_count;
                int angle_count = 5;
                double angle0 = 0, angle1 = 60, angle_shag = (angle1 - angle0) / angle_count;
                var lst = new List<Experiments_Wall_params>();
                int id = 1000;
                for (int i = 0; i < mom_count+1; i++) {
                    for (int j = 0; j < angle_count+1; j++) {
                        var p = pAdam.GetCopy();
                        p.WheelMoment = mom0 + mom_shag * i;
                        p.Angle = angle0 + angle_shag * j;
                        p.id = id++;
                        p.Name = $"{p.Name}_{p.id}_mom{p.WheelMoment:0.###}_angle{p.Angle:0.###}";
                        lst.Add(p);
                    }
                }
                using (var f = new StreamWriter(sd.FileName)) {
                    f.WriteLine(JsonConvert.SerializeObject(lst));
                    f.Close();
                }
            }
        }
        List<Experiments_WallShoot_params> ExperimentParamList = new List<Experiments_WallShoot_params>(), exParsGo = new List<Experiments_WallShoot_params>();
        string resDirPath = "";
        int coresCount = 4;
        private void Button_Click_2(object sender, RoutedEventArgs e) {
            var sd = new Microsoft.Win32.OpenFileDialog() {
                Filter = "Джейсон Files|*.json",
                FileName = "Exper_variants"
            };
            if (sd.ShowDialog() == true) {
                using (var f = new StreamReader(sd.FileName)) {
                    ExperimentParamList = JsonConvert.DeserializeObject<List<Experiments_WallShoot_params>>(f.ReadToEnd());
                    dg_ex.ItemsSource = ExperimentParamList;
                    resDirPath = System.IO.Path.GetDirectoryName(sd.FileName);
                    btn_resDir.Content = resDirPath;
                    btn_resDir.ToolTip = "ResDir : " +  btn_resDir.Content;
                }
                    
            }
        }

        private void btn_resDir_Click(object sender, RoutedEventArgs e) {
            using (var fbd = new FolderBrowserDialog()) {
                DialogResult result = fbd.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath)) {
                    btn_resDir.Content = fbd.SelectedPath;
                    btn_resDir.ToolTip = "ResDir : " + btn_resDir.Content;
                }
            }
        }

        private void Button_Click_4(object sender, RoutedEventArgs e) {
            if (dg_ex.SelectedItems.Count == 0) {
                return;
            }
            var itms = dg_ex.SelectedItems.Cast<Experiments_WallShoot_params>().ToList();
            exParsGo.AddRange(itms);
            ExperimentParamList.RemoveAll(itm => itms.Contains(itm));
            RefreshDG();
        }

        private void Button_Click_3(object sender, RoutedEventArgs e) {
            if (dg_ex_go.SelectedItems.Count == 0) {
                return;
            }
            var itms = dg_ex_go.SelectedItems.Cast<Experiments_WallShoot_params>().ToList();
            ExperimentParamList.AddRange(itms);
            exParsGo.RemoveAll(itm => itms.Contains(itm));
            RefreshDG();
        }

        
        private async void Button_Click_5(object sender, RoutedEventArgs e) {
            try {
                btn_GO.IsEnabled = false;
                coresCount = int.Parse(tb_cores.Text);
                await StartExCalcAsync();
            } finally {
                btn_GO.IsEnabled = true;
            }

        }

        void RefreshDG() {
            dg_ex.ItemsSource = null;
            dg_ex.ItemsSource = ExperimentParamList;
            dg_ex_go.ItemsSource = null;
            dg_ex_go.ItemsSource = exParsGo;
        }

        Task StartExCalcAsync() {
            return Task.Factory.StartNew(StartExCalc);
        }

        void StartExCalc() {
            var po = new ParallelOptions() { MaxDegreeOfParallelism = coresCount };
            Parallel.ForEach(exParsGo, po, StartVar);
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e) {
            if(ExList != null) {

                foreach (var item in ExList) {
                    item.IsChecked = true;
                }
            }

        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e) {
            if (ExList != null) {

                foreach (var item in ExList) {
                    item.IsChecked = false;
                }
            }
        }

        void StartVar(Experiments_WallShoot_params prm) {
            var ex = new Experiments_Wall_Shoot();
            ex.Prs = prm;
            prm.ResultIndex = "Calculating";
            ////RefreshDG();
            //var tsk = Task.Factory.StartNew(RefreshDG, CancellationToken.None, TaskCreationOptions.None, ts_ui);
            //tsk.Wait();
            ex.Start(resDirPath + "\\" + prm.Name + ".txt", resDirPath + "\\" + prm.Name + "_soloints.xml");
        }

        #region Обработка
        ConcurrentStack<(Dictionary<string, double> obrabDict, Experiments_Wall_params prs)> ccStack = new ConcurrentStack<(Dictionary<string, double> obrabDict, Experiments_Wall_params prs)>();
        void Save_ccStackToCSV(string fileName) {
            var cc_lst = ccStack.ToList();
            var zags = cc_lst.First()
                .prs.GetDict().Keys
                .Concat(cc_lst.First().obrabDict.Keys)
                .ToList();
            using(var sw = new StreamWriter(fileName)) {
                var sb = new StringBuilder();
                foreach (var zag in zags) {
                    sb.Append(zag + ";");
                }
                sb.Remove(sb.Length - 1, 1);
                sw.WriteLine(sb.ToString());
                foreach (var tup in cc_lst) {
                    sb.Clear();
                    var prsDict = tup.prs.GetDict();
                    foreach (var kv in tup.obrabDict) {
                        prsDict.Add(kv.Key, $"{kv.Value:0.####}");
                    }
                    foreach (var zag in zags) {
                        sb.Append(prsDict[zag] + ";");
                    }
                    sb.Remove(sb.Length - 1, 1);
                    sw.WriteLine(sb.ToString());
                }
                sw.Close();
            }
        }
        double _t_b, _t_f;
        void LoadExperList(string dir) {
            DirectoryInfo d = new DirectoryInfo(dir);//Assuming Test is your Folder
            FileInfo[] Files = d.GetFiles("*.txt"); //Getting Text files
            int progr_curr = 0, progr_max = Files.Length;
            object _locker = new object();
            ccStack.Clear();
            Parallel.For(0, Files.Length, i => {
               // for (int i = 0; i < Files.Length; i++) {

                
                var ex = new Experiments_Wall();
                ex.LoadResultsFromFile(Files[i].FullName);
                var dict = ex.GetObrabotkaSmoth(_t_b, _t_f);
                ccStack.Push((dict, ex.Prs));
           
                Dispatcher.Invoke(new Action(()=> {
                lock (_locker) {
                    progr_curr++;
                    BtnDir.Content = $"Reading {progr_curr} / {progr_max}";
                }
                }));
            }
                );
        }
        Task LoadExperListAsync(string dir) {
            return Task.Factory.StartNew(() => LoadExperList(dir));
        }
        

        private void BtnDir2_Click(object sender, RoutedEventArgs e) {
            var sd = new Microsoft.Win32.OpenFileDialog() {
                Filter = "файлы движа Files|*.xml",
                FileName = "солпоинтс"
            };
            if (sd.ShowDialog() == true) {
                using (var f = new StreamReader(sd.FileName)) {
                    ex = new Experiments_Wall();
                    ex.LoadSolPoints(sd.FileName);
                    ex.FillDictFromSP();
                    vm_ex.Rebuild(ex);
                    ExList.Clear();
                    foreach (var item in vm_ex.graphs) {
                        ExList.Add(item);
                    }
                    vm_ex.Pm.InvalidatePlot(true);
                    button1.Content = sd.FileName;
                }

            }
        }

        private void Button_Click_6(object sender, RoutedEventArgs e) {

            var sd = new Microsoft.Win32.SaveFileDialog() {
                Filter = "Джейсон Files|*.json",
                FileName = "Exper_variants2"
            };
            if (sd.ShowDialog() == true) {
                var pAdam = new Experiments_Wall_params() {
                    Name = "Ex_res",
                    pawAngleSpeed = 2,
                    Mz = 0
                };
                int power_count = 7;
                double pow0 = 2, pow1 = 7, pow1_shag = (pow1 - pow0) / power_count;
                int mass_count = 7;
                double mass0 = 2, mass1 = 4, mass_shag = (mass1 - mass0) / mass_count;
                var lst = new List<Experiments_Wall_params>();
                int id = 2000;
                for (int i = 0; i < power_count + 1; i++) {
                    for (int j = 0; j < mass_count + 1; j++) {
                        var p = pAdam.GetCopy();
                        p.OmegaMax = pow0 + pow1_shag * i;
                        p.Mass = mass0 + mass_shag * j;
                        p.id = id++;
                        p.Name = $"{p.Name}_{p.id}_pow{p.OmegaMax:0.###}_mass{p.Mass:0.###}";
                        lst.Add(p);
                    }
                }
                using (var f = new StreamWriter(sd.FileName)) {
                    f.WriteLine(JsonConvert.SerializeObject(lst));
                    f.Close();
                }
            }
            
        }
        public Dictionary<string,string> ObrabDict { get; set; }
        private void BtnOpenObr_Click(object sender, RoutedEventArgs e) {
            try {
                var sd = new Microsoft.Win32.OpenFileDialog() {
                    Filter = "csv Files|*.csv",
                    FileName = "AllSmoothResults"
                };
                if (sd.ShowDialog() == true) {

                    dg_obr.ItemsSource = ReadCSV(sd.FileName).DefaultView;
                    
                }
            } finally {

            }
        }

        private DataTable ReadCSV(string FileName) {

            DataTable csvDataTable = new DataTable();
            try {
                //no try/catch - add these in yourselfs or let exception happen
                String[] csvData = File.ReadAllLines(FileName);

                //if no data
                if (csvData.Length == 0) {
                    return csvDataTable;
                }

                String[] headings = csvData[0].Split(';').Select(ss => ss.Trim()).ToArray();

                //for each heading
                for (int i = 0; i < headings.Length; i++) {
                    ////replace spaces with underscores for column names
                    headings[i] = headings[i].Replace("/", " ");

                    //add a column for each heading
                    csvDataTable.Columns.Add(headings[i]);

                }

                //populate the DataTable
                for (int i = 1; i < csvData.Length; i++) {
                    //create new rows
                    DataRow row = csvDataTable.NewRow();
                    var dt = csvData[i].Split(';').Select(ss => ss.Trim()).ToArray();
                    for (int j = 0; j < headings.Length; j++) {
                        //fill them
                        row[j] = dt[j];

                    }

                    //add rows to over DataTable

                    csvDataTable.Rows.Add(row);

                }
            } catch (Exception ex) {
                System.Windows.Forms.MessageBox.Show(ex.Message.ToString());
            }
            //return the CSV DataTable

            return csvDataTable;


        }

        private void Button_Click_7(object sender, RoutedEventArgs e) {
            var sd = new Microsoft.Win32.SaveFileDialog() {
                Filter = "Джейсон Files|*.json",
                FileName = "Exper_variants3"
            };
            if (sd.ShowDialog() == true) {
                var pAdam = new Experiments_WallShoot_params() {
                    Name = "Ex_res",
                    pawAngleSpeed = 0,
                    Mz = 0
                };
                int tetta_count = 4;
                double tetta0 = 0, tetta1 = 180, tetta1_shag = (tetta1 - tetta0) / tetta_count;
                int alpha_count = 2;
                double alpha0 = 0, alpha1 = 90, alpha_shag = (alpha1 - alpha0) / alpha_count;
                int t_imp_count = 4;
                double t_imp0 = 0.00025, t_imp1 = 0.00045, t_imp_shag = (t_imp1 - t_imp0) / t_imp_count;
                int imp_count = 4;
                double imp0 = 1.8, imp1 = 3.8, imp_shag = (imp1 - imp0) / imp_count;
                var lst = new List<Experiments_WallShoot_params>();
                int id = 3000;
                for (int i = 0; i < tetta_count + 1; i++) {
                    for (int j = 0; j < alpha_count + 1; j++) {
                        for (int k = 0; k < t_imp_count+1; k++) {
                            for (int kk = 0; kk < imp_count+1; kk++) {
                                var p = pAdam.GetCopy1();
                                p.Tetta = tetta0 + tetta1_shag * i;
                                p.Alpha = alpha0 + alpha_shag * j;
                                p.ImpulseT = t_imp0 + t_imp_shag * k;
                                p.Impulse = imp0 + imp_shag * kk;
                                p.id = id++;
                                p.Name = $"{p.Name}_{p.id}_teta{p.Tetta:0.###}_alpha{p.Alpha:0.###}_impT{p.ImpulseT:0.#####}_imp{p.Impulse}";
                                lst.Add(p);
                            }
                        }
 
                    }
                }
                using (var f = new StreamWriter(sd.FileName)) {
                    f.WriteLine(JsonConvert.SerializeObject(lst));
                    f.Close();
                }
            }
        }

        private void Button_Click_8(object sender, RoutedEventArgs e) {
            var sd = new Microsoft.Win32.OpenFileDialog() {
                Filter = "Джейсон Files|*.json",
                FileName = "Exper_variants"
            };
            if (sd.ShowDialog() == true) {
                using (var f = new StreamReader(sd.FileName)) {
                    var lst = JsonConvert.DeserializeObject<List<Experiments_WallShoot_params>>(f.ReadToEnd());
                    dg_ex.ItemsSource = lst;
                    resDirPath = System.IO.Path.GetDirectoryName(sd.FileName);
                    btn_resDir.Content = resDirPath;
                    btn_resDir.ToolTip = "ResDir : " + btn_resDir.Content;
                }

            }
        }

        private void Btn_smooth1_Click(object sender, RoutedEventArgs e) {
            var sm = vm_ex.RemoveSmooth();
            foreach (var it in sm) {
                ExList.Remove(it);
            }
            var smDict = ex.GetSmooth(
                Experiments_Wall.GetDouble(tb_b.Text, 0.07),
                Experiments_Wall.GetDouble(tb_f.Text, 0.07));
            vm_ex.AddSmoothDict(smDict);
            var nwGraphs = vm_ex.graphs.Except(ExList).ToList();
            foreach (var item in nwGraphs) {
                ExList.Add(item);
            }
            vm_ex.Pm.InvalidatePlot(true);
        }

        private async void Dir_Click(object sender, RoutedEventArgs e) {
            try {
                var sd = new Microsoft.Win32.SaveFileDialog() {
                    Filter = "csv Files|*.csv",
                    FileName = "AllSmoothResults"
                };
                if (sd.ShowDialog() == true) {
                    BtnDir.IsEnabled = false;
                    string dir = System.IO.Path.GetDirectoryName(sd.FileName)+"\\";
                    _t_b = Experiments_Wall.GetDouble(tb_b.Text, 0.07);
                    _t_f = Experiments_Wall.GetDouble(tb_f.Text, 0.07);
                    await LoadExperListAsync(dir);
                    BtnDir.Content = "Saving...";
                    Save_ccStackToCSV(sd.FileName);
                }
            } finally {
                BtnDir.IsEnabled = true;
                BtnDir.Content = "LoadAll+обработать";
            }

            
        }
        #endregion




    }
}
