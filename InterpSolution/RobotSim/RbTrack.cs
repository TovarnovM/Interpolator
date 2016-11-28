using Sharp3D.Math.Core;
using SimpleIntegrator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RobotSim {
    public class RbTrack: MaterialObjectNewton {
        public double W, L, H;
        public Vector3D[] ConnP = new Vector3D[4];
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3D GetConnPVelWorld(int index) {
             return WorldTransformRot * (Omega.Vec3D & ConnP[index])+ Vel.Vec3D;        
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3D GetConnPWorld(int index) {
            return WorldTransform * ConnP[index];
        }

        public static RbTrack GetStandart(double w = 0.02, double l = 0.005,double h = 0.005, double shagConnL = 0.009,double connH = 0.00125, double mass = 0.0037) {
            var res = new RbTrack() {
                W = w,
                L = l,
                H = h
            };
            res.ConnP[0] = new Vector3D(0.5 * shagConnL,connH,-0.5 * w);
            res.ConnP[1] = new Vector3D(0.5 * shagConnL,connH,0.5 * w);
            res.ConnP[2] = new Vector3D(-0.5 * shagConnL,connH,-0.5 * w);
            res.ConnP[3] = new Vector3D(-0.5 * shagConnL,connH,0.5 * w);

            res.Mass.Value = mass;
            res.Mass.Ix = mass * (w * w + h * h) / 12d;
            res.Mass.Iy = mass * (w * w + l * l) / 12d;
            res.Mass.Iz = mass * (l * l + h * h) / 12d;
            return res;

        }
        public static RbTrack GetFlat(double w = 0.02,double l = 0.005,double mass = 0.0037) {
            return GetStandart(w,l,0,l,0,mass);
        }

    }

    public static class Phys3DHelper {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3D GetKMuForce(IVelPos3D me,IVelPos3D toThis, double k, double mu, double x0) {
            return GetKMuForce(me.Vec3D,me.Vel.Vec3D,toThis.Vec3D,toThis.Vel.Vec3D,k,mu,x0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3D GetKMuForce(Vector3D mePos, Vector3D meVel, Vector3D toThisPos,Vector3D toThisVel,double k,double mu,double x0) {
            var deltPos = toThisPos - mePos;
            double deltX = x0 - deltPos.GetLength();
            var deltPosNorm = deltPos.Norm;
            var Vpr_me = meVel * deltPosNorm;
            var Vpr_toThis = toThisVel * deltPosNorm;
            var VotnPr_me = Vpr_me - Vpr_toThis;

            return (-k * deltX - mu * VotnPr_me) * deltPosNorm;
        }
    }
}
