using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotIM.Core {
    public interface IUnit {
        double UnitTime { get; set; }
        double DeltaT { get; }
        GameLoop Owner { get; set; }
        string Name { get; set; }
        void Update();
        bool Enabled { get; set; }
    }
}
