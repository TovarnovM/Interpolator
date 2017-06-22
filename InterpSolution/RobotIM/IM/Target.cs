using Sharp3D.Math.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace RobotIM.IM {

    public class Target {
        public List<(double prob,Vector2D cp)> Hits = new List<(double prob, Vector2D cp)>();
        public AimSurf AimSurf { get; set; } = new AimSurf();
        public string Name { get; set; } = "Пустышка";
        public static Target Factory(string TrgType) {
            var res = new Target();
            if(TrgType == "fire") {
                res.Name = "Закон поражения для огн оруж";

                res.AimSurf.AddBox(-0.35, 1.45, -0.2, 1.05, 0.45);
                res.AimSurf.AddBox(0.2, 1.45, 0.35, 1.05, 0.3);
                res.AimSurf.AddBox(-0.35, 1.05, -0.25, 0.7, 0.35);
                res.AimSurf.AddBox(0.25, 1.05, 0.35, 0.7, 0.25);
                res.AimSurf.AddBox(-0.2, 0.85, -0.05, 0.45, 0.15);
                res.AimSurf.AddBox(0.05, 0.85, 0.2, 0.45, 0.15);
                res.AimSurf.AddBox(-0.15, 0.45, -0.05, 0, 0.1);
                res.AimSurf.AddBox(0.05, 0.45, 0.15, 0, 0.1);
                res.AimSurf.AddBox(-0.2, 1.45, 0, 1.1, 0.7);
                res.AimSurf.AddBox(0, 1.45, 0.2, 1.1, 0.75);
                res.AimSurf.AddBox(-0.15, 1.1, 0, 0.85, 0.65);
                res.AimSurf.AddBox(0, 1.1, 0.15, 0.85, 0.6);
                res.AimSurf.AddBox(-0.05, 1.5, 0.05, 1.45, 0.8);
                res.AimSurf.AddBox(-0.1, 1.7, 0.1, 1.5, 0.9);

                res.AimSurf.AimPoint = new Vector2D(0, 1.25);
            }
            if (TrgType == "2") {
                res.Name = "Закон поражения для огн оруж";

                res.AimSurf.AddBox(-0.35, 1.45, -0.2, 1.05, 0.55);
                res.AimSurf.AddBox(0.2, 1.45, 0.35, 1.05, 0.4);
                res.AimSurf.AddBox(-0.35, 1.05, -0.25, 0.7, 0.45);
                res.AimSurf.AddBox(0.25, 1.05, 0.35, 0.7, 0.35);
                res.AimSurf.AddBox(-0.2, 0.85, -0.05, 0.45, 0.25);
                res.AimSurf.AddBox(0.05, 0.85, 0.2, 0.45, 0.25);
                res.AimSurf.AddBox(-0.15, 0.45, -0.05, 0, 0.2);
                res.AimSurf.AddBox(0.05, 0.45, 0.15, 0, 0.2);
                res.AimSurf.AddBox(-0.2, 1.45, 0, 1.1, 0.8);
                res.AimSurf.AddBox(0, 1.45, 0.2, 1.1, 0.85);
                res.AimSurf.AddBox(-0.15, 1.1, 0, 0.85, 0.75);
                res.AimSurf.AddBox(0, 1.1, 0.15, 0.85, 0.7);
                res.AimSurf.AddBox(-0.05, 1.5, 0.05, 1.45, 0.9);
                res.AimSurf.AddBox(-0.1, 1.7, 0.1, 1.5, 1);

                res.AimSurf.AimPoint = new Vector2D(0, 1.25);
            }
            if (TrgType == "3") {
                res.Name = "Закон поражения для огн оруж";

                res.AimSurf.AddBox(-0.35, 1.45, -0.2, 1.05, 0.75);
                res.AimSurf.AddBox(0.2, 1.45, 0.35, 1.05, 0.8);
                res.AimSurf.AddBox(-0.35, 1.05, -0.25, 0.7, 0.95);
                res.AimSurf.AddBox(0.25, 1.05, 0.35, 0.7, 0.85);
                res.AimSurf.AddBox(-0.2, 0.85, -0.05, 0.45, 0.85);
                res.AimSurf.AddBox(0.05, 0.85, 0.2, 0.45, 0.85);
                res.AimSurf.AddBox(-0.15, 0.45, -0.05, 0, 0.8);
                res.AimSurf.AddBox(0.05, 0.45, 0.15, 0, 0.8);
                res.AimSurf.AddBox(-0.2, 1.45, 0, 1.1, 0.8);
                res.AimSurf.AddBox(0, 1.45, 0.2, 1.1, 0.95);
                res.AimSurf.AddBox(-0.15, 1.1, 0, 0.85, 0.75);
                res.AimSurf.AddBox(0, 1.1, 0.15, 0.85, 0.7);
                res.AimSurf.AddBox(-0.05, 1.5, 0.05, 1.45, 0.9);
                res.AimSurf.AddBox(-0.1, 1.7, 0.1, 1.5, 1);

                res.AimSurf.AimPoint = new Vector2D(0, 1.25);
            }
            return res;
        }
    }

    [Serializable]
    public class Rect {
        public double xmin, ymin, xmax, ymax, damage;
        [XmlIgnore]
        public double Width {
            get {
                return xmax - xmin;
            }
            set {
                if (value > 0)
                    xmax = xmin + value;
                else
                    xmin = xmax + value;
            }
        }
        [XmlIgnore]
        public double Height {
            get {
                return ymax - ymin;
            }
            set {
                if (value > 0)
                    ymax = ymin + value;
                else
                    ymin = ymax + value;
            }
        }
        public Rect() : this(0, 0, 1, 1, 0) { }
        public Rect(double xmin, double ymin, double xmax, double ymax, double damage) {
            this.xmin = Math.Min(xmin, xmax);
            this.ymin = Math.Min(ymin, ymax);
            this.xmax = Math.Max(xmin, xmax);
            this.ymax = Math.Max(ymin, ymax);
            this.damage = damage;//0-1
        }
        public double isHit(Vector2D point) {
            return isHit(point.X, point.Y);
        }
        public double isHit(double hX, double hY) {
            if ((xmin < hX) && (hX < xmax)) {
                if ((ymin < hY) && (hY < ymax)) {
                    return damage;
                }
            }
            return 0;
        }
        public Rect CopyMe() {
            return new Rect(xmin, ymin, xmax, ymax, damage);
        }
    }
    [Serializable]
    public class AimSurf {

        public Vector2D AimPoint { get; set; }

        public IList<Rect> Boxes { get; set; } = new List<Rect>();
        //List<Vector> hits = new List<Vector>();

        public AimSurf() {
            //
        }

        public void AddBox(double xmin, double ymin, double xmax, double ymax, double damage) {
            Boxes.Add(new Rect(xmin, ymin, xmax, ymax, damage));
        }
        public double getDamage(double x, double y) {
            for (int i = Boxes.Count - 1; i >= 0; i--) {
                if (Boxes[i].isHit(x, y) != 0) {
                    return Boxes[i].isHit(x, y);
                }
            }
            return 0;
        }
        public double getDamage(Vector2D hit) {
            return getDamage(hit.X, hit.Y);

        }
        public void loadFromCSV(String Filename) {
            string[] strings = File.ReadAllLines(Filename);
            foreach (String str in strings) {
                if (!String.IsNullOrEmpty(str)) {
                    string stt = str.Trim(new char[] { '"', ';' });
                    Console.Write(stt);
                    string[] buf = stt.Split(',');
                    Double[] outt = new Double[buf.Length];
                    for (int i = 0; i < buf.Length; i++) {
                        //Double.TryParse(buf[i], out outt[i]);
                        outt[i] = double.Parse(buf[i], CultureInfo.InvariantCulture);
                    }
                    Boxes.Add(new Rect(outt[0], outt[1], outt[2], outt[3], outt[4]));
                    Console.Write("\n");
                }
            }
        }
        public void writeInCSV(String filename) {
            //???

        }

        public AimSurf CopyMe() {
            var res = new AimSurf();
            res.AimPoint = AimPoint;
            foreach (var box in Boxes) {
                res.Boxes.Add(box.CopyMe());
            }
            return res;
        }
    }
}
