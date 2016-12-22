using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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

namespace NikTest {
    public static class dlltest {
        [DllImport("Nik.dll",EntryPoint = "UniBallS",CharSet = CharSet.Auto)]
        public static extern void UniBallS0(ref float Lcone,ref float dout,ref float Lpiston,ref float m1,ref float m2,ref float Vd,ref float pmax);
    }
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
        }

        private void button_Click(object sender,RoutedEventArgs e) {
            float l = 0.5f, d = 0.1f, lp = 0.5f, m1 = 7f, m2 = 7f, Vd = 0f, pmax = 0f;

            dlltest.UniBallS0(ref l,ref d,ref lp,ref m1,ref m2,ref Vd,ref pmax);

            button.Content = pmax.ToString() + "  " + Vd.ToString();
        }
    }
}
