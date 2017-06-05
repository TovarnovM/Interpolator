using RobotIM.Core;
using Sharp3D.Math.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotIM.Scene {
    [Serializable]
    public class UnitXY : UnitBase {
        public double X { get => Pos.X; set => Pos.X = value; }
        public double Y { get => Pos.Y; set => Pos.Y = value; }
        public Vector2D Pos, VelDir;
        public double VelAbs { get; set; } = 1.0;
        public IWayPoints WayPoints { get; set; } 
        public UnitXY(string Name, GameLoop Owner = null) : base(Name, Owner) {
        }

        protected override void PerformUpdate(double toTime) {
            Move(toTime);
        }

        public void Move(double t2) {
            Move(UnitTime, t2);
        }

        public void Move(double t1, double t2) {
            if (WayPoints == null) {
                Pos += VelDir * VelAbs * (t2 - t1);
                return;
            }
                
            var currTrg = WayPoints.Current;
            var dt = t2 - t1;
            var ds = currTrg - Pos;
            var ds_length = ds.GetLength();
            var s_dt = VelAbs * dt;
            if (ds_length >= s_dt) {
                ds.Normalize();
                VelDir = ds;
                Pos += ds * s_dt;
                return;
            } else {              
                Pos = currTrg;
                if (WayPoints.MoveNext()) {
                    var dtReal = ds_length / VelAbs;
                    Move(t1 + dtReal, t2);
                }
            }

        }
    }


}
