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
using System.IO;
using System.Windows.Forms;
using System.Globalization;

namespace BiCalibr {
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
            bttnInitDir.Content = initDir;
        }

        string initDir = @"E:\расчетики\23 мм\2316_research";

        private void Button_Click(object sender,RoutedEventArgs e) {
            var lst = FileSearcher.Search(initDir, ".uhs");
            lb1.Items.Clear();
            foreach (var f in lst) {
                lb1.Items.Add(f);
            }
        }

        private void bttnInitDir_Click(object sender,RoutedEventArgs e) {
            using (var fbd = new FolderBrowserDialog()) {
                DialogResult result = fbd.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath)) {
                    initDir = fbd.SelectedPath;
                    bttnInitDir.Content = initDir;
                }
            }
        }

        private void lb1_SelectionChanged(object sender,SelectionChangedEventArgs e) {
            var s = e.AddedItems[0] as string;
            var data = GetDataFromFile(s);
        }

        List<(string name, double[] data)> GetDataFromFile(string fp) 
        {
            var allLines = File.ReadAllLines(fp);
            int dataCount = allLines.Length - 3;
            int namesCount = allLines
                .Take(1)
                .SelectMany(
                    s => s.Split(',').Skip(1))
                .Count();

            var names = Enumerable
                .Range(0,namesCount)
                .Select(i => "")
                .ToList();
            var data = Enumerable
                .Range(0,namesCount)
                .Select(i => new double[dataCount])
                .ToList();
            foreach (var l in allLines.Take(3).Skip(1)) {
                var elems = l.Split(',').Skip(1);
                int i = 0;
                foreach (var e in elems) {
                    names[i++] += e;
                }
            }

            names = names.Select(n => n.Trim()).ToList();

            foreach (var l in allLines.Skip(3)) {
                var elems = l.Split(',');
                int i = 0;
                int j = int.Parse(elems[0]);
                foreach (var e in elems.Skip(1)) {
                    data[i++][j] = GetDouble(e,0d);
                }
            }

            return names.Zip(data,(n,d) => (n, d)).ToList();
        }
        public static double GetDouble(string value,double defaultValue) {
            double result;

            //Try parsing in the current culture
            if (!double.TryParse(value,System.Globalization.NumberStyles.Any,CultureInfo.CurrentCulture,out result) &&
                //Then try in US english
                !double.TryParse(value,System.Globalization.NumberStyles.Any,CultureInfo.GetCultureInfo("en-US"),out result) &&
                //Then in neutral language
                !double.TryParse(value,System.Globalization.NumberStyles.Any,CultureInfo.InvariantCulture,out result)) {
                result = defaultValue;
            }

            return result;
        }
        public (int num, double vel0, double mass_podd, double mass_porsh) GetFileInfo(string fp) {
            throw new NotImplementedException();
        }
    }
}
