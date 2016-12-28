using DoubleEnumGenetic;
using OxyPlot.Series;
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
using GeneticSharp.Domain;
using GeneticSharp.Domain.Randomizations;
using OxyPlot;

namespace DoubleEnumGeneticWPF {
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private List<ChromosomeD> cs;
        private List<ChromosomeD> cs_tmp;
        private IList<ChromosomeD> par;
        private ViewModel vm;

        public MainWindow() {
            vm = new ViewModel();
            DataContext = vm;
            InitializeComponent();
        }

        private void button_Click(object sender,RoutedEventArgs e) {
            var pm = vm.pm;
            var tf = new TestFit11();
            cs = Enumerable
                .Range(0,300)
                .Select(_ => tf.GetNewChromosome())
                .Where(c => {
                    var dx = 1500 - c["xg"];
                    var dy = 1500 - c["yg"];

                    return Math.Sqrt(dx * dx + dy * dy) < 1300;
                })
                .ToList();
            cs.ForEach(c => {
                c["x"] = c["xg"];
                c["y"] = c["yg"];
            });
            //var doubles = Enumerable.Range(0,cs.Count*2).Select(_ => RandomizationProvider.Current.GetDouble(0,15)).ToArray();
            //for(int j = 0; j < cs.Count; j++) {
            //    cs[j]["x"] = doubles[j * 2];
            //    cs[j]["y"] = doubles[j * 2+1];
            //}

            //for(int j = 0; j < cs.Count; j++) {
            //    if((cs[j]["x"] != doubles[j * 2]) ||
            //    (cs[j]["y"] != doubles[j * 2 + 1])) {
            //        int ggg = 99;
            //    }
            //}

            cs_tmp = new List<ChromosomeD>();
            cs_tmp.AddRange(cs);
            pm.Series.Clear();

            var ss = new ScatterSeries() {
                Title = "All",
                MarkerSize = 7
            };
            pm.Series.Add(ss);
            foreach(var c in cs_tmp) {
                ss.Points.Add(new ScatterPoint(c["x"],c["y"]));
            }


            //int rank = 0;
            //while(rank < 1) {
            //    var sss = new ScatterSeries() {
            //        Title = (rank++).ToString(),
            //        MarkerSize = 5
            //    };
            //    pm.Series.Add(sss);
            //    int nAll = cs_tmp.Count;
            //    par = ChromosomeD.Pareto(cs_tmp,true);
            //    var nPar = par.Count;
            //    foreach(var c in par) {
            //        sss.Points.Add(new ScatterPoint(c["x"],c["y"]));
            //    }
            //    var m = ChromosomeD.GetCritDifferenceMatrix(par);
            //    var remAll = (nPar + cs_tmp.Count) == nAll;
            //}
            //pm.InvalidatePlot(true);




            var unic = cs_tmp;
            var diffM = ChromosomeD.GetCritDifferenceMatrix(unic);
            var inds = ChromosomeD.GetUniquestGuysIndexes(diffM,unic.Count/3);
            ss = new ScatterSeries() {
                Title = "Uniquest",
                MarkerSize = 4,
                MarkerFill = OxyColors.Red
            };
            pm.Series.Add(ss);
            for(int ind = 0; ind < inds.Count; ind++) {
                int i = inds[ind];
                var c = unic[i];
                ss.Points.Add(new ScatterPoint(c["x"],c["y"]));
            }
                
            


            pm.InvalidatePlot(true);


            
        }

        MarkovNameGenerator nameGener = MarkovNameGenerator.GetStandart();
        private void button1_Click(object sender,RoutedEventArgs e) {
            button1.Content = nameGener.GetNextName();
        }
    }

    public class TestFit11 {
        public IList<IGeneDE> GInfo { get; set; }
        public IList<CritInfo> CrInfo { get; set; }

        public IEnumerable<string> GetAllNames() {
            return GInfo.Select(gi => gi.Name).Concat(CrInfo.Select(ci => ci.Name));
        }

        public TestFit11() {
            //var tst = new GeneDoubleRange("Lcone",0.3,0.7);
            GInfo = new List<IGeneDE>(5);
            //GInfo.Add(new GeneDoubleRange("Lcone",0.3,0.7));
            //GInfo.Add(new GeneDoubleRange("dout",0.07,0.125));
            //GInfo.Add(new GeneDoubleRange("Lpiston",0.3,0.7));
            GInfo.Add(new GeneDoubleRange("xg",1,3000));
            GInfo.Add(new GeneDoubleRange("yg",1,3000));

            CrInfo = new List<CritInfo>(3);
            CrInfo.Add(new CritInfo("x",CritExtremum.minimize));
            CrInfo.Add(new CritInfo("y",CritExtremum.minimize));
            CrInfo.Add(new CritInfo("DontMatter",CritExtremum.minimize) { matters = false});
        }

        public ChromosomeD GetNewChromosome() {
            return new ChromosomeD(GInfo,CrInfo);
        }
    }

}

