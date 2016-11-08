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

            pr = new OneDemExample();
            v0 = pr.Rebuild();
            pr.dt = 0.005;
            sol = Ode.RK45(0,v0,pr.f,pr.dt).GetEnumerator();
            vm.Draw(pr);
        }

        private void button_Click(object sender,RoutedEventArgs e) {
            for(int i = 0; i < 1; i++) {
                sol.MoveNext();
            }
            
            vm.Draw(pr);
            button.Content = $"{sol.Current.T:0.###} s,  RoMax = {pr.Particles.Max(p=>p.Ro):0.###},  Pmax = {pr.Particles.Max(p => p.P):0.###}";

        }
    }
}
