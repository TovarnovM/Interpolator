using Microsoft.Research.Oslo;
using Sharp3D.Math.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeetingPro {
    public class InitConditions {
        public static (MT_pos pos, Vector vec, double time_end) GetInitCondition(Vector3D pos0, Vector3D trg_pos, double temperature) {
            double l0 = InterpAbstract(197, 151, temperature);
            double time0 = InterpAbstract(2.15, 1.22, temperature);
            double v0 = InterpAbstract(175, 237, temperature);
            double time_end = InterpAbstract(77, 47, temperature);
            double alpha0 = 5;

            var p0 = pos0 + (trg_pos - pos0).Norm * l0;
            var vel0 = (trg_pos - pos0).Norm * v0;

            var thetta0 = 90d - Math.Acos(vel0.Norm * Vector3D.YAxis) * 180d/ Math.PI;

            var vec0 = new Vector(temperature,
                time0,
                v0,
                alpha0,
                0,
                thetta0
                );
            var mt_pos0 = new MT_pos() {
                X = p0.X,
                Y = p0.Y,
                Z = p0.Z,
                V_x = vel0.X,
                V_y = vel0.Y,
                V_z = vel0.Z
            };

            return (mt_pos0, vec0, time_end);

        }

        public static (double vel, double x, double t) GetOneSol(double temperature) {
            var mis = new Mis();

            mis.Temperature = temperature;

            var tetta0 = 0 * Mis.RAD;
            var VecOX = new Vector3D(Math.Cos(tetta0), Math.Sin(tetta0), 0);
            mis.RotateOxThenNearOy(VecOX, Vector3D.YAxis);
            mis.Vel.Vec3D = VecOX;

            double t1 = mis.GetTrd1();

            var v0 = mis.Rebuild();
            var res = Ode.RK45(0, v0, mis.f, 0.001).SolveTo(t1).Last();

            return (mis.Vel.Vec3D.GetLength(), mis.Vec3D.GetLength(), t1);
        }

        static double InterpAbstract(double m50, double p50, double temper) {
            double t = (temper + 50d) / 100d;
            return m50 + t * (p50 - m50);
        }
    }
}
