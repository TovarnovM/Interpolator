using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DoubleEnumGenetic {
    public static class ChromosomeDIOHelper {

        public static void Serialize(this ChromosomeD dummy,TextWriter writer) {
            var dict = dummy.SaveToDictionary();
            Serialize(writer,dict);
        }

        public static void Deserialize(this ChromosomeD dummy,TextReader reader) {
            var dict = new Dictionary<string,double>();
            Deserialize(reader,dict);
            dummy.LoadFromDictionary(dict);
        }

        public static void Serialize(TextWriter writer,IDictionary<string,double> dictionary) {
            List<DictEntry> entries = new List<DictEntry>(dictionary.Count);
            foreach(var key in dictionary.Keys) {
                entries.Add(new DictEntry(key,dictionary[key]));
            }
            XmlSerializer serializer = new XmlSerializer(typeof(List<DictEntry>));
            serializer.Serialize(writer,entries);


        }

        public static void Deserialize(TextReader reader,IDictionary dictionary) {
            dictionary.Clear();
            XmlSerializer serializer = new XmlSerializer(typeof(List<DictEntry>));
            List<DictEntry> list = (List<DictEntry>)serializer.Deserialize(reader);
            foreach(DictEntry entry in list) {
                dictionary[entry.Key] = entry.Value;
            }
        }


        public class DictEntry {
            [XmlAttribute]
            public string Key;
            [XmlAttribute]
            public double Value;
            public DictEntry() {
            }

            public DictEntry(string key,double value) {
                Key = key;
                Value = value;
            }
        }
    }
}
