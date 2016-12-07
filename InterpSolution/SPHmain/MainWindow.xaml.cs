﻿using Microsoft.Research.Oslo;
using ReactiveODE;
using System;
using Microsoft.Research.Oslo;
using System.Collections.Generic;
using System.Linq;
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
using System.Reactive.Linq;
using Microsoft.Win32;
using System.IO;
using SimpleIntegrator;
using System.Threading;

namespace SPH_2D {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private Sph2D pr;
        private IObservable<SolPoint> sol;
        IEquasionController controller;
        private Microsoft.Research.Oslo.Vector v0;
        public ViewModel vm { get; set; }

        public MainWindow() {
            //hello
            pr = GetTest();
            vm = new ViewModel(pr);
            DataContext = vm;
            InitializeComponent();



            //(0.001875+0.0075) * 0.5
            initObs(pr);
            vm.Model1Rx.Update(new SolPoint(pr.TimeSynch,pr.Rebuild()));

            var trackbarch = Observable.FromEventPattern<RoutedPropertyChangedEventArgs<double>>(slider,"ValueChanged").Select(i=>(int)i.EventArgs.NewValue);
            var rb0 = Observable.FromEventPattern<RoutedEventArgs>(radioButton,"Checked").Select(e => 0);
            var rb1 = Observable.FromEventPattern<RoutedEventArgs>(radioButton_Copy,"Checked").Select(e => 1);
            var grType = rb0.Merge(rb1).StartWith(0);
            

            var rb10 = Observable.FromEventPattern<RoutedEventArgs>(radioButton_Copy1,"Checked").Select(e => 0);
            var rb11 = Observable.FromEventPattern<RoutedEventArgs>(radioButton_Copy2,"Checked").Select(e => 1);
            var rb12 = Observable.FromEventPattern<RoutedEventArgs>(radioButton_Copy3,"Checked").Select(e => 2);
            var rb13 = Observable.FromEventPattern<RoutedEventArgs>(radioButton_Copy4,"Checked").Select(e => 3);
            var wichGraph = rb10.Merge(rb11).Merge(rb12).Merge(rb13).StartWith(0);

            var all = trackbarch.CombineLatest(grType,wichGraph,(i,gT,wG) => new Tuple<int,int,int>(i,gT,wG));

            all.
            Subscribe(t => {
                vm.DrawState = t.Item2;
                vm.WichGraph = t.Item3;

                redrawVm(t.Item1);
            });
        }

        private void initObs(Sph2D calc) {
            pr = calc;
            v0 = pr.Rebuild(pr.TimeSynch);
            var dt = 0.0000001;
            sol = Ode.RK45(pr.TimeSynch,v0,pr.f,dt).WithStepRx(dt * 10,out controller);//.StartWith(new SolPoint(pr.TimeSynch,v0));
            controller.Pause();

            sol.ObserveOnDispatcher().Subscribe(sp => {
                vm.SolPointList.Update(sp);
                slider.Maximum = (double)(vm.SolPointList.Value.Count > 0 ? vm.SolPointList.Value.Count : 0);
            });
        }


        private void button_Click(object sender,RoutedEventArgs e) {
            controller.Paused = !controller.Paused;
            string txt = controller.Paused ? "Paused" : "Playing";
            button.Content = txt;
            
        }

        public static Sph2D GetTest() {
            int N = 300;
            double L = 0.01 ,shag = L/N, xL = 0.5*L;
            double d = 5*L / N, hmax = 1.4 * 2 * d;
            double P1 = 3E4, P2 = 1E4;
            double Ro1 = 1500, Ro2 = 1200;
            double k1 = 3, k2 = 3;

            var particles = new List<IsotropicGasParticle>(N);
            for(int i = 0; i < N; i++) {
                double x = i * shag;
                particles.Add(new IsotropicGasParticle(d,hmax) {
                    X = x,
                    P = x < xL ? P1 : P2,
                    Ro = x < xL ? Ro1 : Ro2,
                    k = x < xL ? k1 : k2,
                    isWall = x < hmax * 2 || x > (L - 2 * hmax)
                });
            }


            for(int i = 0; i <1; i++) {

                particles.ForEach(p => {
                    p.M = p.Ro * Math.Pow(shag,1);
                    

                });

                particles.ForEach(p => {
                   // p.Ro = particles.Sum(n => My_IsotropicGas.W_func(n.GetDistTo(p),p.alpha * (p.D + n.D) * 0.5) * n.M);
                    p.E = p.P / ((p.k - 1d) * p.Ro);
                });
            }
            var masses = particles.Select(p => p.M).ToArray();
            return new Sph2D(particles,null);


            #region Old


            //var dx_granica = 0d;//
            ////0.0075*0.5;


            //int perc = 80;
            //int scaler = 4;
            //int n_pm = perc * scaler;
            //int Np = scaler * 100;

            //int Np_wall = 20 * scaler;

            //double boardL = 0.6;
            //var dx1 = boardL / n_pm;

            //var dx2 = boardL / (Np - n_pm);

            //double d1 = dx1*1.5;
            //double d2 = dx2* 1.5;
            //double hmax = Math.Max(d1,d2) * 2 * 1.4;

            //var Particles = new List<IsotropicGasParticle>();
            //var Wall = new List<IsotropicGasParticle>();

            //for(int i = 0; i < n_pm; i++) {

            //    Particles.Add(new IsotropicGasParticle(d2,hmax) {
            //        Name = i.ToString(),
            //        X = -boardL + i * dx1,
            //        Ro = 1,
            //        P = 1,
            //        E = 2.5,
            //        M = 0.6/n_pm
            //    });
            //}

            //for(int i = n_pm; i < Np; i++) {

            //    Particles.Add(new IsotropicGasParticle(d2,hmax) {
            //        Name = i.ToString(),
            //        X = (i - n_pm) * dx2 + dx_granica,
            //        Ro = 0.25,
            //        P = 0.1795,
            //        E = 1.795,
            //        M = 0.6 / n_pm
            //    });
            //}

            //for(int i = -1; i > -Np_wall - 1; i--) {
            //    Wall.Add(new IsotropicGasParticle(d2,hmax) {
            //        Name = (i).ToString(),
            //        X = -boardL + (i) * dx1,
            //        Ro = 1,
            //        P = 1,
            //        E = 2.5,
            //        isWall = true,
            //        M = 0.6 / n_pm
            //    });
            //}
            //for(int i = Np + 1; i <= Np + Np_wall; i++) {
            //    Wall.Add(new IsotropicGasParticle(d2,hmax) {
            //        Name = (i).ToString(),
            //        X = (i - n_pm - 1) * dx2 + dx_granica,
            //        Ro = 0.25,
            //        P = 0.1795,
            //        E = 1.795,
            //        isWall = true,
            //        M = 0.6 / n_pm
            //    });
            //}

            ////var AllP = Particles.Concat(Wall).ToList();
            ////foreach(var pr in AllP) {
            ////    pr.Ro = AllP.Where(n => n.GetDistTo(pr) < 2.1 * d2).Sum(n => {
            ////        double dist = n.GetDistTo(pr);
            ////        double h = pr.alpha * (pr.D + n.D) * 0.5;
            ////        double w = Particle2DBase.W_func(dist,h);
            ////        return w*n.M;

            ////    });
            ////}

            //return new Sph2D(Particles,Wall);
            #endregion
        }

        private void button_Save_Click(object sender,RoutedEventArgs e) {
            controller.Pause();
            button.Content = "Paused";
            var unit4save = GetTest();
            unit4save.Rebuild();

            int newVal = (int)slider.Value;
            int index = newVal < vm.SolPointList.Value.Count ? newVal : vm.SolPointList.Value.Count - 1;
            if(index < 0)
                return;
            unit4save.SynchMeTo(vm.SolPointList.Value[index]);
            var sd = new SaveFileDialog() {
                Filter = "XML Files|*.xml",
                FileName = "sph2D"
            };
            if(sd.ShowDialog() == true) {
                var sw = new StreamWriter(sd.FileName);
                unit4save.Serialize(sw);
                sw.Close();
            }


        }

        private void button_Copy1_Click(object sender,RoutedEventArgs e) {
            controller.Pause();
            button.Content = "Paused";
            var unit4load = GetTest();
            unit4load.Rebuild();
            var sd = new OpenFileDialog() {
                Filter = "XML Files|*.xml",
                FileName = "sph2D"
            };
            if(sd.ShowDialog() == true) {
                var sr = new StreamReader(sd.FileName);
                unit4load.Deserialize(sr);
                sr.Close();

                controller.Cancel();
                vm.SolPointList.Value.Clear();
                initObs(unit4load);
                vm.Model1Rx.Update(new SolPoint(pr.TimeSynch,pr.Rebuild()));


            }

        }

        void redrawVm(int newVal) {
            int index = newVal < vm.SolPointList.Value.Count ? newVal : vm.SolPointList.Value.Count - 1;
            if(index < 0)
                return;
            vm.Model1Rx.Update(vm.SolPointList.Value[index]);
        }
    }
}
