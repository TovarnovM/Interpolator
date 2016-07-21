using Sharp3D.Math.Core;
using System;

namespace Experiment {
    public interface IModulus {
        void NormilizeDirection();
        double Modulus { get; set; }
        IScnPrm pModulus { get; set; }
    }

    public interface IPosition1D : IScnObj {
        double X { get; set; }
        IScnPrm pX { get; set; }
        void AddDiffVect(IPosition1D dXdt, bool getNewName);
    }

    public interface IPosition2D : IPosition1D {
        Vector2D Vec2D { get; set; }
        double Y { get; set; }
        IScnPrm pY { get; set; }
        void AddDiffVect(IPosition2D dV2Ddt,bool getNewName);
    }

    public interface IPosition3D: IPosition2D {
        Vector3D Vec3D { get; set; }
        double Z { get; set; }
        IScnPrm pZ { get; set; }
        void AddDiffVect(IPosition3D dVdt,bool getNewName);
    }

    public class Position1D : ScnObjDummy, IPosition1D {
        public IScnPrm pX { get; set; }
        public double X { get; set; }

        public Position1D(object x,string name) {
            Name = name;
            SetParam("x",x);
        }
        public Position1D(string name = "Pos1D") : this(0d,name) { }

        public void AddDiffVect(IPosition1D dXdt,bool getNewName = false) {
            AddDiffPropToParam(pX,dXdt.pX,true,getNewName);
        }
    }

    public class Position1DModulus : Position1D, IModulus {
        public double Modulus { get; set; }
        public IScnPrm pModulus { get; set; }
        public Position1DModulus(object modulus,object x,string name):base(x,name) {
            SetParam("modulus",modulus);
        }
        public Position1DModulus(string name = "Pos1DMod"):this(0d,0d,name) {

        }

        public virtual void NormilizeDirection() {
            X = X < 0d ? -1d : 1d;
        }
    }

    public class Position2D : Position1D, IPosition2D {
        public Vector2D Vec2D {
            get { return new Vector2D(X,Y); }
            set {
                X = value.X;
                Y = value.Y;
            }
        }

        public IScnPrm pY { get; set; }
        public double Y { get; set; }

        //protected void ResetParamY() {
        //    RemoveParam("Y");
        //    pY = new ScnPrm("Y",this,Y);
        //    pY.GetVal = t => Y;
        //    pY.SetVal = val => Y = val;
        //}

        public Position2D(object x,object y,string name):base(x,name) {
            SetParam("y",y);
            //ResetAllParams();
        }
        public Position2D(Vector2D posVec,string name = "Pos2D") : this(posVec.X,posVec.Y,name) { }
        public Position2D(string name = "Pos2D") : this(0d,0d,name) { }

        public void AddDiffVect(IPosition2D dVdt,bool getNewName = false) {
            AddDiffPropToParam(pX,dVdt.pX,true,getNewName);
            AddDiffPropToParam(pY,dVdt.pY,true,getNewName);
        }

    }

    public class Position2DModulus: Position2D, IModulus {
        public double Modulus { get; set; }
        public IScnPrm pModulus { get; set; }
        public Position2DModulus(object modulus,object x,object y,string name):base(x,y,name) {
            SetParam("modulus",modulus);
        }
        public Position2DModulus(string name = "Pos2DMod"):this(0d,0d,0d,name) {

        }

        public virtual void NormilizeDirection() {
            var dir = Vec2D;
            dir.Normalize();
            Vec2D = dir;
        }
    }

    public class Position3D : Position2D, IPosition3D {
        public Vector3D Vec3D {
            get { return new Vector3D(X,Y,Z); }
            set {
                X = value.X;
                Y = value.Y;
                Z = value.Z;
            }
        }

        public IScnPrm pZ { get; set; }

        public double Z { get; set; }

        public Position3D(object x,object y,object z, string name = "Pos"):base(x,y,name) {
            SetParam("z",z);
            //ResetAllParams();
        }
        public Position3D(Vector3D posVec, string name = "Pos") : this(posVec.X,posVec.Y,posVec.Z,name) { }
        public Position3D(string name = "Pos") : this(0d,0d,0d,name) { }

        public void AddDiffVect(IPosition3D dVdt, bool getNewName = false) {
            AddDiffPropToParam(pX, dVdt.pX, true, getNewName);
            AddDiffPropToParam(pY, dVdt.pY, true, getNewName);
            AddDiffPropToParam(pZ, dVdt.pZ, true, getNewName);
        }
    }

    public class Position3DModulus : Position3D, IModulus {
        public double Modulus { get; set; }
        public IScnPrm pModulus { get; set; }
        public Position3DModulus(object modulus,object x,object y,object z,string name) : base(x,y,z,name) {
            SetParam("modulus",modulus);
        }
        public Position3DModulus(string name = "Pos3DMod") : this(0d,0d,0d,0d,name) {

        }

        public virtual void NormilizeDirection() {
            var dir = Vec3D;
            dir.Normalize();
            Vec3D = dir;
        }
    }
}
