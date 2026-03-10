using System;
using UnityEngine;

namespace QH.Physics {
    public abstract class ColliderBase : ICollider {
        public readonly Vector3[] SpatialAxises = new Vector3[3] {
            Vector3.right,
            Vector3.up,
            Vector3.forward,
        };

        private EDensityType densityType;
        private Single frictionFactor;
        private Vector3 forceDampingFactor;

        public Int32 InstanceID {
            get; protected set;
        }

        public EDensityType DensityType {
            get {
                return densityType;
            }
            set {
                densityType = value;
            }
        }

        public Single FrictionFactor {
            get {
                return frictionFactor;
            }
            set {
                frictionFactor = value;
            }
        }

        public Vector3 ForceDampingFactor {
            get {
                return forceDampingFactor;
            }
            set {
                forceDampingFactor = value;
            }
        }

        public abstract void Sync(ICollider source);
        public abstract Vector3 TestPoint(Vector3 point, Vector3 priorPoint, out Vector3 normal);
        public virtual void DebugDraw(Color color) {
        }
    }
}