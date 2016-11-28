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
using System.ComponentModel;
using System.Diagnostics;

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
            vm = new ViewModel();
            DataContext = vm;
            InitializeComponent();
            

            pr = new OneDemExample();//(0.001875+0.0075) * 0.5
            v0 = pr.Rebuild();
            pr.dt = 0.001;

            sol = Ode.RK45(0,v0,pr.f,pr.dt).WithStepRx(0.01, out controller);
            controller.Pause();

            var trackbarch = Observable.FromEventPattern<RoutedPropertyChangedEventArgs<double>>(slider,"ValueChanged");



            sol.ObserveOnDispatcher().Subscribe(sp => {
                vm.SolPointList.Update(sp);
                slider.Maximum = (double)(vm.SolPointList.Value.Count > 0 ? vm.SolPointList.Value.Count : 0);
            });

            trackbarch.Subscribe(i => {
                int newVal = (int)i.EventArgs.NewValue;
                int index = newVal < vm.SolPointList.Value.Count ? newVal : vm.SolPointList.Value.Count - 1;
                if(index < 0)
                    return;
                vm.Model1Rx.Update(vm.SolPointList.Value[index]);
            });


        }

        

        private void button_Click(object sender,RoutedEventArgs e) {
            controller.Paused = !controller.Paused;
            string txt = controller.Paused ? "Paused" : "Playing";
            button.Content = txt;

        }

    }
}
