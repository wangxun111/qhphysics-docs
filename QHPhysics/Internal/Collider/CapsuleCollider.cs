using System;
using UnityEngine;
using Mono.Simd.Math;
using UCapsuleCollider = UnityEngine.CapsuleCollider;

namespace QH.Physics {
    public class CapsuleCollider : ColliderBase {
        protected Vector3 position1;
        protected Vector3 position2;
        protected Vector3 axis;
        protected Single length;
        protected Single radius;

        public CapsuleCollider(UCapsuleCollider uCapsuleCollider) {
            InstanceID = uCapsuleCollider.gameObject.GetInstanceID();
            length = (uCapsuleCollider.height - 2f * radius) * uCapsuleCollider.transform.lossyScale[uCapsuleCollider.direction];
            radius = uCapsuleCollider.radius * Mathf.Max(uCapsuleCollider.transform.lossyScale[(uCapsuleCollider.direction + 1) % 3], uCapsuleCollider.transform.lossyScale[(uCapsuleCollider.direction + 2) % 3]);
            axis = uCapsuleCollider.transform.rotation * SpatialAxises[uCapsuleCollider.direction];
            SyncPosition(uCapsuleCollider);
        }

        public override void Sync(ICollider collider) {
            CapsuleCollider capsuleCollider = collider as CapsuleCollider;
            position1 = capsuleCollider.position1;
            position2 = capsuleCollider.position2;
            axis = capsuleCollider.axis;
            length = capsuleCollider.length;
            radius = capsuleCollider.radius;
            InstanceID = capsuleCollider.InstanceID;
        }

        public void SyncPosition(UCapsuleCollider uCapsuleCollider) {
            Vector3 center = uCapsuleCollider.center;
            center.x *= uCapsuleCollider.transform.lossyScale.x;
            center.y *= uCapsuleCollider.transform.lossyScale.y;
            center.z *= uCapsuleCollider.transform.lossyScale.z;
            center = uCapsuleCollider.transform.rotation * center;
            position1 = uCapsuleCollider.transform.position - axis * length * 0.5f + center;
            position2 = uCapsuleCollider.transform.position + axis * length * 0.5f + center;
        }

        public override Vector3 TestPoint(Vector3 point, Vector3 priorPoint, out Vector3 normal) {
            Vector3 vector = point - position1;
            Single num = Vector3.Dot(vector, axis);
            if (num > 0f && num < length) {
                Vector3 vector2 = vector - axis * num;
                Single magnitude = vector2.magnitude;
                if (magnitude < radius) {
                    if (!Mathf.Approximately(magnitude, 0f)) {
                        normal = vector2 / magnitude;
                        return position1 + axis * num + radius * normal - point;
                    }

                    normal = Vector3.Cross(axis, Vector3.up);
                    if (Vector3Extension.Approximately(normal, Vector3.zero)) {
                        normal = Vector3.Cross(axis, Vector3.right);
                    }

                    normal = normal.normalized;
                    return position1 + axis * num + radius * normal - point;
                }
            }
            else if (num <= 0f && num > 0f - radius) {
                Single magnitude2 = vector.magnitude;
                if (magnitude2 < radius) {
                    if (magnitude2 > 0f) {
                        normal = vector / magnitude2;
                        return position1 + normal * radius - point;
                    }

                    normal = -axis;
                    return position1 + normal * radius - point;
                }
            }
            else if (num >= length && num < length + radius) {
                Vector3 vector3 = point - position2;
                Single magnitude3 = vector3.magnitude;
                if (magnitude3 < radius) {
                    if (magnitude3 > 0f) {
                        normal = vector3 / magnitude3;
                        return position2 + normal * radius - point;
                    }

                    normal = axis;
                    return position1 + normal * radius - point;
                }
            }

            normal = Vector3.zero;
            return Vector3.zero;
        }
    }
}