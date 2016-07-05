using System;
using Sharp3D.Math.Core;

namespace Experiment {
    public interface IForce3D:IPosition3D {
        IOrient3D SK { get; set; }
        IPosition3D FPoint { get; set; }
        Vector3D VGlob { get; }
        Vector3D GetMoment();
    }
    public class Force: Position3D, IForce3D {
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
                return SK?.Loc2Glob(V) ?? V;
            }
        }

        public Force(Vector3D posVec,string name = "Frc") : base(posVec.X,posVec.Y,posVec.Z,name) { }
        public Force(string name = "Frc") : base(0d,0d,0d,name) { }

        public Vector3D GetMoment() {
            return fulcrumPoint == null
                 ? Vector3D.Zero
                 : Vector3D.CrossProduct(fulcrumPoint.V,V);
        }
    }
}
