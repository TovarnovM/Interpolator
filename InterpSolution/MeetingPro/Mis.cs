using Interpolator;
using Microsoft.Research.Oslo;
using Sharp3D.Math.Core;
using SimpleIntegrator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static System.Math;

namespace MeetingPro {
    public class Mis: MaterialObjectNewton {
        #region просто функции/константы
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Sqr(double val) {
            return val * val;
        }
        public const double GRAD = 180d / PI;
        public const double RAD = PI / 180d;
        public double GetTemperature() {
            return temperature;
        }
        #endregion

        #region MMMMMM
        public void Synch_0(double t) {
            Set_Mass_Xs(t);
            Set_F_engine(t);
        }
        #region P, masssss

        public double X_m;
        public void Set_Mass_Xs(double t) {
            var deltam_rd = gr.deltam_rd.GetV(Temperature, t);
            var deltam_md = gr.deltam_md.GetV(Temperature, t);
            X_m = gr.x_m.GetV(deltam_rd, deltam_md)/1000;
            Mass.Value = gr.m.GetV(deltam_rd, deltam_md);
            Mass3D.Ix = gr.i_x.GetV(deltam_rd, deltam_md);
            Mass3D.Iy = gr.i_yz.GetV(deltam_rd, deltam_md);
            Mass3D.Iz = Mass3D.Iy;
        }
        public void Set_F_engine(double t) {
            var r_rd = 9.80665 * gr.r_rd.GetV(t);
            var r_md = 9.80665 * gr.r_md.GetV(t);
            F_engine.Value = r_rd + r_md;
        }
        #endregion

        public void Synch_1() {
            SetMach();
            Set_qS();
            Set_V_air();
            Set_fi_n();
            Set_alpha_c();
            Set_alpha_st();
            Set_alpha_i();
            
        }
     
        #region углы, скорости...
        /// <summary>
        /// скорость в СвСК
        /// </summary>
        public Vector3D V_air;
        public void Set_V_air() {
            V_air = -(WorldTransformRot_1 * Vel.Vec3D);
        }

        public double Mach;
        public void SetMach() {
            var a = gr.a;
            Mach = Vel.Vec3D.GetLength() / a.GetV(Temperature);
        }

        public double qS;
        public double Sm;
        public void Set_qS() {
            var ro_gr = gr.ro;
            var v = Vel.Vec3D.GetLength();
            qS = ro_gr.GetV(Temperature) * 0.5 * Sqr(v) * Sm;
        }

        /// <summary>
        /// стр 12 пространственные углы крена консолей крыла в СвСК 0..3
        /// </summary>
        public double[] fi_0;
        /// <summary>
        /// стр 13 аэродинамический угол крена
        /// </summary>
        public double fi_n;
        /// <summary>
        /// стр12 аэродинамический угол крена консолей крыла
        /// </summary>
        public double[] fi_st;
        public double[] fi_i;
        public void Set_fi_n() {
            var znam = Sqrt(Sqr(V_air.Y) + Sqr(V_air.Z));
            double sin_fi_n, cos_fi_n;
            if (znam > 1E-12) {
                sin_fi_n = V_air.Z / znam;
                cos_fi_n = -V_air.Y / znam;
            } else {
                sin_fi_n = 0;
                cos_fi_n = 1;
            }
            fi_n = sin_fi_n < 0 && cos_fi_n < 0
                ? -PI - Asin(sin_fi_n)
                : sin_fi_n > 0 && cos_fi_n < 0
                ? PI - Asin(sin_fi_n)
                : Asin(sin_fi_n);
            for (int i = 0; i < 4; i++) {
                fi_st[i] = fi_0[i] + fi_n;
                fi_i[i] = fi_st[i];
            }
        }

        /// <summary>
        /// стр 12 пространственный угол атани
        /// </summary>
        public double alpha_c;
        public void Set_alpha_c() {
            alpha_c = Atan2(Sqrt(Sqr(V_air.Y) + Sqr(V_air.Z)), V_air.X);
        }

        public double K_st_aa;
        public double K_st_fi;
        /// <summary>
        /// стр 12 пространственный угол атаки для i-ой констоли крыла 0..3
        /// </summary>
        public double[] alpha_st;
        public void Set_alpha_st() {
            for (int i = 0; i < 4; i++) {
                alpha_st[i] = Atan(Tan(alpha_c) * (K_st_aa * Cos(fi_st[i]) + K_st_fi * Sin(2 * fi_st[i]) * Sin(alpha_c)));
            }
        }

        /// <summary>
        /// ch 11 пространственный уг атанки консолей руля 0,,3
        /// в радианах
        /// </summary>
        public double[] alpha_i;
        public Vector delta_i_rad;
        public Matrix matr_lambda;
        public double K_aa;
        public double K_fi;
        public void Set_alpha_i() {
            var delta_alpha = matr_lambda * delta_i_rad;
            for (int i = 0; i < 4; i++) {
                alpha_i[i] = Atan(Tan(alpha_c) * (K_aa * Cos(fi_i[i]) + K_fi * Sin(2 * fi_i[i]) * Sin(alpha_c))) + delta_alpha[i];
            }
        }

        #endregion

        public void Synch_2() {
            Set_delta_C();
            Set_F_air();

            Set_m_air_dempf();
            Set_delta_m_air();
            Set_m_air();
            Set_M_air();
        }
        #region F_air
        public Vector3D delta_C;
        public double Get_delta_C_x() {
            var c_r_x = gr.c_r_x;
            double sum = 0d;
            for (int i = 0; i < 4; i++) {
                sum += c_r_x.GetV(alpha_i[i]* GRAD * Sign(delta_i_rad[i]), Mach, Abs(delta_i_rad[i] * GRAD));
            }
            return sum;
        }
        public void Set_delta_C() {
            var c_kr_y_i = gr.c_kr_y_i;
            var alpha_sk = gr.alpha_sk;
            var c_r_y_i = gr.c_r_y_i;

            var c_kr_y = 1d - 2 * K_st_aa * K_aa_k_st_aa * c_kr_y_i.GetV(1, Mach) * alpha_sk.GetV(alpha_c*GRAD, Mach);

            double sumY = 0;
            double sumZ = 0;
            for (int i = 0; i < 4; i++) {
                double mn1 = c_kr_y_i.GetV(Abs(alpha_st[i] * GRAD), Mach) * Sign(alpha_st[i]) * K_aa_k_st_aa;
                double mn2 = c_r_y_i.GetV(alpha_i[i] * GRAD * Sign(delta_i_rad[i]), Mach, Abs(delta_i_rad[i] * GRAD)) * Sign(delta_i_rad[i]) * K_aa_k_aa * c_kr_y;

                sumY += mn1 * Cos(fi_0[i]) + mn2 * Cos(fi_0[i]);
                sumZ += mn1 * Sin(fi_0[i]) + mn2 * Sin(fi_0[i]);
            }
            
            delta_C.X = Get_delta_C_x();
            delta_C.Y = sumY;
            delta_C.Z = sumZ;
        }
        public void Set_F_air() {
            var c_x = gr.c_x;
            var c_k_y = gr.c_k_y;

            var f_air = Vector3D.Zero;
            f_air.X = -(c_x.GetV(alpha_c * GRAD, Mach) + delta_C.X) * qS;
            f_air.Y = (c_k_y.GetV(alpha_c * GRAD, Mach) * Cos(fi_n) + delta_C.Y) * qS;
            f_air.Z = (-c_k_y.GetV(alpha_c * GRAD, Mach) * Sin(fi_n) + delta_C.Z) * qS;

            F_air.Vec3D_Dir = f_air;
        }
        #endregion

        #region M_air
        /// <summary>
        /// в радианах, как и дельты рулей
        /// </summary>
        public double delta_eler;
        public double L, l_m_x, Z_ct, Z_p;
        public Vector3D m_air_dempf;
        public void Set_m_air_dempf() {
            var m_z_gr = gr.m_omegaz_z_dempf;
            var m_x_gr = gr.m_omegax_x_dempf;
            m_air_dempf = Omega.Vec3D*qS;

            m_air_dempf.X *= m_x_gr.GetV(Mach) * Sqr(l_m_x);
            var m_yz_mn = m_z_gr.GetV(X_m / L, Mach) * Sqr(L);
            m_air_dempf.Y *= m_yz_mn;
            m_air_dempf.Z *= m_yz_mn;
            var vel = Vel.Vec3D.GetLength();
            if(vel > 1E-12) {
                m_air_dempf /= Vel.Vec3D.GetLength();
            } else {
                m_air_dempf = Vector3D.Zero;
            }
            
        }

        public Vector3D delta_m_air;
        public double b_kr, X_kr_pk, b_r, X_r_pk;
        public double K_aa_k_st_aa, K_aa_k_aa;
        public void Set_delta_m_air() {
            var c_kr_y_i = gr.c_kr_y_i;
            var c_r_y_i = gr.c_r_y_i;
            var x_kr_d = gr.x_kr_d;
            var alpha_sk = gr.alpha_sk;

            delta_m_air = Vector3D.Zero;
            for (int i = 0; i < 4; i++) {
                var c_kr = c_kr_y_i.GetV(Abs(alpha_st[i] * GRAD), Mach);
                var c_r = c_r_y_i.GetV(alpha_i[i] * GRAD * Sign(delta_i_rad[i]), Mach, Abs(delta_i_rad[i] * GRAD));

                delta_m_air.X += ( Z_ct * c_kr * Sign(alpha_st[i]) + Z_p * c_r * Sign(delta_i_rad[i])) / l_m_x;

                var x_kr_d_i = x_kr_d.GetV(Abs(alpha_st[i] * GRAD), Mach) * b_kr + X_kr_pk;
                var x_r_d_i = 0.3857 * b_r + X_r_pk;
                var d_m_i = -2d * K_st_aa * K_aa_k_st_aa * c_kr_y_i.GetV(1d, Mach) * alpha_sk.GetV(alpha_c * GRAD, Mach) * (X_m - x_r_d_i) + (X_m - x_r_d_i);

                delta_m_air.Y += (c_r * Sign(delta_i_rad[i]) * K_aa_k_aa * Sin(fi_0[i]) * d_m_i + c_kr * Sign(alpha_st[i]) * K_aa_k_st_aa * (X_m - x_kr_d_i) * Sin(fi_0[i])) / L;
                delta_m_air.Z += (c_r * Sign(delta_i_rad[i]) * K_aa_k_aa * Cos(fi_0[i]) * d_m_i + c_kr * Sign(alpha_st[i]) * K_aa_k_st_aa * (X_m - x_kr_d_i) * Cos(fi_0[i])) / L;
            }
        }

        public Vector3D m_air;
        public void Set_m_air() {
            var m_x0 = gr.m_x0.GetV(alpha_c * GRAD, Mach, Abs(delta_eler * GRAD));
            var c_k_y = gr.c_k_y.GetV(alpha_c * GRAD, Mach);
            var x_k_d = gr.x_k_d.GetV(alpha_c * GRAD, Mach);

            m_air.X = (-m_x0 * Sign(delta_eler) - delta_m_air.X) * qS * l_m_x;
            m_air.Y = (c_k_y * (X_m - x_k_d) * Sin(fi_n) / L - delta_m_air.Y) * qS * L;
            m_air.Z = (c_k_y * (X_m - x_k_d) * Cos(fi_n) / L - delta_m_air.Y) * qS * L;
        }

        public void Set_M_air() {
            var m = m_air + m_air_dempf;
            m.X += 0.00045 * alpha_c * GRAD * Sin(fi_n) * qS * l_m_x;
            M_air.Vec3D_Dir = m;
        }
        #endregion
        #endregion

        #region Creation
        //public MaterialObjectNewton body;
        public Force F_engine, F_air, M_air;
        private double temperature;

        public double Temperature {
            get { return temperature; }
            set {
                temperature = value;
                gr.r_md.Temperature = temperature;
                gr.r_rd.Temperature = temperature;
            }
        }

        public double GetTrd1() {
            return gr.r_rd.actT.Data.Last().Key;         
        }

        public Mis():this(Graphs.GetNew()) {

        }
        public Mis(Graphs graphs) :base("Mis") {
            this.gr = new Gr(graphs);
            InitConsts();
            CreateBodyAndForces();

            SynchMeBefore += SyncAct;
        }
        public void CreateBodyAndForces() {
            F_engine = Force.GetForceCentered(0, new Vector3D(1, 0, 0), this);
            F_engine.Name = "F_engine";
            AddForce(F_engine);

            F_air = Force.GetForceCentered(0, new Vector3D(-1, 0, 0), this);
            F_air.Name = "F_air";
            AddForce(F_air);

            M_air = Force.GetMoment(0, new Vector3D(-1, 0, 0), this);
            M_air.Name = "M_air";
            AddMoment(M_air);

            AddGForce(9.81);
        }
        public void InitConsts() {
            double l1 = -0.0648, l2 = -0.0304, l3 = -0.0648, l4 = 0;
            matr_lambda = new Matrix(new double[,] {
                { l4,l1,l2,l3},
                { l3,l4,l1,l2},
                { l2,l3,l4,l1},
                { l1,l2,l3,l4}
            });
            Sm = 0.031416;
            fi_0 = new double[] {
                (2d*0d+1)*PI*0.25-PI*0.5,
                (2d*1d+1)*PI*0.25-PI*0.5,
                (2d*2d+1)*PI*0.25-PI*0.5,
                (2d*3d+1)*PI*0.25-PI*0.5
            };
            fi_i = new double[4];
            fi_st = new double[4];
            K_st_aa = 1.2518;
            K_st_fi = 0.452;
            alpha_st = new double[4];
            alpha_i = new double[4];
            delta_i_rad = Vector.Zeros(4);
            K_aa = 1.4257;
            K_fi = 0.22;

            delta_eler = 0d;
            L = 1.953;
            l_m_x = 0.672;
            Z_ct = 0.2062;
            Z_p = 0.1495;
            b_kr = 0.308;
            X_kr_pk = 1.5365;
            b_r = 0.07;
            X_r_pk = 0.325;
            K_aa_k_st_aa = 1.348;
            K_aa_k_aa = 1.528;

            m_air = Vector3D.Zero;

            Temperature = 15;
            delta_m_air = Vector3D.Zero;
            m_air_dempf = Vector3D.Zero;
            delta_C = Vector3D.Zero;
        }

        public void SyncAct(double t) {
            Synch_0(t);
            Synch_1();
            Synch_2();
        }

        public Gr gr;

        public class Gr {
            public Interp3D m_x0, c_r_y_i, c_r_x;
            public Interp2D x_k_d, c_k_y, c_kr_y_i, x_kr_d, alpha_sk, m_omegaz_z_dempf, c_x, deltam_rd, deltam_md, x_m, m, i_x, i_yz;
            public Graphs.P_interp r_rd, r_md;
            public InterpXY m_omegax_x_dempf, ro, a;
            public Gr(Graphs graphs) {
                i_yz = graphs["i_yz"] as Interp2D;
                i_x = graphs["i_x"] as Interp2D;
                m = graphs["m"] as Interp2D;
                x_m = graphs["x_m"] as Interp2D;
                deltam_rd = graphs["deltam_rd"] as Interp2D;
                deltam_md = graphs["deltam_md"] as Interp2D;
                r_md = graphs["r_md"] as Graphs.P_interp;
                r_rd = graphs["r_rd"] as Graphs.P_interp;
                m_x0 = graphs["m_x0"] as Interp3D;
                c_k_y = graphs["c_k_y"] as Interp2D;
                x_k_d = graphs["x_k_d"] as Interp2D;
                c_kr_y_i = graphs["c_kr_y_i"] as Interp2D;
                c_r_y_i = graphs["c_r_y_i"] as Interp3D;
                x_kr_d = graphs["x_kr_d"] as Interp2D;
                alpha_sk = graphs["alpha_sk"] as Interp2D;
                m_omegax_x_dempf = graphs["m_omegax_x_dempf"] as InterpXY;
                m_omegaz_z_dempf = graphs["m_omegaz_z_dempf"] as Interp2D;
                c_x = graphs["c_x"] as Interp2D;
                c_r_x = graphs["c_r_x"] as Interp3D;
                ro = graphs["ro"] as InterpXY;
                a = graphs["a"] as InterpXY;
            } 
        }
        #endregion

        #region MyRegion
        public Data4Draw GetData4Draw() {
            return new Data4Draw() {
                T = this.TimeSynch,
                V = Vel.Vec3D.GetLength(),
                V_x = Vel.X,
                V_y = Vel.Y,
                V_z = Vel.Z,
                dV_x = Acc.X,
                dV_y = Acc.Y,
                dV_z = Acc.Z,
                X = this.X,
                Y = this.Y,
                Z = this.Z,
                Alpha = alpha_c,
                P = F_engine.Value,
                Om_x = Omega.X *GRAD,
                Om_y = Omega.Y * GRAD,
                Om_z = Omega.Z * GRAD,
                Kren = GetKren(),
                Thetta = 90d - Math.Acos(Vel.Vec3D.Norm * Vector3D.YAxis) * GRAD
        };
        }

        private double GetKren() {
            var z0 = (XAxis & Vector3D.YAxis).Norm;
            var y0 = (z0 & XAxis).Norm;
            var x = y0 * YAxis;
            var y = z0 * YAxis;
            return Atan2(y,x) * GRAD;
        }
        #endregion

        #region NDem
        public NDemVec GetNDemVec() {
            var res = new NDemVec();
            var v = Vel.Vec3D;
            res.V = v.GetLength();
            res.T = TimeSynch;
            res.Temperature = temperature;
            res.Thetta = 90d - Math.Acos(v.Norm * Vector3D.YAxis) * GRAD;

            var vyn = (v & Vector3D.YAxis).Norm;
            var ox_c = XAxis;
            var xvy = ox_c - (ox_c * vyn) * vyn;

            double signA = v.Norm * Vector3D.YAxis < xvy.Norm * Vector3D.YAxis
                ? 1d
                : -1d;


            var dot = xvy.Norm * v.Norm;
            if (dot > 1d) {
                dot = 1d;
            } else if (dot < -1d) {
                dot = -1d;
            }
            res.Alpha = signA * Math.Acos(dot) * GRAD;

            double signB = ox_c * vyn * signA > 0
                ? 1d
                : -1d;
            dot = xvy.Norm * ox_c;
            if(dot > 1d) {
                dot = 1d;
            } else if(dot < -1d) {
                dot = -1d;
            }
            res.Betta = signB * Math.Acos(dot) * GRAD;
            if (double.IsNaN(res.Betta)) {
                int gg = 77;
            }
            res.Kren = GetKren();
            res.Om_x = Omega.X;
            res.Om_y = Omega.Y;
            res.Om_z = Omega.Z;
            return res;

        }

        public void SetNDemVec(NDemVec res) {
            TimeSynch = res.T;
            Temperature = res.Temperature;
            var v0 = Vel.Vec3D;
            var v_xz = new Vector3D(v0.X, 0, v0.Z).Norm * res.V * Cos(res.Thetta * RAD);
            var v_y = Sqrt(res.V * res.V - v_xz.GetLengthSquared())*Sign(res.Thetta);
            var vel = new Vector3D(v_xz.X, v_y, v_xz.Z);
            Vel.Vec3D = vel;

            var axis_alpha = (vel & Vector3D.YAxis).Norm;
            var q_alpha = QuaternionD.FromAxisAngle(axis_alpha, res.Alpha * RAD);       
            var ox_alpha = q_alpha * vel.Norm;

            var axis_betta = (axis_alpha & ox_alpha).Norm;
            var q_betta = QuaternionD.FromAxisAngle(axis_betta, res.Betta * RAD);
            var ox_betta = q_betta * ox_alpha.Norm;

            SetPosition_LocalPoint_LocalFixed(Vector3D.XAxis, Vec3D + ox_betta, new Vector3D(0, 0, 0));

            var kr_n = (ox_betta & Vector3D.YAxis).Norm;
            var kr_0 = (kr_n & ox_betta).Norm;
            var q_kr = QuaternionD.FromAxisAngle(ox_betta.Norm, res.Kren * RAD);
            var oy_kr = q_kr * kr_0;

            SetPosition_LocalPoint_LocalFixed(Vector3D.YAxis, Vec3D + oy_kr, new Vector3D(-1, 0, 0), new Vector3D(1, 0, 0));

            Omega.X = res.Om_x;
            Omega.Y = res.Om_y;
            Omega.Z = res.Om_z;
        }
        public MT_pos GetMTPos() {
            var delta = new MT_pos() {
                X = this.X,
                Y = this.Y,
                Z = this.Z
            };

            var v = Vel.Vec3D;
            delta.V_x = v.X;
            delta.V_y = v.Y;
            delta.V_z = v.Z;

            return delta;
        }
        public void SetMTpos(MT_pos pos) {
            X = pos.X;
            Y = pos.Y;
            Z = pos.Z;
            
            Vel.X = pos.V_x;
            Vel.Y = pos.V_y;
            Vel.Z = pos.V_z;

            SynchQandM();
        }
        public void SetTimeSynch(double time) {
            TimeSynch = time;
        }
        #endregion
    }  
    
    public class Data4Draw {
        public double T { get; set; }
        public double V { get; set; }
        public double V_x { get; set; }
        public double V_y { get; set; }
        public double V_z { get; set; }
        public double dV_x { get; set; }
        public double dV_y { get; set; }
        public double dV_z { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public double Alpha { get; set; }
        public double Thetta { get; set; }
        public double P { get; set; }
        public double Om_x { get; set; }
        public double Om_y { get; set; }
        public double Om_z { get; set; }
        public double Kren { get; set; }
}

    public class NDemVec {
        public double V { get; set; }
        public double T { get; set; }
        public double Temperature { get; set; }
        public double Thetta { get; set; }
        public double Alpha { get; set; }
        public double Betta { get; set; }
        public double Kren { get; set; }
        public double Om_x { get; set; }
        public double Om_y { get; set; }
        public double Om_z { get; set; }
        public double XPos { get; set; } = 0; //-1.. +1
        public double YPos { get; set; } = 0;//-1.. +1
        public Vector ToVec() {
            return new Vector(V, T, Temperature, Thetta, Alpha, Betta, Kren, Om_x, Om_y, Om_z, XPos, YPos);
        }
        public void FromVec(Vector vec) {
            V = vec[0];
            T = vec[1];
            Temperature = vec[2];
            Thetta = vec[3];
            Alpha = vec[4];
            Betta = vec[5];
            Kren = vec[6];
            Om_x = vec[7];
            Om_y = vec[8];
            Om_z = vec[9];
            XPos = vec[10];
            YPos = vec[11];
        }
        public string[] GetHeader(string prefix = "") {
            return new string[] {
                prefix+"V",
                prefix+"T",
                prefix+"Temperature",
                prefix+"Thetta",
                prefix+"Alpha",
                prefix+"Betta",
                prefix+"Kren",
                prefix+"Om_x",
                prefix+"Om_y",
                prefix+"Om_z",
                prefix+"XPos",
                prefix+"YPos"
            };
        }
        public NDemVec() {

        }
        public NDemVec(Vector vec) {
            FromVec(vec);
        }
        public NDemVec(NDemVec copy) {
            FromVec(copy.ToVec());
        }
    }
    public class MT_pos {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public double V_x { get; set; }
        public double V_y { get; set; }
        public double V_z { get; set; }
        public MT_pos() {

        }
        public MT_pos(double v, double thetta) {
            V_x = v * Cos(thetta * Mis.RAD);
            V_y = v * Sin(thetta * Mis.RAD);
        }
        public MT_pos(Vector vec) {
            FromVec(vec);
        }
        public MT_pos(MT_pos copy) {
            FromVec(copy.ToVec());
        }
        public Vector ToVec() {
            return new Vector(X, Y, Z, V_x, V_y, V_z);
        }
        public void FromVec(Vector vec) {
            X = vec[0];
            Y = vec[1];
            Z = vec[2];
            V_x = vec[3];
            V_y = vec[4];
            V_z = vec[5];
        }
        public Vector3D GetPos0() {
            return new Vector3D(X, Y, Z);
        }
        public Vector3D GetVel0() {
            return new Vector3D(V_x, V_y, V_z);
        }

        public void SetPos0(Vector3D pos0) {
            X = pos0.X;
            Y = pos0.Y;
            Z = pos0.Z;
        }

        public void SetVel0(Vector3D vel0) {
            V_x = vel0.X;
            V_y = vel0.Y;
            V_z = vel0.Z;
        }
        public string[] GetHeader(string prefix = "") {
            return new string[] {
                prefix+"X",
                prefix+"Y",
                prefix+"Z",
                prefix+"V_x",
                prefix+"V_y",
                prefix+"V_z"
            };
        }
    }
}
