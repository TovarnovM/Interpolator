using GeneticSharp.Domain.Randomizations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoubleEnumGenetic {
    public class GeneEnumString: IGeneDE {
        public bool Matters { get; set; } = true;
        public IList<string> Variants { get; private set; }
        int _min, _max;
        public GeneEnumString(string name, IList<string> variants) {
            Name = name;
            Variants = variants;
            _min = 0;
            _max = variants.Count;
        }

        public string Name { get; }

        public double GetRandValue() {
            return (double)RandomizationProvider.Current.GetInt(_min,_max);
        }

        public bool ValidateValue(double value) {
            int v = (int)value;
            return _min <= v && v < _max;
        }

        public double GetDoubleByString(string variant) {
            var answ = Variants.IndexOf(variant);
            if(answ < 0)
                throw new Exception("Такого варианта нет");
            return (double)answ;
        }

        public string GetStringByDouble(double variant) {
            if(!ValidateValue(variant))
                throw new Exception("Такого варианта нет");
            int v = (int)variant;
            return Variants[v];
        }

        public double GetNearestValidate(double value) {
            return ValidateValue(value) ? (int)value :
                value < _min ? _min :
                _max;
        }
    }
}
