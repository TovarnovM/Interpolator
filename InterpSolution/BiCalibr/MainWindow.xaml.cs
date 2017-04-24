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
using MoreLinq;
using EqOptimizer.Data;
using EqOptimizer.Equations;
using EqOptimizer;
using EqOptimizer.Criterias;

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

        private void BttnInitDir_Click(object sender,RoutedEventArgs e) {
            using (var fbd = new FolderBrowserDialog()) {
                DialogResult result = fbd.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath)) {
                    initDir = fbd.SelectedPath;
                    bttnInitDir.Content = initDir;
                }
            }
        }

        private async void lb1_SelectionChanged(object sender,SelectionChangedEventArgs e) {
            if (e.AddedItems.Count == 0)
                return;
            var s = e.AddedItems[0] as string;
            var data = await GetDataFromFileAsync(s);
            LoadToDataGrid(data, s);
            var ss = GetFileInfo(s);
        }

        void LoadToDataGrid(List<(string name, double[] data)> data, string name = "") {
            // dg.ItemsSource 
            int dataCount = data.First().data.Length;
            var dgData = Enumerable
                .Range(0,dataCount)
                .Select(i=> data.ToDictionary(el => el.name,el => el.data[i]))
                .ToList();

            var table = new System.Data.DataTable();
            foreach (var n in dgData[0].Keys) {
                table.Columns.Add(n.Replace(".",""),typeof(string));
            }
            foreach (var i in dgData) {
                var r = table.Rows.Add(i.Values.Select(v => v.ToString()).Cast<object>().ToArray());
                var pr = r.ItemArray;
            }
            dg.DataContext = table.DefaultView;
            if(name != "") {
                var fn = System.IO.Path.GetFileNameWithoutExtension(name);
                tabItemDataGrid.Header = fn;
            }
        }

        Task<List<(string name, double[] data)>> GetDataFromFileAsync(string fp) 
        {
            return Task.Factory.StartNew<List<(string name, double[] data)>>(() => {
                return GetDataFromFile(fp);
            });

        }
        List<(string name, double[] data)> GetDataFromFile(string fp) {
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

        public static double GetDouble(string value,double defaultValue = 0d) {

            //Try parsing in the current culture
            if (!double.TryParse(value,System.Globalization.NumberStyles.Any,CultureInfo.CurrentCulture,out double result) &&
                //Then try in US english
                !double.TryParse(value,System.Globalization.NumberStyles.Any,CultureInfo.GetCultureInfo("en-US"),out result) &&
                //Then in neutral language
                !double.TryParse(value,System.Globalization.NumberStyles.Any,CultureInfo.InvariantCulture,out result)) {
                result = defaultValue;
            }

            return result;
        }

    #region StatisticsStuff

        class StatInfo {
            public int Num { get; set; }
            public double Vel0 { get; set; }
            public double Mass_podd { get; set; }
            public double Mass_porsh { get; set; }
            public double Vel_final { get; set; }
            public double P_max { get; set; }
            public List<(string name, double[] data)> data;

        }

        public (int num, double vel0, double mass_podd, double mass_porsh) GetFileInfo(string fp) {
            var n = System.IO.Path.GetFileNameWithoutExtension(fp)
                .Split(' ')
                .AsEnumerable()
                .Last();
            var answ = n
                .Remove(n.Length - 2)
                .Split('_')
                .Select(s => GetDouble(s))
                .ToArray();
            return ((int)answ[0], answ[1], answ[2], answ[3]);   

        }

        private async void Button_Click_1(object sender,RoutedEventArgs e) {
            btnAnalys.IsEnabled = false;
            var oldContent = btnAnalys.Content;
            btnAnalys.Content = "в процессе....";
            var files = lb1.Items.Cast<string>().ToList();
            var lst = await GetStatListAsync(files);

            dg2.ItemsSource = lst;
            btnAnalys.Content = oldContent;
            btnAnalys.IsEnabled = true;

        }

        List<StatInfo> GetStatList(IEnumerable<string> files) {
            var lst = new List<StatInfo>(files.Count());

            var sameCase = files
                .Select(fp => new{ fileName = fp, info = GetFileInfo(fp)})
                .GroupBy(si => new {si.info.vel0, si.info.mass_podd, si.info.mass_porsh})
                .ToDictionary(ig => ig.Key,ig => ig.ToList());

            
            foreach (var sc in sameCase) {
                var stat = new StatInfo{
                    Vel0 = sc.Key.vel0,
                    Mass_podd = sc.Key.mass_podd,
                    Mass_porsh = sc.Key.mass_porsh,                   
                };

                var mostRelevantData = sc
                    .Value
                    .Select(v => GetDataFromFile(v.fileName))
                    .GroupBy(vt => vt.Count)
                    .ToDictionary(
                        vt => vt.Key, 
                        vt => vt.MaxBy( vvt => vvt.First().data.Length)) ;

                foreach (var rd in mostRelevantData) {
                    var velFinalSeq = rd.Value.Where(v => v.name.ToUpper().Contains("ELEM")).ToList();
                    if(velFinalSeq.Any()) {
                        stat.Vel_final = velFinalSeq.First().data.Last();
                    }

                    var pmaxSeq = rd.Value.Where(v => v.name.ToUpper().Contains("PRESS")).ToList();
                    if(pmaxSeq.Any()) {
                        stat.P_max = pmaxSeq.Select(v => v.data.Max()).Max();
                    }
                    
                }
                lst.Add(stat);
            }

            lst = lst
                .OrderBy(l => l.Vel0)
                .ThenBy(l => l.Mass_podd)
                .ThenBy(l => l.Mass_porsh)
                .ToList();
            int ind = 0;
            foreach (var l in lst) {
                l.Num = ++ind;
            }
            

            return lst;
        }

        Task<List<StatInfo>> GetStatListAsync(IEnumerable<string> files) {
            return Task.Factory.StartNew(() => GetStatList(files));
        }
        #endregion

        private void dg2_SelectionChanged(object sender,SelectionChangedEventArgs e) {
            StatInfo info =(StatInfo)dg2.SelectedItem;

        }

        private async void Button_Click_2(object sender, RoutedEventArgs e) {
            regressButton.IsEnabled = false;

            var lst = dg2.ItemsSource as List<StatInfo>;
            var dataVel = lst
                .Select(si => new OnePoint(si.Vel0, si.Mass_podd, si.Mass_porsh, si.P_max))
                .Aggregate(
                    new MultyData(),
                    (md, op) => {
                        md.Add(op);
                        return md;
                    });
            var eq = new Line_2order_eq(3);
            //eq.FillPars(new double[] { 0, 0, 0, 0, 0, 0, 1, 1, 1, 100 });
            var crit = new MNK();
            var opt = new EqO_genetic(eq, dataVel, crit, -1000000, 1000000);// { minShag = 1e-12};//{ minShag = 1e-11, epsFitn = 1e-11, Multithread = false, lambda = 0.01 };
            
            var sol = await opt.PerformOptimizationAsync();
            
            tb1.Text += $"Genetic: {sol.eq.EqStr}\n Crit = {sol.crit}\n MaxDiff = {new KolomSmirn().GetMaxError(sol.eq, dataVel)}\n MaxDiff0 = {new KolomSmirn().GetMaxError(eq, dataVel)}\n";

            //var opt2 = new EqO_downhill(sol.eq, dataVel, crit, -10, 10);
            //opt2.Multithread = false;
            //sol = await opt2.PerformOptimizationAsync();
            //tb1.Text += $"Hill: {sol.eq.EqStr}\n   Crit = {sol.crit}\n MaxDiff = {new KolomSmirn().GetMaxError(sol.eq, dataVel)}\n";

            regressButton.IsEnabled = true;
        }
    }
}
