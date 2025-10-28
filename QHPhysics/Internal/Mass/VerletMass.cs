using System;
using UnityEngine;
using Mono.Simd;
using Mono.Simd.Math;

namespace QH.Physics {
    
    public class FishData
    {
        public bool isOpenSpring=true;
        public bool isOpenVerletSpring=true;
        public bool isOpenVerletSatisfy=true;
        public float verletSpringlength=0;
        public float verletSpringfriction=0;

        public void SetData(bool isOpenSpring, bool isOpenVerletSpring, bool isOpenSatisfy ,float verletSpringlength,float verletSpringfriction)
        {
            this.isOpenSpring = isOpenSpring;
            this.isOpenVerletSpring = isOpenVerletSpring;
            this.isOpenVerletSatisfy = isOpenVerletSatisfy;
            this.verletSpringlength = verletSpringlength;
            this.verletSpringfriction = verletSpringfriction;
        }
        
    }
    
    public class VerletMass : MassObject {

        #region FishData

        public FishData FishData;

        public void SetData(bool isOpenSpring, bool isOpenVerletSpring, bool isOpenSatisfy ,float verletSpringlength,float verletSpringfriction)
        {
            if (FishData == null)
            {
                FishData = new FishData();
            }
            FishData.SetData(isOpenSpring,  isOpenVerletSpring,  isOpenSatisfy ,verletSpringlength,verletSpringfriction);
        }

        public void SetData(FishData data)
        {
            SetData(data.isOpenSpring, data.isOpenVerletSpring, data.isOpenVerletSatisfy ,data.verletSpringlength,data.verletSpringfriction);
        }
        

        #endregion
        
        protected Vector4f prevPosition4f;
        protected Vector4f kahanAccum = Vector4f.Zero;
        protected Vector4f kahanAccum2 = Vector4f.Zero;
        protected Boolean isStatic;
        private Vector4f kinematicPrevPosition4f;
        private Vector4f kinematicNextPosition4f;

        public Vector4f PositionDelta4f {
            get { return Position4f - prevPosition4f; }
            set { prevPosition4f = Position4f - value; }
        }

        public Vector4f PrevIterationPathDelta { get; private set; }

        public Vector4f PrevPosition4f {
            get { return prevPosition4f; }
        }

        public override Vector3 Position {
            set {
                if (!IsKinematic) {
                    Vector3 vec = value - visualPositionOffset;
                    prevPosition4f += vec.AsPhyVector(ref prevPosition4f) - Position4f;
                    Position4f = vec.AsPhyVector(ref Position4f);
                }
                else {
                    isStatic = false;
                    kinematicPrevPosition4f = Position4f;
                    kinematicNextPosition4f = (value - visualPositionOffset).AsPhyVector(ref kinematicNextPosition4f);
                }
                Sim.IPhyActionsListener?.ChangeMassPosition(base.UID, value);
            }
        }

        public override Boolean IsTensioned {
            get { return false; }
        }

        public VerletMass(Simulation sim, float mass, Vector3 position, EMassObjectType type,float WaterDragCons=10) : base(sim, mass, position, type,WaterDragCons) {
            Position = position;
            prevPosition4f = Position4f;
            isStatic = true;
            base.Collision = ECollisionType.FullBody;
        }

        public VerletMass(Simulation sim, VerletMass source) : base(sim, source) {
            prevPosition4f = Position4f;
            kahanAccum = Vector4f.Zero;
            kahanAccum2 = Vector4f.Zero;
            deltaPos = Vector4f.Zero;
            kinematicPrevPosition4f = source.kinematicPrevPosition4f;
            kinematicNextPosition4f = source.kinematicNextPosition4f;
            isStatic = source.isStatic;
            base.Collision = source.Collision;
        }

        public override void StopMass() {
            base.StopMass();
            prevPosition4f = Position4f;
            Velocity4f = Vector4f.Zero;
            kahanAccum = Vector4f.Zero;
            kahanAccum2 = Vector4f.Zero;
            deltaPos = Vector4f.Zero;
        }

        protected override void RealVisualDiverge(Vector4f realPositionOffset) {
            base.RealVisualDiverge(realPositionOffset);
            prevPosition4f += realPositionOffset;
        }

        public override void KinematicTranslate(Vector4f offset) {
            if (Sim.IPhyActionsListener == null) {
                Position4f += offset;
                prevPosition4f += offset;
            }
            else {
                Sim.IPhyActionsListener.MassKinematicTranslate(base.UID, offset.AsVector3());
            }
        }

        public void ApplyImpulse(Vector4f step, Boolean safe = true) {
            Vector4f vector4f = step.Negative() - kahanAccum2;
            Vector4f vector4f2 = prevPosition4f + vector4f;
            kahanAccum2 = vector4f2 - prevPosition4f - vector4f;
            
            if (FishData!=null&& !FishData.isOpenSpring) return;
            
            prevPosition4f = vector4f2;
            if (safe) {
                Vector4f vec = Position4f - prevPosition4f;
                if (base.CurrentVelocityLimit > 0f && (Mathf.Abs(vec.X) > currentVelocityDeltaLimit || Mathf.Abs(vec.Y) > currentVelocityDeltaLimit || Mathf.Abs(vec.Z) > currentVelocityDeltaLimit)) {
                    float num = vec.Magnitude();
                    vec *= new Vector4f(currentVelocityDeltaLimit / num);
                }
            }
        }

        public override void SyncMain(MassObject source)
        {
            base.SyncMain(source);
            if(FishData!=null)
            {
                (source as VerletMass) .SetData(FishData);
            }
        }

        Vector4f deltaPos = Vector4f.Zero;
        public override void Simulate() {
            if (IsKinematic) {
                prevPosition4f = Position4f;
                if (!isStatic) {
                    Position4f = Vector4fExtensions.Lerp(kinematicPrevPosition4f, kinematicNextPosition4f, Sim.FrameIterationProgress);
                    if (Mathf.Approximately(Sim.FrameIterationProgress, 1f)) {
                        isStatic = true;
                    }
                }
                PrevIterationPathDelta = Position4f - prevPosition4f;
                return;
            }

            Vector4f vector4f2 = force4f * invMassValueDelta;
            Vector4f position4f = Position4f;
            Vector3 position = Position;
            Vector4f vector4f3 = Position4f - prevPosition4f;
             vector4f3.Y = Type == EMassObjectType.Fish && !isstopMass && deltaPos != Vector4f.Zero ? deltaPos.Y+(vector4f3.Y-deltaPos.Y)*0.01f : vector4f3.Y;
            if (base.CurrentVelocityLimit > 0f && (Mathf.Abs(vector4f3.X) > currentVelocityDeltaLimit || Mathf.Abs(vector4f3.Y) > currentVelocityDeltaLimit || Mathf.Abs(vector4f3.Z) > currentVelocityDeltaLimit)) {
                float num = vector4f3.Magnitude();
                vector4f3 *= new Vector4f(currentVelocityDeltaLimit / num);
                IsLimitBreached = true;
            }

            Vector4f vector4f4 = vector4f3 + vector4f2 * Simulation.TimeQuant4f;
            Velocity4f = vector4f4 * Simulation.InvTimeQuant4f;
            Vector4f vector4f5 = kahanAccum;
            Vector4f vector4f6 = vector4f4 - kahanAccum;
            Vector4f vector4f7 = Position4f + vector4f6;
            kahanAccum = vector4f7 - Position4f - vector4f6;
            Position4f = vector4f7;
            prevPosition4f = position4f;
            PrevIterationPathDelta = vector4f4;

           

            if (base.Collision == ECollisionType.FullBody) {
                isCollision = false;
                Vector3 normal = Vector3.zero;
                Vector4f zero = Vector4f.Zero;
                Vector3 position2 = Position;
                Vector3 prevPoint = position;
                if (null != colliders) {
                    for (Int32 i = 0; i < colliders.Length; i++) {
                        if (colliders[i] != null) {
                            zero = colliders[i].TestPoint(position2, prevPoint, out normal).AsPhyVector(ref Position4f);
                            if (!zero.IsZero()) {
                                Vector4f vector4f8 = normal.AsPhyVector(ref Position4f);
                                float num2 = Vector4fExtensions.Dot(vector4f8, Velocity4f);
                                Vector4f vector4f9 = vector4f8 * new Vector4f(num2);
                                Vector4f vector4f10 = Velocity4f - vector4f9;
                                Velocity4f = FrictionVelocity(Velocity4f, vector4f9);
                                Velocity4f += ((!(num2 >= 0f)) ? (vector4f9.Negative() * MassObject.surfaceBounce4f) : vector4f9);
                                vector4f5 = kahanAccum;
                                vector4f6 = zero - kahanAccum;
                                vector4f7 = Position4f + vector4f6;
                                kahanAccum = vector4f7 - Position4f - vector4f6;
                                float distance = vector4f7.Magnitude();
                                Vector4f vec = vector4f7.Normalized();
                                vec.Y = 0;
                                Position4f =vector4f7;
                                PrevIterationPathDelta = Velocity4f * Simulation.TimeQuant4f;
                                prevPosition4f = Position4f - PrevIterationPathDelta;
                                position = Position;
                                prevPoint = position2;
                                position2 = Position;
                                isCollision = true;
                            }
                        }
                    }
                }

                if (heightChunk != null)
                {
                    if (Type != EMassObjectType.Fish ||isstopMass) return;
                     zero = heightChunk.TestPoint(Position, position, out normal).AsPhyVector(ref Position4f);
                     if (!zero.IsZero()) {
                         Vector4f vector4f11 = normal.AsPhyVector(ref Position4f);
                         float num3 = Vector4fExtensions.Dot(vector4f11, Velocity4f);
                         vector4f5 = kahanAccum;
                         vector4f6 = zero - kahanAccum;
                         vector4f7 = Position4f + vector4f6;
                         kahanAccum = vector4f7 - Position4f - vector4f6;
                         float distance = vector4f7.Magnitude();
                         Vector4f vec=vector4f7.Normalized();
                         vec.Y = 0;
                         Position4f =vector4f7;
                         Vector4f vector4f12 = vector4f11 * new Vector4f(num3);
                         Vector4f vector4f13 = Velocity4f - vector4f12;
                         Velocity4f = FrictionVelocity(Velocity4f, vector4f12);
                         Velocity4f += ((!(num3 >= 0f)) ? (vector4f12.Negative() * MassObject.surfaceBounce4f) : vector4f12);
                         PrevIterationPathDelta = Velocity4f * Simulation.TimeQuant4f;//TODO:限制弹出的速度
                         prevPosition4f = Position4f - PrevIterationPathDelta;
                         isCollision = true;
                     }
                }
            }
            else if (base.Collision == ECollisionType.ExternalPlane) {
                isCollision = false;
                float num4 = Mathf.Max(0f, base.ExtGroundPointInterpolated.Y - Position.y);
                if (num4 > 0f) {
                    Vector4f extGroundNormalInterpolated = base.ExtGroundNormalInterpolated;
                    float num5 = Vector4fExtensions.Dot(extGroundNormalInterpolated, Velocity4f);
                    vector4f5 = kahanAccum;
                    vector4f6 = new Vector4f(0f, num4, 0f, 0f) - kahanAccum;
                    vector4f7 = Position4f + vector4f6;
                    kahanAccum = vector4f7 - Position4f - vector4f6;
                    // float distance = vector4f7.Magnitude();
                    // Vector4f vec = vector4f7.Normalized();
                    // vec.Y = 0;
                    // Position4f = vec * distance;
                    Position4f = vector4f7;
                    Vector4f vector4f14 = extGroundNormalInterpolated * new Vector4f(num5);
                    Vector4f vector4f15 = Velocity4f - vector4f14;
                    Velocity4f = FrictionVelocity(Velocity4f, vector4f14);
                    Velocity4f += ((!(num5 >= 0f)) ? (vector4f14.Negative() * MassObject.surfaceBounce4f) : vector4f14);
                    PrevIterationPathDelta = Velocity4f * Simulation.TimeQuant4f;
                    prevPosition4f = Position4f - PrevIterationPathDelta;
                    isCollision = true;
                }
            }
            
            deltaPos = Position4f - prevPosition4f;
            
            kinematicPrevPosition4f = Position4f;
            kinematicNextPosition4f = Position4f;
        }
    }
}