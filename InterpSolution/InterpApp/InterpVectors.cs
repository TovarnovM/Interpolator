using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.Research.Oslo;

namespace Interpolator {
    public interface IInterpVecN : IInterpElemAbs<Microsoft.Research.Oslo.Vector> { }
    [Serializable, XmlRoot(nameof(InterpElemVec))]
    public class InterpElemVec : InterpElemAbs<Vector>, IInterpVecN {
        public InterpElemVec(Vector value) : base(value) {

        }
        public InterpElemVec() : this(default(Vector)) {

        }

        public override object Clone() {
            return new InterpElemVec(Value);
        }
    }

    [Serializable]
    public class InterpVec<T> : InterpAbs<T,Vector>, IInterpVecN where T : IInterpVecN {

        public override Vector InterpMethodLine(int n, params double[] t) {
            Vector t1 = _data.Keys[n];
            Vector t2 = _data.Keys[n + 1];
            Vector y1 = GetVSub(n,t);
            n++;
            Vector y2 = GetVSub(n,t);
            return y1 + (y2 - y1) * (t.Last() - t1) / (t2 - t1);
        }

        public override Vector InerpMethodSpecial4_3_17(int n, params double[] t) {
            return InterpMethodLine(n,t);

        }
    }


    [XmlRoot(nameof(InterpXYVec))]
    public class InterpXYVec : InterpVec<InterpElemVec> {
        public int Add(double t,Vector value,bool allowDublicates = false) {
            if(!allowDublicates && _data.ContainsKey(t))
                return 0;
            return AddElement(t,new InterpElemVec(value));
        }
        public int Add(double t,params double[] elts) {
            return AddElement(t,new InterpElemVec(new Vector(elts)));
        }
        public void CopyDataFrom(InterpXYVec parent,bool delPrevData = false) {
            if(delPrevData)
                _data.Clear();
            _data.Capacity = _data.Capacity > (_data.Count + parent.Data.Count) ?
                                (int)((_data.Count + parent.Data.Count) * 1.5) :
                                _data.Capacity;
            foreach(var item in parent.Data) {
                Add(item.Key,item.Value.Value);
            }
        }
        public InterpXYVec CopyMe() {
            var result = new InterpXYVec();
            result.CopyParamsFrom(this);
            result.CopyDataFrom(this);
            return result;
        }

        public static InterpXYVec LoadFromXmlFile(string fileName) => InterpXYVec.LoadFromXmlFile<InterpXYVec>(fileName);
        public static InterpXYVec LoadFromXmlString(string fileStr) => InterpXYVec.LoadFromXmlString<InterpXYVec>(fileStr);

    }

    [XmlRoot(nameof(Interp2DVec))]
    public class Interp2DVec : InterpVec<InterpXYVec> {
        public Interp2DVec CopyMe() {
            var result = new Interp2DVec();
            result.CopyParamsFrom(this);
            foreach(var item in _data) {
                result.AddElement(item.Key,item.Value.CopyMe());
            }
            return result;
        }
        public static Interp2DVec LoadFromXmlFile(string fileName) => Interp2DVec.LoadFromXmlFile<Interp2DVec>(fileName);
        public static Interp2DVec LoadFromXmlString(string fileStr) => Interp2DVec.LoadFromXmlString<Interp2DVec>(fileStr);
    }

    [XmlRoot(nameof(Interp3DVec))]
    public class Interp3DVec : InterpVec<Interp2DVec> {
        public Interp3DVec CopyMe() {
            var result = new Interp3DVec();
            result.CopyParamsFrom(this);
            foreach(var item in _data) {
                result.AddElement(item.Key,item.Value.CopyMe());
            }
            return result;
        }
        public static Interp3DVec LoadFromXmlFile(string fileName) => Interp3DVec.LoadFromXmlFile<Interp3DVec>(fileName);
        public static Interp3DVec LoadFromXmlString(string fileStr) => Interp3DVec.LoadFromXmlString<Interp3DVec>(fileStr);
    }

    [XmlRoot(nameof(Interp4DVec))]
    public class Interp4DVec : InterpVec<Interp3DVec> {
        public Interp4DVec CopyMe() {
            var result = new Interp4DVec();
            result.CopyParamsFrom(this);
            foreach(var item in _data) {
                result.AddElement(item.Key,item.Value.CopyMe());
            }
            return result;
        }
        public static Interp4DVec LoadFromXmlFile(string fileName) => Interp4DVec.LoadFromXmlFile<Interp4DVec>(fileName);
        public static Interp4DVec LoadFromXmlString(string fileStr) => Interp4DVec.LoadFromXmlString<Interp4DVec>(fileStr);
    }




}
