using Sharp3D.Math.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Experiment {
    public interface ILaw:INamedChild {
        bool ApplyMe();
        bool IsFit();
        bool Applyed { get; }
    }

    public abstract class LawBase : ILaw {
        public bool Applyed { get; protected set; } = false;

        public string FullName {
            get {
                return Owner != null ? Owner.FullName + '.' + Name : Name;
            }
        }
        public string Name { get; set; }
        public IScnObj Owner { get; set; } = null;

        public abstract bool ApplyMe();

        public abstract bool IsFit();
    }

    public class NewtonLaw4MatPoint : LawBase {
        public IPosition3D pos { get; private set; } = null;
        public IMass mass { get; private set; } = null;
        public List<IForce3D> extForceList { get; set; }
        public List<IForce3D> innerForceList { get; set; }

        public NewtonLaw4MatPoint(IEnumerable<IForce3D> externForces) {
            Name = nameof(NewtonLaw4MatPoint);
            extForceList = new List<IForce3D>();
            innerForceList = new List<IForce3D>();
            if(externForces?.Count() > 0) {
                extForceList.AddRange(externForces);
            }
        }
        public NewtonLaw4MatPoint() : this(null) { }

        public override bool ApplyMe() {
            if(Applyed)
                return true;

            if(!IsFit())
                return false;

            var velocity = new Position3D("Vel");
            var aceleration = new Position3D("Aclr");
            pos.Owner.AddChild(velocity);
            pos.Owner.AddChild(aceleration);

            pos.AddDiffVect(velocity,false);
            velocity.AddDiffVect(aceleration,false);

            pos.Owner.SynchMeAfter +=
                t => {
                    var fSum = Vector3D.Zero;
                    foreach(var force in innerForceList) {
                        fSum += force.VGlob;
                    }
                    foreach(var force in extForceList) {
                        fSum += force.V;
                    }
                    if(mass.Value < Double.Epsilon)
                        throw new Exception("масса слишком мала");
                    aceleration.V = fSum / mass.Value;
                };



            return true;
        }

        public override bool IsFit() {
            if(Applyed)
                return true;

            if(Owner == null)
                return false;

            pos = (IPosition3D)Owner.Children.FirstOrDefault(ch => ch is IPosition3D);
            mass = (IMass)Owner.Children.FirstOrDefault(ch => ch is IMass);

            innerForceList.Clear();
            var innerF = Owner.Children.
                         Where(ch => ch is IForce3D).
                         Select(ch => ch as IForce3D);
            if(innerF.Count()>0)
                innerForceList.AddRange(innerF);

            if( pos == null  ||
                mass == null ||
                (innerForceList.Count == 0 && extForceList.Count == 0)) {

                mass = null;
                pos = null;
                return false;
            }
            return true;
        }
    }
}
