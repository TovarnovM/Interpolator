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
        void SendMessageToMe(UnitMessage message);
    }

    public struct UnitMessage {
        public IUnit from;
        public string name;
        public object content;
    }
    [Serializable]
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
        public Queue<UnitMessage> MessQueue = new Queue<UnitMessage>();

        public void SendMessageToMe(UnitMessage message) {
            MessQueue.Enqueue(message);
            Owner?.Logger?.AddLine(this, $"Getts message messName={message.name} from={message.from.Name}");
        }

        public void Update(double toTime) {
            PerformUpdate(toTime);
            UnitTime = toTime;
        }

        protected abstract void PerformUpdate(double toTime);
    }
}
