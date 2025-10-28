using System;
using Mono.Simd;
using Mono.Simd.Math;
using UnityEngine;

namespace QH.Physics {
    public class Spring : ConnectionBase, IImpulseConstraint {
        public const Single LongitudalFrictionMult = 10f;
        public const Single LongitudalFrictionPow = 4f;
        private const Single MinDistance = 1E-06f;
        private const Single SqrMinDistance = MinDistance * MinDistance;

        protected Single springConstant;
        protected Single springLength;
        protected Single targetSpringLength;
        protected Single oldSpringLength;
        protected Single immediateSpringLength = -1f;
        protected Vector4f massesInvDenom;
        protected Single tension;
        protected Vector4f frictionConstant4f;
        public Boolean IsRepulsive;
        protected Vector4f affectMass1Factor;
        protected Vector4f affectMass2Factor;
        protected Single impulseThreshold2;
        protected Single impulseVerletMax2;
        private Vector4f springVector;
        private VerletMass verletMass2;

        public Single SpringConstant {
            get {
                return springConstant;
            }
            set {
                springConstant = value;
                ConnectionNeedSyncMark();
            }
        }

        public Single SpringLength {
            get {
                return targetSpringLength;
            }
            set {
                targetSpringLength = value;
                ConnectionNeedSyncMark();
            }
        }

        public Single CurrentSpringLength {
            get {
                return springLength;
            }
        }

        public Single Tension {
            get {
                return tension;
            }
        }

        public Single FrictionConstant {
            get {
                return frictionConstant4f.X;
            }
            set {
                frictionConstant4f = new Vector4f(value);
                ConnectionNeedSyncMark();
            }
        }

        public Single AffectMass1Factor {
            get {
                return affectMass1Factor.X;
            }
            set {
                affectMass1Factor = new Vector4f(value);
                ConnectionNeedSyncMark();
            }
        }

        public Single AffectMass2Factor {
            get {
                return affectMass2Factor.X;
            }
            set {
                affectMass2Factor = new Vector4f(value);
                ConnectionNeedSyncMark();
            }
        }

        public Single ImpulseThreshold2 {
            get {
                return impulseThreshold2;
            }
            set {
                impulseThreshold2 = value;
                ConnectionNeedSyncMark();
            }
        }

        public Single ImpulseVerletMax2 {
            get {
                return impulseVerletMax2;
            }
            set {
                impulseVerletMax2 = value;
                ConnectionNeedSyncMark();
            }
        }

        public Spring(MassObject mass1, MassObject mass2, Single springConstant, Single springLength, Single frictionConstant) : base(mass1, mass2) {
            if (SpringLength < 0f) {
                throw new ArgumentException("SpringLength can't be negative");
            }

            SpringConstant = springConstant;
            this.springLength = springLength;
            targetSpringLength = springLength;
            FrictionConstant = frictionConstant;
            affectMass1Factor = Vector4fExtensions.one3;
            affectMass2Factor = Vector4fExtensions.one3;
            impulseThreshold2 = 0f;
            impulseVerletMax2 = 100f;
            if (mass1.NextSpring == null) {
                mass1.NextSpring = this;
            }
            if (mass2.PriorSpring == null) {
                mass2.PriorSpring = this;
            }

            massesInvDenom = new Vector4f(1f / (base.Mass1.InvMassValue4f.X + base.Mass2.InvMassValue4f.X));
        }

        public Spring(Simulation sim, Spring source) : base(sim.MassDict[source.Mass1.UID], sim.MassDict[source.Mass2.UID], source.UID) {
            Mass1.NextSpring = this;
            Mass2.PriorSpring = this;
            SpringConstant = source.SpringConstant;
            springLength = source.springLength;
            targetSpringLength = source.springLength;
            FrictionConstant = source.FrictionConstant;
            IsRepulsive = source.IsRepulsive;
            verletMass2 = Mass2 as VerletMass;
            affectMass1Factor = Vector4fExtensions.one3;
            affectMass2Factor = Vector4fExtensions.one3;
            impulseThreshold2 = source.ImpulseThreshold2;
            impulseVerletMax2 = source.ImpulseVerletMax2;
            massesInvDenom = new Vector4f(1f / (base.Mass1.InvMassValue4f.X + base.Mass2.InvMassValue4f.X));
        }

        public Spring(Spring source, MassObject mass1, MassObject mass2) : base(mass1, mass2) {
            Mass1.NextSpring = this;
            Mass2.PriorSpring = this;
            SpringConstant = source.SpringConstant;
            springLength = source.springLength;
            targetSpringLength = source.springLength;
            FrictionConstant = source.FrictionConstant;
            IsRepulsive = source.IsRepulsive;
            verletMass2 = Mass2 as VerletMass;
            affectMass1Factor = Vector4fExtensions.one3;
            affectMass2Factor = Vector4fExtensions.one3;
            impulseThreshold2 = source.ImpulseThreshold2;
            impulseVerletMax2 = source.ImpulseVerletMax2;
            massesInvDenom = new Vector4f(1f / (base.Mass1.InvMassValue4f.X + base.Mass2.InvMassValue4f.X));
        }

        public void SetSpringLengthImmediate(Single length) {
            immediateSpringLength = length;
            ConnectionNeedSyncMark();
        }

        public override void Sync(ConnectionBase source) {
            base.Sync(source);
            Mass1.NextSpring = this;
            Mass2.PriorSpring = this;
            Spring spring = source as Spring;
            targetSpringLength = spring.targetSpringLength;
            springConstant = spring.SpringConstant;
            verletMass2 = Mass2 as VerletMass;
            affectMass1Factor = spring.affectMass1Factor;
            affectMass2Factor = spring.affectMass2Factor;
            impulseThreshold2 = spring.impulseThreshold2;
            impulseVerletMax2 = spring.impulseVerletMax2;
            frictionConstant4f = spring.frictionConstant4f;
            if (spring.immediateSpringLength >= 0f) {
                springLength = spring.immediateSpringLength;
                oldSpringLength = spring.immediateSpringLength;
                targetSpringLength = spring.immediateSpringLength;
                spring.immediateSpringLength = -1f;
            }
            massesInvDenom = new Vector4f(1f / (base.Mass1.InvMassValue4f.X + base.Mass2.InvMassValue4f.X));
        }

        public override void SyncSimUpdate() {
            base.SyncSimUpdate();
            massesInvDenom = new Vector4f(1f / (base.Mass1.InvMassValue4f.X + base.Mass2.InvMassValue4f.X));
        }

        public static void SatisfyImpulseConstraintMass0(Spring s, MassObject mass1, MassObject mass2, Single springLength, Vector4f friction, Single impulseThreshold2) {
            if (mass1.IsKinematic && mass2.IsKinematic) {
                return;
            }

            Vector4f vector4f = mass1.Velocity4f - mass2.Velocity4f;
            Vector4f vector4f2 = mass2.Position4f - mass1.Position4f - vector4f * Simulation.TimeQuant4f;
            Single num = vector4f2.SqrMagnitude();
            Vector4f vector4f3 = Vector4fExtensions.Zero;
            Vector4f vector4f4 = Vector4fExtensions.Zero;
            s.tension = 0f;
            s.DeltaLength = 0f;
            if (!(num > SqrMinDistance)) {
                return;
            }

            Vector4f vector4f5 = Vector4f.Zero;
            if (s.IsRepulsive || num > springLength * springLength) {
                num = Mathf.Sqrt(num);
                Vector4f vector4f6 = new Vector4f(num);
                vector4f5 = vector4f2 / vector4f6;
                s.tension = (num - springLength) * s.massesInvDenom.X * Simulation.InvTimeQuant4f.X;
                vector4f3 = vector4f5 * new Vector4f(s.tension);
                if (s.tension > impulseThreshold2) {
                    vector4f4 = vector4f3.Negative() + vector4f5 * new Vector4f(impulseThreshold2);
                }

                s.DeltaLength = num - springLength;
            }

            if (s.tension >= impulseThreshold2) {
                Vector4f vector4f7 = (vector4f + vector4f3 - vector4f4) * s.massesInvDenom * friction;
                vector4f3 -= vector4f7;
                vector4f4 += vector4f7;
            }

            mass1.Velocity4f += vector4f3 * mass1.InvMassValue4f * s.affectMass1Factor;
            if (mass2 is VerletMass) {
                if (s.tension > s.ImpulseVerletMax2) {
                    vector4f4 += vector4f5 * new Vector4f(s.tension - s.ImpulseVerletMax2);
                }

                (mass2 as VerletMass).ApplyImpulse(vector4f4 * Simulation.TimeQuant4f * mass2.InvMassValue4f * s.affectMass2Factor, false);
            }
            else {
                mass2.Velocity4f += vector4f4 * mass2.InvMassValue4f * s.affectMass2Factor;
            }
        }

        public static Single SatisfyImpulseConstraintMass(Spring spring, MassObject mass1, MassObject mass2, Single springLength, Vector4f friction, Single impulseThreshold2, Boolean isHitch = false) {
            Single num = 0f;
            Vector4f massesInvDenom = spring.massesInvDenom;
            Vector4f mass2InvMassValue4f = mass2.InvMassValue4f;
            if (isHitch) {
                massesInvDenom = mass1.MassValue4f;
                mass2InvMassValue4f = Vector4fExtensions.Zero;
            }

            Vector4f velocityOffsetMass1ToMass2 = mass1.Velocity4f - mass2.Velocity4f;
            Vector4f posOffsetMass1ToMass2 = mass2.Position4f - mass1.Position4f - velocityOffsetMass1ToMass2 * Simulation.TimeQuant4f;
            Single posOffsetMass1ToMass2Len = posOffsetMass1ToMass2.SqrMagnitude();
            Vector4f velocityMass1Delta = Vector4fExtensions.Zero;
            Vector4f velocityMass2Delta = Vector4fExtensions.Zero;
            spring.DeltaLength = 0f;
            if (posOffsetMass1ToMass2Len > SqrMinDistance) {
                Vector4f posOffsetMass1ToMass2Normalized = Vector4f.Zero;
                if (spring.IsRepulsive || posOffsetMass1ToMass2Len > springLength * springLength) {
                    posOffsetMass1ToMass2Len = Mathf.Sqrt(posOffsetMass1ToMass2Len);
                    posOffsetMass1ToMass2Normalized = posOffsetMass1ToMass2 / new Vector4f(posOffsetMass1ToMass2Len);
                    num = (posOffsetMass1ToMass2Len - springLength) * massesInvDenom.X * Simulation.InvTimeQuant4f.X;
                    velocityMass1Delta = posOffsetMass1ToMass2Normalized * new Vector4f(num);
                    if (num > impulseThreshold2) {
                        velocityMass2Delta = velocityMass1Delta.Negative() + posOffsetMass1ToMass2Normalized * new Vector4f(impulseThreshold2);
                    }

                    spring.DeltaLength = posOffsetMass1ToMass2Len - springLength;
                }

                if (num >= impulseThreshold2) {
                    Vector4f vector4f9 = (velocityOffsetMass1ToMass2 + velocityMass1Delta - velocityMass2Delta) * massesInvDenom * friction;
                    velocityMass1Delta -= vector4f9;
                    velocityMass2Delta += vector4f9;
                }

                mass1.Velocity4f += velocityMass1Delta * mass1.InvMassValue4f * spring.affectMass1Factor;
                if (mass2 is VerletMass) {
                    if (num > spring.ImpulseVerletMax2) {
                        velocityMass2Delta += posOffsetMass1ToMass2Normalized * new Vector4f(num - spring.ImpulseVerletMax2);
                    }

                    (mass2 as VerletMass).ApplyImpulse(velocityMass2Delta * Simulation.TimeQuant4f * mass2InvMassValue4f * spring.affectMass2Factor, false);
                }
                else {
                    mass2.Velocity4f += velocityMass2Delta * mass2InvMassValue4f * spring.affectMass2Factor;
                }
            }

            return num;
        }

        public static void SatisfyVirtualSpring(Single length, MassObject mass1, MassObject mass2, Single k) {
            //Vector4f velOffset = mass1.Velocity4f - mass2.Velocity4f;
            //Vector4f posOffset = mass2.Position4f - mass1.Position4f - velOffset * Simulation.TimeQuant4f;
            Vector4f posOffset = mass2.Position4f - mass1.Position4f;
            Single posOffsetLen = posOffset.SqrMagnitude();

            if (posOffsetLen > SqrMinDistance && 0 < Math.Abs(posOffsetLen - length * length)) {
                Vector4f invMass2 = mass2.InvMassValue4f;
                posOffsetLen = Mathf.Sqrt(posOffsetLen);
                Vector4f posOffsetNor = posOffset / new Vector4f(posOffsetLen);
                Single vDeltaLen = Math.Abs((posOffsetLen - length)) * k * invMass2.X * Simulation.TimeQuant4f.X;
                Vector4f velDelta = posOffsetNor.Negative() * vDeltaLen;
                Vector4f lastV = mass2.Velocity4f;
                mass2.Velocity4f += velDelta;
            }
        }

        public virtual void SatisfyImpulseConstraint() {
            if (base.Mass1.IsKinematic && base.Mass2.IsKinematic) {
                return;
            }

            Vector4f vector4f = base.Mass1.Velocity4f - base.Mass2.Velocity4f;
            Vector4f vector4f2 = base.Mass2.Position4f - base.Mass1.Position4f - vector4f * Simulation.TimeQuant4f;
            Single num = vector4f2.SqrMagnitude();
            Vector4f vector4f3 = Vector4fExtensions.Zero;
            Vector4f vector4f4 = Vector4fExtensions.Zero;
            if (!(num > SqrMinDistance)) {
                return;
            }

            Vector4f vector4f5 = new Vector4f(1f / (base.Mass1.InvMassValue4f.X + base.Mass2.InvMassValue4f.X));
            Single num2 = 0f;
            num = Mathf.Sqrt(num);
            Vector4f vector4f6 = new Vector4f(num);
            Vector4f vector4f7 = vector4f2 / vector4f6;
            if (IsRepulsive || num > springLength * springLength) {
                num2 = (num - springLength) * massesInvDenom.X * Simulation.InvTimeQuant4f.X;
                vector4f3 = vector4f7 * new Vector4f(num2);
                if (num2 > impulseThreshold2) {
                    vector4f4 = vector4f3.Negative() + vector4f7 * new Vector4f(impulseThreshold2);
                }
            }

            if (num2 >= impulseThreshold2) {
                Single f = Vector4fExtensions.Dot(base.Mass2.Velocity4f, vector4f2) - Vector4fExtensions.Dot(base.Mass1.Velocity4f, vector4f2);
                Vector4f vector4f8 = vector4f7 * new Vector4f(f) * frictionConstant4f;
                vector4f3 -= vector4f8;
                vector4f4 += vector4f8;
            }

            base.Mass1.Velocity4f += vector4f3 * base.Mass1.InvMassValue4f;
            if (verletMass2 == null) {
                base.Mass2.Velocity4f += vector4f4 * base.Mass2.InvMassValue4f;
            }
            else {
                verletMass2.ApplyImpulse(vector4f4 * Simulation.TimeQuant4f * base.Mass2.InvMassValue4f, false);
            }
        }

        public override void Solve() {
            if (base.Mass1.Sim.FrameIterationIndex == 0) {
                oldSpringLength = springLength;
            }

            if (!Mathf.Approximately(targetSpringLength, springLength)) {
                springLength = Mathf.Lerp(oldSpringLength, targetSpringLength, base.Mass1.Sim.FrameIterationProgress);
            }

            if (!base.Mass1.IsKinematic || !base.Mass2.IsKinematic) {
                tension = SatisfyImpulseConstraintMass(this, base.Mass1, base.Mass2, springLength, frictionConstant4f, ImpulseThreshold2);
            }
        }

        public Single EquilibrantLength(Single forceMagnitude) {
            return springLength + forceMagnitude / springConstant;
        }

        public override Single CalculatePotentialEnergy() {
            springVector = mass1.Position4f - mass2.Position4f;
            Single num = springVector.Magnitude() - springLength;
            if (num > 0f || IsRepulsive) {
                return springConstant * num * num;
            }
            return 0f;
        }
    }
}