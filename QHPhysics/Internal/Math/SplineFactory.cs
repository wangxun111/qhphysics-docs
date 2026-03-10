using System;
using System.Collections.Generic;
using UnityEngine;

namespace QH.Physics {
    public static class SplineFactory {
        private const Single MaxSplineSegmentAllLength = 100f;
        private const Single MaxSplineSegmentLength = 0.05f;
        private const Int32 MinSplineSegmentCount = 2;
        public const Int32 MaxBezier5CatmullRomCompositePoints = 1000;

        private static UInt16[] BuildHermiteCountBuffer = new UInt16[512];
        private static TransformPoint priorSourceBezierPoint;
        private static Vector3[] catmullRomPointPositiones = new Vector3[400];
        private static Single[] catmullRomFactors = new Single[400];

        public static void BuildHermiteSpline(IList<Vector3> massPositiones, Vector3 posEnd, LineRenderer lineRenderer) {
            Int32 count = 0;
            Vector3? posLast = null;
            Int32 index = 0;
            Single length = 0f;
            for (Int32 i = 0; i < massPositiones.Count; i++) {
                Vector3 pos = massPositiones[i];
                if (posLast.HasValue) {
                    Single distance = (pos - posLast.Value).magnitude;
                    length += distance;
                    if (length < MaxSplineSegmentAllLength) {
                        BuildHermiteCountBuffer[index] = (UInt16)((Int32)(distance / MaxSplineSegmentLength) + 1);
                    }
                    else {
                        BuildHermiteCountBuffer[index] = MinSplineSegmentCount;
                    }
                    count += BuildHermiteCountBuffer[index];
                    index++;
                }
                posLast = pos;
            }

            lineRenderer.positionCount = count + 1;

            Int32 lineRendererIndex = 0;
            for (index = 0; index < massPositiones.Count - 1; index++) {
                Vector3 pos = massPositiones[index];
                Vector3 posNext = massPositiones[index + 1];
                Vector3 posMiddleOfNextToLast = ((index <= 0) ? (posNext - pos) : (0.5f * (posNext - massPositiones[index - 1])));
                Vector3 posMiddleOfNext2ToNow = ((index >= massPositiones.Count - 2) ? (posNext - pos) : (0.5f * (massPositiones[index + 2] - pos)));
                Int32 hCount = BuildHermiteCountBuffer[index];
                Single invHCount = 1f / (Single)hCount;
                if (index == massPositiones.Count - 2 && hCount > 1) {
                    invHCount = 1f / (Single)(hCount - 1);
                }
                for (Int32 j = 0; j < hCount; j++) {
                    Single indexFactor = (Single)j * invHCount;
                    Vector3 position = (2f * indexFactor * indexFactor * indexFactor - 3f * indexFactor * indexFactor + 1f) * pos + (indexFactor * indexFactor * indexFactor - 2f * indexFactor * indexFactor + indexFactor) * posMiddleOfNextToLast + (-2f * indexFactor * indexFactor * indexFactor + 3f * indexFactor * indexFactor) * posNext + (indexFactor * indexFactor * indexFactor - indexFactor * indexFactor) * posMiddleOfNext2ToNow;
                    lineRenderer.SetPosition(lineRendererIndex, position);
                    lineRendererIndex++;
                }
            }
            lineRenderer.SetPosition(lineRendererIndex, posEnd);
        }

        public static Int32 BuildCatmullRom(List<Vector3> points, Single alpha, Vector3[] spline, Int32 verticesPerSegment = 10, Int32 startVertexIndex = 0, Single noiseAmp = 0f) {
            return BuildCatmullRom(2f * points[0] - points[1], 2f * points[points.Count - 1] - points[points.Count - 2], points, noiseAmp, alpha, spline, verticesPerSegment, startVertexIndex);
        }

        public static Int32 BuildCatmullRom(Vector3 extraLeft, Vector3 extraRight, List<Vector3> points, Single noiseAmp, Single alpha, Vector3[] spline, Int32 verticesPerSegment = 10, Int32 startVertexIndex = 0) {
            Int32 num = Math.Min(catmullRomPointPositiones.Length, points.Count);
            Int32 num2 = num + 2;
            catmullRomPointPositiones[0] = extraLeft;
            for (Int32 i = 0; i < num; i++) {
                Single num3 = Mathf.Pow((Single)i / (Single)num, 0.1f);
                catmullRomPointPositiones[i + 1] = points[i] + PerlinVector(points[i], 4f) * num3 * noiseAmp;
            }
            catmullRomPointPositiones[num2 - 1] = extraRight;
            catmullRomFactors[0] = 0f;
            for (Int32 j = 0; j < num2 - 1; j++) {
                catmullRomFactors[j + 1] = catmullRomFactors[j] + Mathf.Pow((catmullRomPointPositiones[j + 1] - catmullRomPointPositiones[j]).sqrMagnitude, alpha * 0.5f);
            }
            Int32 num4 = num2 - 2 + (num2 - 3) * verticesPerSegment;
            if (num4 + startVertexIndex >= MaxBezier5CatmullRomCompositePoints) {
                verticesPerSegment = (MaxBezier5CatmullRomCompositePoints - startVertexIndex - (num2 - 2)) / (num2 - 3);
                num4 = num2 - 2 + (num2 - 3) * verticesPerSegment;
            }
            Int32 num5 = startVertexIndex;
            for (Int32 k = 0; k < num2 - 3; k++) {
                spline[num5] = catmullRomPointPositiones[k + 1];
                Vector3 vector = catmullRomPointPositiones[k];
                Vector3 vector2 = catmullRomPointPositiones[k + 1];
                Vector3 vector3 = catmullRomPointPositiones[k + 2];
                Vector3 vector4 = catmullRomPointPositiones[k + 3];
                Single num6 = catmullRomFactors[k];
                Single num7 = catmullRomFactors[k + 1];
                Single num8 = catmullRomFactors[k + 2];
                Single num9 = catmullRomFactors[k + 3];
                for (Int32 l = 1; l <= verticesPerSegment; l++) {
                    num5++;
                    if (Mathf.Approximately(num6, num7) || Mathf.Approximately(num7, num8) || Mathf.Approximately(num8, num9)) {
                        spline[num5] = vector2;
                        continue;
                    }
                    Single num10 = num7 + (Single)l * (num8 - num7) / (Single)(verticesPerSegment + 1);
                    Vector3 vector5 = vector * (num7 - num10) / (num7 - num6) + vector2 * (num10 - num6) / (num7 - num6);
                    Vector3 vector6 = vector2 * (num8 - num10) / (num8 - num7) + vector3 * (num10 - num7) / (num8 - num7);
                    Vector3 vector7 = vector3 * (num9 - num10) / (num9 - num8) + vector4 * (num10 - num8) / (num9 - num8);
                    Vector3 vector8 = vector5 * (num8 - num10) / (num8 - num6) + vector6 * (num10 - num6) / (num8 - num6);
                    Vector3 vector9 = vector6 * (num9 - num10) / (num9 - num7) + vector7 * (num10 - num7) / (num9 - num7);
                    Vector3 vector10 = vector8 * (num8 - num10) / (num8 - num7) + vector9 * (num10 - num7) / (num8 - num7);
                    spline[num5] = vector10;
                }
                num5++;
            }
            spline[num5] = catmullRomPointPositiones[num2 - 2];
            return num4;
        }

        public static Int32 BuildBezier5CatmullRomComposite(List<Vector3> points, Single noiseAmp, Single alpha, Vector3[] spline, Int32 bezierPoints, Int32 catmullPointsPerSegment) {
            Int32 num = 0;
            if (points.Count < 2) {
                return 0;
            }
            return BuildCatmullRom(points, alpha, spline, catmullPointsPerSegment, 0, noiseAmp);
        }

        public static Vector3 PerlinVector(Vector3 point, Single frequency = 1f) {
            Vector3 vector = point * frequency;
            return new Vector3(Mathf.PerlinNoise(vector.x, vector.y), Mathf.PerlinNoise(vector.y, vector.z), Mathf.PerlinNoise(vector.z, vector.x)) * 2f - Vector3.one;
        }
    }
}