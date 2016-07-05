using Sharp3D.Math.Core;

namespace Experiment {
    public interface IPosition1D : IScnObj {
        double X { get; set; }
        IScnPrm pX { get; set; }
        void AddDiffVect(IPosition1D dXdt, bool getNewName);
    }

    public interface IPosition2D : IPosition1D {
        Vector2D V { get; set; }
        double Y { get; set; }
        IScnPrm pY { get; set; }
        void AddDiffVect(IPosition2D dV2Ddt,bool getNewName);
    }

    public interface IPosition3D: IPosition2D {
        new Vector3D V { get; set; }
        double Z { get; set; }
        IScnPrm pZ { get; set; }
        void AddDiffVect(IPosition3D dVdt,bool getNewName);
    }

    public class Position1D : ScnObjDummy, IPosition1D {
        public IScnPrm pX { get; set; }

        protected void ResetParamX() {
            RemoveParam("X");
            pX = new ScnPrm("X",this,X);
            pX.GetVal = t => X;
            pX.SetVal = val => X = val;
        }

        public double X { get; set; }

        public Position1D(double x,string name) {
            X = x;
            Resetparams();
            Name = name;
        }
        public Position1D(string name = "Pos1D") : this(0d,name) { }

        public void AddDiffVect(IPosition1D dXdt,bool getNewName = false) {
            AddDiffPropToParam(pX,dXdt.pX,true,getNewName);
        }

        public override void Resetparams() {
            ResetParamX();
        }
    }

    public class Position2D : Position1D, IPosition2D {
        private Vector2D v;
        public Vector2D V {
            get { return v; }
            set { v = value; }
        }

        public IScnPrm pY { get; set; }

        protected void ResetParamY() {
            RemoveParam("Y");
            pY = new ScnPrm("Y",this,Y);
            pY.GetVal = t => Y;
            pY.SetVal = val => Y = val;
        }
        protected new void ResetParamX() {
            RemoveParam("X");
            pX = new ScnPrm("X",this,X);
            pX.GetVal = t => X;
            pX.SetVal = val => X = val;
        }

        public new double X {
            get {
                return v.X;
            }
            set {
                v.X = value;
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

        public Position2D(double x,double y,string name) {
            X = x;
            Y = y;
            Resetparams();
            Name = name;
        }
        public Position2D(Vector2D posVec,string name = "Pos2D") : this(posVec.X,posVec.Y,name) { }
        public Position2D(string name = "Pos2D") : this(0d,0d,name) { }

        public void AddDiffVect(IPosition2D dVdt,bool getNewName = false) {
            AddDiffPropToParam(pX,dVdt.pX,true,getNewName);
            AddDiffPropToParam(pY,dVdt.pY,true,getNewName);
        }

        public override void Resetparams() {
            ResetParamX();
            ResetParamY();
        }
    }

    public class Position3D : Position2D, IPosition3D {
        private Vector3D v;
        public new Vector3D V {
            get { return v; }
            set { v = value; }
        }

        public IScnPrm pZ { get; set; }

        protected new void ResetParamX() {
            RemoveParam("X");
            pX = new ScnPrm("X",this,X);
            pX.GetVal = t => X;
            pX.SetVal = val => X = val;
        }
        protected new void ResetParamY() {
            RemoveParam("Y");
            pY = new ScnPrm("Y",this,Y);
            pY.GetVal = t => Y;
            pY.SetVal = val => Y = val;
        }
        protected void ResetParamZ() {
            RemoveParam("Z");
            pZ = new ScnPrm("Z", this, Z);
            pZ.GetVal = t => Z;
            pZ.SetVal = val => Z = val;
        }

        public new double X {
            get {
                return v.X;
            }
            set {
                v.X = value;
            }
        }
        public new double Y {
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

        public Position3D(double x, double y, double z, string name) {
            X = x;
            Y = y;
            Z = z;
            Resetparams();
            Name = name;
        }
        public Position3D(Vector3D posVec, string name = "Pos") : this(posVec.X,posVec.Y,posVec.Z,name) { }
        public Position3D(string name = "Pos") : this(0d,0d,0d,name) { }

        public void AddDiffVect(IPosition3D dVdt, bool getNewName = false) {
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
