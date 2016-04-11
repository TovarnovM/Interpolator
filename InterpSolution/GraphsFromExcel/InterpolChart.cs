using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Interpolator;
using Charting = System.Windows.Forms.DataVisualization.Charting;
using System.Xml.Serialization;
using System.IO;

namespace GraphsFromExcel
{
    public partial class InterpolChart : UserControl
    {
        public List<InterpXY> lstInterp;
        public SaveFileDialog _sd;
        public OpenFileDialog _od;
        public InterpolChart()
        {
            InitializeComponent();
            lstInterp = new List<InterpXY>();
            chart1.Series.Clear();
            _sd = new SaveFileDialog()
            {
                Filter = "XML Files|*.xml",
                FileName = "NewInterp2d"
            };
            _od = new OpenFileDialog()
            {
                Filter = "XML Files|*.xml",
            };

        }

        public void AddInterp(InterpXY interp)
        {
            lstInterp.Add(interp);
            var tmpSeries = new Charting.Series(interp.Title);
            tmpSeries.ChartType = Charting.SeriesChartType.Line;
            tmpSeries.BorderWidth = 3;
            chart1.Series.Add(tmpSeries);
            foreach (var item in interp.Data)
            {
                tmpSeries.Points.AddXY(item.Key, item.Value.Value);
            }
            int w =checkedListBox1.Items.Add(interp, true);
            if (checkedListBox1.SelectedIndex == -1)
                checkedListBox1.SelectedIndex = w;
        }

        private void btnCollapse_Click(object sender, EventArgs e)
        {
            splitContainer1.Panel2Collapsed = !splitContainer1.Panel2Collapsed;
            if (splitContainer1.Panel2Collapsed)
                btnCollapse.Text = "<";
            else
                btnCollapse.Text = ">";
        }

        private int Count { get; set; } = 1;
        private Random rnd = new Random();
        private void btnTestAdd_Click(object sender, EventArgs e)
        {
            int n = 10;
            double min = -7;
            double max = 10;
            var ts = new double[n];
            var vals = new double[n];
            for (int i = 0; i < n; i++)
            {
                ts[i] = rnd.NextDouble() * (max - min) + min; ;
                vals[i] = rnd.NextDouble() * (max - min) + min;
            }
            AddInterp(new InterpXY(ts, vals)
                     { Title = $"{Count++}"});
        }

        private void checkedListBox1_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            chart1.Series[e.Index].Enabled = e.NewValue == CheckState.Checked;
        }

        private void checkedListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateSelectedItem();
        }

        public void UpdateSelectedItem()
        {
            if (checkedListBox1.SelectedIndex < 0)
                return;
            txtBx_Titile.Text = lstInterp[checkedListBox1.SelectedIndex].Title;
            domainUpDown1.SelectedIndex = chart1.Series[checkedListBox1.SelectedIndex].BorderWidth - 1;
            checkBox1.Checked = chart1.Series[checkedListBox1.SelectedIndex].IsValueShownAsLabel;

            radioBtnLeftZero.Checked  = lstInterp[checkedListBox1.SelectedIndex].ET_left == ExtrapolType.etZero;
            radioBtnLeftValue.Checked = lstInterp[checkedListBox1.SelectedIndex].ET_left == ExtrapolType.etValue;
            
            radioBtnInterpStep.Checked = lstInterp[checkedListBox1.SelectedIndex].InterpType == InterpolType.itStep;
            radioBtnInterpLine.Checked = lstInterp[checkedListBox1.SelectedIndex].InterpType == InterpolType.itLine;
            
            radioBtnRightZero.Checked = lstInterp[checkedListBox1.SelectedIndex].ET_right == ExtrapolType.etZero;
            radioBtnRightValue.Checked= lstInterp[checkedListBox1.SelectedIndex].ET_right == ExtrapolType.etValue;
        }

        private void txtBx_Titile_TextChanged(object sender, EventArgs e)
        {
            if (checkedListBox1.SelectedIndex < 0)
                return;
            lstInterp[checkedListBox1.SelectedIndex].Title = txtBx_Titile.Text;
            checkedListBox1.Refresh();
            chart1.Series[checkedListBox1.SelectedIndex].Name = txtBx_Titile.Text;
        }

        private void domainUpDown1_Click(object sender, EventArgs e)
        {
            if (checkedListBox1.SelectedIndex < 0)
                return;
            chart1.Series[checkedListBox1.SelectedIndex].BorderWidth = domainUpDown1.SelectedIndex+1;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkedListBox1.SelectedIndex < 0)
                return;
            chart1.Series[checkedListBox1.SelectedIndex].IsValueShownAsLabel = checkBox1.Checked;
        }

        private void radioBtnInterpStep_CheckedChanged(object sender, EventArgs e)
        {
            if (checkedListBox1.SelectedIndex < 0)
                return;
            if (radioBtnLeftZero.Checked)
            {
                lstInterp[checkedListBox1.SelectedIndex].ET_left = ExtrapolType.etZero;
            }
            if (radioBtnLeftValue.Checked)
            {
                lstInterp[checkedListBox1.SelectedIndex].ET_left = ExtrapolType.etValue;
            }

            if (radioBtnInterpStep.Checked)
            {
                lstInterp[checkedListBox1.SelectedIndex].InterpType = InterpolType.itStep;
                chart1.Series[checkedListBox1.SelectedIndex].ChartType = Charting.SeriesChartType.StepLine;
            }
            if (radioBtnInterpLine.Checked)
            {
                lstInterp[checkedListBox1.SelectedIndex].InterpType = InterpolType.itLine;
                chart1.Series[checkedListBox1.SelectedIndex].ChartType = Charting.SeriesChartType.Line;
            }

            if (radioBtnRightZero.Checked)
            {
                lstInterp[checkedListBox1.SelectedIndex].ET_right = ExtrapolType.etZero;
            }
            if (radioBtnRightValue.Checked)
            {
                lstInterp[checkedListBox1.SelectedIndex].ET_right = ExtrapolType.etValue;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (checkedListBox1.SelectedIndex < 0)
                return;
            if (checkedListBox1.Items.Count > 0 && checkedListBox1.SelectedIndex >=0)
            {
                lstInterp.RemoveAt(checkedListBox1.SelectedIndex);
                chart1.Series.RemoveAt(checkedListBox1.SelectedIndex);
                checkedListBox1.Items.RemoveAt(checkedListBox1.SelectedIndex);
                if (checkedListBox1.Items.Count > 0)
                    checkedListBox1.SelectedIndex = 0;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (checkedListBox1.SelectedIndex < 0)
                return;
            if (checkedListBox1.SelectedIndices.Count == 0)
            {
                MessageBox.Show("Не выделено ни одного интерполятора(");
                return;
            }
            if (_sd.ShowDialog() == DialogResult.OK)
            {
                var tmpInterp2D = new Interp2D();
                if (radioButton6.Checked)
                    tmpInterp2D.ET_left = ExtrapolType.etZero;
                if (radioButton5.Checked)
                    tmpInterp2D.ET_left = ExtrapolType.etValue;
                if (radioButton4.Checked)
                    tmpInterp2D.InterpType = InterpolType.itStep;
                if (radioButton3.Checked)
                    tmpInterp2D.InterpType = InterpolType.itLine;
                if (radioButton2.Checked)
                    tmpInterp2D.ET_right = ExtrapolType.etZero;
                if (radioButton1.Checked)
                   tmpInterp2D.ET_right = ExtrapolType.etValue;
                foreach (var item in checkedListBox1.CheckedItems)
                {
                    tmpInterp2D.AddElement(Convert.ToDouble((item as InterpXY).Title),
                                           (item as InterpXY));
                }
                tmpInterp2D.SaveToXmlFile(_sd.FileName);
                //XmlSerializer serial = new XmlSerializer(typeof(Interp2D));
                //var sw = new StreamWriter(sd.FileName);
                //serial.Serialize(sw, tmpInterp2D);
                //sw.Close();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (checkedListBox1.SelectedIndex == -1)
            {
                MessageBox.Show("Не выделено ни одного интерполятора(");
                return;
            }
            if (_sd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    lstInterp[checkedListBox1.SelectedIndex].SaveToXmlFile(_sd.FileName);
                    //var tmpInterpXY = new InterpXY();
                    //tmpInterpXY = (InterpXY)tmpInterpXY.LoadFromXmlFile(sd.FileName);
                    //AddInterp(tmpInterpXY);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (_od.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var tmpInterpXY = InterpXY.LoadFromXmlFile(_od.FileName);
                   // tmpInterpXY = (InterpXY)tmpInterpXY.LoadFromXmlFile();
                    AddInterp(tmpInterpXY);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (_od.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var tmpInterp2D = Interp2D.LoadFromXmlFile(_od.FileName);
                    foreach (var item in tmpInterp2D.Data)
                    {
                        item.Value.Title = item.Key.ToString();
                        AddInterp(item.Value);
                    }
                    
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            var od = new OpenFileDialog()
            {
                Filter = "CSV Files|*.csv",
            };
            if (od.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var lines = File.ReadAllLines(od.FileName).Select(a => a.Split(';'));
                    var matr = new double[lines.Count(), lines.First().Count()];
                    int i = 0;
                    foreach (var items in lines)
                    {
                        int j = 0;
                        foreach (var item in items)
                        {
                            //MessageBox.Show($"i={i}, j={j++}, val={Convert.ToDouble(item.Replace('.',',').Trim())}");
                            matr[i,j++] = Convert.ToDouble(item.Replace('.',',').Trim());
                        }
                        i++;
                    }
                    var interp2D = new Interp2D();
                    interp2D.ImportDataFromMatrix(matr);
                    foreach (var item in interp2D.Data)
                    {
                        item.Value.Title = item.Key.ToString();
                        AddInterp(item.Value);
                    }

                }
                catch(Exception ex)
                {
                    throw ex;
                }
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            try
            {
                var od = new OpenFileDialog()
                {
                    Filter = "CSV Files|*.csv",
                    Multiselect = true
                };
                if (od.ShowDialog() == DialogResult.OK)
                {
                    foreach (var fileName in od.FileNames)
                    {
                        var lines = File.ReadAllLines(fileName).Select(a => a.Split(';'));
                        var matr = new double[lines.Count(), lines.First().Count()];
                        int i = 0;
                        foreach (var items in lines)
                        {
                            int j = 0;
                            foreach (var item in items)
                            {
                                //MessageBox.Show($"i={i}, j={j++}, val={Convert.ToDouble(item.Replace('.',',').Trim())}");
                                matr[i, j++] = Convert.ToDouble(item.Replace('.', ',').Trim());
                            }
                            i++;
                        }
                        var interp2D = new Interp2D();
                        interp2D.ImportDataFromMatrix(matr);
                        foreach (var item in interp2D.Data)
                        {
                            item.Value.Title = item.Key.ToString();
                        }
                        interp2D.SaveToXmlFile(Path.ChangeExtension(fileName,".xml"));
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            var interpLL4P = new PotentGraff4P();

            var dir = @"C:\Users\MISHA\Desktop\3_17";
            interpLL4P.Title = "_3_17";
            Dictionary<double, string> makeFile = new Dictionary<double, string>()
            {
                [0.0] = dir + "\\" + interpLL4P.Title + "_3D_эта0.xml",
                [1.0] = dir + "\\" + interpLL4P.Title + "_3D_эта1.xml",
                [0.5] = dir + "\\" + interpLL4P.Title + "_3D_эта05.xml"
            };
            foreach (var item in makeFile)
            {
                interpLL4P.AddElement(item.Key, PotentGraff3P.LoadFromXmlFile(item.Value));
            }
            string str = dir + "\\" + interpLL4P.Title + "_4P.xml";
            interpLL4P.SaveToXmlFile(str);
        }

        private void button9_Click(object sender, EventArgs e)
        {
            try
            {
                var tmpInterp= Interp2D.LoadFromXmlFile(@"G:\OneDrive\графики\3_35_M2_old.xml");
                foreach (var itemXY in tmpInterp.Data.Values)
                {

                
                        foreach (var item in itemXY.Data.Values)
                        {
                            item.Value = 0.4 + item.Value*2.4/2.828;
                        }
                }
                tmpInterp.SaveToXmlFile(@"G:\OneDrive\графики\3_35_M2.xml");

            }
            catch (Exception ex)
            {
                throw ex;
            }
            
        }

        private void button10_Click(object sender, EventArgs e)
        {
            try
            {
                var od = new OpenFileDialog()
                {
                    Filter = "CSV Files|*.csv",
                    Multiselect = true
                };
                if (od.ShowDialog() == DialogResult.OK)
                {
                    int k = 0;
                    var oneGraff = new PotentGraff2P();
                    chart1.Series.Clear();
                    foreach (var fileName in od.FileNames)
                    {
                        var lines = File.ReadAllLines(fileName).
                                    Select(a => a.Split(new []{ ';' }, StringSplitOptions.RemoveEmptyEntries) ).
                                    Where(a => a.Count() > 0);
                        var ddd = lines.ToArray();
                        var matr = new double[lines.Count(), lines.First().Count()];
                        int i = 0;
                        
                        foreach (var items in lines)
                        {
                            int j = 0;
                            foreach (var item in items)
                            {
                                //MessageBox.Show($"i={i}, j={j++}, val={Convert.ToDouble(item.Replace('.',',').Trim())}");
                                matr[i, j++] = Convert.ToDouble(item.Replace('.', ',').Trim());
                            }
                            i++;
                        }
                        var ss = fileName.ElementAt(fileName.Length - 6).ToString();
                        var ss2 = fileName.ElementAt(fileName.Length - 5).ToString();
                        double value = Convert.ToDouble(ss) +
                                       Convert.ToDouble(ss2) / 10.0;

                        var interpLevLine = new LevelLine(value);
                        var s = $"№ {k}, value = {value}";
                        chart1.Series.Add(s);
                        chart1.Series[s].ChartType = Charting.SeriesChartType.Line;
                        for (int j = 1; j < matr.GetLength(0); j++)
                        {
                            interpLevLine.AddPoint(matr[j, 0], matr[j, 1]);
                            chart1.Series[s].Points.AddXY(matr[j, 0], matr[j, 1]);
                        }
                        oneGraff.AddElement(k++, interpLevLine);
                    }
                    if(!oneGraff.ValidData())
                    {
                        MessageBox.Show("Плохие данные");
                    }
                    oneGraff.SaveToXmlFile(Path.ChangeExtension(od.FileNames[0], ".xml"));
                }
            }
            catch (Exception)
            {
               
            }
        }
    }
}
