using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RobotIM.Core;
using Sharp3D.Math.Core;
using static System.Math;

namespace RobotIM.Scene {
    public class UnitWithVision : UnitXY {
        public Vector2D viewDir = new Vector2D(1,0);
        public double rotateSpeed = 90; //градусов в секунду
        public UnitWithVision(string Name, GameLoop Owner = null) : base(Name, Owner) {
        }
        protected override void PerformUpdate(double toTime) {
            base.PerformUpdate(toTime);
            SignToVel(toTime);
        }

        public void SignToVel(double toTime) {
            viewDir = RotateFromTo(viewDir, VelDir, rotateSpeed, toTime - UnitTime);
        }

        private int _rotDir =1;

        public int RotDir {
            get { return _rotDir; }
            set {
                _rotDir = value;
                _rotDir = _rotDir >= 0 ? 1 : -1;
            }
        }

        public void Rotate(double t2) {
            var angle = rotateSpeed * PI / 180 * (t2-UnitTime);
            var c = Cos(angle);
            var s = Sin(angle);

            viewDir = new Vector2D(viewDir.X * c - viewDir.Y * s, viewDir.Y * c + viewDir.X * s);
        }

        public static Vector2D RotateFromTo(Vector2D f, Vector2D t, double speed, double dt) {
            var f_l = f.GetLength();
            var t_l = t.GetLength();
            if(f_l < 1E-8 || t_l < 1E-8) {
                return f;
            }
            var angleSign = Sign(Vector2D.KrossProduct(f, t));
            var cosA = f * t / (f_l * t_l);
            var angle = Acos(cosA);
            var angleMax = speed * PI / 180 * dt;
            var angleReal = angle <= angleMax ? angleSign * angle : angleSign * angleMax;
            var c = Cos(angleReal);
            var s = Sin(angleReal);

            var answ = new Vector2D(f.X * c - f.Y * s, f.Y * c + f.X * s);
            return answ;
        }

    }
}
