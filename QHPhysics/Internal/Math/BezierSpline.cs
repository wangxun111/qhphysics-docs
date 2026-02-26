using System;
using UnityEngine;
using UnityEngine.Profiling;

namespace QH.Physics {
    public class BezierSpline {
        protected Int32 order;
        protected Single[] binomialValues1;
        protected Single[] binomialValues2;
        public Single[] CombinationFactors;
        public Single[] DerivativeCombinationFactors;
        public Vector3[] Points;
        protected Single[] samples;

        public Int32 Order {
            get { return order; }
        }

        public Int32 PointCount {
            get { return order + 1; }
        }

        public BezierSpline(Int32 order = 2) {
            order = ((order >= 2) ? order : 2);
            this.order = order;
            Points = new Vector3[order + 1];
            binomialValues1 = new Single[order + 1];
            binomialValues2 = new Single[order + 1];
            CombinationFactors = new Single[order + 1];
            CombinationFactors[0] = 1f;
            CombinationFactors[order] = 1f;
            DerivativeCombinationFactors = new Single[order];
            Int32 n = order - 1;
            DerivativeCombinationFactors[0] = 1f;
            DerivativeCombinationFactors[n] = 1f;
            if (order == 2) {
                CombinationFactors[1] = 2f;
                return;
            }
            Single n2 = n;
            Int32 n3 = 1;
            Int32 n4 = n - n3;
            while (n3 < n4 - 1) {
                DerivativeCombinationFactors[n3++] = n2;
                DerivativeCombinationFactors[n4] = n2;
                n2 *= (Single)n4--;
                n2 /= (Single)n3;
            }
            DerivativeCombinationFactors[n3] = n2;
            if (n4 > n3) {
                DerivativeCombinationFactors[n4] = n2;
            }
            n2 = order;
            n3 = 1;
            n4 = order - n3;
            while (n3 < n4 - 1) {
                CombinationFactors[n3++] = n2;
                CombinationFactors[n4--] = n2;
                n2 = DerivativeCombinationFactors[n3] + DerivativeCombinationFactors[n3 - 1];
            }
            CombinationFactors[n3] = n2;
            if (n4 > n3) {
                CombinationFactors[n4] = n2;
            }
        }

        public void SetT(Single t) {
            Single n = 1f - t;
            Single n2 = 1f;
            Single n3 = 1f;
            binomialValues1[0] = n2;
            binomialValues2[0] = n3;
            for (Int32 i = 1; i <= order; i++) {
                n2 *= t;
                n3 *= n;
                binomialValues1[i] = n2;
                binomialValues2[i] = n3;
            }
        }

        protected Vector3 Point() {
            Vector3 point = Vector3.zero;
            for (Int32 i = 0; i <= order; i++) {
                point += CombinationFactors[i] * binomialValues1[i] * binomialValues2[order - i] * Points[i];
            }
            return point;
        }

        public Vector3 Point(Single t) {
            SetT(t);
            return Point();
        }

        public Vector3 PointDerivative() {
            Vector3 point = Vector3.zero;
            for (Int32 i = 0; i < order; i++) {
                point += DerivativeCombinationFactors[i] * binomialValues1[i] * binomialValues2[order - 1 - i] * (Points[i + 1] - Points[i]);
            }
            return point;
        }

        public Vector3 PointDerivative(Single t) {
            SetT(t);
            return PointDerivative();
        }

        public Vector3 Derivative() {
            return PointDerivative() * order;
        }

        public Vector3 Derivative(Single t) {
            SetT(t);
            return Derivative();
        }

        public Quaternion Rotation(Single t) {
            SetT(t);
            return Rotation();
        }

        public Quaternion Rotation() {
            Vector3 vec1 = Derivative().normalized;
            Vector3 vec2 = Vector3.Cross(vec1, Vector3.right).normalized;
            if (!Mathf.Approximately(vec2.magnitude, 0f)) {
                return Quaternion.LookRotation(vec1, vec2);
            }
            return Quaternion.identity;
        }

        public virtual Vector3 TransformByCylinder(Vector3 pos) {
            
            Vector3 point = Point();
            Vector3 vecDerivative = Derivative();
            Vector3 vec1 = vecDerivative.normalized;
            Vector3 vec2 = Vector3.Cross(Vector3.forward, vec1).normalized;
            Single radian = Mathf.Acos(Vector3.Dot(Vector3.forward, vec1));
            Quaternion quaternion = Quaternion.AngleAxis(radian * 57.29578f, vec2);
            Vector3 vector = new Vector3(pos.x, pos.y, 0f);
            Vector3 position = point + quaternion * vector;
            
            
            return position;
        }

        public virtual Vector3 TransformByCylinder2(Vector3 pos) {
            Vector3 point = Point();
            Vector3 vecDerivative = PointDerivative();
            Vector3 vec1 = vecDerivative.normalized;
            Vector2 posXY = new Vector2(vec1.x, -vec1.y).normalized;
            Single vec1ZValue = Mathf.Clamp01(vec1.z);
            Single vec1ZValueSqrt = Mathf.Sqrt(1 - vec1ZValue);
            Single radian = (Single)(((vec1ZValue * -0.0187292993f + 0.0742610022f) * vec1ZValue - 0.212114394f) * vec1ZValue + 1.57072878f) * vec1ZValueSqrt;
            Single sinValue1 = Mathf.Sin(radian * 57.29578f);
            Single cosValue1 = Mathf.Cos(radian * 57.29578f);
            Vector2 vecAngle = Vector2.zero;
            vecAngle.y = (1 - cosValue1) * posXY.y * posXY.x;
            vecAngle.x = (1 - cosValue1) * posXY.y * posXY.y + cosValue1;
            Single cosValue2 = posXY.x * posXY.x * (1 - cosValue1) + cosValue1;
            Vector2 sinValue2 = sinValue1 * posXY;
            Vector2 posPoint2 = new Vector2(pos.x, pos.y);
            Single rotateX = Vector2.Dot(vecAngle, posPoint2);
            Single rotateY = Vector2.Dot(new Vector2(vecAngle.y, cosValue2), posPoint2);
            Single rotateZ = Vector2.Dot(new Vector2(-sinValue2.x, sinValue2.y), posPoint2);
            Vector3 position = point + new Vector3(rotateX, rotateY, rotateZ);
            return position;
        }

        public Single SampleLength(Int32 iterationCount, out Single[] points) {
            Single length = 0f;
            points = new Single[iterationCount + 1];
            points[0] = 0f;
            Single progress = 1f / (Single)iterationCount;
            for (Int32 index = 0, indexMax = iterationCount; index < indexMax; index++) {
                Single t = progress * (Single)index;
                SetT(t);
                length += Derivative().magnitude * progress;
                points[index + 1] = length;
            }

            return length;
        }

        public void Build(Int32 size, Int32 iterationCount) {
            Single[] points;
            Single length = SampleLength(iterationCount, out points);
            if (samples == null || samples.Length != size + 1) {
                samples = new Single[size + 1];
            }

            Int32 count = 1;
            for (Int32 i = 0; i < size; i++) {
                Single num3 = (Single)i / size;
                while (num3 > points[count] / length) {
                    count++;
                    if (count >= points.Length) {
                        Debug.LogError("BuildReparametrizationCurveMap: cannot reach path for x = " + num3);
                        return;
                    }
                }
                Single index1 = (Single)(count - 1) / iterationCount;
                Single index2 = (Single)count / iterationCount;
                Single value1 = points[count - 1] / length;
                Single value2 = points[count] / length;
                samples[i] = Mathf.Lerp(index1, index2, (num3 - value1) / (value2 - value1));
            }

            samples[size] = 1f;
        }

        public Single GetCurve(Single pos) {
            if (samples.Length == 0) {
                return 0f;
            }
            if (pos <= 0f || samples.Length == 1) {
                return samples[0];
            }
            if (pos < 1f) {
                Single index1 = pos * (samples.Length - 1);
                Single index2 = Mathf.Floor(index1);
                Int32 index3 = (Int32)index1;
                return Mathf.Lerp(samples[index3], samples[index3 + 1], index1 - index2);
            }
            return samples[samples.Length - 1];
        }
    }
}