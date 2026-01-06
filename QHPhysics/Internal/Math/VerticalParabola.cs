using System;
using UnityEngine;

namespace QH.Physics {
    public class VerticalParabola {
        private Single a;
        private Single b;
        private Single x3;

        private Vector3 xAxis;
        private Vector3 yAxis;
        private Vector3 zAxis; // 侧抛方向轴

        private Vector3 normal;
        private Vector3 pivot;

        // 添加侧抛角度属性
        public Single LateralAngle { get; private set; }

        public Vector3 PointStart { get; private set; }
        public Vector3 PointEnd { get; private set; }
        public Single PointMiddleY { get; private set; }

        public VerticalParabola(Vector3 StartPoint, Vector3 FinishPoint, Single midPointY)
            : this(StartPoint, FinishPoint, midPointY, 0f) // 默认侧抛角度为0
        {
        }

        // 新增构造函数：支持侧抛角度
        public VerticalParabola(Vector3 StartPoint, Vector3 FinishPoint, Single midPointY, Single lateralAngle) {
            this.PointStart = StartPoint;
            this.PointEnd = FinishPoint;
            PointMiddleY = midPointY;
            this.LateralAngle = lateralAngle; // 保存侧抛角度

            yAxis = Vector3.up;
            xAxis = (FinishPoint - StartPoint).normalized;
            xAxis -= yAxis * Vector3.Dot(xAxis, yAxis);
            xAxis.Normalize();

            // 计算侧抛方向轴
            zAxis = Vector3.Cross(xAxis, yAxis).normalized;

            normal = Vector3.Cross(xAxis, yAxis);
            normal.Normalize();
            pivot = StartPoint;

            // 原有抛物线参数计算
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
            zAxis = source.zAxis;
            normal = source.normal;
            pivot = source.pivot;
            LateralAngle = source.LateralAngle; // 同步侧抛角度
        }


        // 修改GetPoint方法，支持侧抛
        public Vector3 GetPoint(Single t) {
            Single num = t * x3;
            Single y = a * num * num + b * num;

            // 基础点（垂直抛物线）
            Vector3 basePoint = PlaneToSpace(new Vector2(num, y));

            // 应用侧抛偏移
            if (Mathf.Abs(LateralAngle) > 0.01f) {
                // 计算侧抛偏移量
                float lateralRad = LateralAngle * Mathf.Deg2Rad;
                float lateralOffset = Mathf.Tan(lateralRad) * Mathf.Abs(y) * 0.3f;

                // 在侧抛方向应用偏移
                basePoint -= zAxis * lateralOffset;
            }

            return basePoint;
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


        // 修改绘制方法，显示侧抛角度
        public void DrawTrajectory(int segments = 30) {
#if UNITY_EDITOR
            Color trajectoryColor = GetTrajectoryColor();

            // 绘制完整抛物线
            Vector3 prevPoint = GetPoint(0);
            for (int i = 1; i <= segments; i++) {
                float t = i / (float)segments;
                Vector3 currentPoint = GetPoint(t);
                Debug.DrawLine(prevPoint, currentPoint, trajectoryColor, 10f);

                // 每5个点标记一次
                if (i % 5 == 0) {
                    Debug.DrawRay(currentPoint, Vector3.up * 0.1f, trajectoryColor, 10f);
                }

                prevPoint = currentPoint;
            }

            // 标记起点和终点
            Debug.DrawRay(PointStart, Vector3.up * 0.5f, Color.yellow, 10f);
            Debug.DrawRay(PointEnd, Vector3.up * 0.5f, Color.yellow, 10f);

            // 显示侧抛角度信息
            if (Mathf.Abs(LateralAngle) > 0.1f) {
                Vector3 infoPos = (PointStart + PointEnd) * 0.5f + Vector3.up * 2f;
                Debug.DrawRay(infoPos, Vector3.up * 0.3f, trajectoryColor, 10f);
            }
#endif
        }

        private Color GetTrajectoryColor() {
            if (Mathf.Abs(LateralAngle) < 0.1f)
                return Color.cyan; // 垂直抛
            else if (LateralAngle > 0)
                return Color.green; // 右侧抛
            else
                return Color.red; // 左侧抛
        }
    }
}
