using System;
using UnityEngine;
using UBoxCollider = UnityEngine.BoxCollider;

namespace QH.Physics {
    public class BoxCollider : ColliderBase {
        private Vector3 position;
        private Vector3 scale;
        private Quaternion rotation;
        private Quaternion invRotation;

        public Vector3 Position {
            get => position;
            set => position = value;
        }

        public Vector3 Scale {
            get => scale;
            set => scale = value;
        }

        public Quaternion Rotation {
            get {
                return rotation;
            }
            set {
                rotation = value;
                invRotation = Quaternion.Inverse(value);
            }
        }

        public BoxCollider(UBoxCollider sourceUCollider) {
            InstanceID = sourceUCollider.gameObject.GetInstanceID();
            SyncPosition(sourceUCollider);
            scale = sourceUCollider.size * 0.5f;
            scale.x *= sourceUCollider.transform.lossyScale.x;
            scale.y *= sourceUCollider.transform.lossyScale.y;
            scale.z *= sourceUCollider.transform.lossyScale.z;
        }

        public override void Sync(ICollider collider) {
            BoxCollider boxCollider = collider as BoxCollider;
            if (boxCollider != null) {
                position = boxCollider.Position;
                scale = boxCollider.Scale;
                Rotation = boxCollider.Rotation;
            }
        }

        public void SyncPosition(UBoxCollider sourceUCollider) {
            Vector3 center = sourceUCollider.center;
            center.x *= sourceUCollider.transform.lossyScale.x;
            center.y *= sourceUCollider.transform.lossyScale.y;
            center.z *= sourceUCollider.transform.lossyScale.z;
            center = sourceUCollider.transform.rotation * center;
            position = sourceUCollider.transform.position + center;
            Rotation = sourceUCollider.transform.rotation;
        }

        public override void DebugDraw(Color color) {
            Vector3 vector = position + Rotation * new Vector3(0f - scale.x, scale.y, 0f - scale.z);
            Vector3 vector2 = position + Rotation * new Vector3(0f - scale.x, 0f - scale.y, 0f - scale.z);
            Vector3 vector3 = position + Rotation * new Vector3(scale.x, 0f - scale.y, 0f - scale.z);
            Vector3 vector4 = position + Rotation * new Vector3(scale.x, scale.y, 0f - scale.z);
            Vector3 vector5 = position + Rotation * new Vector3(0f - scale.x, scale.y, scale.z);
            Vector3 vector6 = position + Rotation * new Vector3(0f - scale.x, 0f - scale.y, scale.z);
            Vector3 vector7 = position + Rotation * new Vector3(scale.x, 0f - scale.y, scale.z);
            Vector3 vector8 = position + Rotation * new Vector3(scale.x, scale.y, scale.z);
            Debug.DrawLine(vector, vector2, color);
            Debug.DrawLine(vector2, vector3, color);
            Debug.DrawLine(vector3, vector4, color);
            Debug.DrawLine(vector4, vector, color);
            Debug.DrawLine(vector5, vector6, color);
            Debug.DrawLine(vector6, vector7, color);
            Debug.DrawLine(vector7, vector8, color);
            Debug.DrawLine(vector8, vector5, color);
            Debug.DrawLine(vector, vector5, color);
            Debug.DrawLine(vector2, vector6, color);
            Debug.DrawLine(vector3, vector7, color);
            Debug.DrawLine(vector4, vector8, color);
        }

        public override Vector3 TestPoint(Vector3 point, Vector3 priorPoint, out Vector3 normal) {
            Vector3 vector = invRotation * (point - position);
            vector.x /= scale.x;
            vector.y /= scale.y;
            vector.z /= scale.z;
            if (vector.x >= -1f && vector.x <= 1f && vector.y >= -1f && vector.y <= 1f && vector.z >= -1f && vector.z <= 1f) {
                int num = 0;
                float num2 = 1f - Mathf.Abs(vector.x);
                float num3 = 1f - Mathf.Abs(vector.y);
                float num4 = 1f - Mathf.Abs(vector.z);
                float num5 = num2;
                if (num3 < num2 && num3 < num4) {
                    num = 1;
                    num5 = num3;
                }
                else if (num4 < num2) {
                    num = 2;
                    num5 = num4;
                }

                Vector3 vector2 = invRotation * (priorPoint - position);
                vector2.x /= scale.x;
                vector2.y /= scale.y;
                vector2.z /= scale.z;
                Vector3 normalized = (vector - vector2).normalized;
                Vector3 vector3 = SpatialAxises[num] * num5 * Mathf.Sign(vector[num]);
                normal = vector3.normalized;
                vector3.x *= scale.x;
                vector3.y *= scale.y;
                vector3.z *= scale.z;
                return Rotation * vector3;
            }

            normal = Vector3.zero;
            return Vector3.zero;
        }
    }
}