using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RobotIM.Core;
using Sharp3D.Math.Core;
using Interpolator;

namespace RobotIM.Scene {
    public interface INoisePoint {
        double noiseDB { get; }
        double GetDBTo(Vector2D hearPoint, Room _r, bool prescision =false);
        Vector2D GetPos();
    }

    public class StaticNoisePoint : INoisePoint {
        private Vector2D _pos;
        public string Name { get; set; } = "StaticNoise";
        public string FullName => ToString();
        static double Zeros(double v) => v < 0 ? 0 : v;
        public StaticNoisePoint(Vector2D pos, double db) {
            _pos = pos;
            noiseDB = db;
        }
        private double _db;
        InterpXY dbInterp = new InterpXY();
        public double noiseDB {
            get { return _db; }
            set {
                _db = value;
                dbInterp.Clear();
                double d = 1;
                for (double i = 0; i <= _db+6; i += 6) {
                    dbInterp.Add(d, Zeros(_db - i));
                    d *= 2;
                }

            }
        }
        public double GetDBTo(Vector2D hearPoint, Room _r, bool prescision = false) {
            var d0 = _r.GetDistanceBetween(_pos, hearPoint, prescision);
            return GetDBTo(d0);
        }
        public double GetDBTo(double d0) {
            return dbInterp.GetV(d0);
        }

        public Vector2D GetPos() {
            return _pos;
        }
        public void SetPos(Vector2D pos) {
            _pos = pos;
        }
        public double X {
            get {
                return _pos.X;
            }
            set {
                _pos.X = value;
            }
        }
        public double Y{
            get {
                return _pos.Y;
            }
            set {
                _pos.Y= value;
            }
        }
        public override string ToString() {
            return $"({_pos.X:0.###};{_pos.Y:0.###}), {Name} = {_db} dB";
        }
    }

    public class IMMR : UnitWithStates, INoisePoint  {
        private Room _r;
        public IMMR(string Name, Room r,GameLoop Owner = null) : base(Name, Owner) {
            _r = r;
            noiseDB = 1;
            Configurate();
            InitMe();
        }
        private double _db;
        InterpXY dbInterp = new InterpXY();
        public double noiseDB {
            get { return _db; }
            set {
                _db = value;
                dbInterp.Clear();
                double d = 1;
                for (double i = 0; i <= _db+6; i+=6) {
                    dbInterp.Add(d, Zeros(_db - i));
                    d *= 2;
                }

            }
        }
        static double Zeros(double v) => v < 0 ? 0 : v;
        public double GetDBTo(Vector2D hearPoint, Room _r, bool prescision = false) {
            var d0 = _r.GetDistanceBetween(Pos, hearPoint, prescision);
            return GetDBTo(d0);
        }
        public double GetDBTo(double d0) {
            return dbInterp.GetV(d0);
        }

        public Vector2D GetPos() => Pos;

        #region States
        public void Configurate() {
            State = _1US;
        }


        UnitState _1US = new UnitState(null, nameof(_1US));
        void _1US_WTD(double t2) {
            if (t2 < 100)
                return;
            else if (noiseDB < 40) {
                noiseDB = 60;
            }
        }
        #endregion
    }
}
