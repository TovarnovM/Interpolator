using Sharp3D.Math.Core;
using SimpleIntegrator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPH_2D {
    public class IsotropicGas : Particle2DBase {
        public double D;
        public double alpha; //для формулы 1.19

        public double M;

        public double Ro { get; set; }
        public IScnPrm pRo { get; set; }

        public double dRo { get; set; }
        public IScnPrm pdRo { get; set; }

        public double E { get; set; }
        public IScnPrm pE { get; set; }

        public double dE { get; set; }
        public IScnPrm pdE { get; set; }

        public double P { get; set; }
        public IScnPrm pP { get; set; }

        public Position2D dV { get; set; }

        public double k = 1.4;

        #region Constructor + Abstracts realiz
        public IsotropicGas(double d, double hmax) : base(hmax) {
            this.D = d;

            dV = new Position2D();
            AddChild(dV);
            Vel.AddDiffVect(dV,true);

            AddDiffPropToParam(pRo,pdRo);
            AddDiffPropToParam(pE,pdE);
        }

        public override int StuffCount {
            get {
                return 2;
            }
        }

        public override void DoStuff(int stuffIndex) {
            switch(stuffIndex) {
                case 0: {
                    SetP();
                    break;
                }
                case 1: {
                    FillDts();
                    break;
                }

                default:
                break;
            }
        }

        public void FillDts() {
            foreach(var neib in Neibs.Where(n=>GetDistTo(n) < hmax).Cast<IsotropicGas>()) {
                double h = alpha * (D + neib.D) * 0.5;
                double dw = dW_func(GetDistTo(neib),h);
                if(dw == 0d)
                    continue;
                double m_j = neib.M;
                double Ro_j = neib.Ro;
                double P_j = neib.P;
                double Cl_j = neib.GetCl(); //скорость звука
                double Cl_i = neib.GetCl();
                Vector2D Rji_norm = (neib.Vec2D - Vec2D).Norm;
                double U_Ri = Vel.Vec2D * Rji_norm;
                double U_Rj = neib.Vel.Vec2D * Rji_norm;
                double U_starRij = (U_Rj * Ro_j * Cl_j + U_Ri * Ro * Cl_i - P_j + P) / (Ro_j * Cl_j + Ro * Cl_i);// 1.20
                double P_starij = (P_j * Ro * Cl_i + P * Ro * Cl_j + Ro_j * Cl_j * Ro * Cl_i * (U_Rj - U_Rj)) / (Ro_j * Cl_j + Ro * Cl_i); //1.21

                double mn4VandE = 2d * m_j * P_starij / (Ro * Ro_j)* dw;

                dRo += -2d * m_j * Ro / Ro_j * dw * (U_Ri - U_starRij); //1.24
                dV.Vec2D += mn4VandE  * Rji_norm;//1.25
                dE += -0.5*mn4VandE * (U_Ri - U_starRij);//1.26
            }
        }

        public void SetP() {
            P = (k - 1d) * Ro * E;
            dRo = 0d;
            dE = 0d;
            dV.Vec2D = Vector2D.Zero;
        }
    
        /// <summary>
        /// Получить скорость звука
        /// </summary>
        /// <returns></returns>
        public double GetCl() {
            return 343.1;
        }
        #endregion
    }   
}
