using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using UnityEngine;
using Mono.Simd;
using Mono.Simd.Math;
using SObject = System.Object;
using UObject = UnityEngine.Object;

namespace QH.Physics {
    public class MassObject {
        public const Single DefaultRadius = 0.0125f;

        public const Single DefaultStaticFrictionFactor = 0.3f;
        public const Single DefaultDynamicFrictionFactor = 0.25f;

        public const Single DefaultVelocityLimit = 20f;
        public const Single DefaultVelocityLimitWithFish = 100f;

        public static readonly Vector4f collisionEpsilon4f = new Vector4f(0.05f);
        public static readonly Vector4f surfaceFriction4f = new Vector4f(0.98f);
        public static readonly Vector4f surfaceBounce4f = new Vector4f(0.15f);
        public static readonly Single surfaceStaticFrictionSqr = 0.01f;

        private static Int32 IIDCounter = 0;

        private Single currentVelocityLimit = DefaultVelocityLimit;
        protected Single currentVelocityDeltaLimit = DefaultVelocityLimit * Simulation.TimeQuant;

        public Simulation Sim;
        public MassObject SourceMass;
        public MassObject CopyMass;
        private EMassObjectType massType;
        private Single radius = DefaultRadius;
        public Vector4f MassValue4f;
        protected Vector4f invMassValueDelta;
        protected Vector4f invMassValue;
        public Vector4f Position4f;
        protected Vector3 visualPositionOffset;
        protected Vector4f visualPositionOffset4f;
        public Quaternion rotation;
        public Vector4f Velocity4f;
        protected Vector4f force4f;
        public Vector4f Motor4f;
        public Vector4f WaterMotor4f;
        public Vector4f perFrameForcesCache4f;
        public Vector4f perPeriodForcesCache4f;
        protected Vector4f buoyancy4f;
        public Vector4f WaterDragConstant4f;
        public Vector4f AirDragConstant4f;
        protected Vector4f buoyancyAtMinimalSpeed4f;
        protected Vector4f extGroundPointPrev;
        protected Vector4f extGroundPointCurrent;
        protected Vector4f extGroundNormalPrev;
        protected Vector4f extGroundNormalCurrent;
        protected Vector4f groundPoint;
        protected Vector4f groundNormal;
        public ICollider[] colliders;
        public Vector3 sphereOverlapCenter;
        public PlaneSudokuCollider heightChunk;
        public Vector4f FlowVelocity;
        public Vector4f WindVelocity;
        public Boolean IgnoreEnvironment;
        public Boolean IgnoreEnvForces;
        private Boolean disableSimulation;
        private ECollisionType collisionType;
        protected Single staticFrictionFactor = DefaultStaticFrictionFactor;
        protected Single slidingFrictionFactor = DefaultDynamicFrictionFactor;
        private Boolean isRef;
        private Boolean isKinematic;
        private Boolean isFreeze;
        protected Boolean isCollision;
        public Spring PriorSpring;
        public Spring NextSpring;
        private Vector4f avgForce;
        private Int32 avgForceCount;
        public Boolean IsLimitBreached;
        private Single motionDamping;
        private EDensityType densityType = EDensityType.None;
        private Vector4f dragForce;

        private Single forceFactor;

        public Single ForceFactor {
            get { return forceFactor; }
            set {
                forceFactor = value;
                Sim.IPhyActionsListener?.ChangeMassForceFactor(UID, value);
            }
        }

        public Int32 UID { get; private set; }
        public Int32 IID { get; private set; }

        public static Int32 NewIID {
            get { return Interlocked.Decrement(ref IIDCounter); }
        }

        public Single CurrentVelocityLimit {
            get { return currentVelocityLimit; }
            set {
                currentVelocityLimit = value;
                currentVelocityDeltaLimit = value * Simulation.TimeQuant;
                Sim.IPhyActionsListener?.ChangeMassVelocityLimit(UID, value);
            }
        }

        public EMassObjectType Type {
            get { return massType; }
            set {
                massType = value;
                Sim.IPhyActionsListener?.ChangeMassType(UID, value);
            }
        }

        public Single Radius {
            get { return radius; }
            set { radius = (0 <= value ? value : 0f); }
        }

        public Single MassValue {
            get { return MassValue4f.X; }
            set {
                MassValue4f = new Vector4f(value);
                invMassValueDelta = new Vector4f(Simulation.TimeQuant / value);
                invMassValue = new Vector4f(1f / value);
                Weight = MassValue * 9.81f;
                if (massType == EMassObjectType.Rod && MassValue < 0.002f) {
                    Weight *= 0.1f;
                }

                CompoundWaterResistance = MassValue * WaterDragConstant;
                Sim.IPhyActionsListener?.ChangeMassValue(UID, value);
            }
        }

        public Single InvMassValue {
            get { return invMassValue.X; }
        }

        public Single Weight { get; private set; }

        public Vector4f InvMassValue4f {
            get { return (!isKinematic) ? invMassValue : Vector4f.Zero; }
        }

        public virtual Vector3 Position {
            get { return Position4f.AsVector3() + visualPositionOffset; }
            set {
                if (Single.IsNaN(value.x) || Single.IsNaN(value.y) || Single.IsNaN(value.z)) {
                    Debug.LogError($"[FISHING]There is a fatal error occured.");
                    return;
                }

                Vector3 vec = value - visualPositionOffset;
                Position4f = vec.AsPhyVector();
                Sim.IPhyActionsListener?.ChangeMassPosition(UID, value);
            }
        }

        public Vector3 VisualPositionOffset {
            get { return visualPositionOffset; }
            set {
                if (Sim.IPhyActionsListener != null) {
                    Sim.IPhyActionsListener.ChangeMassVisualPositionOffset(UID, value);
                    return;
                }

                Vector4f repr = Vector4f.Zero;
                RealVisualDiverge((visualPositionOffset - value).AsPhyVector(ref repr));
            }
        }

        public virtual Quaternion Rotation {
            get { return rotation; }
            set {
                rotation = value;
                Sim.IPhyActionsListener?.ChangeMassRotation(UID, value);
            }
        }

        public Vector3 Velocity {
            get { return Velocity4f.AsVector3(); }
            set {
                Velocity4f = value.AsPhyVector();
                Sim.IPhyActionsListener?.ChangeMassVelocity(UID, value);
            }
        }

        public Vector3 Force {
            get { return force4f.AsVector3(); }
            set { force4f = value.AsPhyVector(); }
        }

        public Vector3 Motor {
            get { return Motor4f.AsVector3(); }
            set {
                Motor4f = value.AsPhyVector(ref Motor4f);
                Sim.IPhyActionsListener?.ChangeMassMotor(UID, value);
            }
        }

        public Vector3 WaterMotor {
            get { return WaterMotor4f.AsVector3(); }
            set {
                WaterMotor4f = value.AsPhyVector(ref WaterMotor4f);
                Sim.IPhyActionsListener?.ChangeMassWaterMotor(UID, value);
            }
        }

        public Single Buoyancy {
            get { return buoyancy4f.X; }
            set {
                buoyancy4f =massType==EMassObjectType.Fish? new Vector4f(value+1) : new Vector4f(value);
                Sim.IPhyActionsListener?.ChangeMassBuoyancy(UID, value);
            }
        }

        public Single WaterDragConstant {
            get { return WaterDragConstant4f.X; }
            set {
                WaterDragConstant4f = new Vector4f(value);
                CompoundWaterResistance = MassValue * WaterDragConstant;
                Sim.IPhyActionsListener?.ChangeMassWaterDragConstant(UID, value);
            }
        }

        public Single AirDragConstant {
            get { return AirDragConstant4f.X; }
            set {
                AirDragConstant4f = new Vector4f(value);
                Sim.IPhyActionsListener?.ChangeMassAirDragConstant(UID, value);
            }
        }

        public Single CompoundWaterResistance { get; private set; }

        public Single BuoyancyAtMinimalSpeed {
            get { return buoyancyAtMinimalSpeed4f.X; }
            set { buoyancyAtMinimalSpeed4f = new Vector4f(value); }
        }

        public Single GroundHeight {
            get { return groundPoint.Y; }
            set {
                extGroundPointPrev = extGroundPointCurrent;
                extGroundPointCurrent.Y = value;
            }
        }

        public Vector4f GroundPoint4f {
            get { return groundPoint; }
        }

        public Vector3 GroundPoint {
            get { return groundPoint.AsVector3(); }
            set {
                extGroundPointPrev = extGroundPointCurrent;
                extGroundPointCurrent = value.AsPhyVector(ref extGroundPointCurrent);
            }
        }

        public Vector4f ExtGroundPointInterpolated {
            get { return Vector4fExtensions.Lerp(extGroundPointPrev, extGroundPointCurrent, Sim.FrameIterationProgress); }
        }

        public Vector4f GroundNormal4f {
            get { return groundNormal; }
        }

        public Vector3 GroundNormal {
            get { return groundNormal.AsVector3(); }
            set {
                extGroundNormalPrev = extGroundNormalCurrent;
                extGroundNormalCurrent = value.AsPhyVector(ref extGroundNormalCurrent);
            }
        }

        public Vector4f ExtGroundNormalInterpolated {
            get { return Vector4fExtensions.Lerp(extGroundNormalPrev, extGroundNormalCurrent, Sim.FrameIterationProgress); }
        }

        public Single WaterHeight {
            get { return 0f; }
        }

        public Boolean DisableSimulation {
            get { return disableSimulation; }
            set {
                disableSimulation = value;
                if (CopyMass != null) {
                    CopyMass.DisableSimulation = value;
                }

                Sim.IPhyActionsListener?.ChangeMassDisableSimualtion(UID, value);
            }
        }

        public ECollisionType Collision {
            get { return collisionType; }
            set {
                collisionType = value;
                Sim.IPhyActionsListener?.ChangeMassCollisionType(UID, value);
            }
        }

        public Single MotionDamping {
            get { return motionDamping; }
            set { motionDamping = value; }
        }

        public Single StaticFrictionFactor {
            get { return staticFrictionFactor; }
            set {
                staticFrictionFactor = (0 <= value ? value : 0f);
                if (slidingFrictionFactor > staticFrictionFactor) {
                    slidingFrictionFactor = staticFrictionFactor;
                }
            }
        }

        public Single SlidingFrictionFactor {
            get { return slidingFrictionFactor; }
            set {
                slidingFrictionFactor = (0 <= value ? value : 0f);
                if (slidingFrictionFactor > staticFrictionFactor) {
                    staticFrictionFactor = slidingFrictionFactor;
                }
            }
        }

        public Boolean IsRef {
            get { return isRef; }
            set {
                isRef = value;
                Sim.IPhyActionsListener?.ChangeMassIsRef(UID, value);
            }
        }

        public Boolean IsKinematic {
            get { return isKinematic; }
            set {
                isKinematic = value;
                Sim.IPhyActionsListener?.ChangeMassIsKinematic(UID, value);
            }
        }

        public Boolean IsFreeze {
            get { return isFreeze;  }
            set {
                isFreeze = value;
                Sim.IPhyActionsListener?.ChangeMassIsFreeze(UID, value);
            }
        }

        public Boolean IsLying {
            get { return Position.y < GroundHeight || Position.y - GroundHeight < radius || (IsCollision && Position.y - GroundHeight < radius * 2f); }
        }

        public virtual Boolean IsCollision {
            get { return isCollision; }
        }

        public virtual Boolean IsTensioned {
            get {
                if (PriorSpring != null) {
                    MassObject mass = PriorSpring.Mass1;
                    Single num = mass.Position4f.Distance(Position4f);
                    if (num >= PriorSpring.SpringLength * 1.0001f) {
                        return true;
                    }
                }

                if (NextSpring != null) {
                    MassObject mass2 = NextSpring.Mass2;
                    Single num2 = mass2.Position4f.Distance(Position4f);
                    if (num2 >= NextSpring.SpringLength * 1.0001f) {
                        return true;
                    }
                }

                return false;
            }
        }

        public EDensityType DensityType {
            get { return densityType; }
        }

        public Boolean IsTrapped {
            get { return EDensityType.Sticky == densityType; }
        }

        public Boolean IsBlocked {
            get { return EDensityType.Sticky == densityType || EDensityType.Sparse == densityType; }
        }

        public Vector3 AvgForce {
            get { return (0 >= avgForceCount) ? Vector3.zero : (avgForce * new Vector4f(1f / avgForceCount)).AsVector3(); }
        }

        public MassObject(Simulation sim, Single mass, Vector3 position, EMassObjectType type,float WaterDragCons=10) {
            Sim = sim;
            UID = Sim.NewUID;
            IID = NewIID;
            massType = type;
            Position4f = position.AsPhyVector();
            VisualPositionOffset = Sim.VisualPositionOffset;
            MassValue = mass;
            Motor4f = Vector4f.Zero;
            WaterMotor4f = Vector4f.Zero;
            buoyancy4f = new Vector4f(-1f);
            BuoyancyAtMinimalSpeed = 0.7f;
            WaterDragConstant = WaterDragCons;
            Collision = ECollisionType.FullBody;
            Vector3 point = position;
            Vector3 normal = Vector3.up;
            MathTool.GetGroundCollision(point, out point, out normal);
            heightChunk = new PlaneSudokuCollider(4, position, 1f, 1f);
            if (EMassObjectType.Padding != Type) {
                Sim.IPhyActionsListener?.CreateAMass(this);
            }

            colliders = new ICollider[6];
        }

        public MassObject(Simulation sim, MassObject source) {
            Sim = sim;
            UID = source.UID;
            IID = -NewIID;
            massType = source.Type;
            Sim.MassDict[UID] = this;
            source.CopyMass = this;
            SourceMass = source;
            MassValue = source.MassValue;
            Position4f = source.Position4f;
            VisualPositionOffset = Sim.VisualPositionOffset;
            Rotation = source.Rotation;
            Velocity = source.Velocity;
            Motor = source.Motor;
            WaterMotor = source.WaterMotor;
            Buoyancy = source.Buoyancy;
            BuoyancyAtMinimalSpeed = source.BuoyancyAtMinimalSpeed;
            WaterDragConstant = source.WaterDragConstant;
            AirDragConstant = source.AirDragConstant;
            groundPoint = source.groundPoint;
            FlowVelocity = source.FlowVelocity;
            WindVelocity = source.WindVelocity;
            IgnoreEnvironment = source.IgnoreEnvironment;
            IgnoreEnvForces = source.IgnoreEnvForces;
            IsRef = source.IsRef;
            IsKinematic = source.IsKinematic;
            IsFreeze = source.IsFreeze;
            CurrentVelocityLimit = source.CurrentVelocityLimit;
            Collision = source.Collision;
            isCollision = source.isCollision;
            staticFrictionFactor = source.staticFrictionFactor;
            slidingFrictionFactor = source.slidingFrictionFactor;
            Radius = source.Radius;
            if (source.heightChunk != null) {
                heightChunk = new PlaneSudokuCollider(source.heightChunk);
            }

            colliders = new ICollider[6];
            if (source.colliders != null) {
                Array.Copy(source.colliders, colliders, source.colliders.Length);
            }

            extGroundPointPrev = source.extGroundPointPrev;
            extGroundPointCurrent = source.extGroundPointCurrent;
            extGroundNormalPrev = source.extGroundNormalPrev;
            extGroundNormalCurrent = source.extGroundNormalCurrent;
        }

        public virtual void KinematicTranslate(Vector4f offset) {
            Position += offset.AsVector3();
        }

        protected Boolean isstopMass;
        public virtual void StopMass() {
            Force = Vector3.zero;
            Velocity = Vector3.zero;
            Motor = Vector3.zero;
            WaterMotor4f = Vector4f.Zero;
            perFrameForcesCache4f = Vector4f.Zero;
            perPeriodForcesCache4f = Vector4f.Zero;
            Sim.IPhyActionsListener?.StopMass(UID);
            SetStopMass(true);
        }

        public void SetStopMass(Boolean state = true) {
            if (Sim != null)
                Sim.IPhyActionsListener?.SetStopMassState(UID, state);
            isstopMass = state;
        }

        protected virtual void RealVisualDiverge(Vector4f realPositionOffset) {
            visualPositionOffset -= realPositionOffset.AsVector3();
            visualPositionOffset4f = visualPositionOffset.AsPhyVector(ref visualPositionOffset4f);
            Position4f += realPositionOffset;
        }

        public virtual void UpdateReferences() { }

        public virtual void DetectRefLeaks(List<MassObject> masses, List<ConnectionBase> connections, List<PhysicsObject> objects, StringBuilder msg) {
            if (PriorSpring != null && NextSpring != null && (!(Sim as SimulationSystem).DictConnections.ContainsKey(NextSpring.UID) || NextSpring.Mass1.Sim != Sim)) {
                msg.AppendFormat($"{UID}.{Type} {GetType()}: NextSpring {NextSpring.UID}.{NextSpring.GetType()} [{NextSpring.Mass1.UID}.{NextSpring.Mass1.Type} {NextSpring.Mass2.UID}.{NextSpring.Mass2.Type}] is not present in the system.\n");
                connections.Add(NextSpring);
            }

            if (NextSpring != null && PriorSpring != null && (!(Sim as SimulationSystem).DictConnections.ContainsKey(PriorSpring.UID) || PriorSpring.Mass1.Sim != Sim)) {
                msg.AppendFormat($"{UID}.{Type} {GetType()}: PriorSpring {NextSpring.UID}.{PriorSpring.GetType()} [{PriorSpring.Mass1.UID}.{PriorSpring.Mass1.Type} {PriorSpring.Mass2.UID}.{PriorSpring.Mass2.Type}] is not present in the system.\n");
                connections.Add(PriorSpring);
            }
        }

        public virtual void SyncMain(MassObject source) {
            source.extGroundPointCurrent = extGroundPointCurrent;
            source.extGroundPointPrev = extGroundPointPrev;
            source.extGroundNormalCurrent = extGroundNormalCurrent;
            source.extGroundNormalPrev = extGroundNormalPrev;
            if (source.heightChunk != null && heightChunk != null) {
                source.heightChunk.Sync(heightChunk);
            }

            if (source.colliders != null) {
                Array.Copy(colliders, source.colliders, source.colliders.Length);
            }

            Position4f = source.Position4f;
            Velocity4f = source.Velocity4f;
            rotation = source.Rotation;
            avgForce = source.avgForce;
            isstopMass = source.isstopMass;
            avgForceCount = source.avgForceCount;
            visualPositionOffset = source.visualPositionOffset;
            visualPositionOffset4f = visualPositionOffset.AsPhyVector(ref visualPositionOffset4f);
            IsLimitBreached = source.IsLimitBreached;
            currentVelocityLimit = source.currentVelocityLimit;
            currentVelocityDeltaLimit = source.currentVelocityDeltaLimit;
            isCollision = source.isCollision;
            densityType = source.DensityType;
            dragForce = source.dragForce;
            UpdateGroundData();
            source.groundPoint = groundPoint;
            source.groundNormal = groundNormal;
        }

        protected virtual void UpdateGroundData() {
            Vector3 point = Position;
            Vector3 normal = Vector3.up;
            Boolean hit = false;
            MathTool.GetGroundCollision(point, out hit, out point, out normal, 0.5f);
            groundPoint = point.AsPhyVector(ref groundPoint);
            groundNormal = normal.AsPhyVector(ref groundPoint);
        }

        public virtual void ApplyForce(Vector3 force, Boolean capture = false) {
            Vector4f repr = Vector4f.Zero;
            ApplyForce(force.AsPhyVector(ref repr), capture);
        }

        public virtual void ApplyForce(Vector4f force, Boolean capture = false) {
            if (!isKinematic) {
                force4f += force;
                if (capture) {
                    UpdateAvgForce(force);
                }

                Sim.IPhyActionsListener?.ChangeMassAppliedForce(UID, force.AsVector3());
            }
        }

        public virtual void Reset() {
            if (!IsKinematic) {
                Force = Vector3.zero;
                Sim.IPhyActionsListener?.ResetMass(UID);
            }
        }

        public virtual void Simulate() {
            if (isKinematic) {
                if (Collision != ECollisionType.FullBody) {
                    groundPoint = ExtGroundPointInterpolated;
                    groundNormal = ExtGroundNormalInterpolated;
                }
                return;
            }

            if (isFreeze) {
                return;
            }

            if (ForceFactor > 0 && !Mathf.Approximately(0, forceFactor)) {
                Velocity4f += force4f * invMassValueDelta * ForceFactor;
                ForceFactor = 0;
            }
            else {
                Velocity4f += force4f * invMassValueDelta;
            }

            if (CurrentVelocityLimit > 0f && Velocity4f.Magnitude() > CurrentVelocityLimit) {
                Velocity4f *= new Vector4f(CurrentVelocityLimit / Velocity4f.Magnitude());
                IsLimitBreached = true;
            }

            Vector4f posNew4f;
            if (!IgnoreEnvForces && Position4f.Y < DefaultRadius) {
                Vector4f velocity4f = Velocity4f - FlowVelocity;
                velocity4f.W = 0f;
                Single speed = Mathf.Clamp(velocity4f.Magnitude(), 0f, CurrentVelocityLimit * 0.5f);
                Single radiusFactor = 0.5f * (1f - Mathf.Clamp(Position4f.Y, -DefaultRadius, DefaultRadius) / DefaultRadius);
                Single waterResitanceFactor = CompoundWaterResistance * radiusFactor * invMassValue.X * Simulation.TimeQuant;
                Vector4f flowVelocityDelta = FlowVelocity * Simulation.TimeQuant4f;
                if (speed < 1f) {
                    Velocity4f += new Vector4f(waterResitanceFactor * (0.5f * waterResitanceFactor - 1f)) * velocity4f;
                    flowVelocityDelta += new Vector4f((1f - 0.5f * waterResitanceFactor) * Simulation.TimeQuant) * velocity4f;
                }
                else {
                    Velocity4f += new Vector4f(1f / (1f + speed * waterResitanceFactor) - 1f) * velocity4f;
                    flowVelocityDelta += new Vector4f((1f - 0.5f * speed * waterResitanceFactor) * Simulation.TimeQuant) * velocity4f;
                }
                posNew4f = Position4f + flowVelocityDelta;
            }
            else {
                posNew4f = Position4f + Velocity4f * Simulation.TimeQuant4f;
            }

            EDensityType lastDensityType = densityType;
            densityType = EDensityType.None;
            if (ECollisionType.FullBody == Collision) {
                isCollision = false;
                Vector4f posNewWithVisualOffset4f = posNew4f + visualPositionOffset4f;
                Vector4f posHit4f = Vector4f.Zero;
                Vector3 normal3f = Vector3.zero;
                Vector3 posNewWithVisualOffset3f = posNewWithVisualOffset4f.AsVector3();
                Vector3 prevPoint = Position;
                Vector3? posCollider = null;
                Vector3? forceCollider = null;
                if (0 < colliders.Length && null != colliders[0]) {
                    for (Int32 i = 0; i < colliders.Length; i++) {
                        if (colliders[i] != null) {
                            posHit4f = colliders[i].TestPoint(posNewWithVisualOffset3f, prevPoint, out normal3f).AsPhyVector();
                            if (!posHit4f.IsZero()) {
                                if (colliders[i] is BoxCollider) {
                                    posCollider = (colliders[i] as BoxCollider).Position;
                                    forceCollider = (colliders[i] as BoxCollider).ForceDampingFactor;
                                }
                                else if (colliders[i] is SphereCollider) {
                                    posCollider = (colliders[i] as SphereCollider).position;
                                    forceCollider = (colliders[i] as SphereCollider).ForceDampingFactor;
                                }

                                densityType = colliders[i].DensityType;
                                Vector4f normal4f = normal3f.AsPhyVector();
                                Single dot = Vector4fExtensions.Dot(normal4f, Velocity4f);
                                Vector4f vDelta = normal4f * new Vector4f(dot);
                                // Velocity4f = FrictionVelocity(Velocity4f, vDelta);
                                // Velocity4f += ((0 > dot ? (vDelta.Negative() * surfaceBounce4f) : vDelta));

                                if (EDensityType.Solid == colliders[i].DensityType) {
                                    posNew4f += posHit4f;
                                }

                                if (EDensityType.Sticky == colliders[i].DensityType) {
                                    Velocity4f += (vDelta.Negative() * surfaceBounce4f);
                                    if (posCollider.HasValue && 0.01f >= Math.Abs(prevPoint.y - posCollider.Value.y)) {
                                        if (forceCollider.Value.magnitude > dragForce.Magnitude()) {
                                            densityType = EDensityType.Sticky;
                                        }
                                        else {
                                            densityType = EDensityType.Sparse;
                                        }
                                    }
                                    else {
                                        densityType = EDensityType.Sparse;
                                    }
                                }
                                else {
                                    if (0 > dot) {
                                        Velocity4f += (vDelta.Negative() * colliders[i].FrictionFactor);
                                    }
                                }

                                prevPoint = posNewWithVisualOffset3f;
                                posNewWithVisualOffset4f = posNew4f + visualPositionOffset4f;
                                posNewWithVisualOffset3f = posNewWithVisualOffset4f.AsVector3();
                                isCollision = true;
                            }
                        }
                    }
                }

                if (!isCollision) {
                    ResetDragForce();
                }

                if (null != heightChunk) {
                    posHit4f = heightChunk.TestPoint(posNewWithVisualOffset3f, Position, out normal3f).AsPhyVector();
                    if (!posHit4f.IsZero()) {
                        posNew4f += posHit4f;
                        Vector4f normal4f = normal3f.AsPhyVector();
                        Single dot = Vector4fExtensions.Dot(normal4f, Velocity4f);
                        Vector4f vDelta = normal4f * new Vector4f(dot);
                        Velocity4f = FrictionVelocity(Velocity4f, vDelta);
                        Velocity4f += (0 > dot ? (vDelta.Negative() * surfaceBounce4f) : vDelta);

                        isCollision = true;
                    }
                }
            }
            else if (ECollisionType.ExternalPlane == Collision) {
                isCollision = false;
                Single posHeight = Mathf.Max(0f, ExtGroundPointInterpolated.Y - Position.y);
                if (posHeight > 0f) {
                    posNew4f += new Vector4f(0f, posHeight, 0f, 0f);
                    Vector4f extGroundNormalInterpolated = ExtGroundNormalInterpolated;
                    Single dot = Vector4fExtensions.Dot(extGroundNormalInterpolated, Velocity4f);
                    Vector4f vDelta = extGroundNormalInterpolated * new Vector4f(dot);
                    Velocity4f = FrictionVelocity(Velocity4f, vDelta);
                    Velocity4f += (0 > dot ? (vDelta.Negative() * surfaceBounce4f) : vDelta);
                    isCollision = true;
                }
            }

            if (!IsTrapped) {
                Position4f = posNew4f;
            }
        }

        public Vector4f FrictionVelocity(Vector4f velocity, Vector4f velocityNormal) {
            Vector4f velocityOffset = velocity - velocityNormal;
            Single velocityNormalValue = velocityNormal.Magnitude() * (Sim.FrameDeltaTime / Simulation.TimeQuant);
            Single velocityOffsetValue = velocityOffset.Magnitude();
            Single velocityNormalStaticValue = velocityNormalValue * StaticFrictionFactor;
            Vector4f result = Vector4f.Zero;
            if (velocityOffsetValue > velocityNormalStaticValue) {
                Single velocityNormalSlidingValue = velocityNormalValue * SlidingFrictionFactor;
                if (velocityOffsetValue > velocityNormalSlidingValue) {
                    result = velocity * new Vector4f((velocityOffsetValue - velocityNormalSlidingValue) / velocityOffsetValue);
                }
            }
            return result;
        }

        public void ResetAvgForce() {
            avgForce = Vector4f.Zero;
            avgForceCount = 0;
        }

        public void ResetDragForce() {
            dragForce = Vector4f.Zero;
        }

        public void UpdateAvgForce(Vector4f force) {
            avgForce += force;
            avgForceCount++;
        }

        public void UpdateDragForce(Vector4f force) {
            dragForce = force;
        }

        public Vector4f GetFrictionFroce() {
            if (!IsLying) {
                return Vector4f.Zero;
            }

            Boolean isStatic = Velocity4f.Magnitude() > 0;
            Vector4f dirNormal = groundNormal.Normalized();
            Vector4f dirForce = force4f + Vector3.down.AsVector4f();
            Single fFrictionValue = Weight * (Vector3.Dot(Vector3.down, dirNormal.AsVector3())) * (isStatic ? staticFrictionFactor : slidingFrictionFactor);
            fFrictionValue = Math.Abs(fFrictionValue);
            Vector4f fFriction = Velocity4f.Negative().Normalized() * fFrictionValue;
            return fFriction;
        }

        public override string ToString() {
            return $"{Type}-{UID} Mass:{MassValue}({Weight}) Force:{force4f} Velocity:{Velocity4f} AvgForce:({avgForceCount}){avgForce} Motor:{Motor4f}";
        }
    }
}