using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using RocketAero;
using System.Linq;
using Interpolator;

namespace AeroApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            
        }
        private AeroGraphs rr = new AeroGraphs();
        private void button1_Click(object sender, EventArgs e)
        {
            bool i = radioButton1.Checked;


            //MessageBox.Show($"{ rr.GetV("3_212", 1, 4.5)}");
            double ss = rr.GetV("3_17", 0.55, 0.0, 0.45, 0.5);

            var rrord = from r in rr.Graphs
                        orderby r.Key
                        select r;

            foreach (var item in rrord)
            {
                MessageBox.Show($@"{item.Key} = {item.Value.GetType()}, кол-во параметров = {rr.HowManyParams(item.Key)}"); //, params = {rr.GetParams(item.Key)}");

            }


        }

        private void button2_Click(object sender, EventArgs e)
        {
            

            //InterpXY interp = new InterpXY();
            //interp.Add(0, 2.5);
            //interp.Add(1.4, 0.9);
            //interp.Add(3, 1.0);
            //var sd = new SaveFileDialog();
            //if (sd.ShowDialog() == DialogResult.OK)
            //    interp.SaveToXmlFile(sd.FileName);
        }
    }
}
