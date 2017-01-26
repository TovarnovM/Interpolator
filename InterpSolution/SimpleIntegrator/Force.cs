using Sharp3D.Math.Core;
using System.Linq;
using System;

namespace SimpleIntegrator {
    public interface IForceCenter {
        double Value { get; set; }
        IScnPrm pValue { get; set; }
        IPosition3D Direction { get; set; }
        IOrient3D SK { get; set; }
        Vector3D VecGlob { get; }
        Vector3D VecLoc { get; }
        Vector3D VecWorld{ get; }
    }


    public interface IForce : IForceCenter {
        IPosition3D FPoint { get; set; }
        Vector3D MomentGlob { get; }
        Vector3D MomentLoc { get; }
        Vector3D MomentWorld { get; }
    }

    public class ForceCenter : ScnObjDummy, IForceCenter, IScnObj {
        public double Value { get; set; }
        public IScnPrm pValue { get; set; } = null;
        public IPosition3D Direction { get; set; } = null;
        public IOrient3D SK { get; set; } = null;
        public Vector3D VecGlob {
            get {
                return
                    SK == null ?
                    VecLoc :
                    SK.MRot * VecLoc;
            }
        }
        public Vector3D VecLoc {
            get {
                var normDirect = Direction.Vec3D;
                normDirect.Normalize();
                return normDirect * Value;
            }
        }

        public ForceCenter(double value, IPosition3D dir, IOrient3D sk, string name = "ForceCntr") {
            Name = name;
            Value = value;
            SK = sk;
            Direction = dir == null ?
                new Position3D(1, 0, 0, "Direction") :
                dir;
            if (dir == null) {
                AddChild(Direction);
            } else if(dir.Owner == null){
                AddChild(Direction);
            }
                
            
        }
        public ForceCenter(double value = 1d) : this(value, null, null) { }

        public Vector3D VecWorld {
            get {
                return
                    SK == null ?
                    VecLoc :
                    Matrix4D.TransformDir(SK.WorldTransform,VecLoc);
            }
        }
    }

    public class Force : ForceCenter, IForce {
        public IPosition3D FPoint { get; set; }
        public Vector3D MomentWorld {
            get {
                return
                    SK == null ?
                    MomentLoc :
                    Matrix4D.TransformDir(SK.WorldTransform,MomentLoc);
            }
        }
        public Vector3D MomentGlob {
            get {
                return
                    SK == null ?
                    MomentLoc :
                    SK.MRot * MomentLoc;
            }
        }
        public Vector3D MomentLoc {
            get {
                return Vector3D.CrossProduct(FPoint.Vec3D, VecLoc);
            }
        }
        public Force(double value, IPosition3D dir, IPosition3D fpoint, IOrient3D sk,string name = "ForcePoint") : base(value, dir, sk,name) {
            FPoint = fpoint == null ?
                new Position3D(0, 0, 0, "FPoint") :
                fpoint;
            if (fpoint == null)
                AddChild(FPoint);
        }
        public Force(double value = 1d,string name = "ForcePoint") : this(value, null, null, null,name) { }

    }

}
