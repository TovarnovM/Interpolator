using Microsoft.Research.Oslo;
using Sharp3D.Math.Core;
using SimpleIntegrator;
using System.Collections.Generic;
using System.Text;

namespace MeetingPro {
    public class Grammy {
        public Vector vUp, vDown, vLeft, vRight, vCenter;
        //0 - temperat
        //1 - time
        //2 - vel
        //3 - alph
        //4 - bet
        //5 - thetta

        //6 - x
        //7 - y
        //8 - z
        //9 - Vx
        //10 - Vy
        //11 - Vz
        Vector FormVector(OneWay ow, Orient3D sk0) {
            var res = Vector.Zeros(vecLength);
            res[0] = ow.Vec1.Temperature;
            res[1] = ow.Vec1.T;
            res[2] = ow.Vec1.V;
            res[3] = ow.Vec1.Alpha;
            res[4] = ow.Vec1.Betta;
            res[5] = ow.Vec1.Thetta;
            var vec1 = sk0.WorldTransform_1 * ow.Pos1.GetPos0();
            res[6] = vec1.X;
            res[7] = vec1.Y;
            res[8] = vec1.Z;
            var vel1 = sk0.WorldTransformRot_1 * ow.Pos1.GetVel0();
            res[9] = vel1.X;
            res[10] = vel1.Y;
            res[11] = vel1.Z;
            for (int i = 0; i < res.Length; i++) {
                if (double.IsNaN(res[i])) {
                    int gg = 77;
                }
            }
            return res;
        }
        public GrammyPolygon[] polygons;
        public int polyCount;
        public Vector vBegin;
        public static int vecBeginLength = 6;
        public static int vecLength = 12;
        public Grammy() {
            vBegin = Vector.Zeros(vecBeginLength);
            vUp = Vector.Zeros(vecLength);
            vDown = Vector.Zeros(vecLength);
            vLeft = Vector.Zeros(vecLength);
            vRight = Vector.Zeros(vecLength);
            vCenter = Vector.Zeros(vecLength);
            IntiPolygons();
        }
        public Vector ToOneVector() {
            var res = new double[vecBeginLength + vecLength * 5];
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
            for (int j = 0; j < vCenter.Length; j++) {
                res[i] = vCenter[j];
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
            for (int j = 0; j < vCenter.Length; j++) {
                vCenter[j] = vec[i];
                i++;
            }
            IntiPolygons();
        }
        public void FromOneWayList(List<OneWay> list) {
            var uniq = list
                .AddCoord(180,180)
                .Uniquest();

            var sk0 = new Orient3D();

            var ow0 = uniq[0].ow;
            var pos0 = ow0.Pos0;

            sk0.SetPosition_LocalPoint_LocalMoveToIt_LocalFixed(Vector3D.XAxis, new Vector3D(pos0.V_x, 0, pos0.V_z), -Vector3D.YAxis, Vector3D.YAxis);
            sk0.Vec3D = pos0.GetPos0();
            sk0.SynchQandM();

            vBegin[0] = ow0.Vec0.Temperature;
            vBegin[1] = ow0.Vec0.T;
            vBegin[2] = ow0.Vec0.V;
            vBegin[3] = ow0.Vec0.Alpha;
            vBegin[4] = ow0.Vec0.Betta;
            vBegin[5] = ow0.Vec0.Thetta;

            vUp = FormVector(uniq.Find(tp => tp.ow.GetPos().EqualsApprox(new Vector2D(0, 1))).ow, sk0);
            vLeft = FormVector(uniq.Find(tp => tp.ow.GetPos().EqualsApprox(new Vector2D(-1, 0))).ow, sk0);
            vRight = FormVector(uniq.Find(tp => tp.ow.GetPos().EqualsApprox(new Vector2D(1, 0))).ow, sk0);
            vDown = FormVector(uniq.Find(tp => tp.ow.GetPos().EqualsApprox(new Vector2D(0, -1))).ow, sk0);
            vCenter = FormVector(uniq.Find(tp => tp.ow.GetPos().EqualsApprox(new Vector2D(0, 0))).ow, sk0);
            IntiPolygons();
        }
        public void IntiPolygons() {
            polygons = new GrammyPolygon[4];
            polygons[0] = new GrammyPolygon(vCenter, vUp, vLeft);
            polygons[1] = new GrammyPolygon(vCenter, vLeft, vDown);
            polygons[2] = new GrammyPolygon(vCenter, vDown, vRight);
            polygons[3] = new GrammyPolygon(vCenter, vRight, vUp);
            polyCount = 4;
        }

        public double Temperature => vBegin[0];
        public double T => vBegin[1];
        public double V => vBegin[2];
        public double Alpha => vBegin[3];
        public double Betta => vBegin[4];
        public double Thetta => vBegin[5];

        public Vector PolygonsIntercept(Vector3D ray_p, Vector3D ray_dir) {
            double minDist = 1E10;
            Vector3D minDist_p = new Vector3D(0, 0, 0); ;
            int min_dist_index = 0;
            for (int i = 0; i < polyCount; i++) {
                Vector3D minDist_i = new Vector3D(0,0,0);
                double dist_i = 0d;
                if (polygons[i].IsCross(ray_p, ray_dir, ref minDist_i, ref dist_i)) {
                    minDist_p = minDist_i;
                    min_dist_index = i;
                    break;
                } 
                if(dist_i < minDist) {
                    minDist_p = minDist_i;
                    minDist = dist_i;
                    min_dist_index = i;
                }
            }
            return polygons[min_dist_index].InterpV(minDist_p);
        }
        public static MT_pos PosFromVec(Vector vec) {
            return new MT_pos() {
                X = vec[6],
                Y = vec[7],
                Z = vec[8],
                V_x = vec[9],
                V_y = vec[10],
                V_z = vec[11]
            };
        } 

        public static Vector BeginVecFromVec(Vector interpVec) {
            return interpVec[0, vecBeginLength - 1];
        }

    }
}
