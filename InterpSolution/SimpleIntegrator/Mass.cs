using Sharp3D.Math.Core;
using System;

namespace SimpleIntegrator {
    public interface IMassPoint:IScnObj {
        double Value { get; set; }
        IScnPrm pValue { get; set; }
    }

    public interface IMass3D {
        Matrix3D Tensor { get; }
        Matrix3D TensorInverse { get; }
        double Ix { get; set; }
        double Iy { get; set; }
        double Iz { get; set; }
        IScnPrm pIx { get; set; }
        IScnPrm pIy { get; set; }
        IScnPrm pIz { get; set; }
        void SynchTensor();
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
    }


    public class Mass3D : ScnObjDummy, IMass3D {
        private Matrix3D tensor = Matrix3D.Identity;

        public Matrix3D Tensor {
            get {
                return tensor;
            } }
        public double Ix {
            get {
                return tensor.M11;
            }
            set {
                tensor.M11 = value;
                SynchTensor();
            }
        }
        public double Iy {
            get {
                return tensor.M22;
            }
            set {
                tensor.M22 = value;
                SynchTensor();
            }
        }
        public double Iz {
            get {
                return tensor.M33;
            }
            set {
                tensor.M33 = value;
                SynchTensor();
            }
        }
        public IScnPrm pIx { get; set; }
        public IScnPrm pIy { get; set; }
        public IScnPrm pIz { get; set; }

        Matrix3D tensorInverse;
        public Matrix3D TensorInverse {
            get {
                return tensorInverse;
            }
        }

        public void SynchTensor() {
            tensorInverse = tensor.Inverse;
        }



        //protected void ResetParamIx() {
        //    RemoveParam("Ix");
        //    pX = new ScnPrm("Ix",this,X);
        //    pX.GetVal = t => X;
        //    pX.SetVal = val => X = val;
        //}
    }
}
