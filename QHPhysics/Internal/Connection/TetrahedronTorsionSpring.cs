using System;
using Mono.Simd;
using Mono.Simd.Math;
using UnityEngine;

namespace QH.Physics
{
    public sealed class TetrahedronTorsionSpring : VerletConstraint
    {
        public float TorsionStiffness;

        public float TorsionFriction;

        public float BendStiffness;

        public float SpringLength;

        public VerletMass[] Tetrahedron;

        private VerletMass vmass1;

        private float torsion;

        private float torsionKahanAccum;

        private float lengthsqr;

        private float invMassSum;

        private Vector4f springFriction4f;

        private Vector4f bendFriction4f;

        private Vector3 initMass1Forward;

        private Vector4f prevMass1Forward;

        public float BendFriction {
            get { return bendFriction4f.X; }
            set { bendFriction4f = new Vector4f(value); }
        }

        public float SpringFriction {
            get { return springFriction4f.X; }
            set { springFriction4f = new Vector4f(value); }
        }

        public float Torsion {
            get { return torsion; }
            set { torsion = value; }
        }

        public TetrahedronTorsionSpring(VerletMass mass1, VerletMass[] t)
            : base(mass1, t[0]) {
            vmass1 = base.Mass1 as VerletMass;
            Tetrahedron = t;
        }

        public TetrahedronTorsionSpring(Simulation sim, TetrahedronTorsionSpring source)
            : base(sim.MassDict[source.Mass1.UID] as VerletMass, sim.MassDict[source.Mass2.UID] as VerletMass, source.UID) {
            Tetrahedron = new VerletMass[4];
            torsion = source.torsion;
            Sync(source);
            Vector3 vector = (Tetrahedron[1].Position + Tetrahedron[2].Position + Tetrahedron[3].Position) / 3f;
            Vector3 normalized = (Tetrahedron[1].Position - vector).normalized;
            initMass1Forward = Quaternion.Inverse(base.Mass1.Rotation) * normalized;
            prevMass1Forward = (base.Mass1.Rotation * Vector3.forward).AsPhyVector(ref prevMass1Forward);
        }

        public override void Sync(ConnectionBase source) {
            base.Sync(source);
            TetrahedronTorsionSpring tetrahedronTorsionSpring = source as TetrahedronTorsionSpring;
            for (int i = 0; i < Tetrahedron.Length; i++) {
                if (Tetrahedron[i] == null || Tetrahedron[i].UID != tetrahedronTorsionSpring.Tetrahedron[i].UID) {
                    Tetrahedron[i] = base.Mass1.Sim.MassDict[tetrahedronTorsionSpring.Tetrahedron[i].UID] as VerletMass;
                }
            }

            vmass1 = base.Mass1 as VerletMass;
            SpringLength = tetrahedronTorsionSpring.SpringLength;
            springFriction4f = tetrahedronTorsionSpring.springFriction4f;
            TorsionFriction = tetrahedronTorsionSpring.TorsionFriction;
            TorsionStiffness = tetrahedronTorsionSpring.TorsionStiffness;
            bendFriction4f = tetrahedronTorsionSpring.bendFriction4f;
            BendStiffness = tetrahedronTorsionSpring.BendStiffness;
            invMassSum = 1f / (vmass1.InvMassValue + Tetrahedron[0].InvMassValue);
            lengthsqr = SpringLength * SpringLength;
        }

        public override void Satisfy() {
            Vector4f vector4f = Tetrahedron[0].Position4f - vmass1.Position4f;
            float num = Vector4fExtensions.Dot(vector4f, vector4f);
            if (num > lengthsqr) {
                float num2 = (lengthsqr / (num + lengthsqr) - 0.5f) * invMassSum;
                if (!vmass1.IsKinematic) {
                    vmass1.Position4f -= vector4f * new Vector4f(num2 * vmass1.InvMassValue);
                }

                if (!Tetrahedron[0].IsKinematic) {
                    Tetrahedron[0].Position4f += vector4f * new Vector4f(num2 * Tetrahedron[0].InvMassValue);
                }
            }
        }

        public override void Solve() {
            Vector4f vector4f = Vector4fExtensions.onethird * (Tetrahedron[1].Position4f + Tetrahedron[2].Position4f + Tetrahedron[3].Position4f);
            Vector4f vector4f2 = (Tetrahedron[0].Position4f - vector4f).Normalized();
            Vector4f b = (Tetrahedron[1].PrevPosition4f - vector4f).Normalized();
            Vector4f repr = (Tetrahedron[1].Position4f - vector4f).Normalized();
            Vector4f a = (base.Mass1.Rotation * Vector3.forward).AsPhyVector(ref repr);
            Vector4f a2 = (base.Mass1.Rotation * Vector3.up).AsPhyVector(ref repr);
            float num = Mathf.Asin(Vector4fExtensions.Dot(vector4f2, Vector4fExtensions.Cross(repr, b)));
            float num2 = Mathf.Asin(Vector4fExtensions.Dot(Vector4fExtensions.up, Vector4fExtensions.Cross(a, prevMass1Forward)));
            float num3 = num - num2 - torsionKahanAccum;
            float num4 = torsion + num3;
            torsionKahanAccum = num4 - torsion - num3;
            torsion = Mathf.Clamp(num4, (float)Math.PI * -3f, (float)Math.PI * 3f);
            prevMass1Forward = a;
            Vector4f vector4f3 = new Vector4f((0f - torsion) * TorsionStiffness);
            for (int i = 1; i <= 3; i++) {
                Vector4f a3 = Tetrahedron[i].Position4f - vector4f;
                Vector4f vector4f4 = Vector4fExtensions.Cross(a3, vector4f2);
                Vector4f vector4f5 = vector4f4 * vector4f3;
                Vector4f vector4f6 = vector4f4 * new Vector4f((0f - TorsionFriction) * Vector4fExtensions.Dot(vector4f4, Tetrahedron[i].Velocity4f));
                Tetrahedron[i].ApplyForce(vector4f5 + vector4f6);
            }

            Vector4f vector4f7 = (vmass1.PositionDelta4f - Tetrahedron[0].PositionDelta4f) * springFriction4f;
            if (!vmass1.IsKinematic) {
                vmass1.Position4f -= vector4f7;
            }

            if (!Tetrahedron[0].IsKinematic) {
                Tetrahedron[0].Position4f += vector4f7;
            }

            Vector3 eulerAngles = base.Mass1.Rotation.eulerAngles;
            if (!Mathf.Approximately(BendStiffness, 0f)) {
                Quaternion quaternion = Quaternion.LookRotation(repr.AsVector3(), vector4f2.AsVector3());
                Vector3 eulerAngles2 = quaternion.eulerAngles;
                float num5 = Mathf.Clamp01(Vector4fExtensions.Dot(a2, vector4f2));
                Quaternion quaternion2 = Quaternion.Slerp(quaternion, Quaternion.Euler(eulerAngles2.x, eulerAngles.y, eulerAngles2.z), BendStiffness * num5);
                for (int j = 1; j <= 3; j++) {
                    Vector4f repr2 = Tetrahedron[j].Position4f - vector4f;
                    repr2 = (quaternion2 * (Quaternion.Inverse(quaternion) * repr2.AsVector3())).AsPhyVector(ref repr2);
                    Tetrahedron[j].Position4f = vector4f + repr2;
                }
            }

            if (Mathf.Approximately(BendFriction, 0f)) {
                return;
            }

            for (int k = 1; k <= 3; k++) {
                Vector4f vector4f8 = (Tetrahedron[k].PositionDelta4f - vmass1.PositionDelta4f) * bendFriction4f;
                if (!Tetrahedron[k].IsKinematic) {
                    Tetrahedron[k].Position4f -= vector4f7;
                }

                if (!vmass1.IsKinematic) {
                    vmass1.Position4f += vector4f7;
                }
            }
        }
    }
}