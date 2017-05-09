using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotIM.Core {
    public interface IUnit {
        double UnitTime { get; set; }
        GameLoop Owner { get; set; }
        string Name { get; set; }
        void Update(double toTime);
        bool Enabled { get; set; }
    }

    public abstract class UnitBase : IUnit {
        public UnitBase(string Name, GameLoop Owner = null) {
            this.Name = Name;
            this.Owner = Owner;
            UnitTime = 0d;
            Enabled = true;
        }

        public double UnitTime { get; set; }        

        public GameLoop Owner { get; set; }
        public string Name { get; set; }
        public bool Enabled { get; set; }

        public void Update(double toTime) {
            PerformUpdate(toTime);
            UnitTime = toTime;
        }

        protected abstract void PerformUpdate(double toTime);
    }
}
