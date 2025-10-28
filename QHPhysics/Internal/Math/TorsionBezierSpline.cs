using System;
using UnityEngine;

namespace QH.Physics {
    public class TorsionBezierSpline : BezierSpline {
        public Vector3[] RightAxis;
        public Vector2 LateralScale = Vector2.one;

        public TorsionBezierSpline(Int32 order = 2) : base(order) {
            RightAxis = new Vector3[base.Order];
        }

        public static Single TriangleWindow(Int32 c, Single x) {
            return Mathf.Max(1f - Mathf.Abs(x - (Single)c), 0f);
        }

        public override Vector3 TransformByCylinder(Vector3 point) {
            Vector3 normalized = Derivative().normalized;
            Vector3 normalized2 = GetRightAxis().normalized;
            Vector3 normalized3 = Vector3.Cross(normalized, normalized2).normalized;
            Vector3 vector = normalized2 * point.x * LateralScale.x + normalized3 * point.y * LateralScale.y;
            return Point() + vector;
        }

        public Quaternion CurvedCylinderRotation() {
            Vector3 normalized = Derivative().normalized;
            Vector3 normalized2 = GetRightAxis().normalized;
            Vector3 normalized3 = Vector3.Cross(normalized, normalized2).normalized;
            Single magnitude = normalized2.magnitude;
            Single magnitude2 = normalized3.magnitude;
            if (!Mathf.Approximately(magnitude, 0f) && !Mathf.Approximately(magnitude2, 0f)) {
                return Quaternion.LookRotation(normalized, normalized3);
            }

            return Quaternion.identity;
        }

        public Quaternion CurvedCylinderTransformRotation(Quaternion rotation) {
            Vector3 normalized = Derivative().normalized;
            Vector3 normalized2 = GetRightAxis().normalized;
            Vector3 normalized3 = Vector3.Cross(normalized, normalized2).normalized;
            Single magnitude = normalized2.magnitude;
            Single magnitude2 = normalized3.magnitude;
            if (!Mathf.Approximately(magnitude, 0f) && !Mathf.Approximately(magnitude2, 0f)) {
                return Quaternion.LookRotation(normalized, normalized3) * rotation;
            }

            return rotation;
        }

        public Vector3 GetRightAxis() {
            Single x = binomialValues1[1] * (Single)(base.Order - 1);
            Vector3 zero = Vector3.zero;
            for (Int32 i = 0; i < base.Order; i++) {
                zero += RightAxis[i] * TriangleWindow(i, x);
            }
            return zero;
        }
    }
}