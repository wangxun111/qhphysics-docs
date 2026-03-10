using System;

namespace QH.Physics {
    public abstract class VerletConstraint : ConnectionBase {
        public VerletConstraint(VerletMass mass1, VerletMass mass2, Int32 forceUID = -1) : base(mass1, mass2, forceUID) { }

        public abstract void Satisfy();
    }
}