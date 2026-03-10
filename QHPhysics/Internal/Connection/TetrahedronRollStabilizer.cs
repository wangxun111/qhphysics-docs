using System;
using Mono.Simd;
using Mono.Simd.Math;
using UnityEngine;

namespace QH.Physics {
    public sealed class TetrahedronRollStabilizer : VerletConstraint {
        public const Single StiffnessTimeConst = 1f;
        public VerletMass[] Tetrahedron;
        public Single BaseStiffness;
        public Single StiffnessMultiplier = 1f;
        private Vector4f target4f;

        public Vector3 TargetDirection {
            get {
                return target4f.AsVector3();
            }
            set {
                target4f = value.AsPhyVector(ref target4f);
            }
        }

        public TetrahedronRollStabilizer(VerletMass[] t, Single stiffness, Vector3 targetDirection) : base(t[0], t[1]) {
            Tetrahedron = t;
            BaseStiffness = stiffness;
            TargetDirection = targetDirection;
        }

        public TetrahedronRollStabilizer(Simulation sim, TetrahedronRollStabilizer source) : base(sim.MassDict[source.Mass1.UID] as VerletMass, sim.MassDict[source.Mass2.UID] as VerletMass, source.UID) {
            Tetrahedron = new VerletMass[4];
            Sync(source);
        }

        public override void Sync(ConnectionBase source) {
            base.Sync(source);
            TetrahedronRollStabilizer tetrahedronRollStabilizer = source as TetrahedronRollStabilizer;
            for (int i = 0; i < Tetrahedron.Length; i++) {
                Tetrahedron[i] = base.Mass1.Sim.MassDict[tetrahedronRollStabilizer.Tetrahedron[i].UID] as VerletMass;
            }
            target4f = tetrahedronRollStabilizer.target4f;
            BaseStiffness = tetrahedronRollStabilizer.BaseStiffness;
            StiffnessMultiplier = tetrahedronRollStabilizer.StiffnessMultiplier;
        }

        public override void Satisfy() {
            if (!Mathf.Approximately(StiffnessMultiplier, 0f)) {
                Vector4f vector4f = (Tetrahedron[1].Position4f + Tetrahedron[2].Position4f + Tetrahedron[3].Position4f) * Vector4fExtensions.onethird3;
                Vector4f vector4f2 = Tetrahedron[1].Position4f - vector4f;
                Vector4f vector4f3 = Tetrahedron[2].Position4f - vector4f;
                Vector4f vector4f4 = Tetrahedron[3].Position4f - vector4f;
                Single num = vector4f2.Magnitude();
                Single num2 = vector4f3.Magnitude();
                Single num3 = vector4f4.Magnitude();
                Vector4f a = Vector4fExtensions.Cross(Tetrahedron[1].Position4f - Tetrahedron[3].Position4f, Tetrahedron[2].Position4f - Tetrahedron[3].Position4f).Normalized();
                Single num4 = Vector4fExtensions.Dot(a, target4f);
                Vector4f vector4f5 = vector4f4 / new Vector4f(num3);
                Vector4f vector4f6 = Vector4fExtensions.Cross(a, vector4f5).Normalized();
                Single value = Mathf.Atan2(Vector4fExtensions.Dot(vector4f6, target4f), Vector4fExtensions.Dot(vector4f5, target4f));
                Single num5 = Mathf.Atan2(Vector4fExtensions.Dot(vector4f6, vector4f2), Vector4fExtensions.Dot(vector4f5, vector4f2));
                Single num6 = Mathf.Atan2(Vector4fExtensions.Dot(vector4f6, vector4f3), Vector4fExtensions.Dot(vector4f5, vector4f3));
                Single num7 = 0f;
                value = Mathf.Clamp(value, -0.05f, 0.05f);
                Single num8 = value * BaseStiffness * StiffnessMultiplier * 1f;
                num5 += num8;
                num6 += num8;
                num7 += num8;
                Vector4f vector4f7 = Tetrahedron[1].Position4f + Tetrahedron[2].Position4f + Tetrahedron[3].Position4f;
                Tetrahedron[1].Position4f = vector4f + vector4f5 * new Vector4f(Mathf.Cos(num5) * num) + vector4f6 * new Vector4f(Mathf.Sin(num5) * num);
                Tetrahedron[2].Position4f = vector4f + vector4f5 * new Vector4f(Mathf.Cos(num6) * num2) + vector4f6 * new Vector4f(Mathf.Sin(num6) * num2);
                Tetrahedron[3].Position4f = vector4f + vector4f5 * new Vector4f(Mathf.Cos(num7) * num3) + vector4f6 * new Vector4f(Mathf.Sin(num7) * num3);
                Vector4f vector4f8 = Tetrahedron[1].Position4f + Tetrahedron[2].Position4f + Tetrahedron[3].Position4f;
                Vector4f vector4f9 = (vector4f8 - vector4f7) * Vector4fExtensions.onethird3;
                Tetrahedron[1].Position4f -= vector4f9;
                Tetrahedron[2].Position4f -= vector4f9;
                Tetrahedron[3].Position4f -= vector4f9;
            }
        }

        public override void Solve() {
        }
    }
}
