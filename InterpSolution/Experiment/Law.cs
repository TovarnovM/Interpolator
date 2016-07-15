using System;

namespace Experiment {
    public interface ILaw : INamedChild {
        bool ApplyMe();
        bool Applyed { get; }
    }

    public abstract class LawBase : NamedChild, ILaw {
        public bool Applyed { get; protected set; } = false;
        public bool ApplyMe() {
            if (Applyed)
                return true;

            if (!IsFitAndFull())
                return false;

            Applyed = ApplyMeAction();
            return Applyed;
        }
        public bool IsFitAndFull() {
            if (Applyed)
                return true;

            if (Owner == null)
                return false;

            return IsFitAndFullAction();
        }

        public abstract bool IsFitAndFullAction();

        public abstract bool ApplyMeAction();
    }

    public class NewtonLaw : LawBase {
        public override bool ApplyMeAction() {
            throw new NotImplementedException();
        }

        public override bool IsFitAndFullAction() {
            throw new NotImplementedException();
        }
    }
}
