using Sharp3D.Math.Core;
using SimpleIntegrator;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace MeetingPro {
    public static class GramSLoader {
        public static void SaveToFile(this List<OneWay> lst, string filePath, string separator = ";") {
            var csv = new StringBuilder();
            foreach (var h in lst[0].GetHeaders()) {
                csv.Append(h);
                csv.Append(separator);
            }
            csv.Length--;
            csv.Append("\n");
            foreach (var ow in lst) {
                foreach (var h in ow.ToArray()) {
                    csv.Append(h);
                    csv.Append(separator);
                }
                csv.Length--;
                csv.Append("\n");
            }
            File.WriteAllText(filePath, csv.ToString());
        }

        public static List<OneWay> LoadFromFile(string filePath, char separator = ';', bool headers = true) {
            var res = new List<OneWay>();
            using (var reader = new StreamReader(filePath)) {
                if(headers)
                    reader.ReadLine();
                
                while (!reader.EndOfStream) {
                    var line = reader.ReadLine();
                    var lns = line.Split(separator);
                    var arr = new double[lns.Length];
                    int i = 0;
                    foreach (var s in lns) {
                        arr[i] = GetDouble(s.Trim());
                        i++;
                    }
                    var ow = new OneWay();
                    ow.FromArray(arr);
                    res.Add(ow);
                }

            }
            return res;
        }

        public static List<Vector2D> Uniquest(this List<OneWay> list) {
            var goodList = list
                .Where(ow => true)//ow.Vec1.Kren > -50 && ow.Vec1.Kren < 50)
                .ToList();
          
            Vector3D pos0 = goodList[0].Pos0.GetPos0();
            Vector3D vel0 = goodList[0].Pos0.GetVel0();

            var sk = new Orient3D();
            sk.Vec3D = pos0;
            sk.SynchQandM();
            var ox = (vel0 & Vector3D.YAxis).Norm;
            sk.SetPosition_LocalPoint_LocalFixed(Vector3D.XAxis, pos0 + ox, new Vector3D(0, 0, 0));

            var oy = (ox & vel0).Norm;
            sk.SetPosition_LocalPoint_LocalFixed(Vector3D.YAxis, pos0 + oy, new Vector3D(-1, 0, 0), new Vector3D(1, 0, 0));

            var tups = goodList
                .Select(ow => {
                    return (oneW: ow, coord: PlaneMe(sk, ow.Pos1.GetPos0()));
                })
                .ToList();



            return tups.Select(tp => tp.coord).ToList() ;
        }

        static Vector2D PlaneMe(Orient3D sk, Vector3D pos1) {         
            var pos1_loc = sk.WorldTransform_1 * pos1;
            return new Vector2D(pos1_loc.X, pos1_loc.Y);
        }

        static double NormMnozj(Vector2D mo, double sigmXRo, Vector2D coord) {
            return 1 / (sigmXRo * Math.Sqrt(2 * 3.14159)) * Math.Exp(-(coord - mo).GetLengthSquared() / 2 / sigmXRo / sigmXRo);
        }

        public static double GetDouble(string value, double defaultValue = 0d) {

            //Try parsing in the current culture
            if (!double.TryParse(value, System.Globalization.NumberStyles.Any, CultureInfo.CurrentCulture, out double result) &&
                //Then try in US english
                !double.TryParse(value, System.Globalization.NumberStyles.Any, CultureInfo.GetCultureInfo("en-US"), out result) &&
                //Then in neutral language
                !double.TryParse(value, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out result)) {
                result = defaultValue;
            }

            return result;
        }
    }
}
