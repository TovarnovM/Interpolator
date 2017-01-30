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

namespace RobotSim {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private RobotDynamics pr;
        IEquasionController controller;
        private Microsoft.Research.Oslo.Vector v0;

        public ViewModel vm { get; set; }
        TestVM tstVm { get; set; }


        public static RobotDynamics GetNewRD() {
            var sol = new RobotDynamics();
            sol.Body.Vec3D = new Vector3D(20,100,0);
            sol.floor = new RbSurfFloor(100,100,new Vector3D(1,0,1));

            //var moment = Force.GetMoment(0.05,new Vector3D(0,1,0));
            //sol.Body.AddMoment(moment);

            sol.Body.RotateOXtoVec(new Vector3D(1,1,1));

            var f1 = Force.GetForce(
                new Vector3D(-0.1,0,0),null,
                new Vector3D(0.05,0.025,0.1),sol.Body);

            var f2 = Force.GetForce(
                new Vector3D(0.1,0,0),null,
                new Vector3D(0,0,0),sol.Body);

            sol.Body.AddForce(f1);
            sol.Body.AddForce(f2);
            return sol;
        }

        public MainWindow() {
            vm = new ViewModel();
            DataContext = vm;
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


            tstVm = new TestVM();
            tstPV.DataContext = tstVm;
        }

        private void initObs(RobotDynamics calc) {
            pr = calc;
            v0 = pr.Rebuild(pr.TimeSynch);
            var names = pr.GetDiffPrms().Select(dp => dp.FullName).ToList();
            var dt = 0.00001;


            //var s = Ode.RK45(pr.TimeSynch,v0,pr.f,dt).SolveFromToStep(0,1,0.01);
            //foreach(var ss in s) {
            //    int f = 11;
               
            //}



            var sol = Ode.RK45(pr.TimeSynch,v0,pr.f,dt).WithStepRx(0.01,out controller).StartWith(new SolPoint(pr.TimeSynch,v0)).Publish();
            controller.Pause();

            sol.ObserveOnDispatcher().Subscribe(sp => {
                vm.SolPointList.Update(sp);
                slider.Maximum = (double)(vm.SolPointList.Value.Count > 0 ? vm.SolPointList.Value.Count : 0);
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
            var unit4save = new RobotDynamics();
            unit4save.Rebuild();

            int newVal = (int)slider.Value;
            int index = newVal < vm.SolPointList.Value.Count ? newVal : vm.SolPointList.Value.Count - 1;
            if(index < 0)
                return;
            unit4save.SynchMeTo(vm.SolPointList.Value[index]);
            var sd = new SaveFileDialog() {
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
            var unit4load = new RobotDynamics();
            unit4load.Rebuild();
            var sd = new OpenFileDialog() {
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
            double dt = 0.1, t0 = 0, t1 = 10;
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

            tstVm.Draw(ts,rightAnsw,answrs);


        }

         Task<List<SolPoint>> getSol(IEnumerable<SolPoint> s) {
            return Task.Factory.StartNew<List<SolPoint>>(() => {
                var answ = s.ToList();
                return answ;
            });
         }
    }
}
