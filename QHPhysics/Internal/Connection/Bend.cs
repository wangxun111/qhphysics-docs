using System;
using UnityEngine;
using Mono.Simd;
using Mono.Simd.Math;

namespace QH.Physics {
    public class Bend : ConnectionBase {
        public Single BendConstant {
            get; set;
        }
        public Single SpringConstant {
            get; set;
        }
        public Single FrictionConstant {
            get; set;
        }
        public Single SpringLength {
            get; set;
        }
        public Vector3 OriginalLocalUnbentPosition {
            get; set;
        }

        private Vector4f vecFrictionConstant4f;

        public Vector3 UnbentPosition {
            get {
                return Mass1.rotation * OriginalLocalUnbentPosition + Mass1.Position;
            }
        }

        public Vector4f UnbentPosition4f {
            get {
                return (Mass1.rotation * OriginalLocalUnbentPosition).AsPhyVector(ref Mass1.Position4f) + Mass1.Position4f;
            }
        }

        public Vector3 LastMassLocalPosition {
            get {
                return Quaternion.Inverse(Mass1.rotation) * (Mass2.Position - Mass1.Position);
            }
        }

        public Single Mass1Factor { get; set; } = 1f;
        public Single Mass2Factor { get; set; } = 1f;

        public Bend(MassObject firstMass, MassObject lastMass, Single bendConstant, Single springConstant, Single frictionConstant, Vector3 originalLocalUnbentPosition, Single springLength) : base(firstMass, lastMass) {
            BendConstant = bendConstant;
            SpringConstant = springConstant;
            FrictionConstant = frictionConstant;
            OriginalLocalUnbentPosition = originalLocalUnbentPosition;
            SpringLength = springLength;
            vecFrictionConstant4f = new Vector4f(FrictionConstant);
        }

        public Bend(Simulation sim, Bend source) : base(sim.MassDict[source.Mass1.UID], sim.MassDict[source.Mass2.UID], source.UID) {
            BendConstant = source.BendConstant;
            SpringConstant = source.SpringConstant;
            FrictionConstant = source.FrictionConstant;
            OriginalLocalUnbentPosition = source.OriginalLocalUnbentPosition;
            SpringLength = source.SpringLength;
            vecFrictionConstant4f = new Vector4f(FrictionConstant);
            Mass1Factor = source.Mass1Factor;
            Mass2Factor = source.Mass2Factor;
        }

        public override void Sync(ConnectionBase source) {
            base.Sync(source);
            Mass1Factor = (source as Bend).Mass1Factor;
            Mass2Factor = (source as Bend).Mass2Factor;
        }

        public override void Solve() {
            Vector3 posLocalMass2 = Quaternion.Inverse(Mass1.rotation) * (Mass2.Position - Mass1.Position);
            Mass2.rotation = Mass1.rotation * Quaternion.FromToRotation(OriginalLocalUnbentPosition, posLocalMass2);
            Vector4f dirUnbentMass1ToMass2 = UnbentPosition4f - Mass2.Position4f;
            Single disUnbentMass1ToMass2 = dirUnbentMass1ToMass2.Magnitude();
            if (!Mathf.Approximately(disUnbentMass1ToMass2, 0f)) {
                Vector4f dirMass1ToMass2 = Mass1.Position4f - Mass2.Position4f;
                Single disMass1ToMass2 = dirMass1ToMass2.Magnitude();
                if (!Mathf.Approximately(disMass1ToMass2, 0f)) {
                    Vector4f dirUnbentMass1ToMass2Normalized = dirUnbentMass1ToMass2 / new Vector4f(disUnbentMass1ToMass2);
                    Vector4f dirMass1ToMass2Normalized = dirMass1ToMass2 / new Vector4f(disMass1ToMass2);
                    Vector4f dirBendConstantMass1ToMass2 = dirUnbentMass1ToMass2Normalized * new Vector4f(disUnbentMass1ToMass2 * BendConstant);
                    dirBendConstantMass1ToMass2 += dirMass1ToMass2Normalized * new Vector4f((disMass1ToMass2 - SpringLength) * SpringConstant);
                    dirBendConstantMass1ToMass2 += (Mass1.Velocity4f - Mass2.Velocity4f) * vecFrictionConstant4f;
                    Vector4f velocityDelta = dirBendConstantMass1ToMass2 * Simulation.TimeQuant4f;
                    Mass2.Velocity4f += velocityDelta * Mass2.InvMassValue4f * Mass2Factor;
                    Mass1.Velocity4f -= velocityDelta * Mass1.InvMassValue4f * Mass1Factor;
                    Mass2.UpdateAvgForce(dirBendConstantMass1ToMass2);
                    Mass1.UpdateAvgForce(dirBendConstantMass1ToMass2.Negative());
                }
            }
        }
    }
}