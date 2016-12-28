using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoubleEnumGenetic {
    public enum CritExtremum { minimize, maximize };

    public class CritInfo {
        public CritExtremum Extremum { get; set; }
        public double? Min { get; set; }
        public double? Max { get; set; }
        public bool matters { get; set; } = true;
        public string Name { get; set; }
        public CritInfo(string name,CritExtremum extr = CritExtremum.maximize,double? min = null,double? max = null) {
            Name = name;
            Extremum = extr;
            Min = min;
            Max = max;
        }
        public bool ValidValue(double? value) {
            if(value == null)
                return false;
            if(Min != null && value < Min)
                return false;
            if(Max != null && value > Max)
                return false;
            return true;
        }

    }


    public class Criteria {
        public CritInfo Info { get; set; }
        public double? Value { get; set; }
        public Criteria(CritInfo info,double? value = null) {
            Info = info;
            Value = value;
        }
        public Criteria(Criteria cloneMe) : this(cloneMe.Info,cloneMe.Value) { }
        public Criteria Clone() {
            return new Criteria(this);
        }
    }
}
