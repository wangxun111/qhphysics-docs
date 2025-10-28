using System;
using UnityEngine;

namespace QH.Physics {
    public class VerticalParabola {
        private Single a;
        private Single b;
        private Single x3;

        private Vector3 xAxis;
        private Vector3 yAxis;

        private Vector3 normal;
        private Vector3 pivot;

        public Vector3 PointStart {
            get; private set;
        }

        public Vector3 PointEnd {
            get; private set;
        }

        public Single PointMiddleY {
            get; private set;
        }

        public VerticalParabola(Vector3 StartPoint, Vector3 FinishPoint, Single midPointY) {
            this.PointStart = StartPoint;
            this.PointEnd = FinishPoint;
            PointMiddleY = midPointY;
            yAxis = Vector3.up;
            xAxis = (FinishPoint - StartPoint).normalized;
            xAxis -= yAxis * Vector3.Dot(xAxis, yAxis);
            xAxis.Normalize();
            normal = Vector3.Cross(xAxis, yAxis);
            normal.Normalize();
            pivot = StartPoint;
            Vector3 vector = (StartPoint + FinishPoint) * 0.5f;
            vector += yAxis * (midPointY - Vector3.Dot(vector, yAxis));
            Vector2 vector2 = SpaceToPlane(vector);
            Vector2 vector3 = SpaceToPlane(FinishPoint);
            x3 = vector3.x;
            Single num = vector2.x * vector2.x;
            Single x = vector2.x;
            Single y = vector2.y;
            Single num2 = (0f - vector2.x) * vector2.x + vector3.x * vector3.x;
            Single num3 = 0f - vector2.x + vector3.x;
            Single num4 = 0f - vector2.y + vector3.y;
            Single num5 = (0f - num3) / x;
            Single num6 = num5 * num + num2;
            Single num7 = num5 * y + num4;
            a = num7 / num6;
            b = (y - num * a) / x;
        }

        public VerticalParabola(VerticalParabola source) {
            Sync(source);
        }

        public void Sync(VerticalParabola source) {
            a = source.a;
            b = source.b;
            x3 = source.x3;
            xAxis = source.xAxis;
            yAxis = source.yAxis;
            normal = source.normal;
            pivot = source.pivot;
        }

        public Vector3 GetPoint(Single t) {
            Single num = t * x3;
            Single y = a * num * num + b * num;
            return PlaneToSpace(new Vector2(num, y));
        }

        public Single GetDerivative(Single t) {
            Single num = t * x3;
            return a * 2f * num + b;
        }

        private Vector2 SpaceToPlane(Vector3 point) {
            Vector3 rhs = point - pivot;
            return new Vector2(Vector3.Dot(xAxis, rhs), Vector3.Dot(yAxis, rhs));
        }

        private Vector3 PlaneToSpace(Vector2 point) {
            return pivot + xAxis * point.x + yAxis * point.y;
        }
    }
}
