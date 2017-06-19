using OxyPlot;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotSim {
    public class CheckedListItem<T> : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;

        private bool isChecked;
        private T item;

        public CheckedListItem() { }

        public CheckedListItem(T item, bool isChecked = false) {
            this.item = item;
            this.isChecked = isChecked;
        }

        public T Item {
            get { return item; }
            set {
                item = value;
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("Item"));
            }
        }


        public bool IsChecked {
            get { return isChecked; }
            set {
                isChecked = value;
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("IsChecked"));
            }
        }
    }

    public class GraffLine {
        public string Name { get; set; }
        public LineSeries LineSer { get; set; }
        public void ChLstItem_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if (e.PropertyName == "IsChecked") {
                LineSer.IsVisible = (sender as CheckedListItem<GraffLine>).IsChecked;
                LineSer.PlotModel.InvalidatePlot(false);
            }
                
        }
    }
}
