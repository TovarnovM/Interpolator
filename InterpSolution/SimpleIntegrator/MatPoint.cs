using Sharp3D.Math.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SimpleIntegrator {
    public interface IVelPos3D: IPosition3D {
        IPosition3D Vel { get; set; }
    }

    public interface IMaterialPoint : IOrient3D {
        IPosition3D Vel { get; set; }
        IPosition3D Acc { get; set; }
        IMassPoint Mass { get; set; }
        List<Force> Forces { get; set; }
        List<Force> ForcesNegative { get; set; }
        void AddForce(Force force);
        void AddForceNegative(Force force);
        Vector3D GetImpulse();
        double GetEnergy();
        void AddGForce(Vector3D dir,double g = 9.8);

    }
    /// <summary>
    /// X, y, z, - желательно, чтобы были в WORLD СК
    /// </summary>
    public class MaterialPointNewton : Orient3D, IMaterialPoint, IVelPos3D {
        public IPosition3D Vel { get; set; }
        public IPosition3D Acc { get; set; }
        public IMassPoint Mass { get; set; }
        public List<Force> Forces { get; set; }
        public List<Force> ForcesNegative { get; set; }
        const string DEFNAME = "MatPoint";
        public MaterialPointNewton(object x,object y,object z,string name = DEFNAME):base(x,y,z,name) {
            Vel = new Position3D("Vel");
            Acc = new Position3D("Acc");
            Mass = Mass == null ? new MassPoint(): Mass;
            Forces = new List<Force>();
            ForcesNegative = new List<Force>();
            AddChild(Vel);
            AddChild(Acc);
            AddChild(Mass);

            AddDiffVect(Vel,false);
            Vel.AddDiffVect(Acc,false);
            SynchMeAfter += NewtonLaw;
        }

        public MaterialPointNewton(Vector3D posVec,string name = DEFNAME) : this(posVec.X,posVec.Y,posVec.Z,name) { }
        public MaterialPointNewton(string name = DEFNAME) : this(0d,0d,0d,name) { }

        public virtual void NewtonLaw(double t) {
            var fsummWorld = ForceWorldSumm(t);
            UpdateAcc(fsummWorld);
            
        }

        public Vector3D ForceWorldSumm(double t) {
            Vector3D fsumm = Vector3D.Zero;
            foreach(var force in Forces) {
                fsumm += force.Vec3D_Dir_World;
            }
            foreach(var force in ForcesNegative) {
                fsumm -= force.Vec3D_Dir_World * force.ValueMultyplyer4Negative;
            }
            return fsumm;
        }

        public void UpdateAcc(Vector3D forceWorldSumm) {
            Acc.Vec3D = forceWorldSumm / Mass.Value;
        }

        public virtual void AddForce(Force force) {
            if(force.Owner == null)
                AddChild(force);
            Forces.Add(force);           
        }

        public virtual Vector3D GetImpulse() {
            return Mass.Value * Vel.Vec3D;
        }

        public virtual double GetEnergy() {
            var vel =  Vel.Vec3D.GetLength();
            return Mass.Value * vel * vel;
        }

        public void AddForceNegative(Force force) {
            if(force.Owner == null)
                AddChild(force);
            ForcesNegative.Add(force);
        }

        public void AddGForce(Vector3D dir, double g = 9.8) {
            var f = new ForceG(this,dir,g);
            AddForce(f);
        }
        public void AddGForce() {
            AddGForce(new Vector3D(0,-1,0));
        }
    }

    public class ForceG : Force {
        IMaterialPoint who;
        public double g;
        public ForceG(IMaterialPoint who, Vector3D dir, double g =9.8) : base(who.Mass.Value*g,new RelativePoint(dir),null) {
            this.who = who;
            this.g = g;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SynchAction(double t) {
            Value = who.Mass.Value * g;
        }
    }

    /// <summary>
    /// Угловые ускорения и скорости беруться в СВЯЗАННОЙ СК
    /// </summary>
    public interface IMaterialObject : IMaterialPoint {
        List<Force> Moments { get; set; }
        List<Force> MomentsNegative { get; set; }
        IPosition3D Omega { get; set; }
        IPosition3D Eps { get; set; }
        IMass3D Mass3D { get; set; }
        void AddMoment(Force moment);
        void AddMomentNegative(Force moment);
        Vector3D GetL();
        double GetRotEnergy();
        Vector3D GetVelWorld(Vector3D pointLocal);
    }

    public class MaterialObjectNewton : MaterialPointNewton, IMaterialObject, IVelPos3D {
        public IMass3D Mass3D { get; set; } = new Mass3D();
        public IPosition3D Omega { get; set; } = new Position3D("Omega");
        public IPosition3D Eps { get; set; } = new Position3D("Eps");
        public List<Force> Moments { get; set; }= new List<Force>();
        public List<Force> MomentsNegative { get; set; } = new List<Force>();
        const string DEFNAME = "MatObj";
        public MaterialObjectNewton(object x,object y,object z,string name = DEFNAME) : base(x,y,z,name) {
            AddChild(Omega);
            AddChild(Eps);

            Omega.AddDiffVect(Eps,false);
            AddDiffPropToParam(pQw,pdQWdt);
            AddDiffPropToParam(pQx,pdQXdt);
            AddDiffPropToParam(pQy,pdQYdt);
            AddDiffPropToParam(pQz,pdQZdt);

            SynchMeAfter = NewtonLaw3D;
        }

        public MaterialObjectNewton(Vector3D posVec,string name = DEFNAME) : this(posVec.X,posVec.Y,posVec.Z,name) { }
        public MaterialObjectNewton(string name = DEFNAME) : this(0d,0d,0d,name) { }

        public void NewtonLaw3D(double t) {
            var fsummWorld = ForceWorldSumm(t);
            UpdateAcc(fsummWorld);
            var momSumLocal = WorldTransformRot_1 * GetMomentsWorldSum(t);
            UpdatedQdt(momSumLocal);
        }

        public Vector3D GetMomentsWorldSum(double t) {
            var momSum = Vector3D.Zero;
            foreach(var mom in Moments) {
                momSum += mom.Vec3D_Dir_World;
            }
            foreach(var mom in MomentsNegative) {
                momSum -= mom.Vec3D_Dir_World * mom.ValueMultyplyer4Negative;
            }
            foreach(var force in Forces) {
                momSum += force.GetMoment_World(Vec3D);
            }
            foreach(var force in ForcesNegative) {
                momSum -= force.GetMoment_World(Vec3D) * force.ValueMultyplyer4Negative;
            }
            return momSum;
        }
        public void UpdatedQdt(Vector3D momSumLocal) {
            var om = Omega.Vec3D;
            Eps.Vec3D = Mass3D.TensorInverse * (momSumLocal - Vector3D.CrossProduct(om,Mass3D.Tensor * om));
            dQWdt = -0.5 * (om.X * Qx + om.Y * Qy + om.Z * Qz);
            dQXdt = 0.5 * (om.X * Qw - om.Y * Qz + om.Z * Qy);
            dQYdt = 0.5 * (om.Y * Qw - om.Z * Qx + om.X * Qz);
            dQZdt = 0.5 * (om.Z * Qw - om.X * Qy + om.Y * Qx);
        }

        public virtual void AddMoment(Force moment) {
            if(moment.Owner == null)
                AddChild(moment);
            Moments.Add(moment);
        }
        public IScnPrm pdQWdt { get; set; }
        public IScnPrm pdQXdt { get; set; }
        public IScnPrm pdQYdt { get; set; }
        public IScnPrm pdQZdt { get; set; }
        public double dQWdt { get; set; }
        public double dQXdt { get; set; }
        public double dQYdt { get; set; }
        public double dQZdt { get; set; }

        public override double GetEnergy() {
            return GetRotEnergy() * base.GetEnergy();
        }

        public Vector3D GetL() {
            return Mass3D.Tensor * Omega.Vec3D;
        }

        public double GetRotEnergy() {
            var om = Omega.Vec3D;
            return 0.5*om * (Mass3D.Tensor * om);
        }

        public Vector3D GetVelWorld(Vector3D pointLocal) {
            return WorldTransformRot * (Omega.Vec3D & pointLocal) + Vel.Vec3D;
        }

        public Vector3D GetVelLocal(Vector3D pointLocal) {
            return (Omega.Vec3D & pointLocal);
        }

        public void AddMomentNegative(Force moment) {
            if(moment.Owner == null)
                AddChild(moment);
            MomentsNegative.Add(moment);
        }
    }
}
