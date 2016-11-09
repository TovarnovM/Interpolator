using Microsoft.Research.Oslo;
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

namespace OneDemSPH {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private OneDemExample pr;
        private IEnumerator<SolPoint> sol;
        private Microsoft.Research.Oslo.Vector v0;
        public ViewModel vm { get; set; }

        public MainWindow() {
            InitializeComponent();
            vm = new ViewModel();
            DataContext = vm;

            pr = new OneDemExample();//(0.001875+0.0075) * 0.5
            v0 = pr.Rebuild();
            pr.dt = 0.001;

            sol = Ode.RK45(0,v0,pr.f,pr.dt).WithStep(0.01).GetEnumerator();
            vm.Draw(pr);
        }

        private void button_Click(object sender,RoutedEventArgs e) {
            sol.MoveNext();
            var x = sol.Current.X;
            pr.SynchMeTo(sol.Current.T,ref x);
            vm.Draw(pr);
            button.Content = $"{sol.Current.T:0.###} s,  RoMax = {pr.Particles.Max(p=>p.Ro):0.###},  Pmax = {pr.Particles.Max(p => p.P):0.###}";

        }
    }
}
