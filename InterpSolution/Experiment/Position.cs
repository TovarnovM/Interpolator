using Sharp3D.Math.Core;

namespace Experiment {
    class Position : ScnObjDummy {
        private Vector4D v;
        public Vector4D V {
            get { return v; }
            set { v = value; }
        }
        public IScnPrm pX { get; set; }
        public IScnPrm pY { get; set; }
        public IScnPrm pZ { get; set; }

        protected void AddParamX() {
            RemoveParam("X");
            pX = new ScnPrm("X", this, X);
            pX.GetVal = v.GetX;
            pX.SetVal = v.SetX;
        }
        protected void AddParamY() {
            RemoveParam("Y");
            pY = new ScnPrm("Y", this, Y);
            pY.GetVal = v.GetY;
            pY.SetVal = v.SetY;
        }
        protected void AddParamZ() {
            RemoveParam("Z");
            pZ = new ScnPrm("Z", this, Z);
            pZ.GetVal = v.GetZ;
            pZ.SetVal = v.SetZ;
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
            AddParamX();
            AddParamY();
            AddParamZ();
            Name = name;
        }
        public Position() : this(0d, 0d, 0d, 1d, "Pos") {

        }

        public void AddDiffVect(Position dVdt, bool getNewName = false) {
            AddDiffPropToParam(pX, dVdt.pX, true, getNewName);
            AddDiffPropToParam(pY, dVdt.pY, true, getNewName);
            AddDiffPropToParam(pZ, dVdt.pZ, true, getNewName);
        }

    }
}
