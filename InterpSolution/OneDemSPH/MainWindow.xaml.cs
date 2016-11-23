using Microsoft.Research.Oslo;
using ReactiveODE;
using System;
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
using System.Reactive.Concurrency;

namespace SPH_1D {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private OneDemExample pr;
        private IObservable<SolPoint> sol;
        IEquasionController controller;
        private Microsoft.Research.Oslo.Vector v0;
        public ViewModel vm { get; set; }

        public MainWindow() {
            InitializeComponent();
            vm = new ViewModel();
            DataContext = vm;

            pr = new OneDemExample();//(0.001875+0.0075) * 0.5
            v0 = pr.Rebuild();
            pr.dt = 0.001;

            sol = Ode.RK45(0,v0,pr.f,pr.dt).WithStepRx(0.01, out controller);
            controller.Pause();
            var pr2 = new OneDemExample();
            pr2.Rebuild();
            sol.Subscribe(sp => {
                pr2.SynchMeTo(sp);
                vm.Draw(pr2);
                vm.Model1.Title = $"{sp.T:0.###} s,  RoMax = {pr2.Particles.Max(p => p.Ro):0.###},  Pmax = {pr2.Particles.Max(p => p.P):0.###}";
            });

            
        }

        

        private void button_Click(object sender,RoutedEventArgs e) {
            controller.Paused = !controller.Paused;
            button.Content = $"{controller.Paused}";
            vm.Draw(pr);

        }
    }
}
