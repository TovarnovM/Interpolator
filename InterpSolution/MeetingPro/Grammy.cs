using Microsoft.Research.Oslo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeetingPro {
    public class Grammy {
        public Vector vUp, vDown, vLeft, vRight, vCenter;
        public Vector vBegin;
        public int vecLength = 7;
        public Grammy() {
            vBegin = Vector.Zeros(vecLength);
            vUp = Vector.Zeros(vecLength);
            vDown = Vector.Zeros(vecLength);
            vLeft = Vector.Zeros(vecLength);
            vRight = Vector.Zeros(vecLength);
            vCenter = Vector.Zeros(vecLength);
        }
        public Vector ToOneVector() {
            var res = new double[vBegin.Length + vUp.Length*5];
            int i = 0;
            for (int j = 0; j < vBegin.Length; j++) {
                res[i] = vBegin[j];
                i++;
            }
            for (int j = 0; j < vUp.Length; j++) {
                res[i] = vUp[j];
                i++;
            }
            for (int j = 0; j < vDown.Length; j++) {
                res[i] = vDown[j];
                i++;
            }
            for (int j = 0; j < vLeft.Length; j++) {
                res[i] = vLeft[j];
                i++;
            }
            for (int j = 0; j < vRight.Length; j++) {
                res[i] = vRight[j];
                i++;
            }
            return new Vector(res);
        }
        public void FromOneVector(Vector vec) {
            int i = 0;
            for (int j = 0; j < vBegin.Length; j++) {
                 vBegin[j] = vec[i];
                i++;
            }
            for (int j = 0; j < vUp.Length; j++) {
                vUp[j] = vec[i];
                i++;
            }
            for (int j = 0; j < vDown.Length; j++) {
                vDown[j] = vec[i];
                i++;
            }
            for (int j = 0; j < vLeft.Length; j++) {
                vLeft[j] = vec[i];
                i++;
            }
            for (int j = 0; j < vRight.Length; j++) {
                vRight[j] = vec[i];
                i++;
            }
        }
    }
}
