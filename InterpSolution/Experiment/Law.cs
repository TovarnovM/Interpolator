namespace Experiment {
    public interface ILaw : INamedChild {
        bool ApplyMe();
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
}
