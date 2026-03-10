using System;
using UnityEngine;
using Mono.Simd;

namespace QH.Physics {
    public struct CollisionContactPointData {
        public Vector4f Point;
        public Vector4f Normal;
        public Vector3 LocalPoint;
        public Single Distance;
        public Single DynamicFrictionFactor;
        public Single StaticFrictionFactor;
        public Single Bounce;
    }
}