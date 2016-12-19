using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reactive.Linq;
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

namespace MassDrummer {
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        List<CheckBox> chBxes = new List<CheckBox>(20);
        List<IObservable<string>> chBxesStreams = new List<IObservable<string>>(20);
        IObservable<string> chBxesAllStreams;
        void FillListChBx() {
            chBxes.Add(chbxRo);
            chBxes.Add(chbxM);

            chBxes.Add(chbx1D);
            chBxes.Add(chbx1H);

            chBxes.Add(chbx2D1);
            chBxes.Add(chbx2D2);

            chBxes.Add(chbx3D1);
            chBxes.Add(chbx3D2);
            chBxes.Add(chbx3H);

            chBxes.Add(chbx4D);
            chBxes.Add(chbx4H1);
            chBxes.Add(chbx4H2);

            chBxes.Add(chbx5H);
            chBxes.Add(chbx5h);
            chBxes.Add(chbx5R);

            for(int i = 0; i < chBxes.Count; i++) {
                chBxesStreams.Add(
                    Observable.FromEventPattern<RoutedEventArgs>(chBxes[i],"Checked")
                    .Select(e => {
                        return (e.Sender as CheckBox).Name;
                    }));
            }
            chBxesAllStreams = Observable.Merge(chBxesStreams).StartWith("chbxM");
            chBxesAllStreams.Subscribe(uncheckall);
           // chBxesAllStreams.TakeLast()
        }
        ViewModel vm;

        Dictionary<string,TbTuple> tb_dict = new Dictionary<string,TbTuple>(20);
        List<IObservable<Tuple<string,double>>> tb_valsStreams = new List<IObservable<Tuple<string,double>>>(20);
        IObservable<Tuple<string,double>> tbx_AllStreams;
        void fillTxtBxes() {
            foreach(TextBox tb in GetLogicalChildCollection<TextBox>(this)) {
                var t = new TbTuple();
                t.tb = tb;
                t.pName = tb.Name.Skip(2).ToString();

                tb_dict.Add(tb.Name,t);
                tb_valsStreams.Add(
                    Observable.FromEventPattern<TextChangedEventArgs>(tb,"TextChanged")
                    .Select(e => {
                        var value = (e.Sender as TextBox).Text;

                        double result = StrToDouble(value);
                        var s = (e.Sender as TextBox).Name;
                        s = s.Substring(2,s.Length - 2);
                        return new Tuple<string,double>(s,result);
                    })
                    .Where(tsd => !Double.IsNaN(tsd.Item2) )
                    .Throttle(TimeSpan.FromMilliseconds(200))
                    .DistinctUntilChanged(tss => tss.Item2)
                    );
            }
            tbx_AllStreams = Observable.Merge(tb_valsStreams);



        }

        double StrToDouble(string value) {
            double result;

            //Try parsing in the current culture
            if(!double.TryParse(value,System.Globalization.NumberStyles.Any,CultureInfo.CurrentCulture,out result) &&
                //Then try in US english
                !double.TryParse(value,System.Globalization.NumberStyles.Any,CultureInfo.GetCultureInfo("en-US"),out result) &&
                //Then in neutral language
                !double.TryParse(value,System.Globalization.NumberStyles.Any,CultureInfo.InvariantCulture,out result)) {
                result = Double.NaN;
            }
            return result;
        }

        void uncheckall(string exeptIt) {
            foreach(var cb in chBxes.Where(c => c.Name != exeptIt)) {
                cb.IsChecked = false;
            }
        }

        public MainWindow() {
            vm = new ViewModel();
            DataContext = vm;
            InitializeComponent();
           
            
        }

        void recheckWhenTabCh(int tbi) {
            if(chbxRo.IsChecked == true || chbxM.IsChecked == true)
                return;
            switch(tbi) {
                case (1): {
                    chbx1D.IsChecked = true;
                    break;
                }
                case (2): {
                    chbx2D1.IsChecked = true;
                    break;
                }
                case (3): {
                    chbx3H.IsChecked = true;
                    break;
                }
                case (4): {
                    chbx4D.IsChecked = true;
                    break;
                }
                case (5): {
                    chbx5H.IsChecked = true;
                    break;
                }
                default:
                break;
            }

        }

        private void tabControl_SelectionChanged(object sender,SelectionChangedEventArgs e) {

        }

        private void chbxM_Checked(object sender,RoutedEventArgs e) {

        }

        public static List<T> GetLogicalChildCollection<T>(object parent) where T : DependencyObject {
            List<T> logicalCollection = new List<T>();
            GetLogicalChildCollection(parent as DependencyObject,logicalCollection);
            return logicalCollection;
        }

        private static void GetLogicalChildCollection<T>(DependencyObject parent,List<T> logicalCollection) where T : DependencyObject {
            IEnumerable children = LogicalTreeHelper.GetChildren(parent);
            foreach(object child in children) {
                if(child is DependencyObject) {
                    DependencyObject depChild = child as DependencyObject;
                    if(child is T) {
                        logicalCollection.Add(child as T);
                    }
                    GetLogicalChildCollection(depChild,logicalCollection);
                }
            }
        }

        private void textBox_Copy_TextChanged(object sender,TextChangedEventArgs e) {

        }

        private void Window_Loaded(object sender,RoutedEventArgs e) {
            fillTxtBxes();
            CreateShapes();          
            FillListChBx();
            ShapesToTextBoxes();
            var tbStream = Observable.FromEventPattern<SelectionChangedEventArgs>(tabControl,"SelectionChanged")
                .Where(ee => ee.Sender is TabControl)
                .Select(_ => {
                    if(ti1 != null && ti1.IsSelected)
                        return 1;
                    if(ti2 != null && ti2.IsSelected)
                        return 2;
                    if(ti3 != null && ti3.IsSelected)
                        return 3;
                    if(ti4 != null && ti4.IsSelected)
                        return 4;
                    if(ti5 != null && ti5.IsSelected)
                        return 5;
                    return 0;
                })
                .StartWith(1);
            tbStream.Subscribe(recheckWhenTabCh);

            //var changes = chBxesAllStreams
            //    .Select(cbn => cbn.Substring(4, cbn.Length-4))
            //    .CombineLatest(tbx_AllStreams,
            //        (findName,t) => {
            //            return new Tuple<string,string,double>(findName,t.Item1,t.Item2);
            //        })
            //    .Where(fpv => fpv.Item1 != fpv.Item2);

            //var draw = changes
            //    .CombineLatest(tbStream,(t,shpe) => {
            //        return new Tuple<ShapeBase,string,string,double>(shpe,t.Item1,t.Item2,t.Item3);
            //    });

            //draw.SubscribeOnDispatcher().Subscribe(t => {
            //    t.Item1[t.Item3] = t.Item4;
            //    var d = t.Item1.FindParam(t.Item2);
            //    vm.Draw(t.Item1,t.Item2,d);
            //   // tb_dict["tb" + t.Item2].tb.Text = t.Item1[t.Item2].ToString();
            //});
        }

        private void button_Click(object sender,RoutedEventArgs e) {
            
            int ind = 0;
            if(ti1.IsSelected)
                ind = 0;
            else if(ti2.IsSelected)
                ind = 1;
            else if(ti3.IsSelected)
                ind = 2;
            else if(ti4.IsSelected)
                ind = 3;
            else if(ti5.IsSelected)
                ind = 4;
            ShapeBase sh = shapes[ind];
            TextToShape(sh);
            var cb = chBxes.First(c => c.IsChecked == true).Name;
            var find = cb.Substring(4,cb.Length - 4);

            var answ = sh.FindParam(find);
            sh[find] = answ;
            vm.Draw(sh,find,answ);

            ShapeToTextBox(sh);
        }

        #region Datas
        class TbTuple {
            public TextBox tb;
            public string pName;
        }
        

        List<ShapeBase> shapes = new List<ShapeBase>(10);
        void ShapesToTextBoxes() {
            foreach(var s in shapes) {
                ShapeToTextBox(s);
            }
            ShapeToTextBox(shapes[0]);
        }

        void ShapeToTextBox(ShapeBase s) {
            foreach(var par in s.Params.Keys) {
                tb_dict["tb" + par].tb.Text = s[par].ToString();
            }
        }

        void TextToShape(ShapeBase s) {
            var keys = s.Params.Keys.ToList();
            foreach(var par in keys) {
                s[par] = StrToDouble(tb_dict["tb" + par].tb.Text);
            }
        }

        void CreateShapes() {
            var ro = 2.7;
            var c = new Cylynder("1");
            c.Ro = ro;
            c.H = 3;
            c.D = 3;
            c.M = c.FindParam("M");
            shapes.Add(c);

            var e = new Ellipse("2");
            e.D1 = 4;
            e.D2 = 3;
            e.Ro = ro;
            e.M = e.FindParam("M");
            shapes.Add(e);

            var eo = new Ellipse_Obrez("3");
            eo.D1 = 4;
            eo.D2 = 3;
            eo.H = 3;
            eo.Ro = ro;
            eo.M = eo.FindParam("M");
            shapes.Add(eo);

            var cc = new ConeCone("4");
            cc.Ro = ro;
            cc.H1 = 3;
            cc.H2 = 1;
            cc.D = 3;
            cc.M = cc.FindParam("M");
            shapes.Add(cc);

            var ss = new SphereShell("5");
            ss.Ro = ro;
            ss.h = 0.1;
            ss.R = 3;
            ss.H = 2;
            ss.M = ss.FindParam("M");
            shapes.Add(ss);
        }

        #endregion

        private void image_Loaded(object sender,RoutedEventArgs e) {
            // ... Create a new BitmapImage.
            BitmapImage b = new BitmapImage();
            b.BeginInit();
            b.UriSource = new Uri(AppDomain.CurrentDomain.BaseDirectory + "\\bin\\Debug\\pic.jpg");
            b.EndInit();

            // ... Get Image reference from sender.
            var image = sender as Image;
            // ... Assign Source.
            image.Source = b;
        }
    }
}
