using System;
using UnityEngine;

namespace QH.Physics {
    public interface ICollider {
        Int32 InstanceID {
            get;
        }
        EDensityType DensityType {
            get;
        }
        Single FrictionFactor {
            get;
        }

        void Sync(ICollider source);
        Vector3 TestPoint(Vector3 point, Vector3 priorPoint, out Vector3 normal);
    }
}