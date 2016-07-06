using Sharp3D.Math.Core;
using System;

namespace Experiment {
    public interface IMassPoint:IScnObj {
        double Value { get; set; }
        IScnPrm pValue { get; set; }
    }

    public interface IMass3D: IMassPoint {
        Matrix3D Tensor { get; set; }
        double Ix { get; set; }
        double Iy { get; set; }
        double Iz { get; set; }
        IScnPrm pIx { get; set; }
        IScnPrm pIy { get; set; }
        IScnPrm pIz { get; set; }
    }

    public class MassPoint : ScnObjDummy, IMassPoint {
        public double Value { get; set; }
        public IScnPrm pValue { get; set; }

        public static string DefName = "Mass";
        public MassPoint() : this(1) {
        }

        public MassPoint(double value) {
            Name = DefName;
            Value = value;
            //ResetAllParams();
        }


        public class Mass3D : MassPoint, IMass3D {
            private Matrix3D tensor = Matrix3D.Identity;

            public Matrix3D Tensor { get; set; }
            public double Ix {
                get {
                    return tensor.M11;
                }
                set {
                    tensor.M11 = value;
                }
            }
            public double Iy {
                get {
                    return tensor.M22;
                }
                set {
                    tensor.M22 = value;
                }
            }
            public double Iz {
                get {
                    return tensor.M33;
                }
                set {
                    tensor.M33 = value;
                }
            }
            public IScnPrm pIx { get; set; }
            public IScnPrm pIy { get; set; }
            public IScnPrm pIz { get; set; }



            //protected void ResetParamIx() {
            //    RemoveParam("Ix");
            //    pX = new ScnPrm("Ix",this,X);
            //    pX.GetVal = t => X;
            //    pX.SetVal = val => X = val;
            //}
        }


    }


}
