using Microsoft.Research.Oslo;
using MoreLinq;
using Sharp3D.Math.Core;
using SimpleIntegrator;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeetingPro {
    public static class GramSLoader {
        public static List<Grammy> LoadGrammyFromFolder(string folderPath, char separator = ';') {
            var lst = Directory.EnumerateFiles(folderPath).ToList();
            var res = new List<Grammy>(lst.Count);
            var locker = new object();
            Parallel.ForEach(lst,new ParallelOptions() { MaxDegreeOfParallelism = 9 }, fp => {
                var owList = LoadFromFile(fp, separator);
                var gr = new Grammy();
                gr.FromOneWayList(owList);
                lock (locker) {
                    res.Add(gr);
                }
                
            });
            return res;
        }

        public static Task<List<Grammy>> LoadGrammyFromFolderAsync(string folderPath, char separator = ';') {
            return Task.Factory.StartNew(() => LoadGrammyFromFolder(folderPath, separator));
        }

        public static void SaveToFile(this List<Grammy> lst, string filePath, string separator = ";") {
            var csv = new StringBuilder();
            foreach (var ow in lst) {
                foreach (var h in ow.ToOneVector().ToArray()) {
                    csv.Append(h);
                    csv.Append(separator);
                }
                csv.Length--;
                csv.Append("\n");
            }
            File.WriteAllText(filePath, csv.ToString());
        }

        public static List<Grammy> LoadGrammyFromFile(string filePath, char separator = ';') {
            var res = new List<Grammy>();
            using (var reader = new StreamReader(filePath)) {
                while (!reader.EndOfStream) {
                    var line = reader.ReadLine();
                    var lns = line.Trim().Split(separator);
                    var arr = new double[lns.Length];
                    int i = 0;
                    foreach (var s in lns) {
                        arr[i] = GetDouble(s.Trim());
                        i++;
                    }
                    foreach (var d in arr) {
                        if (Double.IsNaN(d)) {
                            int y = 77;
                        }
                    }
                    var gr = new Grammy();
                    gr.FromOneVector(new Vector(arr));
                    res.Add(gr);
                }

            }
            return res;
        }

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
                        if (double.IsNaN(arr[i])) {
                            int gg = 77;
                        }
                        i++;
                    }
                    var ow = new OneWay();
                    ow.FromArray(arr);
                    res.Add(ow);
                }

            }
            return res;
        }

        public static List<(Vector2D pos, OneWay ow)> AddCoord(this List<OneWay> list, double krenMax = 180d, double thettaMax = 185d) {
            var goodList = list
                .Where(ow => ow.Vec1.Kren > -krenMax && ow.Vec1.Kren < krenMax)
                .Where(ow => ow.Vec1.Thetta > -thettaMax && ow.Vec1.Thetta < thettaMax)
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
                    return (pos: PlaneMe(sk, ow.Pos1.GetPos0()), ow: ow);
                })
                .ToList();

            return tups ;
        }

        public static List<(Vector2D pos, OneWay ow)> Uniquest(this List<(Vector2D pos, OneWay ow)> list) {
            //var up = list.MaxBy(tp => tp.pos.Y).pos;
            int eliteCount = 7;// list.Count / 10;
            var up = list
                .OrderBy(tp => tp.pos.Y)
                .TakeLast(eliteCount)
                .Aggregate(new Vector2D(0, 0), (sum, tp) => {
                    sum += tp.pos;
                    return sum;
                },
                sum => {
                    sum /= eliteCount;
                    return sum;
                });
            var down = list
                .OrderBy(tp => tp.pos.Y)
                .Take(eliteCount)
                .Aggregate(new Vector2D(0, 0), (sum, tp) => {
                    sum += tp.pos;
                    return sum;
                },
                sum => {
                    sum /= eliteCount;
                    return sum;
                });
            var right = list
                .OrderBy(tp => tp.pos.X)
                .TakeLast(eliteCount)
                .Aggregate(new Vector2D(0, 0), (sum, tp) => {
                    sum += tp.pos;
                    return sum;
                },
                sum => {
                    sum /= eliteCount;
                    return sum;
                });
            var left = list
                .OrderBy(tp => tp.pos.X)
                .Take(eliteCount)
                .Aggregate(new Vector2D(0, 0), (sum, tp) => {
                    sum += tp.pos;
                    return sum;
                },
                sum => {
                    sum /= eliteCount;
                    return sum;
                });
            var center = 0.25 * (up + down + right + left);

            var dists = new Vector2D[] { up, down, right, left, center };
            var paramPoss = new(double x, double y)[] { (0,1), (0,-1), (1,0), (-1,0), (0,0) };
            var sko = dists
                .Select(pos => (pos - center).GetLength() / 12)
                .Max();

            var res = dists
                .Zip(paramPoss, (p, pp) => new { pos = p, ppos = pp })
                .Select(tp => {
                    var (pos, ow) = list.GetSmoothP(tp.pos, sko);
                    ow.XPos = tp.ppos.x;
                    ow.YPos = tp.ppos.y;
                    ow.Vec1.XPos = tp.ppos.x;
                    ow.Vec1.YPos = tp.ppos.y;
                    return (pos, ow);
                })
                .ToList();
            return res;
                
        }

        public static (Vector2D pos, OneWay ow) GetSmoothP(this List<(Vector2D pos, OneWay ow)> list, Vector2D mo, double sko) {
            var vec = Vector.Zeros(list[0].ow.ToVector().Length);
            double sum = 0d;
            foreach (var tp in list) {
                var normmn = NormMnozj(mo, sko, tp.pos);
                var db = tp.ow.ToVector() * normmn;
                for (int i = 0; i < db.Length; i++) {
                    if (double.IsNaN(db[i])) {
                        int gg = 77;
                    }
                }
                vec += db;
                sum += normmn;
            }
            vec /= sum;
            var ow = new OneWay();
            for (int i = 0; i < vec.Length; i++) {
                if (double.IsNaN(vec[i])) {
                    int gg = 77;
                }
            }
            ow.FromVector(vec);
            return (pos: mo, ow: ow);
        }

        static Vector2D PlaneMe(Orient3D sk, Vector3D pos1) {         
            var pos1_loc = sk.WorldTransform_1 * pos1;
            return new Vector2D(pos1_loc.X, pos1_loc.Y);
        }

        static double NormMnozj(Vector2D mo, double sigmXRo, Vector2D coord) {
            return 1 / (sigmXRo * Math.Sqrt(2 * 3.14159)) * Math.Exp(-(coord - mo).GetLengthSquared() / 2 / sigmXRo / sigmXRo);
        }

        public static double GetDouble(string value, double defaultValue = 0d) {
            try {
                //Try parsing in the current culture
                if (!double.TryParse(value, System.Globalization.NumberStyles.Any, CultureInfo.CurrentCulture, out double result) &&
                    //Then try in US english
                    !double.TryParse(value, System.Globalization.NumberStyles.Any, CultureInfo.GetCultureInfo("en-US"), out result) &&
                    //Then in neutral language
                    !double.TryParse(value, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out result)) {
                    result = defaultValue;
                }
                if (double.IsNaN(result)) {
                    result = defaultValue;
                }
                return result;
            } catch {
                return defaultValue;
            }

        }
    }
}
