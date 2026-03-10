using System;
using UnityEngine;
using Mono.Simd;
using Mono.Simd.Math;

namespace QH.Physics {
    public sealed class TetrahedronWithBall : VerletConstraint {
        public const Single DefaultFriction = 0.003f;
        public const Single BaseStiffness = 0.099999994f;

        private readonly Vector4f inv3 = new Vector4f(0.333f);
        private readonly Vector4f inv2 = new Vector4f(0.5f);
        private static readonly Vector4f inv8 = new Vector4f(0.125f);

        public Single[] BendModifiers;
        public Single BendAngle;
        public VerletMass[] Tetrahedron1;
        public VerletMass[] Tetrahedron2;
        public VerletMass FirstChordMass;
        public VerletMass SecondChordMass;
        public Single StiffnessMultiplier = 1f;
        public Single WaveDeviationFreq;
        public Single WaveDeviationPhase;
        public Single WaveDeviationAmp;
        public Single WaveDeviationMult = 1f;
        public Vector3 WaveAxis;

        private Vector4f stiffness4f;
        private Vector4f friction4f;
        private Single chordPathAccum;

        public Single Stiffness {
            get { return stiffness4f.X; }
            set { stiffness4f = new Vector4f(value); }
        }

        public TetrahedronWithBall(VerletMass[] t1, VerletMass[] t2, Single stiffness, Single friction = 0.003f) : base(t1[0], t2[0]) {
            stiffness4f = new Vector4f(stiffness);
            friction4f = new Vector4f(friction);
            Tetrahedron1 = t1;
            Tetrahedron2 = t2;
            BendModifiers = new Single[3] { 1f, 1f, 1f };
        }

        public TetrahedronWithBall(Simulation sim, TetrahedronWithBall source) : base(sim.MassDict[source.Mass1.UID] as VerletMass, sim.MassDict[source.Mass2.UID] as VerletMass, source.UID) {
            Tetrahedron1 = new VerletMass[4];
            Tetrahedron2 = new VerletMass[4];
            Sync(source);
            BendModifiers = source.BendModifiers;
        }

        public override void Sync(ConnectionBase source) {
            base.Sync(source);
            TetrahedronWithBall tetrahedronBallJoint = source as TetrahedronWithBall;
            stiffness4f = tetrahedronBallJoint.stiffness4f;
            friction4f = tetrahedronBallJoint.friction4f;
            for (Int32 i = 0; i < Tetrahedron1.Length; i++) {
                if (Tetrahedron1[i] == null || Tetrahedron1[i].UID != tetrahedronBallJoint.Tetrahedron1[i].UID) {
                    Tetrahedron1[i] = base.Mass1.Sim.MassDict[tetrahedronBallJoint.Tetrahedron1[i].UID] as VerletMass;
                }
                if (Tetrahedron2[i] == null || Tetrahedron2[i].UID != tetrahedronBallJoint.Tetrahedron2[i].UID) {
                    Tetrahedron2[i] = base.Mass1.Sim.MassDict[tetrahedronBallJoint.Tetrahedron2[i].UID] as VerletMass;
                }
            }
            if (FirstChordMass == null || FirstChordMass.UID != tetrahedronBallJoint.FirstChordMass.UID) {
                FirstChordMass = base.Mass1.Sim.MassDict[tetrahedronBallJoint.FirstChordMass.UID] as VerletMass;
            }
            if (SecondChordMass == null || SecondChordMass.UID != tetrahedronBallJoint.SecondChordMass.UID) {
                SecondChordMass = base.Mass1.Sim.MassDict[tetrahedronBallJoint.SecondChordMass.UID] as VerletMass;
            }
            WaveDeviationFreq = tetrahedronBallJoint.WaveDeviationFreq;
            WaveDeviationPhase = tetrahedronBallJoint.WaveDeviationPhase;
            WaveDeviationAmp = tetrahedronBallJoint.WaveDeviationAmp;
            WaveDeviationMult = tetrahedronBallJoint.WaveDeviationMult;
            WaveAxis = tetrahedronBallJoint.WaveAxis;
        }

        protected Quaternion ApplyWaveDeviation() {
            Vector4f b = (FirstChordMass.Position4f - SecondChordMass.Position4f).Normalized();
            Single f = Vector4fExtensions.Dot(FirstChordMass.PrevIterationPathDelta, b);
            chordPathAccum += Mathf.Min(Mathf.Abs(f), 0.002f) * WaveDeviationFreq * 2f;
            chordPathAccum %= 1f;
            Single num = WaveDeviationMult * WaveDeviationAmp * Mathf.Sin((0f - chordPathAccum) * (Single)Math.PI * 2f + WaveDeviationPhase);
            return Quaternion.AngleAxis((BendAngle + num) * 57.29578f, WaveAxis);
        }

        public override void Satisfy() {
            Vector4f vector4f = Tetrahedron1[0].Position4f + Tetrahedron1[1].Position4f + Tetrahedron1[2].Position4f + Tetrahedron1[3].Position4f + Tetrahedron2[0].Position4f + Tetrahedron2[1].Position4f + Tetrahedron2[2].Position4f + Tetrahedron2[3].Position4f;
            Quaternion quaternion = ApplyWaveDeviation();
            Vector4f vector4f2 = (Tetrahedron1[1].Position4f + Tetrahedron1[2].Position4f + Tetrahedron1[3].Position4f) * inv3;
            Vector4f vector4f3 = (Tetrahedron2[0].Position4f - vector4f2) * inv2;
            Tetrahedron2[0].Position4f -= vector4f3;
            vector4f3 *= inv3;
            Tetrahedron1[1].Position4f += vector4f3;
            Tetrahedron1[2].Position4f += vector4f3;
            Tetrahedron1[3].Position4f += vector4f3;
            Vector4f vector4f4 = (Tetrahedron2[1].Position4f + Tetrahedron2[2].Position4f + Tetrahedron2[3].Position4f) * inv3;
            Vector4f vec = Tetrahedron1[0].Position4f - vector4f2;
            Vector4f vec2 = vec.Normalized();
            Vector4f vec3 = Tetrahedron2[0].Position4f - vector4f4;
            Vector4f vec4 = vec3.Normalized();
            Vector4f vec5 = (vector4f2 - Tetrahedron1[3].Position4f).Normalized();
            Vector4f vec6 = (vector4f4 - Tetrahedron2[3].Position4f).Normalized();
            Vector3 upwards = -vec2.AsVector3();
            Vector3 upwards2 = -vec4.AsVector3();
            Quaternion quaternion2 = Quaternion.LookRotation(vec5.AsVector3(), upwards);
            Quaternion quaternion3 = Quaternion.LookRotation(vec6.AsVector3(), upwards2);
            Single t = Stiffness * StiffnessMultiplier * 0.099999994f;
            Quaternion quaternion4 = Quaternion.Slerp(quaternion2, quaternion3 * Quaternion.Inverse(quaternion), t);
            Quaternion quaternion5 = Quaternion.Slerp(quaternion3, quaternion2 * quaternion, t);
            for (Int32 i = 1; i <= 3; i++) {
                Vector4f repr = Tetrahedron1[i].Position4f - vector4f2;
                repr = (quaternion4 * (Quaternion.Inverse(quaternion2) * repr.AsVector3())).AsPhyVector(ref repr);
                Tetrahedron1[i].Position4f = vector4f2 + repr;
                Vector4f repr2 = Tetrahedron2[i].Position4f - vector4f4;
                repr2 = (quaternion5 * (Quaternion.Inverse(quaternion3) * repr2.AsVector3())).AsPhyVector(ref repr2);
                Tetrahedron2[i].Position4f = vector4f4 + repr2;
            }

            Vector4f vector4f5 = Tetrahedron1[0].Position4f + Tetrahedron1[1].Position4f + Tetrahedron1[2].Position4f + Tetrahedron1[3].Position4f + Tetrahedron2[0].Position4f + Tetrahedron2[1].Position4f + Tetrahedron2[2].Position4f + Tetrahedron2[3].Position4f;
            Vector4f vector4f6 = (vector4f5 - vector4f) * inv8;
            Tetrahedron1[0].Position4f -= vector4f6;
            Tetrahedron1[1].Position4f -= vector4f6;
            Tetrahedron1[2].Position4f -= vector4f6;
            Tetrahedron1[3].Position4f -= vector4f6;
            Tetrahedron2[0].Position4f -= vector4f6;
            Tetrahedron2[1].Position4f -= vector4f6;
            Tetrahedron2[2].Position4f -= vector4f6;
            Tetrahedron2[3].Position4f -= vector4f6;
        }

        public override void Solve() {
            for (Int32 i = 0; i < 4; i++) {
                Vector4f vector4f = (Tetrahedron1[i].PositionDelta4f - Tetrahedron2[i].PositionDelta4f) * friction4f;
                if (!base.Mass1.IsKinematic) {
                    Tetrahedron1[i].Position4f -= vector4f;
                }
                if (!base.Mass2.IsKinematic) {
                    Tetrahedron2[i].Position4f += vector4f;
                }
            }
        }
    }
}