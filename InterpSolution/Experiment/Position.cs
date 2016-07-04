using Sharp3D.Math.Core;

namespace Experiment {
    public interface IPosition {
        double X { get; set; }
        double Y { get; set; }
        double Z { get; set; }
        double W { get; set; }
        IScnPrm pX { get; set; }
        IScnPrm pY { get; set; }
        IScnPrm pZ { get; set; }
        void AddDiffVect(IPosition dVdt,bool getNewName);
    }

    public class Position :  ScnObjDummy, IPosition {
        private Vector4D v;
        public Vector4D V {
            get { return v; }
            set { v = value; }
        }
        public IScnPrm pX { get; set; }
        public IScnPrm pY { get; set; }
        public IScnPrm pZ { get; set; }

        protected void ResetParamX() {
            RemoveParam("X");
            pX = new ScnPrm("X", this, X);
            pX.GetVal = t => X;
            pX.SetVal = val => X = val;
        }
        protected void ResetParamY() {
            RemoveParam("Y");
            pY = new ScnPrm("Y", this, Y);
            pY.GetVal = t => Y;
            pY.SetVal = val => Y = val;
        }
        protected void ResetParamZ() {
            RemoveParam("Z");
            pZ = new ScnPrm("Z", this, Z);
            pZ.GetVal = t => Z;
            pZ.SetVal = val => Z = val;
        }

        public double X {
            get {
                return v.X;
            }
            set {
                v.SetX(value);
            }
        }
        public double Y {
            get {
                return v.Y;
            }
            set {
                v.Y = value;
            }
        }
        public double Z {
            get {
                return v.Z;
            }
            set {
                v.Z = value;
            }
        }
        public double W {
            get {
                return v.W;
            }
            set {
                v.W = value;
            }
        }

        public Position(double x, double y, double z, double w, string name) {
            X = x;
            Y = y;
            Z = z;
            W = w;
            Resetparams();
            Name = name;
        }
        public Position() : this(0d, 0d, 0d, 1d, "Pos") {

        }

        public void AddDiffVect(IPosition dVdt, bool getNewName = false) {
            AddDiffPropToParam(pX, dVdt.pX, true, getNewName);
            AddDiffPropToParam(pY, dVdt.pY, true, getNewName);
            AddDiffPropToParam(pZ, dVdt.pZ, true, getNewName);
        }

        public override void Resetparams() {
            ResetParamX();
            ResetParamY();
            ResetParamZ();
        }
    }
}
