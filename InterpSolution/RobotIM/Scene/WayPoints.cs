using Sharp3D.Math.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotIM.Scene {
    public interface IWayPoints {
        Vector2D Current { get; }
        bool MoveNext();
    }
    public class WayPoints : IWayPoints{
        public enum RepeatMode  { noRepeat, repeat, upDown };
        public RepeatMode repMode = RepeatMode.noRepeat;

        int currentInd = 0, incr = 1;
        public Vector2D Current => points[currentInd];

        public bool MoveNext() {
            if(points.Count == 1) {
                return false;
            }
            switch (repMode) {
                case RepeatMode.noRepeat:
                    if(currentInd == points.Count-1) {
                        return false;
                    }
                    currentInd++;
                    break;
                case RepeatMode.repeat:
                    currentInd++;
                    if(currentInd == points.Count) {
                        currentInd = 0;
                    }
                    break;
                case RepeatMode.upDown:
                    currentInd += incr;
                    if (currentInd == points.Count || currentInd == -1) {
                        incr *= -1;
                        currentInd += 2*incr;
                    }                   
                    break;
                default:
                    break;
            }
            return true;
        }

        List<Vector2D> points;

        public WayPoints(IEnumerable<Vector2D> points) {
            this.points = new List<Vector2D>(points);
        }
    }
}
