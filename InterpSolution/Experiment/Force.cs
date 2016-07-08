using Sharp3D.Math.Core;
using System.Linq;

namespace Experiment {
    public interface IForceCenter {
        double Value { get; set; }
        IScnPrm pValue { get; set; }
        IPosition3D Direction { get; set; }
        IOrient3D SK { get; set; }
        Vector3D VecGlob { get; }
        Vector3D VecLoc { get; }
        bool StickToThis(IScnObj stickToThis);
    }


    public interface IForce : IForceCenter {
        IPosition3D FPoint { get; set; }
        Vector3D MomentGlob { get; }
        Vector3D MomentLoc { get; }
    }

    public class ForceCenter : ScnObjDummy, IForceCenter {
        private IScnObj _owner;
        public double Value { get; set; }
        public IScnPrm pValue { get; set; } = null;
        public IPosition3D Direction { get; set; } = null;
        public IOrient3D SK { get; set; } = null;
        public Vector3D VecGlob {
            get {
                return
                    SK == null ?
                    VecLoc :
                    SK.M_1 * VecLoc;
            }
        }
        public Vector3D VecLoc {
            get {
                var normDirect = Direction.Vec3D;
                normDirect.Normalize();
                return normDirect * Value;
            }
        }

        public ForceCenter(double value, IPosition3D dir, IOrient3D sk) {
            Value = value;
            SK = sk;
            Direction = dir == null ?
                new Position3D(1, 0, 0, "Direction") :
                dir;
            if (dir == null)
                AddChild(Direction);
        }
        public ForceCenter(double value = 1d) : this(value, null, null) { }

        public bool StickToThis(IScnObj stickToThis) {
            if (stickToThis == null)
                return false;

            var owner = stickToThis;
            do {
                SK = (IOrient3D)owner.Children.FirstOrDefault(ch => ch is IOrient3D);
                owner = owner.Owner;
            } while (owner != null || SK != null);
            return true;
        }
        public new IScnObj Owner {
            get {
                return _owner;
            }
            set {
                _owner = value;
                StickToThis(_owner);
            }
        }
    }

    public class Force : ForceCenter, IForce {
        public IPosition3D FPoint { get; set; }
        public Vector3D MomentGlob {
            get {
                return
                    SK == null ?
                    MomentLoc :
                    SK.M_1 * MomentLoc;
            }
        }
        public Vector3D MomentLoc {
            get {
                return Vector3D.CrossProduct(FPoint.Vec3D, VecLoc);
            }
        }
        public Force(double value, IPosition3D dir, IPosition3D fpoint, IOrient3D sk) : base(value, dir, sk) {
            FPoint = fpoint == null ?
                new Position3D(0, 0, 0, "FPoint") :
                fpoint;
            if (fpoint == null)
                AddChild(FPoint);
        }
        public Force(double value = 1d) : this(value, null, null, null) { }

    }










    public interface IForce3D : IPosition3D {
        IOrient3D SK { get; set; }
        IPosition3D FPoint { get; set; }
        Vector3D VGlob { get; }
        Vector3D GetMoment();
    }

    public interface IForce2D : IPosition2D {
        IOrient2D SK { get; set; }
        IPosition2D FPoint { get; set; }
        Vector2D VGlob { get; }
        double GetMoment();
    }

    public class Force2D : Position2D, IForce2D {
        private IPosition2D fulcrumPoint = null;
        /// <summary>
        /// Координаты вектора силы указаны в координатах вот этой Системы Координат
        /// Если null - то в глобальной СК
        /// </summary>
        public IOrient2D SK { get; set; } = null;
        public IPosition2D FPoint {
            get {
                return fulcrumPoint;
            }
            set {
                RemoveChild(fulcrumPoint);
                fulcrumPoint = value;
                fulcrumPoint.Name = "FcmPoint";
                AddChild(fulcrumPoint);
            }
        }

        public Vector2D VGlob {
            get {
                return SK?.Loc2Glob(Vec2D) ?? Vec2D;
            }
        }

        public Force2D(Vector2D posVec, string name = "Frc") : base(posVec.X, posVec.Y, name) { }
        public Force2D(string name = "Frc") : base(0d, 0d, name) { }

        public double GetMoment() {
            return fulcrumPoint == null
                 ? 0d
                 : Vector2D.KrossProduct(fulcrumPoint.Vec2D, Vec2D);
        }
    }

    public class Force3D : Position3D, IForce3D {
        private IPosition3D fulcrumPoint = null;
        /// <summary>
        /// Координаты вектора силы указаны в координатах вот этой Системы Координат
        /// Если null - то в глобальной СК
        /// </summary>
        public IOrient3D SK { get; set; } = null;
        public IPosition3D FPoint {
            get {
                return fulcrumPoint;
            }
            set {
                RemoveChild(fulcrumPoint);
                fulcrumPoint = value;
                fulcrumPoint.Name = "FcmPoint";
                AddChild(fulcrumPoint);
            }
        }

        public Vector3D VGlob {
            get {
                return SK?.Loc2Glob(Vec3D) ?? Vec3D;
            }
        }

        public Force3D(Vector3D posVec, string name = "Frc") : base(posVec.X, posVec.Y, posVec.Z, name) { }
        public Force3D(string name = "Frc") : base(0d, 0d, 0d, name) { }

        public Vector3D GetMoment() {
            return fulcrumPoint == null
                 ? Vector3D.Zero
                 : Vector3D.CrossProduct(fulcrumPoint.Vec3D, Vec3D);
        }
    }

}
