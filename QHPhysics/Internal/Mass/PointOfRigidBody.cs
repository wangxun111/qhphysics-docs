using System;
using Mono.Simd;
using Mono.Simd.Math;
using UnityEngine;

namespace QH.Physics {
    public class PointOfRigidBody : MassObject {
        public RigidBodyObject RigidBody;
        private Vector3 localPosition;

        public Vector3 LocalPosition {
            get {
                return localPosition;
            }
            set {
                localPosition = value;
                Sim.IPhyActionsListener?.ChangeMassLocalPosition(base.UID, value);
            }
        }

        public PointOfRigidBody(Simulation simulation, RigidBodyObject rigidBody, Vector3 localPosition, EMassObjectType massType) : base(simulation, 0f, rigidBody.PointLocalToWorld(localPosition), massType) {
            RigidBody = rigidBody;
            LocalPosition = localPosition;
        }

        public PointOfRigidBody(Simulation simulation, PointOfRigidBody sourcePORB) : base(simulation, sourcePORB) {
            RigidBody = sourcePORB.RigidBody;
            LocalPosition = sourcePORB.LocalPosition;
        }

        public override void UpdateReferences() {
            RigidBody = Sim.MassDict[RigidBody.UID] as RigidBodyObject;
        }

        public override void ApplyForce(Vector3 force, Boolean capture = false) {
            ApplyForce(force.AsPhyVector(), capture);
        }

        public override void ApplyForce(Vector4f force4f, Boolean capture = false) {
            RigidBody.ApplyForce(force4f, LocalPosition, capture);
            base.force4f += force4f;
            if (capture) {
                UpdateAvgForce(force4f);
            }
        }

        public override void Simulate() {
            Position4f = RigidBody.PointLocalToWorld4f(LocalPosition);
            Velocity4f = RigidBody.PointLocalToWorldVelocity4f(LocalPosition);
            groundPoint = RigidBody.GroundPoint4f;
            groundNormal = RigidBody.GroundNormal4f;
        }
    }
}