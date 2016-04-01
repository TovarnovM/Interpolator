using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using RocketAero;
using System.Linq;

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
            
            var rrord = from r in rr.Graphs
                        orderby r.Key
                        select r;

            foreach (var item in rrord)
            {
                MessageBox.Show($@"{item.Key} = {item.Value.GetType()}, кол-во параметров = {rr.HowManyParams(item.Key)}"); //, params = {rr.GetParams(item.Key)}");

            }
            //MessageBox.Show($"{ rr.GetV("3_212", 1, 4.5)}");
            double ss = rr.GetV("3_5", 1, 4.5,2);

        }
    }
}
