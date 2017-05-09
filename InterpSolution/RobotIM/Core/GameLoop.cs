using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace RobotIM.Core {
    public class GameLoop {
        public List<IUnit> Units { get; private set; } = new List<IUnit>();
        public Dictionary<string, object> Stats { get; set; } = new Dictionary<string, object>();
        public double Time { get; set; }
        public double dT { get; set; } = 0.01;
        public Func<int> StopFunc { get; set; }
        public static int TIME_LIMIT_RESULT = 77;
        public double MaxTime { get; set; } = 100d;
        public int Result { get; set; } = 0;

        public GameLoop() {
            StopFunc += StopFuncStandart;
        }

        public int StopFuncStandart() {
            return Time >= MaxTime ? TIME_LIMIT_RESULT : 0;
        }

        public void AddUnit(IUnit unit) {
            Units.Add(unit);
            unit.Owner = this;
        }
        public void UpdateAllUnits(double toTime) {
            foreach (var unit in Units) {
                if(unit.Enabled)
                    unit.Update(toTime);
            }
        }
        public void EnableAllUnits() {
            Units.ForEach(u => u.Enabled = true);
        }
        public IEnumerable<T> GetUnitsSpec<T>(bool enabletMatters = true) {
            return Units.Where(u => (!enabletMatters || u.Enabled) && u is T).Cast<T>();
        }

        public bool StepUp() {
            Time += dT;
            UpdateAllUnits(Time);
            Result = StopFunc();
            return Result <= 0;
           
        }

        public void StartLoop() {
            Time = 0d;
            while (StepUp()) {
            }
        }

        public static void SaveToXmlFile<T>(T saveMe, string filePath) {
            try {
                XmlSerializer serial = new XmlSerializer(typeof(T));
                var sw = new StreamWriter(filePath);
                serial.Serialize(sw, saveMe);
                sw.Close();
            } catch (Exception) { }
        }

        public static T LoadFromXmlFile<T>(string filePath) {
            try {
                XmlSerializer serial = new XmlSerializer(typeof(T));
                var sw = new StreamReader(filePath);
                T result = (T)serial.Deserialize(sw);
                sw.Close();
                return result;
            } catch (Exception) { }
            return default(T);
        }
    }
}
