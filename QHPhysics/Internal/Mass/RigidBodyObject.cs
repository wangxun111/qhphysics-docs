using System;
using System.Collections.Generic;
using UnityEngine;
using Mono.Simd;
using Mono.Simd.Math;

namespace QH.Physics {
    public class RigidBodyObject : MassObject {
        public const Single MaxAngularVelocity = (Single)Math.PI * 20f;
        public const Int32 MaxCollision = 20;

        private Vector4f worldInertiaTensorFactor4f;
        private Matrix4x4 worldInverseInertiaTensor;
        private Vector4f angleKahanDelta4f;
        private Single rotationDampingFactor;
        private Vector4f rotationDampingFactorPoweredTime4f;
        private Single underwaterRotationDampingFactor;
        private Vector4f underwaterRotationDampingFactorPoweredTime4f;

        protected Vector3 inertiaTensorFactor;
        protected Vector4f inertiaTensorFactor4f;
        protected Quaternion invRotation;

        public Vector3 Dimensions;
        public Vector4f AngularVelocity4f;
        public Vector4f Torque4f;
        public Vector3 BuoyancyNormal;
        public Single RotationalBuoyancyFactor;
        public Single WaterY;
        public Vector3 AxialWaterDragFactor;
        public Boolean AxialWaterDragEnabled;
        public CollisionContactPointData[] CollisionContactPointDatas;
        public Int32 CollisionPlaneCount;
        public Single BounceFactor;
        public Single ExtrudeFactor = 0.01f;

        public Vector4f InertiaTensorFactor4f {
            get { return inertiaTensorFactor4f; }
            set {
                inertiaTensorFactor4f = value;
                inertiaTensorFactor = inertiaTensorFactor4f.AsVector3();
            }
        }

        public Vector4f WorldInertiaTensorFactor4f {
            get { return worldInertiaTensorFactor4f; }
        }

        public Matrix4x4 WorldInverseInertiaTensor {
            get { return worldInverseInertiaTensor; }
        }

        public Single RotationDampingFactor {
            get { return rotationDampingFactor; }
            set {
                rotationDampingFactor = value;
                rotationDampingFactorPoweredTime4f = new Vector4f(Mathf.Pow(value, Simulation.TimeQuant));
            }
        }

        public override Quaternion Rotation {
            get { return rotation; }
            set {
                base.Rotation = value;
                invRotation = Quaternion.Inverse(rotation);
                UpdateGlobalInertiaTensor();
            }
        }

        public Quaternion InvRotation {
            get { return invRotation; }
        }

        public Single UnderwaterRotationDampingFactor {
            get { return underwaterRotationDampingFactor; }
            set {
                underwaterRotationDampingFactor = value;
                underwaterRotationDampingFactorPoweredTime4f = new Vector4f(Mathf.Pow(value, Simulation.TimeQuant));
            }
        }

        public override Boolean IsCollision {
            get {
                if (base.IsCollision) {
                    return true;
                }

                for (Int32 i = 0; i < CollisionPlaneCount; i++) {
                    if (CollisionContactPointDatas[i].Distance < 0f) {
                        return true;
                    }
                }

                return false;
            }
        }

        public Vector4f CollisionImpulseVelocity4f { get; protected set; }

        public RigidBodyObject(Simulation simulation, Single massValue, Vector3 position, EMassObjectType massType, Single radius = DefaultRadius) : base(simulation, massValue, Vector3.zero, massType) {
            InertiaTensorFactor4f = new Vector4f(0.4f * massValue);
            Dimensions = new Vector3(2f * radius, 2f * radius, 2f * radius);
            Rotation = Quaternion.identity;
            BuoyancyNormal = Vector3.up;
            RotationalBuoyancyFactor = 0f;
            WaterY = 0f;
            AxialWaterDragFactor = Vector3.one;
            AxialWaterDragEnabled = true;
            RotationDampingFactor = 1f;
            Position = position;
            angleKahanDelta4f = Vector4f.Zero;
            base.Radius = radius;
            CollisionContactPointDatas = new CollisionContactPointData[MaxCollision];
            for (Int32 i = 0; i < MaxCollision; i++) {
                CollisionContactPointDatas[i] = default(CollisionContactPointData);
            }

            if (Mathf.Approximately(worldInertiaTensorFactor4f.X, 0f) || Mathf.Approximately(worldInertiaTensorFactor4f.Y, 0f) || Mathf.Approximately(worldInertiaTensorFactor4f.Z, 0f)) {
                throw new ArithmeticException($"RigidBody's worldInertiaTensorFactor4f with uid {base.UID} is Invalid.");
            }
        }

        public RigidBodyObject(Simulation simulation, RigidBodyObject sourceRigidBody) : base(simulation, sourceRigidBody) {
            InertiaTensorFactor4f = sourceRigidBody.InertiaTensorFactor4f;
            Dimensions = sourceRigidBody.Dimensions;
            Rotation = sourceRigidBody.Rotation;
            BuoyancyNormal = sourceRigidBody.BuoyancyNormal;
            RotationalBuoyancyFactor = sourceRigidBody.RotationalBuoyancyFactor;
            WaterY = sourceRigidBody.WaterY;
            AxialWaterDragFactor = sourceRigidBody.AxialWaterDragFactor;
            AxialWaterDragEnabled = sourceRigidBody.AxialWaterDragEnabled;
            RotationDampingFactor = sourceRigidBody.RotationDampingFactor;
            UnderwaterRotationDampingFactor = sourceRigidBody.UnderwaterRotationDampingFactor;
            AngularVelocity4f = sourceRigidBody.AngularVelocity4f;

            if (Mathf.Approximately(base.MassValue, 0f) || Mathf.Approximately(InertiaTensorFactor4f.X, 0f) || Mathf.Approximately(InertiaTensorFactor4f.Y, 0f) || Mathf.Approximately(InertiaTensorFactor4f.Z, 0f)) {
                Debug.LogError(String.Concat("RigidBody ", base.UID, " ", base.IID, " invalid params MassValue = ", base.MassValue, " InertiaTensor = ", InertiaTensorFactor4f, " source.IID = ", sourceRigidBody.IID));
            }

            if (!AngularVelocity4f.IsFinite()) {
                throw new ArithmeticException("AngularVelocity of RigidBody " + base.UID + " is NAN.");
            }

            if (Mathf.Approximately(worldInertiaTensorFactor4f.X, 0f) || Mathf.Approximately(worldInertiaTensorFactor4f.Y, 0f) || Mathf.Approximately(worldInertiaTensorFactor4f.Z, 0f)) {
                throw new ArithmeticException("_globalInertiaTensor of RigidBody " + base.UID + " is NAN.");
            }

            Torque4f = sourceRigidBody.Torque4f;
            angleKahanDelta4f = Vector4f.Zero;
            BounceFactor = sourceRigidBody.BounceFactor;
        }

        private void UpdateGlobalInertiaTensor() {
            Matrix4x4 matrix4x = default(Matrix4x4);
            matrix4x.SetTRS(Vector3.zero, Rotation, Vector3.one);
            Matrix4x4 matrix4x2 = default(Matrix4x4);
            matrix4x2.m00 = InertiaTensorFactor4f.X;
            matrix4x2.m11 = InertiaTensorFactor4f.Y;
            matrix4x2.m22 = InertiaTensorFactor4f.Z;
            matrix4x2.m33 = 1f;
            Matrix4x4 matrix4x3 = matrix4x2;
            matrix4x3 = matrix4x * matrix4x3 * matrix4x.inverse;
            worldInertiaTensorFactor4f = new Vector4f(matrix4x3.m00, matrix4x3.m11, matrix4x3.m22, 1f);
            worldInverseInertiaTensor = matrix4x3.inverse;
        }

        public static Vector4f CalculateSolidBoxInertiaTensor(Single massValue, Single width, Single height, Single depth) {
            Single num = massValue / 12f;
            return new Vector4f(num * (height * height + depth * depth), num * (width * width + depth * depth), num * (width * width + height * height), 1f);
        }

        public virtual void CalculateInertiaTensor() { }

        public Vector4f PointLocalToWorld4f(Vector3 localPoint) {
            return (rotation * localPoint).AsPhyVector(ref Position4f) + Position4f;
        }

        public Vector3 PointLocalToWorld(Vector3 localPoint) {
            return rotation * localPoint + Position;
        }

        public Vector4f PointWorldToLocal4f(Vector3 worldPoint) {
            return (InvRotation * (worldPoint - Position)).AsPhyVector(ref Position4f);
        }

        public Vector3 PointWorldToLocal(Vector3 worldPoint) {
            return InvRotation * (worldPoint - Position);
        }

        public Vector4f PointLocalToWorldVelocity4f(Vector3 localpoint) {
            return Velocity4f + Vector4fExtensions.Cross(AngularVelocity4f, (rotation * localpoint).AsPhyVector(ref AngularVelocity4f));
        }

        public void ApplyForce(Vector4f force4f, Vector3 localPoint, Boolean capture = false) {
            ApplyForce(force4f, capture);
            if (!IsKinematic) {
                Vector3 vector = invRotation * force4f.AsVector3();
                Torque4f += Vector4fExtensions.Cross((rotation * localPoint).AsPhyVector(ref Torque4f), force4f);
            }
        }

        public void ApplyLocalForce(Vector3 localForce, Vector3 localPoint, Boolean capture = false) {
            ApplyForce((rotation * localForce).AsPhyVector(ref Torque4f), capture);
            if (!IsKinematic) {
                Torque4f += Vector4fExtensions.Cross((rotation * localPoint).AsPhyVector(ref Torque4f), (rotation * localForce).AsPhyVector(ref Torque4f));
            }
        }

        public void ApplyLocalForce(Vector3 localForce, Boolean capture = false) {
            ApplyForce((rotation * localForce).AsPhyVector(), capture);
        }

        public void ApplyTorque(Vector4f torque4f) {
            if (!IsKinematic) {
                Torque4f += torque4f;
            }
        }

        public void ApplyImpulse(Vector4f impulse4f, Vector3 localPoint) {
            if (!IsKinematic) {
                Vector4f vector4f = PointLocalToWorld4f(localPoint);
                Vector4f a = vector4f - Position4f;
                Vector4f vector4f2 = Vector4fExtensions.Cross(a, impulse4f);
                Vector4f vector4f3 = vector4f2 / WorldInertiaTensorFactor4f - angleKahanDelta4f;
                Vector4f vector4f4 = AngularVelocity4f + vector4f3;
                angleKahanDelta4f = vector4f4 - AngularVelocity4f - vector4f3;
                AngularVelocity4f = vector4f4;
                Velocity4f += impulse4f * invMassValue;
            }
        }

        protected void UpdateBuoyantRotation() {
            if (!IsKinematic) {
                Vector3 vector = Vector3.Cross(rotation * BuoyancyNormal, Vector3.up);
                Torque4f += (invRotation * vector * RotationalBuoyancyFactor).AsPhyVector(ref Torque4f);
            }
        }

        protected void UpdateAxialWaterDrag() {
            Vector3 vector = invRotation * (FlowVelocity - Velocity4f).AsVector3();
            ApplyLocalForce(Vector3.right * vector.x * Mathf.Max(Mathf.Abs(vector.x), 1f) * AxialWaterDragFactor.x);
            ApplyLocalForce(Vector3.up * vector.y * Mathf.Max(Mathf.Abs(vector.y), 1f) * AxialWaterDragFactor.y);
            ApplyLocalForce(Vector3.forward * vector.z * Mathf.Max(Mathf.Abs(vector.z), 1f) * AxialWaterDragFactor.z);
        }

        protected void UpdateAxialForceWaterConstraint() {
            Vector3 vector = invRotation * force4f.AsVector3();
            Vector3 normalized = AxialWaterDragFactor.normalized;
            vector.x *= 1f - normalized.x;
            vector.y *= 1f - normalized.y;
            vector.z *= 1f - normalized.z;
            force4f = (rotation * vector).AsPhyVector();
        }

        public override void Reset() {
            base.Reset();
            Torque4f = Vector4f.Zero;
            angleKahanDelta4f = Vector4f.Zero;
        }

        public override void StopMass() {
            base.StopMass();
            Torque4f = Vector4f.Zero;
            AngularVelocity4f = Vector4f.Zero;
            angleKahanDelta4f = Vector4f.Zero;
        }

        public override void SyncMain(MassObject sourceMass) {
            base.SyncMain(sourceMass);
            invRotation = Quaternion.Inverse(rotation);
            RigidBodyObject rigidBody = sourceMass as RigidBodyObject;
            CollisionImpulseVelocity4f = rigidBody.CollisionImpulseVelocity4f;
            rigidBody.CollisionImpulseVelocity4f = Vector4f.Zero;
        }

        public void UpdateCollisions(Dictionary<Int32, Collision> collisionDict, LayerMask layerMask) {
            if (collisionDict == null) {
                return;
            }
            base.Motor = Vector3.zero;
            Int32 num = 0;
            foreach (Collision value in collisionDict.Values) {
                if (value.gameObject == null || ((1 << value.gameObject.layer) & layerMask.value) == 0) {
                    continue;
                }
                for (Int32 i = 0; i < value.contacts.Length; i++) {
                    Vector3 point = value.contacts[i].point;
                    Vector3 normal = value.contacts[i].normal;
                    Single separation = value.contacts[i].separation;
                    CollisionContactPointDatas[num].Point = point.AsVector4f();
                    CollisionContactPointDatas[num].LocalPoint = PointWorldToLocal(point + separation * normal);
                    CollisionContactPointDatas[num].Normal = normal.AsVector4f();
                    CollisionContactPointDatas[num].Distance = separation;
                    CollisionContactPointDatas[num].Bounce = 1f;
                    CollisionContactPointDatas[num].DynamicFrictionFactor = 1f;
                    CollisionContactPointDatas[num].StaticFrictionFactor = 1f;
                    if (value.collider.material != null) {
                        CollisionContactPointDatas[num].DynamicFrictionFactor = value.collider.material.dynamicFriction;
                        CollisionContactPointDatas[num].StaticFrictionFactor = value.collider.material.staticFriction;
                        CollisionContactPointDatas[num].Bounce = value.collider.material.bounciness;
                    }
                    num++;
                    if (num == CollisionContactPointDatas.Length) {
                        break;
                    }
                }
                if (num != CollisionContactPointDatas.Length) {
                    continue;
                }
                break;
            }
            CollisionPlaneCount = num;
        }

        public virtual void SyncCollisionPlanes(RigidBodyObject source) {
            if (source.CollisionContactPointDatas == null) {
                return;
            }

            if (CollisionContactPointDatas == null) {
                CollisionContactPointDatas = new CollisionContactPointData[source.CollisionContactPointDatas.Length];
                for (Int32 i = 0; i < CollisionContactPointDatas.Length; i++) {
                    CollisionContactPointDatas[i] = default(CollisionContactPointData);
                }
            }

            Array.Copy(source.CollisionContactPointDatas, CollisionContactPointDatas, CollisionContactPointDatas.Length);
            CollisionPlaneCount = source.CollisionPlaneCount;
        }

        protected virtual void ResolveCollisions() {
            Vector4f vector4f = Position.AsPhyVector(ref Position4f);
            Vector4f velocity4f = Velocity4f;
            if (CollisionContactPointDatas != null) {
                for (Int32 i = 0; i < CollisionContactPointDatas.Length && i < CollisionPlaneCount; i++) {
                    Single distance = CollisionContactPointDatas[i].Distance;
                    Vector4f normal = CollisionContactPointDatas[i].Normal;
                    Vector4f repr = CollisionContactPointDatas[i].Point;
                    Vector3 localPoint = CollisionContactPointDatas[i].LocalPoint;
                    Vector4f vector4f2 = PointLocalToWorldVelocity4f(localPoint);
                    Vector4f vector4f3 = PointLocalToWorld(localPoint).AsPhyVector(ref repr);
                    Single num = Vector4fExtensions.Dot(vector4f3 - repr, normal);
                    if (!(Mathf.Abs(localPoint.x) > Dimensions.x) && !(Mathf.Abs(localPoint.y) > Dimensions.y) && !(Mathf.Abs(localPoint.z) > Dimensions.z) && num < 0f) {
                        Single num2 = Vector4fExtensions.Dot(vector4f2, normal);
                        Vector4f vector4f4 = vector4f2 - normal * new Vector4f(num2);
                        Vector4f vector4f5 = vector4f4 - FrictionVelocity(vector4f2, vector4f4);
                        Vector4f vector4f6 = vector4f2 * Simulation.TimeQuant4f;
                        Single num3 = num2 * Simulation.TimeQuant;
                        Single num4 = (0f - ExtrudeFactor) * num / Sim.FrameDeltaTime;
                        CollisionContactPointDatas[i].Distance += num4 * Simulation.TimeQuant;
                        if (num2 < 2f * num4) {
                            Vector4f vector4f7 = repr - vector4f;
                            Vector4f vector4f8 = new Vector4f(base.InvMassValue + Vector4fExtensions.Dot(normal, Vector4fExtensions.Cross(Vector4fExtensions.Cross(vector4f7, normal) / WorldInertiaTensorFactor4f, vector4f7)));
                            ApplyImpulse((normal * new Vector4f(num4 - (1f + BounceFactor) * num2) - vector4f5) / vector4f8, localPoint);
                        }
                    }
                }
            }

            CollisionImpulseVelocity4f += Velocity4f - velocity4f;
        }

        public override void Simulate() {
            if (Position4f.Y <= WaterY && AxialWaterDragEnabled) {
                UpdateAxialWaterDrag();
            }

            if (base.Collision == ECollisionType.RigidbodyContacts) {
                ResolveCollisions();
            }

            base.Simulate();
            if (!IsKinematic) {
                if (Position4f.Y <= WaterY) {
                    AngularVelocity4f *= underwaterRotationDampingFactorPoweredTime4f;
                }
                else {
                    AngularVelocity4f *= rotationDampingFactorPoweredTime4f;
                }

                Vector4f vector4f = WorldInverseInertiaTensor.MultiplyPoint(Torque4f.AsVector3()).AsPhyVector(ref Torque4f);
                vector4f = Vector4f.Zero;
                Vector4f vector4f2 = vector4f * Simulation.TimeQuant4f - angleKahanDelta4f;
                Vector4f vector4f3 = AngularVelocity4f + vector4f2;
                angleKahanDelta4f = vector4f3 - AngularVelocity4f - vector4f2;
                AngularVelocity4f = vector4f3;
                Single num = AngularVelocity4f.Magnitude();
                if (num > (Single)Math.PI * 20f) {
                    AngularVelocity4f *= new Vector4f((Single)Math.PI * 20f / num);
                }

                Vector4f vec = AngularVelocity4f * Simulation.TimeQuant4f;
                Single num2 = vec.Magnitude();
                if (!Mathf.Approximately(num2, 0f)) {
                    Vector3 axis = vec.AsVector3() / num2;
                    rotation = Quaternion.AngleAxis(num2 * (180 / (Single)Math.PI), axis) * rotation;
                }

                rotation = Vector4fExtensions.NormalizeQuaternion(rotation);
                invRotation = Quaternion.Inverse(rotation);
                UpdateGlobalInertiaTensor();
            }
        }
    }
}