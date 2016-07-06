using Sharp3D.Math.Core;

namespace Experiment {
    public interface IForce3D:IPosition3D {
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

        public Force2D(Vector2D posVec,string name = "Frc") : base(posVec.X,posVec.Y,name) { }
        public Force2D(string name = "Frc") : base(0d,0d,name) { }

        public double GetMoment() {
            return fulcrumPoint == null
                 ? 0d
                 : Vector2D.KrossProduct(fulcrumPoint.Vec2D,Vec2D);
        }
    }

    public class Force3D: Position3D, IForce3D {
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

        public Force3D(Vector3D posVec,string name = "Frc") : base(posVec.X,posVec.Y,posVec.Z,name) { }
        public Force3D(string name = "Frc") : base(0d,0d,0d,name) { }

        public Vector3D GetMoment() {
            return fulcrumPoint == null
                 ? Vector3D.Zero
                 : Vector3D.CrossProduct(fulcrumPoint.Vec3D,Vec3D);
        }
    }
}
