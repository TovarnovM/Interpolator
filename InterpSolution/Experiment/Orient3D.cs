using Sharp3D.Math.Core;

namespace Experiment {

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
        protected void ResetParamW() {
            RemoveParam("W");
            pX = new ScnPrm("W",this,X);
            pX.GetVal = t => q.W;
            pX.SetVal = val => q.W = val;
        }
        protected void ResetParamX() {
            RemoveParam("X");
            pX = new ScnPrm("X",this,X);
            pX.GetVal = t => q.X;
            pX.SetVal = val => q.X = val;
        }
        protected void ResetParamY() {
            RemoveParam("Y");
            pY = new ScnPrm("Y",this,Y);
            pY.GetVal = t => q.Y;
            pY.SetVal = val => q.Y = val;
        }
        protected void ResetParamZ() {
            RemoveParam("Z");
            pZ = new ScnPrm("Z",this,Z);
            pZ.GetVal = t => q.Z;
            pZ.SetVal = val => q.Z = val;
        }

        public override void Resetparams() {
            ResetParamW();
            ResetParamX();
            ResetParamY();
            ResetParamZ();
        }

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
        }

    }
}
