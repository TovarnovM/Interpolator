using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoubleEnumGenetic {
    public interface IGeneDE {
        string Name { get; }
        double GetRandValue();
        bool ValidateValue(double value);
    }
}
