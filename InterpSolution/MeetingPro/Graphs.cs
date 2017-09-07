using Interpolator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeetingPro {
    public class Graphs {
        static readonly Lazy<Graphs> lazyInstance = new Lazy<Graphs>(LoadFromFile);
        static Graphs LoadFromFile() {
            throw new NotImplementedException();
        }
        public static Graphs Instance => lazyInstance.Value;
        public static Graphs GetNew => Instance.Copy();
        protected Graphs(List<IInterpElem> list) {
            throw new NotImplementedException();
        }

        protected Graphs(Graphs copyFrom) {
            foreach (var gr in copyFrom.lstGr) {
                lstGr.Add((IInterpElem)gr.Clone());
            }
            
            foreach (var kv in copyFrom.dictGr) {
                dictGr.Add(kv.Key, (IInterpElem)kv.Value.Clone());
            }
        }

        public Graphs Copy() {
            return new Graphs(this);
        }

        private static string filePath;
        public static string FilePath {
            get { return filePath; }
            set { filePath = value; }
        }

        List<IInterpElem> lstGr = new List<IInterpElem>(30);
        Dictionary<string, IInterpElem> dictGr = new Dictionary<string, IInterpElem>(30);

        public IInterpElem this[string grName] {
            get {
                return dictGr[grName];
            }
        }

        public IInterpElem this[int grNum] {
            get {
                return lstGr[grNum];
            }
        }

        public int Count => lstGr.Count;

        public string[] Names => dictGr.Keys.ToArray();
    }
}
