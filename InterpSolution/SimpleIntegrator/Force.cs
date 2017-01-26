using Sharp3D.Math.Core;
using System.Linq;
using System;

namespace SimpleIntegrator {
    #region OldForce
    //public interface IForceCenter {
    //    double Value { get; set; }
    //    IScnPrm pValue { get; set; }
    //    IPosition3D Direction { get; set; }
    //    IOrient3D SK { get; set; }
    //    Vector3D VecGlob { get; }
    //    Vector3D VecLoc { get; }
    //    Vector3D VecWorld{ get; }
    //}


    //public interface IForce : IForceCenter {
    //    IPosition3D FPoint { get; set; }
    //    Vector3D MomentGlob { get; }
    //    Vector3D MomentLoc { get; }
    //    Vector3D MomentWorld { get; }
    //}

    //public class ForceCenter : ScnObjDummy, IForceCenter, IScnObj {
    //    public double Value { get; set; }
    //    public IScnPrm pValue { get; set; } = null;
    //    public IPosition3D Direction { get; set; } = null;
    //    public IOrient3D SK { get; set; } = null;
    //    public Vector3D VecGlob {
    //        get {
    //            return
    //                SK == null ?
    //                VecLoc :
    //                SK.MRot * VecLoc;
    //        }
    //    }
    //    public Vector3D VecLoc {
    //        get {
    //            var normDirect = Direction.Vec3D;
    //            normDirect.Normalize();
    //            return normDirect * Value;
    //        }
    //    }

    //    public ForceCenter(double value, IPosition3D dir, IOrient3D sk, string name = "ForceCntr") {
    //        Name = name;
    //        Value = value;
    //        SK = sk;
    //        Direction = dir == null ?
    //            new Position3D(1, 0, 0, "Direction") :
    //            dir;
    //        if (dir == null) {
    //            AddChild(Direction);
    //        } else if(dir.Owner == null){
    //            AddChild(Direction);
    //        }


    //    }
    //    public ForceCenter(double value = 1d) : this(value, null, null) { }

    //    public Vector3D VecWorld {
    //        get {
    //            return
    //                SK == null ?
    //                VecLoc :
    //                Matrix4D.TransformDir(SK.WorldTransform,VecLoc);
    //        }
    //    }
    //}

    //public class Force : ForceCenter, IForce {
    //    public IPosition3D FPoint { get; set; }
    //    public Vector3D MomentWorld {
    //        get {
    //            return
    //                SK == null ?
    //                VecLoc :
    //                Matrix4D.TransformDir(SK.WorldTransform,MomentLoc);
    //        }
    //    }
    //    public Vector3D MomentGlob {
    //        get {
    //            return
    //                SK == null ?
    //                MomentLoc :
    //                SK.MRot * MomentLoc;
    //        }
    //    }
    //    public Vector3D MomentLoc {
    //        get {
    //            return Vector3D.CrossProduct(FPoint.Vec3D, VecLoc);
    //        }
    //    }
    //    public Force(double value, IPosition3D dir, IPosition3D fpoint, IOrient3D sk,string name = "ForcePoint") : base(value, dir, sk,name) {
    //        FPoint = fpoint == null ?
    //            new Position3D(0, 0, 0, "FPoint") :
    //            fpoint;
    //        if (fpoint == null)
    //            AddChild(FPoint);
    //    }
    //    public Force(double value = 1d,string name = "ForcePoint") : this(value, null, null, null,name) { }

    //}

    #endregion

    //=============================================================================================================
    //=== New Vision ==============================================================================================
    //=============================================================================================================

    public class RelativePoint: Position3D {
        public IOrient3D SK { get; set; } = null;
        public Vector3D Vec3D_World {
            get {
                return SK == null
                    ? Vec3D
                    : SK.WorldTransform * Vec3D;
            }
            set {
                Vec3D = SK == null
                    ? value
                    : SK.WorldTransform_1 * value;
            }
        }
        public Vector3D Vec3D_Dir_World {
            get {
                return SK == null
                    ? Vec3D
                    : SK.WorldTransformRot * Vec3D;
            }
            set {
                Vec3D = SK == null
                    ? value
                    : SK.WorldTransformRot_1 * value;
            }
        }

        public RelativePoint(Vector3D posVec, IOrient3D sk = null, string name = "relPoint") :this(posVec.X,posVec.Y,posVec.Z,sk,name) {
            
        }

        public RelativePoint(object x, object y, object z, IOrient3D sk = null,string name = "relPoint"):base(x,y,z,name) {
            SK = sk;
        }

    }

    public class Force : ScnObjDummy {
        public double Value { get; set; }
        public IScnPrm pValue { get; set; } = null;
        public RelativePoint Direction { get; set; } = null;
        public RelativePoint AppPoint { get; set; } = null;

        public Vector3D Vec3D_Dir {
            get {
                return Direction.Vec3D.Norm * Value;
            }
            set {
                Direction.Vec3D = value.Norm;
                Value = value.GetLength();
            }
        }

        public Vector3D Vec3D_Dir_World {
            get {
                return Direction.Vec3D_Dir_World.Norm * Value;
            }
            set {
                Direction.Vec3D_Dir_World = value.Norm;
                Value = value.GetLength();
            }
        }

        public Vector3D GetMoment_World(Vector3D worldPoint) {
            return AppPoint == null
                ? Vector3D.Zero
                : (AppPoint.Vec3D_World - worldPoint) & Vec3D_Dir_World;
        }

        public Vector3D GetMoment_Local(IOrient3D localSK) {
            return AppPoint == null
                ? Vector3D.Zero
                : localSK.WorldTransformRot_1 * ((AppPoint.Vec3D_World - localSK.Vec3D) & Vec3D_Dir_World);
        }

        void InitMe(double value,RelativePoint direction,RelativePoint appPoint = null) {
            Value = value;
            RemoveChild(Direction);
            Direction = direction;
            if(Direction != null)
                AddChild(Direction);

            RemoveChild(AppPoint);
            AppPoint = appPoint;
            if(AppPoint != null)
                AddChild(AppPoint);
        }

        public Force(double value, RelativePoint direction, RelativePoint appPoint = null) {
            InitMe(value,direction,appPoint);
        }

        public static Force GetForceCentered(Vector3D force, IOrient3D sk = null) {
            var val = force.GetLength();
            var dir = new RelativePoint(force,sk,"direction");
            return new Force(val,dir);
        }

        public static Force GetForceCentered(double value, Vector3D force,IOrient3D sk = null) {
            var val = value;
            var dir = new RelativePoint(force,sk,"direction");
            return new Force(val,dir);
        }

        public static Force GetMoment(double value,Vector3D force,IOrient3D sk = null) {
            var m = GetForceCentered(value,force,sk);
            m.Name = "moment";
            return m;
        }

        public static Force GetForce(Vector3D force, IOrient3D skDir, Vector3D appPoint, IOrient3D skAppPoint) {
            var val = force.GetLength();
            var dir = new RelativePoint(force,skDir,"direction");
            var appP = new RelativePoint(appPoint,skAppPoint,"appPoint");
            return new Force(val,dir,appP);
        }

    }
}
