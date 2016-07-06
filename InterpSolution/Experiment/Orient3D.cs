using System;
using Sharp3D.Math.Core;

namespace Experiment {

    public interface IOrient2D: IScnObj {
        Vector2D Loc2Glob(Vector2D loc);
        Vector2D Glob2Loc(Vector2D wrld);
        Matrix2D M { get; }
        Matrix2D M_1 { get; }
        double AngleX { get; set; }
        IScnPrm pAngleX { get; set; }
    }

    public interface IOrient3D:IScnObj {
        Vector3D Loc2Glob(Vector3D loc);
        Vector3D Glob2Loc(Vector3D wrld);

        /// <summary>
        /// матрица перехода из Глобальной СК в локальную СК
        /// </summary>
        Matrix3D M { get; }
        /// <summary>
        /// матрица перехода из Локальнойй СК в глобальнуюСК
        /// </summary>
        Matrix3D M_1 { get; }
        QuaternionD Q { get; set; }

        double W { get; set; }
        double X { get; set; }
        double Y { get; set; }
        double Z { get; set; }
        
        IScnPrm pW { get; set; }
        IScnPrm pX { get; set; }
        IScnPrm pY { get; set; }
        IScnPrm pZ { get; set; }
    }

    public class Orient2D : ScnObjDummy, IOrient2D {
        private Matrix2D m = Matrix2D.Identity;
        private Matrix2D m_1 = Matrix2D.Identity;
        private double angleX = 0d;

        public Vector2D Loc2Glob(Vector2D loc) {
            return m * loc;
        }

        public Vector2D Glob2Loc(Vector2D wrld) {
            return m_1 * wrld;
        }

        public Matrix2D M { get { return m; } }
        public Matrix2D M_1 { get { return m_1; } }
        /// <summary>
        /// от -PI до PI в итоге)
        /// </summary>
        public double AngleX {
            get { return angleX; }
            set {
                angleX = value % MathFunctions.PI;
                SynchAngleAndM();
            }
        }

        public IScnPrm pAngleX { get; set; }

        private void SynchAngleAndM() {
            var sin = System.Math.Sin(angleX);
            var cos = System.Math.Cos(angleX);
            m.M11 = cos;   m.M12 = -sin;
            m.M21 = sin;   m.M22 = cos;

            m.M11 = cos;   m.M12 = sin;
            m.M21 = -sin;   m.M22 = cos;
        }

        protected void MySynchMeBefore(double t) {
            SynchAngleAndM();
        }

        public Orient2D() {
            SynchMeBefore = new System.Action<double>(MySynchMeBefore);
            //ResetAllParams();
        }

    }

    public class Orient3D : ScnObjDummy, IOrient3D {
        private Matrix3D m = Matrix3D.Identity;
        private Matrix3D m_1 = Matrix3D.Identity;
        private QuaternionD q = QuaternionD.Identity;

        public Vector3D Loc2Glob(Vector3D loc) {
            return m*loc;
        }

        public Vector3D Glob2Loc(Vector3D wrld) {
            return m_1 * wrld;
        }

        public Matrix3D M { get { return m; } }
        public Matrix3D M_1 { get { return m_1; } }
        public QuaternionD Q {
            get { return q; }
            set {
                q = value;
                SynchQandM();
            }
        }
        public double W {
            get {
                return q.W;
            }
            set {
                q.W = value;
            }
        }
        public double X {
            get {
                return q.X;
            }
            set {
                q.X = value;
            }
        }
        public double Y {
            get {
                return q.Y;
            }
            set {
                q.Y = value;
            }
        }
        public double Z {
            get {
                return q.Z;
            }
            set {
                q.Z = value;
            }
        }
        public IScnPrm pW { get; set; }
        public IScnPrm pX { get; set; }
        public IScnPrm pY { get; set; }
        public IScnPrm pZ { get; set; }

        private void SynchQandM() {
            q.Normalize();
            m = QuaternionD.QuaternionToMatrix3D(q);
            m_1 = m;
            m_1.Transpose();
        }

        protected void MySynchMeBefore(double t) {
            SynchQandM();
        }

        public Orient3D() {
            SynchMeBefore = new System.Action<double>(MySynchMeBefore);
            //ResetAllParams();
        }

    }
}
