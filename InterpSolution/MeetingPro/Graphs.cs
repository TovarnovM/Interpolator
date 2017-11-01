using Interpolator;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MeetingPro {
    public class Graphs {
        static readonly Lazy<Graphs> lazyInstance = new Lazy<Graphs>(LoadFromFile);
        static Graphs LoadFromFile() {
            XmlSerializer serial = new XmlSerializer(typeof(List<Interp2D>));
            var sr = new StreamReader(filePath);
            var nLst = (List<Interp2D>)serial.Deserialize(sr);
            var dct = nLst.ToDictionary(it => it.Title);

            var c_r_x = new Interp3D() {
                Title = "c_r_x"
            };
            c_r_x.AddElement(0d, dct["А.8"]);
            c_r_x.AddElement(5d, dct["А.9"]);
            c_r_x.AddElement(10d, dct["А.10"]);
            c_r_x.AddElement(15d, dct["А.11"]);
            dct.Remove("А.8");
            dct.Remove("А.9");
            dct.Remove("А.10");
            dct.Remove("А.11");

            var c_r_y_i = new Interp3D() {
                Title = "c_r_y_i"
            };
            c_r_y_i.AddElement(0d, dct["А.17"]);
            c_r_y_i.AddElement(5d, dct["А.18"]);
            c_r_y_i.AddElement(10d, dct["А.19"]);
            c_r_y_i.AddElement(15d, dct["А.20"]);
            dct.Remove("А.17");
            dct.Remove("А.18");
            dct.Remove("А.19");
            dct.Remove("А.20");

            var m_omegax_x_dempf = dct["А.21"].Data.Values[0];
            m_omegax_x_dempf.Title = "m_omegax_x_dempf";
           dct.Remove("А.21");

            var m_x0 = new Interp3D() {
                Title = "m_x0"
            };
            m_x0.AddElement(0d, dct["А.23"]);
            m_x0.AddElement(5d, dct["А.24"]);
            m_x0.AddElement(10d, dct["А.25"]);
            m_x0.AddElement(15d, dct["А.26"]);
            dct.Remove("А.23");
            dct.Remove("А.24");
            dct.Remove("А.25");
            dct.Remove("А.26");

            foreach (var tp in altNames) {
                dct[tp.aName].Title = tp.aName;
            }

            var ro = GetRo_atmo();
            var a = GetA_atmo();

            var replDict = altNames
                .ToDictionary(tp => tp.aName, tp => tp.altName);

            var resDict = dct
                .Select(kv => (kv.Value.Title, (IInterpElem)kv.Value))
                .Concat(new(string, IInterpElem)[] {
                    (c_r_x.Title,c_r_x),
                    (c_r_y_i.Title,c_r_y_i),
                    (m_x0.Title,m_x0),
                    (ro.Title,ro),
                    (a.Title,a),
                    (m_omegax_x_dempf.Title,m_omegax_x_dempf)
                })
                .Select(tp => {
                    if (replDict.ContainsKey(tp.Item1))
                        return (replDict[tp.Item1], tp.Item2);
                    else
                        return tp;
                })
                .ToDictionary(tp => tp.Item1, tp => tp.Item2);

            return new Graphs(resDict);
        }
        public static Graphs Instance => lazyInstance.Value;
        public static Graphs GetNew() => Instance.Copy();

        public static readonly List<(string aName, string altName)> altNames = new List<(string aName, string altName)>(new(string aName, string altName)[] {
            ("А.3","x_m"),
            ("А.4","m"),
            ("А.5","i_x"),
            ("А.6","i_yz"),
            ("А.7","c_x"),
            ("А.12","c_k_y"),
            ("А.13","x_k_d"),
            ("А.14","x_kr_d"),
            ("А.15","c_kr_y_i"),
            ("А.16","alpha_sk"),
            ("А.22","m_omegaz_z_dempf")
        }
        );

        protected Graphs(Dictionary<string, IInterpElem> dict) {
            dictGr = dict;
        }

        protected Graphs(Graphs copyFrom) {
            foreach (var kv in copyFrom.dictGr) {
                dictGr.Add(kv.Key, (IInterpElem)kv.Value.Clone());
            }
        }

        public Graphs Copy() {
            return new Graphs(this);
        }

        private static string filePath = @"C:\Users\User\Documents\data.xml";
        public static string FilePath {
            get { return filePath; }
            set { filePath = value; }
        }

        Dictionary<string, IInterpElem> dictGr = new Dictionary<string, IInterpElem>();

        public IInterpElem this[string grName] {
            get {
                return dictGr[grName];
            }
        }

        public int Count => dictGr.Count;

        public string[] Names => dictGr.Keys.ToArray();

        static InterpXY GetA_atmo() {
            var res = new InterpXY() {
                Title = "a"
            };
            res.Add(-100, 263.7);
            res.Add(-50,299.3);
            res.Add(-20,318.8);
            res.Add(-10,325.1);
            res.Add(0,331.5);
            res.Add(10,337.3);
            res.Add(20,343.1);
            res.Add(30,348.9);
            res.Add(50,360.3);
            res.Add(100,387.1);
            res.SynchArrays();
            return res;
        }

        static InterpXY GetRo_atmo() {
            var res = new InterpXY() {
                Title = "ro"
            };
            res.Add(-50, 1.584);
            res.Add(-45, 1.549);
            res.Add(-40,1.515 );
            res.Add(-35,1.484 );
            res.Add(-30,1.453 );
            res.Add(-25, 1.484);
            res.Add(-20,1.395 );
            res.Add(-15,1.369 );
            res.Add(-10,1.342 );
            res.Add(-5,1.318 );
            res.Add(0, 1.293);
            res.Add(10, 1.247);
            res.Add(15, 1.226);
            res.Add(20, 1.205);
            res.Add(30, 1.165);            
            res.Add(40, 1.128);
            res.Add(50, 1.093);

            res.SynchArrays();
            return res;
        }
    }
}
