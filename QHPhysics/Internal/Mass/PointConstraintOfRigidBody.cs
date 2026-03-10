using System;
using UnityEngine;
using Mono.Simd;
using Mono.Simd.Math;

namespace QH.Physics {
    public class PointConstraintOfRigidBody : PointOfRigidBody {
        public Single BounceFactor;
        public Single FrictionFactor;

        public Vector3[] RaycastLocalDirections;
        public Vector4f[] CollisionPlanePoint4fs;
        public Vector4f[] CollisionPlaneNormal4fs;

        public PointConstraintOfRigidBody(Simulation simulation, RigidBodyObject rigidBody, Vector3 localPosition, EMassObjectType massType) : base(simulation, rigidBody, localPosition, massType) { }

        public PointConstraintOfRigidBody(Simulation simulation, PointConstraintOfRigidBody sourceCPORB) : base(simulation, sourceCPORB) {
            BounceFactor = sourceCPORB.BounceFactor;
            FrictionFactor = sourceCPORB.FrictionFactor;
            RaycastLocalDirections = sourceCPORB.RaycastLocalDirections;
        }

        public void SyncCollisionPlanes(PointConstraintOfRigidBody source) {
            if (RaycastLocalDirections == null) {
                RaycastLocalDirections = source.RaycastLocalDirections;
            }

            if (source.CollisionPlanePoint4fs != null) {
                if (CollisionPlanePoint4fs == null) {
                    CollisionPlanePoint4fs = new Vector4f[source.CollisionPlanePoint4fs.Length];
                    CollisionPlaneNormal4fs = new Vector4f[source.CollisionPlaneNormal4fs.Length];
                }

                Array.Copy(source.CollisionPlanePoint4fs, CollisionPlanePoint4fs, CollisionPlanePoint4fs.Length);
                Array.Copy(source.CollisionPlaneNormal4fs, CollisionPlaneNormal4fs, CollisionPlaneNormal4fs.Length);
            }
        }

        public override void Simulate() {
            base.Simulate();
            Vector4f up = Vector4fExtensions.up;
            Single num = Vector4fExtensions.Dot(Velocity4f, up);
            Vector4f vector4f = (Velocity4f - up * new Vector4f(num)) * new Vector4f(FrictionFactor);
            if (base.Collision == ECollisionType.ExternalPlane) {
                if (Position4f.Y < base.GroundHeight) {
                    RigidBody.ApplyForce(up * RigidBody.MassValue4f * new Vector4f(1f), base.LocalPosition);
                }

                if (Position4f.Y + Velocity4f.Y * Simulation.TimeQuant <= base.GroundHeight && num < 0f) {
                    Vector4f vector4f2 = Position4f - RigidBody.Position4f;
                    Single invMassValue = RigidBody.InvMassValue;
                    Single num2 = Vector4fExtensions.Dot(up, Vector4fExtensions.Cross(Vector4fExtensions.Cross(vector4f2, up) / RigidBody.WorldInertiaTensorFactor4f, vector4f2));
                    Single f = (0f - (1f + BounceFactor)) * num / (invMassValue + num2);
                    RigidBody.ApplyImpulse(up * new Vector4f(f) - vector4f, base.LocalPosition);
                }
            }

            if (CollisionPlanePoint4fs == null) {
                return;
            }

            for (Int32 i = 0; i < CollisionPlanePoint4fs.Length; i++) {
                up = CollisionPlaneNormal4fs[i];
                Vector4f vector4f3 = CollisionPlanePoint4fs[i];
                num = Vector4fExtensions.Dot(Velocity4f, up);
                vector4f = (Velocity4f - up * new Vector4f(num)) * new Vector4f(FrictionFactor);
                Vector4f a = Position4f + Velocity4f * Simulation.TimeQuant4f - vector4f3;
                Vector4f a2 = Position4f - vector4f3;
                if (Vector4fExtensions.Dot(a, up) <= 0f && num <= 0.001f) {
                    Single num3 = Mathf.Min(Vector4fExtensions.Dot(a2, up), 0f);
                    Vector4f vector4f4 = Position4f - RigidBody.Position4f;
                    Single invMassValue2 = RigidBody.InvMassValue;
                    Single num4 = Vector4fExtensions.Dot(up, Vector4fExtensions.Cross(Vector4fExtensions.Cross(vector4f4, up) / RigidBody.WorldInertiaTensorFactor4f, vector4f4));
                    Single f2 = (0f - (1f + BounceFactor)) * (num + num3 * Simulation.TimeQuant) / (invMassValue2 + num4);
                    RigidBody.ApplyImpulse(up * new Vector4f(f2) - vector4f, base.LocalPosition);
                }
            }
        }
    }
}