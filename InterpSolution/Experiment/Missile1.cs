using Interpolator;
using Microsoft.Research.Oslo;
using RocketAero;
using Sharp3D.Math.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Experiment {
    public class Missile1:Position3D {
        private InterpXY _GInterp;
        public double G { get { return _simpleEnv ? 9.81 : _GInterp.GetV(Y / 1000); } }
        private InterpXY _SoundSpeedInterp;
        public double SoundSpeed { get { return _simpleEnv ? 340.1 : _SoundSpeedInterp.GetV(Y); } }
        private InterpXY _RoVozduhaInterp;
        public double RoVozduha { get { return _simpleEnv ? 1.225 : _RoVozduhaInterp.GetV(Y / 1000); } }

        private bool _simpleEnv = true;
        /// <summary>
        /// Использовать ли константные значения параметров среды g, ro , a
        /// </summary>
        public bool SimpleEnv {
            get { return _simpleEnv; }
            set {
                if(_simpleEnv && !value && _GInterp==null && _RoVozduhaInterp == null&& _SoundSpeedInterp == null) {
                    _GInterp = AeroGraphs.Instance.GetGraphCopy<InterpXY>("G");
                    _SoundSpeedInterp = AeroGraphs.Instance.GetGraphCopy<InterpXY>("Atmo_A");
                    _RoVozduhaInterp = AeroGraphs.Instance.GetGraphCopy<InterpXY>("Atmo_Ro");
                    _simpleEnv = value;
                }
                _simpleEnv = value;
            }
        }


        public MissileTarget Trg { get; set; }
        public MethNav MethNav { get; set; }
        public object OtherStuff4MwthNav { get; set; } = null;

        public double VelModul { get; set; }
        public IScnPrm pVelModul { get; set; }
        public Position3D Vel { get; set; }
        private void UpdateStuff(double t) {
            var vel = MethNav(t,Trg,this,VelModul,OtherStuff4MwthNav);
            Vel.Vec3D = vel;
            var f_summ = P
                       + vel.Norm * new Vector3D(0d,-Mass * G,0d)
                       - (Cx[VelModul/SoundSpeed] + (Cx_dop == null ? 0d: Cx_dop[t]))*RoVozduha*VelModul* VelModul*SMid*0.5;
            Accl = f_summ / Mass;
            dMass = -P / J1;
        }

        public double Mass { get; set; }
        public IScnPrm pMass { get; set; }
        public double dMass { get; set; }
        public IScnPrm pdMass { get; set; }

        private double _sMid;
        public double SMid {
            get { return _sMid; }
        }
        private double _d;

        public double D {
            get { return _d; }
            set {
                _d = value;
                _sMid = Math.PI * _d * _d * 0.25;
            }
        }

        public double MassPoleznNagr { get; set; }

        public double MassTopl { get; private set; }
        public double BettaDvig { get; private set; }
        public double J1 { get; private set; }

        public double V0 { get; private set; }

        public double T0 { get; private set; }
        public double P1 { get; private set; }
        public double Tau1 { get; private set; }
        public double P2 { get; private set; }
        public double Tau2 { get; private set; }

        public double P { get; set; }
        public IScnPrm pP { get; set; }
        public InterpXY Thrust { get; set; }

        public double Accl { get; set; }
        public IScnPrm pAccl { get; set; }

        public InterpXY Cx { get; set; }
        public InterpXY Cx_dop { get; set; }



        /// <summary>
        /// Необходимо передать список , в котором есть следующие ключи:
        /// "D">диаметр миделя
        /// "MassPolNagr">масса полезной нагрузки
        /// "J1">Единичный импульс
        /// "BettaDvig">Коэффициент массового совершенства двигтеля (больше единицы)
        /// "T0">Время включения двигателя
        /// "P1">Тяга на стартовом участке
        /// "Tau1">Продолжительность стартового участка
        /// "P2">Тяга на маршевом участке
        /// "Tau2">Продолжительность маршевого участка
        /// </summary>
        /// <param name="prmDict"></param>
        /// <returns></returns>
        public bool SetInitParams(IDictionary<string, double> prmDict) {
            try {                
                D = prmDict["D"];
                MassPoleznNagr = prmDict["MassPoleznNagr"];

                J1 = prmDict["J1"];

                V0 = prmDict["V0"];
                VelModul = V0;

                T0 = prmDict["T0"];
                P1 = prmDict["P1"];
                Tau1 = prmDict["Tau1"];
                P2 = prmDict["P2"];
                Tau2 = prmDict["Tau2"];
                MassTopl = (P1 * Tau1 + P2 * Tau2) / J1;

                BettaDvig = prmDict["BettaDvig"];

                Mass = MassTopl * BettaDvig + MassPoleznNagr;

                Thrust.Clear();
                Thrust.InterpType = InterpolType.itStep;
                Thrust.ET_left = ExtrapolType.etZero;
                Thrust.ET_right = ExtrapolType.etZero;
                Thrust.Add(T0,P1);
                Thrust.Add(T0 + Tau1,P2);
                Thrust.Add(T0 + Tau1 + Tau2,0d);
                

            }
            catch(Exception) {

                return false;
            }
            return true;
        }

        /// <summary>
        /// Траекторный анализ упрощенной модели 1
        /// </summary>
        public Missile1( MethNav methNav,
                         MissileTarget trg,
                         InterpXY cx,
                         InterpXY cx_dop = null) :base("Missile1") 
        {
            MethNav = methNav;
            Trg = trg;

            Vel = new Position3D("Vel");
            AddChild(Vel);
            AddDiffVect(Vel);
            pVelModul.MyDiff = pAccl;
            SynchMeAfter += UpdateStuff;

            pMass.MyDiff = pdMass;

            Thrust = new InterpXY();
            pP.SealInterp(Thrust);

            Cx = cx;
            Cx_dop = cx_dop;

            AddChild(trg);

            FlagDict.Add("VelOtric",VelOtric);
            FlagDict.Add("HitTarget",HitTarget);
        }

        public bool VelOtric(params SolPoint[] sp) {
            return VelModul < 0;
        }

        public bool HitTarget(params SolPoint[] sp) {
            var vector0 = sp[0].X;
            var vector1 = sp[0].X;
            var myVec0 = new Vector3D(vector0[pX.NumInVector],
                                      vector0[pY.NumInVector],
                                      vector0[pZ.NumInVector]);
            var trgVec0 = new Vector3D(vector0[Trg.pX.NumInVector],
                          vector0[Trg.pY.NumInVector],
                          vector0[Trg.pZ.NumInVector]);

            var myVec1 = new Vector3D(vector1[pX.NumInVector],
                                      vector1[pY.NumInVector],
                                      vector1[pZ.NumInVector]);
            var trgVec1 = new Vector3D(vector1[Trg.pX.NumInVector],
                                       vector1[Trg.pY.NumInVector],
                                       vector1[Trg.pZ.NumInVector]);

            return Vector3D.HitRadius(myVec0,myVec1,trgVec0,trgVec1,Trg.Radius) >= 0;
        }

        public override void Dispose() {
            base.Dispose();
            Thrust.Clear();
            Thrust = null;
        }
    }
}
