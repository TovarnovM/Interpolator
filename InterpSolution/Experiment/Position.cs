using Sharp3D.Math.Core;
using System;

namespace Experiment {
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

        public Position1D(double x,string name) {
            Name = name;
            X = x;
            //ResetAllParams();
        }
        public Position1D(string name = "Pos1D") : this(0d,name) { }

        public void AddDiffVect(IPosition1D dXdt,bool getNewName = false) {
            AddDiffPropToParam(pX,dXdt.pX,true,getNewName);
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

        public Position2D(double x,double y,string name):base(x,name) {
            Y = y;
            //ResetAllParams();
        }
        public Position2D(Vector2D posVec,string name = "Pos2D") : this(posVec.X,posVec.Y,name) { }
        public Position2D(string name = "Pos2D") : this(0d,0d,name) { }

        public void AddDiffVect(IPosition2D dVdt,bool getNewName = false) {
            AddDiffPropToParam(pX,dVdt.pX,true,getNewName);
            AddDiffPropToParam(pY,dVdt.pY,true,getNewName);
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

        public Position3D(double x, double y, double z, string name = "Pos"):base(x,y,name) {
            Z = z;
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
}
