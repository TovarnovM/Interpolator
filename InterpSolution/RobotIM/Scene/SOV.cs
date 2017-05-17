using Sharp3D.Math.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Math;

namespace RobotIM.Scene {
    public class SOV {
        public UnitWithVision unit;
        public Room room;
        public double sighLength = 10, sighCloseLength = 0.05;
        double sangle, cosHalfAngle;
        public double SighAngle {
            get {
                return sangle;
            }
            set {
                sangle = value;
                cosHalfAngle = Cos(sangle * 0.5 * PI / 180d);
            }
        }
        public bool SeeYou(Vector2D who) {
            var visLine = who - unit.Pos;
            var visLineLength = visLine.GetLength();
            if (visLineLength > sighLength)
                return false;
            if (visLineLength < sighCloseLength)
                return true;

            var cos = visLine * unit.viewDir / unit.viewDir.GetLength() / visLineLength;
            if (cos < cosHalfAngle)
                return false;

            return !room.IsCrossWalls(who, unit.Pos);
        }
    }
}
