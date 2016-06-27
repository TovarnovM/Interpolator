using Microsoft.Research.Oslo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sharp3D.Math.Core;

namespace Experiment {
    class Position: ScnObjDummy {
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
            pX = new ScnPrmConst("X",this,X);
            pX.GetVal = t => this.X;
            pX.SetVal =
                val => {
                    this.X = val;
                    return null;
                };
            AddParam(pX);
        }
        protected void AddParamY() {
            RemoveParam("Y");
            pY = new ScnPrmConst("Y",this,Y);
            pY.GetVal = t => this.Y;
            pY.SetVal =
                val => {
                    this.Y = val;
                    return null;
                };
            AddParam(pY);
        }
        protected void AddParamZ() {
            RemoveParam("Z");
            pZ = new ScnPrmConst("Z",this,Z);
            pZ.GetVal = t => this.Z;
            pZ.SetVal =
                val => {
                    this.Z = val;
                    return null;
                };
            AddParam(pZ);
        }

        public double X {
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
        public Position():this(0d,0d,0d,1d,"Pos") {

        }

        public void AddDiffVect(Position dVdt, bool getNewName = false) {
            AddDiffPropToParam(pX,dVdt.pX,true,getNewName);
            AddDiffPropToParam(pY,dVdt.pY,true,getNewName);
            AddDiffPropToParam(pZ,dVdt.pZ,true,getNewName);

        }

    }
}
