using Microsoft.Research.Oslo;
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
            v0 = pr.Rebuild();
            var dt = 0.0000001;

            sol = Ode.RK45(0,v0,pr.f,dt).WithStepRx(dt * 10,out controller);
            controller.Pause();
            sol.Subscribe(sp => {
                vm.Model1Rx.Update(sp);
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
                    p.Ro = particles.Sum(n => IsotropicGasParticle.W_func(n.GetDistTo(p),p.alpha * (p.D + n.D) * 0.5) * n.M);
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


    }
}
