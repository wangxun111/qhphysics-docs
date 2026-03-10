using System;

using UnityEngine;

using Mono.Simd;
using Mono.Simd.Math;

namespace QH.Physics {
    public class VerletBend : VerletConstraint {
        private Vector4f _frictionConstant4f;
        public Single RotationalStiffness;
        private Vector4f _displacementStiffness4f;
        private Vector4f _normalStiffness4f;
        private Vector3 p12;
        private Vector3 p21;
        private Single r12;
        private Single r12sqr;
        private Quaternion q12;
        private Quaternion q21;
        protected VerletMass vmass1;
        protected VerletMass vmass2;

        public Single FrictionConstant {
            get { return _frictionConstant4f.X; }
            set { _frictionConstant4f = new Vector4f(value); }
        }

        public Single DisplacementStiffness {
            get { return _displacementStiffness4f.X; }
            set { _displacementStiffness4f = new Vector4f(value); }
        }

        public Single NormalStiffness {
            get { return _normalStiffness4f.X; }
            set { _normalStiffness4f = new Vector4f(value); }
        }

        public VerletBend(VerletMass mass1, VerletMass mass2, Single friction, Single rotationalStiffness, Single displacementStiffness, Single normalStiffness) : base(mass1, mass2) {
            vmass1 = mass1;
            vmass2 = mass2;
            FrictionConstant = friction;
            RotationalStiffness = rotationalStiffness;
            DisplacementStiffness = displacementStiffness;
            NormalStiffness = normalStiffness;
            p21 = Quaternion.Inverse(base.Mass2.Rotation) * (base.Mass1.Position4f - base.Mass2.Position4f).AsVector3();
            p12 = Quaternion.Inverse(base.Mass1.Rotation) * (base.Mass2.Position4f - base.Mass1.Position4f).AsVector3();
            r12sqr = p12.sqrMagnitude;
            r12 = Mathf.Sqrt(r12sqr);
            q12 = base.Mass2.Rotation * Quaternion.Inverse(base.Mass1.Rotation);
            q21 = base.Mass1.Rotation * Quaternion.Inverse(base.Mass2.Rotation);
        }

        public override void SetMasses(MassObject mass1, MassObject mass2 = null) {
            if (mass1 != null) {
                base.Mass1 = mass1;
                vmass1 = mass1 as VerletMass;
            }

            if (mass2 != null) {
                base.Mass2 = mass2;
                vmass2 = mass2 as VerletMass;
            }
        }

        public override void Satisfy() {
            Vector3 fromDirection = base.Mass2.Rotation * p21;
            Vector3 fromDirection2 = base.Mass1.Rotation * p12;
            Vector3 toDirection = (base.Mass1.Position4f - base.Mass2.Position4f).AsVector3();
            Vector3 toDirection2 = (base.Mass2.Position4f - base.Mass1.Position4f).AsVector3();
            Quaternion quaternion = Quaternion.Slerp(Quaternion.identity, Quaternion.FromToRotation(fromDirection, toDirection), 0.5f);
            Quaternion quaternion2 = Quaternion.Slerp(Quaternion.identity, Quaternion.FromToRotation(fromDirection2, toDirection2), 0.5f);
            Quaternion rotation = base.Mass1.Rotation;
            if (!base.Mass2.IsKinematic) {
                base.Mass2.Rotation = Quaternion.Lerp(base.Mass2.Rotation, quaternion2 * (q12 * base.Mass1.Rotation), RotationalStiffness);
            }

            Vector4f position4f = base.Mass1.Position4f;
            if (!base.Mass2.IsKinematic) {
                base.Mass2.Position4f = Vector4fExtensions.Lerp(base.Mass2.Position4f, position4f + (base.Mass1.Rotation * p12).AsPhyVector(ref base.Mass2.Position4f), _displacementStiffness4f.X);
            }

            Vector4f vector4f = base.Mass2.Position4f - base.Mass1.Position4f;
            Single num = Vector4fExtensions.Dot(vector4f, vector4f);
            Single f = (r12sqr / (num + r12sqr) - 0.5f) * NormalStiffness;
            if (!base.Mass1.IsKinematic) {
                base.Mass1.Position4f -= vector4f * new Vector4f(f);
            }

            if (!base.Mass2.IsKinematic) {
                base.Mass2.Position4f += vector4f * new Vector4f(f);
            }
        }

        public override void Solve() {
            Vector4f vector4f = (vmass1.PositionDelta4f - vmass2.PositionDelta4f) * _frictionConstant4f;
            if (!base.Mass1.IsKinematic) {
                base.Mass1.Position4f -= vector4f;
            }

            if (!base.Mass2.IsKinematic) {
                base.Mass2.Position4f += vector4f;
            }
        }
    }
}