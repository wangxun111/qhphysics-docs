using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using Mono.Simd;
using Mono.Simd.Math;
using QH.Physics;

public static class MathTool {
    public static ITerrainTool TerrainTool { get; set; }

    public static void Clear() {
        TerrainTool = null;
        
    }
    public static Single Sqrt(Single x, Single y) {
        return Mathf.Sqrt(x * x + y * y);
    }

    public static Vector3 IncreaseVector(Vector3 vector, Single size) {
        Single num = Vector3.Magnitude(vector);
        num += size;
        Vector3 a = Vector3.Normalize(vector);
        return Vector3.Scale(a, new Vector3(num, num, num));
    }

    public static Vector3 SetVectorLength(Vector3 vector, Single size) {
        Vector3 vector2 = Vector3.Normalize(vector);
        return vector2 * size;
    }

    public static Vector3 NormalOfPlane(Vector3 point1, Vector3 point2, Vector3 point3) {
        Vector3 vec1 = point2 - point1;
        Vector3 vec2 = point3 - point1;
        return Vector3.Normalize(Vector3.Cross(vec1, vec2));
    }

    public static Boolean IntersectionPlaneToPlane(out Vector3 linePosition, out Vector3 lineDirection, Vector3 normalPlane1, Vector3 posPlane1, Vector3 normalPlane2, Vector3 posPlane2) {
        linePosition = Vector3.zero;
        lineDirection = Vector3.zero;
        lineDirection = Vector3.Cross(normalPlane1, normalPlane2);
        Vector3 vector = Vector3.Cross(normalPlane2, lineDirection);
        Single num = Vector3.Dot(normalPlane1, vector);
        if (Mathf.Abs(num) > 0.006f) {
            Vector3 rhs = posPlane1 - posPlane2;
            Single num2 = Vector3.Dot(normalPlane1, rhs) / num;
            linePosition = posPlane2 + num2 * vector;
            return true;
        }

        return false;
    }

    public static Boolean IntersectionLineToPlane(out Vector3 intersection, Vector3 linePoint, Vector3 lineVec, Vector3 planeNormal, Vector3 planePoint) {
        intersection = Vector3.zero;
        Single num = Vector3.Dot(planePoint - linePoint, planeNormal);
        Single num2 = Vector3.Dot(lineVec, planeNormal);
        if (num2 != 0f) {
            Single size = num / num2;
            Vector3 vector = SetVectorLength(lineVec, size);
            intersection = linePoint + vector;
            return true;
        }

        return false;
    }

    public static Boolean IntersectionLineToPlane(out Vector4f intersection, Vector4f linePoint, Vector4f lineVec, Vector4f planeNormal, Vector4f planePoint) {
        intersection = Vector4f.Zero;
        Single num = Vector4fExtensions.Dot(planePoint - linePoint, planeNormal);
        Single num2 = Vector4fExtensions.Dot(lineVec, planeNormal);
        if (num2 != 0f) {
            Single f = num / num2;
            Vector4f vector4f = lineVec * new Vector4f(f);
            intersection = linePoint + vector4f;
            return true;
        }

        return false;
    }

    public static Boolean IntersectionSegmentToPlane(out Vector3 intersection, Vector3 segmentA, Vector3 segmentB, Vector3 planeNormal, Vector3 planePoint) {
        intersection = Vector3.zero;
        Vector3 lhs = segmentB - segmentA;
        Single magnitude = lhs.magnitude;
        lhs /= magnitude;
        Single num = Vector3.Dot(planePoint - segmentA, planeNormal);
        Single num2 = Vector3.Dot(lhs, planeNormal);
        if (num2 != 0f) {
            Single num3 = num / num2;
            if (num3 > magnitude) {
                intersection = segmentB;
                return false;
            }

            Vector3 vector = lhs.normalized * num3;
            intersection = segmentA + vector;
            return true;
        }

        return false;
    }

    public static Boolean IntersectionSegmentToPlane(out Vector4f intersection, Vector4f segmentA, Vector4f segmentB, Vector4f planeNormal, Vector4f planePoint) {
        intersection = Vector4f.Zero;
        Vector4f vector4f = segmentB - segmentA;
        Single num = (segmentB - segmentA).Magnitude();
        vector4f /= new Vector4f(num);
        Single num2 = Vector4fExtensions.Dot(planePoint - segmentA, planeNormal);
        Single num3 = Vector4fExtensions.Dot(vector4f, planeNormal);
        if (num3 != 0f) {
            Single num4 = num2 / num3;
            if (num4 > num) {
                intersection = segmentB;
                return false;
            }

            Vector4f vector4f2 = vector4f.Normalized() * new Vector4f(num4);
            intersection = segmentA + vector4f2;
            return true;
        }

        return false;
    }

    public static Boolean IntersectionSegmentToSegment(Vector2 s1start, Vector2 s1end, Vector2 s2start, Vector2 s2end, out Single t1, out Single t2) {
        Single x = s1start.x;
        Single x2 = s1end.x;
        Single x3 = s2start.x;
        Single x4 = s2end.x;
        Single y = s1start.y;
        Single y2 = s1end.y;
        Single y3 = s2start.y;
        Single y4 = s2end.y;
        Single num = 1f / ((x - x2) * (y3 - y4) - (y - y2) * (x3 - x4));
        if (!Mathf.Approximately(num, 0f)) {
            t1 = ((x - x3) * (y3 - y4) - (y - y3) * (x3 - x4)) * num;
            t2 = ((x - x2) * (y - y3) - (y - y2) * (x - x3)) * num;
            return true;
        }

        t1 = 0f;
        t2 = 0f;
        return false;
    }

    public static Boolean IntersectionLineToLine(out Vector3 intersection, Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2) {
        intersection = Vector3.zero;
        Vector3 lhs = linePoint2 - linePoint1;
        Vector3 rhs = Vector3.Cross(lineVec1, lineVec2);
        Vector3 lhs2 = Vector3.Cross(lhs, lineVec2);
        Single num = Vector3.Dot(lhs, rhs);
        if (num >= 1E-05f || num <= -1E-05f) {
            return false;
        }

        Single num2 = Vector3.Dot(lhs2, rhs) / rhs.sqrMagnitude;
        if (num2 >= 0f && num2 <= 1f) {
            intersection = linePoint1 + lineVec1 * num2;
            return true;
        }

        return false;
    }

    public static Int32 IntersectionSegmentToSphere(Vector4f segA, Vector4f segB, Vector4f sphereCenter, Single sphereRadius, out Single i1, out Single i2) {
        Vector4f vector4f = segA - sphereCenter;
        Vector4f vector4f2 = segB - segA;
        Single num = vector4f2.SqrMagnitude();
        Single num2 = 2f * Vector4fExtensions.Dot(vector4f2, vector4f);
        Single num3 = vector4f.SqrMagnitude() - sphereRadius * sphereRadius;
        Single num4 = num2 * num2 - 4f * num * num3;
        if (num4 < 0f) {
            i1 = 0f;
            i2 = 0f;
            return 0;
        }

        if (Mathf.Approximately(num4, 0f)) {
            i1 = -0.5f * num2 / num;
            i2 = 0f;
            return 1;
        }

        num4 = Mathf.Sqrt(num4);
        i1 = 0.5f * (0f - num2 - num4) / num;
        i2 = 0.5f * (0f - num2 + num4) / num;
        return 2;
    }

    public static Boolean ClosestPointsOnTwoLines(out Vector3 closestPointLine1, out Vector3 closestPointLine2, Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2) {
        closestPointLine1 = Vector3.zero;
        closestPointLine2 = Vector3.zero;
        Single num = Vector3.Dot(lineVec1, lineVec1);
        Single num2 = Vector3.Dot(lineVec1, lineVec2);
        Single num3 = Vector3.Dot(lineVec2, lineVec2);
        Single num4 = num * num3 - num2 * num2;
        if (num4 != 0f) {
            Vector3 rhs = linePoint1 - linePoint2;
            Single num5 = Vector3.Dot(lineVec1, rhs);
            Single num6 = Vector3.Dot(lineVec2, rhs);
            Single num7 = (num2 * num6 - num5 * num3) / num4;
            Single num8 = (num * num6 - num5 * num2) / num4;
            closestPointLine1 = linePoint1 + lineVec1 * num7;
            closestPointLine2 = linePoint2 + lineVec2 * num8;
            return true;
        }

        return false;
    }

    public static Vector3 ProjectPointOnLine(Vector3 linePoint, Vector3 lineVec, Vector3 point) {
        Vector3 lhs = point - linePoint;
        Single num = Vector3.Dot(lhs, lineVec);
        return linePoint + lineVec * num;
    }

    public static Vector3 ProjectPointOnLineSegment(Vector3 linePoint1, Vector3 linePoint2, Vector3 point) {
        Vector3 vector = ProjectPointOnLine(linePoint1, (linePoint2 - linePoint1).normalized, point);
        switch (PointOnWhichSideOfLineSegment(linePoint1, linePoint2, vector)) {
            case 0:
                return vector;
            case 1:
                return linePoint1;
            case 2:
                return linePoint2;
            default:
                return Vector3.zero;
        }
    }

    public static Vector3 ProjectPointOnPlane(Vector3 planeNormal, Vector3 planePoint, Vector3 point) {
        Single num = SignedDistancePlanePoint(planeNormal, planePoint, point);
        num *= -1f;
        Vector3 vector = SetVectorLength(planeNormal, num);
        return point + vector;
    }

    public static Vector3 ProjectVectorOnPlane(Vector3 planeNormal, Vector3 vector) {
        return vector - Vector3.Dot(vector, planeNormal) * planeNormal;
    }

    public static Single SignedDistancePlanePoint(Vector3 planeNormal, Vector3 planePoint, Vector3 point) {
        return Vector3.Dot(planeNormal, point - planePoint);
    }

    public static Single SignedDotProduct(Vector3 vectorA, Vector3 vectorB, Vector3 normal) {
        Vector3 lhs = Vector3.Cross(normal, vectorA);
        return Vector3.Dot(lhs, vectorB);
    }

    public static Single SignedVectorAngle(Vector3 referenceVector, Vector3 otherVector, Vector3 normal) {
        Vector3 lhs = Vector3.Cross(normal, referenceVector);
        Single num = Vector3.Angle(referenceVector, otherVector);
        return num * Mathf.Sign(Vector3.Dot(lhs, otherVector));
    }

    public static Single AngleVectorPlane(Vector3 vector, Vector3 normal) {
        Single num = Vector3.Dot(vector, normal);
        Single num2 = (Single)Math.Acos(num);
        return (Single)Math.PI / 2f - num2;
    }

    public static Single DotProductAngle(Vector3 vec1, Vector3 vec2) {
        Double num = Vector3.Dot(vec1, vec2);
        if (num < -1.0) {
            num = -1.0;
        }

        if (num > 1.0) {
            num = 1.0;
        }

        Double num2 = Math.Acos(num);
        return (Single)num2;
    }

    public static void PlaneFrom3Points(out Vector3 planeNormal, out Vector3 planePoint, Vector3 pointA, Vector3 pointB, Vector3 pointC) {
        planeNormal = Vector3.zero;
        planePoint = Vector3.zero;
        Vector3 vector = pointB - pointA;
        Vector3 vector2 = pointC - pointA;
        planeNormal = Vector3.Normalize(Vector3.Cross(vector, vector2));
        Vector3 vector3 = pointA + vector / 2f;
        Vector3 vector4 = pointA + vector2 / 2f;
        Vector3 lineVec = pointC - vector3;
        Vector3 lineVec2 = pointB - vector4;
        Vector3 closestPointLine;
        ClosestPointsOnTwoLines(out planePoint, out closestPointLine, vector3, lineVec, vector4, lineVec2);
    }

    public static Int32 PointOnWhichSideOfLineSegment(Vector3 linePoint1, Vector3 linePoint2, Vector3 point) {
        Vector3 rhs = linePoint2 - linePoint1;
        Vector3 lhs = point - linePoint1;
        Single num = Vector3.Dot(lhs, rhs);
        if (num > 0f) {
            if (lhs.magnitude <= rhs.magnitude) {
                return 0;
            }

            return 2;
        }

        return 1;
    }

    public static Single MouseDistanceToLine(Vector3 linePoint1, Vector3 linePoint2) {
        Camera main = Camera.main;
        Vector3 mousePosition = Input.mousePosition;
        Vector3 linePoint3 = main.WorldToScreenPoint(linePoint1);
        Vector3 linePoint4 = main.WorldToScreenPoint(linePoint2);
        Vector3 vector = ProjectPointOnLineSegment(linePoint3, linePoint4, mousePosition);
        vector = new Vector3(vector.x, vector.y, 0f);
        return (vector - mousePosition).magnitude;
    }

    public static Single MouseDistanceToCircle(Vector3 point, Single radius) {
        Camera main = Camera.main;
        Vector3 mousePosition = Input.mousePosition;
        Vector3 vector = main.WorldToScreenPoint(point);
        vector = new Vector3(vector.x, vector.y, 0f);
        Single magnitude = (vector - mousePosition).magnitude;
        return magnitude - radius;
    }

    public static Boolean IsLineInRectangle(Vector3 linePoint1, Vector3 linePoint2, Vector3 rectA, Vector3 rectB, Vector3 rectC, Vector3 rectD) {
        Boolean flag = false;
        Boolean flag2 = false;
        flag = IsPointInRectangle(linePoint1, rectA, rectC, rectB, rectD);
        if (!flag) {
            flag2 = IsPointInRectangle(linePoint2, rectA, rectC, rectB, rectD);
        }

        if (!flag && !flag2) {
            Boolean flag3 = AreLineSegmentsCrossing(linePoint1, linePoint2, rectA, rectB);
            Boolean flag4 = AreLineSegmentsCrossing(linePoint1, linePoint2, rectB, rectC);
            Boolean flag5 = AreLineSegmentsCrossing(linePoint1, linePoint2, rectC, rectD);
            Boolean flag6 = AreLineSegmentsCrossing(linePoint1, linePoint2, rectD, rectA);
            if (flag3 || flag4 || flag5 || flag6) {
                return true;
            }

            return false;
        }

        return true;
    }

    public static Boolean IsPointInRectangle(Vector3 point, Vector3 rectA, Vector3 rectC, Vector3 rectB, Vector3 rectD) {
        Vector3 vector = rectC - rectA;
        Single size = 0f - vector.magnitude / 2f;
        vector = IncreaseVector(vector, size);
        Vector3 linePoint = rectA + vector;
        Vector3 vector2 = rectB - rectA;
        Single num = vector2.magnitude / 2f;
        Vector3 vector3 = rectD - rectA;
        Single num2 = vector3.magnitude / 2f;
        Vector3 vector4 = ProjectPointOnLine(linePoint, vector2.normalized, point);
        Single magnitude = (vector4 - point).magnitude;
        vector4 = ProjectPointOnLine(linePoint, vector3.normalized, point);
        Single magnitude2 = (vector4 - point).magnitude;
        if (magnitude2 <= num && magnitude <= num2) {
            return true;
        }

        return false;
    }

    public static Boolean AreLineSegmentsCrossing(Vector3 pointA1, Vector3 pointA2, Vector3 pointB1, Vector3 pointB2) {
        Vector3 vector = pointA2 - pointA1;
        Vector3 vector2 = pointB2 - pointB1;
        Vector3 closestPointLine;
        Vector3 closestPointLine2;
        if (ClosestPointsOnTwoLines(out closestPointLine, out closestPointLine2, pointA1, vector.normalized, pointB1, vector2.normalized)) {
            Int32 num = PointOnWhichSideOfLineSegment(pointA1, pointA2, closestPointLine);
            Int32 num2 = PointOnWhichSideOfLineSegment(pointB1, pointB2, closestPointLine2);
            if (num == 0 && num2 == 0) {
                return true;
            }

            return false;
        }

        return false;
    }

    public static Single AngleSigned(Vector3 v1, Vector3 v2, Vector3 n) {
        return Mathf.Atan2(Vector3.Dot(n, Vector3.Cross(v1, v2)), Vector3.Dot(v1, v2)) * 57.29578f;
    }

    public static Vector3 GetBoxNearestCenterPoint(Vector3 boxPosition, Vector3 boxRotation, Vector3 boxScale, Vector3 point) {
        if (boxScale.x == boxScale.y && boxScale.x == boxScale.z) {
            return boxPosition;
        }

        Quaternion quaternion = Quaternion.Euler(boxRotation.x, boxRotation.y, boxRotation.z);
        Single num = Mathf.Min(boxScale.x, boxScale.y, boxScale.z) / 2f;
        Vector3 vector;
        Vector3 vector2;
        if (boxScale.x > boxScale.y && boxScale.x > boxScale.z) {
            vector = new Vector3(boxScale.x / 2f - num, 0f, 0f);
            vector2 = new Vector3(0f - vector.x, 0f, 0f);
        }
        else if (boxScale.x > boxScale.y && boxScale.x > boxScale.z) {
            vector = new Vector3(0f, boxScale.y / 2f - num, 0f);
            vector2 = new Vector3(0f, 0f - vector.y, 0f);
        }
        else {
            vector = new Vector3(0f, 0f, boxScale.z / 2f - num);
            vector2 = new Vector3(0f, 0f, 0f - vector.z);
        }

        vector = boxPosition + quaternion * vector;
        vector2 = boxPosition + quaternion * vector2;
        Vector3 normalized = (vector2 - vector).normalized;
        Vector3 vector3 = ProjectPointOnLine(vector, normalized, point);
        switch (PointOnWhichSideOfLineSegment(vector, vector2, vector3)) {
            case 0:
                return vector3;
            case 1:
                return vector;
            default:
                return vector2;
        }
    }

    public static void GetConstraintPointCollisionInfo(PointConstraintOfRigidBody cp) {
        LayerMask FishMask = Int32.MaxValue & ~((1 << LayerMask.NameToLayer("Water")) | (1 << LayerMask.NameToLayer("Player")) | (1 << LayerMask.NameToLayer("Ignore Raycast")) | (1 << LayerMask.NameToLayer("AssetField")));
        for (Int32 i = 0; i < cp.RaycastLocalDirections.Length; i++) {
            Vector3 vector = cp.RigidBody.Rotation * cp.RaycastLocalDirections[i];
            RaycastHit hitInfo;
            if (Physics.Raycast(cp.Position - vector * 0.1f, vector, out hitInfo, 1f, FishMask)) {
                cp.CollisionPlanePoint4fs[i] = hitInfo.point.AsVector4f();
                cp.CollisionPlaneNormal4fs[i] = hitInfo.normal.AsVector4f();
            }
            else {
                cp.CollisionPlanePoint4fs[i] = (cp.RaycastLocalDirections[i] * 10000f).AsVector4f();
                cp.CollisionPlaneNormal4fs[i] = (-cp.RaycastLocalDirections[i]).AsVector4f();
            }
        }
    }

    public static Single GetGroundHight(Vector3 position, Boolean modify = false, Boolean light = false) {
        Single result = -1000f;
        Vector3 origin = new Vector3(position.x, position.y + 0.5f, position.z);
        RaycastHit hitInfo;
        if (!light && Physics.Raycast(origin, Vector3.down, out hitInfo, 50f, Int32.MaxValue & ~((1 << LayerMask.NameToLayer("Water")) | (1 << LayerMask.NameToLayer("Player")) | (1 << LayerMask.NameToLayer("Ignore Raycast")) | (1 << LayerMask.NameToLayer("Fish")) | (1 << LayerMask.NameToLayer("AssetField"))))) {
            result = hitInfo.point.y;
            if (modify) {
                result += 0.0125f;
            }
        }
        else {
            if (null != TerrainTool && TerrainTool.IsValid) {
                result = TerrainTool.GetHeight(position);
                if (modify) {
                    result += 0.0125f;
                }
            }
        }

        return result;
    }

    public static void UpdateTerrainHeightChunk(Vector3 position, Terrain terrain, PlaneSudokuCollider heightFieldChunk) {
        Single num = terrain.terrainData.size.x / (terrain.terrainData.heightmapResolution - 1);
        Single num2 = terrain.terrainData.size.z / (terrain.terrainData.heightmapResolution - 1);
        Single num3 = (heightFieldChunk.SizeX - 1) * num;
        Vector3 vector = position - terrain.transform.position - new Vector3(num3 * 0.5f, 0f, num3 * 0.5f);
        Single num4 = vector.x / num;
        Single num5 = vector.z / num2;
        Int32 num6 = (Int32)num4;
        Int32 num7 = (Int32)num5;
        Vector3 newPosition = new Vector3(num6 * num, 0f, num7 * num2) + terrain.transform.position;
        if (num6 == heightFieldChunk.IndexX && num7 == heightFieldChunk.IndexZ) {
            return;
        }

        heightFieldChunk.Move(newPosition, num, num6, num7);
        for (Int32 i = 0; i < heightFieldChunk.SizeX; i++) {
            for (Int32 j = 0; j < heightFieldChunk.SizeZ; j++) {
                heightFieldChunk.Data[i, j] = terrain.terrainData.GetHeight(num6 + i, num7 + j) + 0.02f;
            }
        }
    }

    private const Single HeightChunkDelta = (Single)0.5f / (4 - 1);

    public static void UpdateTerrainHeightChunk(Vector3 position, ITerrainTool terrain, PlaneSudokuCollider heightFieldChunk, Boolean modify = false, Boolean light = false) {
        Single l = (heightFieldChunk.SizeX - 1) * HeightChunkDelta * 0.5f;
        Vector3 newPosition = new Vector3(position.x - l, 0, position.z - l);
        Int32 idxX = (Int32)(newPosition.x * 10);
        Int32 idxY = (Int32)(newPosition.z * 10);
        if (idxX == heightFieldChunk.IndexX && idxY == heightFieldChunk.IndexZ) {
            return;
        }

        heightFieldChunk.Move(newPosition, HeightChunkDelta, idxX, idxY);
        Vector3 posXY = default;
        Single heightXY = 0;
        for (Int32 i = 0; i < heightFieldChunk.SizeX; i++) {
            for (Int32 j = 0; j < heightFieldChunk.SizeZ; j++) {
                posXY = newPosition + new Vector3(i * HeightChunkDelta, 0, j * HeightChunkDelta);
                heightXY = GetGroundHight(posXY, modify, light);
                heightFieldChunk.Data[i, j] = heightXY + 0.02f;
            }
        }
    }

    private static RaycastHit[] hit = new RaycastHit[1];
    private static int layerMask = -1;

    public static Vector3? GetGroundCollision(Vector3 origin, Boolean precisely = false, Single distance = 200) {
        Vector3 offset = Vector3.zero;
        if (!precisely) {
            origin.y += 100f;
        }

        if (layerMask == -1)
        {
            layerMask = (1 << LayerMask.NameToLayer("Terrain")) | (1 << LayerMask.NameToLayer("Default"));
        }

        int hitCount = Physics.RaycastNonAlloc(origin, Vector3.down, hit, distance, layerMask);
        if (hitCount > 1)
        {
            return hit[0].point;
        }
        else if (null != TerrainTool && TerrainTool.IsValid)
        {
            Single height = TerrainTool.GetHeight(origin);
            return new Vector3(origin.x, height, origin.z);
        }

        return null;
    }

    public static void GetGroundCollision(Vector3 origin, out Vector3 point, out Vector3 normal, Single raycastHeight = 5f) {
        Vector3 origin2 = new Vector3(origin.x, origin.y + raycastHeight, origin.z);
        RaycastHit hitInfo;
        if (Physics.Raycast(origin2, Vector3.down, out hitInfo, 50f, Int32.MaxValue & ~((1 << LayerMask.NameToLayer("Water")) | (1 << LayerMask.NameToLayer("Player")) | (1 << LayerMask.NameToLayer("Ignore Raycast")) | (1 << LayerMask.NameToLayer("AssetField"))))) {
            point = hitInfo.point;
            normal = hitInfo.normal;
        }
        else {
            if (null != TerrainTool && TerrainTool.IsValid) {
                Single height = TerrainTool.GetHeight(origin);
                point = new Vector3(origin.x, height, origin.z);
                normal = TerrainTool.GetNormal(origin);
                return;
            }

            point = origin;
            point.y = -1000f;
            normal = Vector3.up;
        }
    }

    public static void GetGroundCollision(Vector3 origin, out Boolean hit, out Vector3 point, out Vector3 normal, Single raycastHeight = 5f, Single? targetPos = null) {
        Vector3 origin2 = new Vector3(origin.x, origin.y + raycastHeight, origin.z);
        RaycastHit hitInfo;
        if (Physics.Raycast(origin2, Vector3.down, out hitInfo, 50f, Int32.MaxValue & ~((1 << LayerMask.NameToLayer("Water")) | (1 << LayerMask.NameToLayer("Player")) | (1 << LayerMask.NameToLayer("Ignore Raycast")) | (1 << LayerMask.NameToLayer("AssetField"))))) {
            hit = true;
            point = hitInfo.point;
            normal = hitInfo.normal;
        }
        else {
            if (null != TerrainTool && TerrainTool.IsValid) {
                hit = true;
                Single height = TerrainTool.GetHeight(origin);
                point = new Vector3(origin.x, height, origin.z);
                normal = TerrainTool.GetNormal(point);
                return;
            }

            hit = false;
            point = origin;
            point.y = -1000f;
            normal = Vector3.up;
        }
    }

    public static Vector3? GetMaskedRayContactPoint(Vector3 from, Vector3 to, Int32 layerMask) {
        Vector3 vector = to - from;
        RaycastHit hitInfo;
        if (Physics.Raycast(from, vector.normalized, out hitInfo, vector.magnitude, layerMask)) {
            return hitInfo.point;
        }
        else {
            if (null != TerrainTool && TerrainTool.IsValid) {
                Single height = TerrainTool.GetHeight(from);
                if (height <= vector.magnitude) {
                    return new Vector3(from.x, height, from.z);
                }
            }
        }

        return null;
    }

    public static RaycastHit GetMaskedRayHit(Vector3 from, Vector3 to, Int32 layerMask) {
        Vector3 vector = to - from;
        RaycastHit hitInfo;
        Physics.Raycast(from, vector.normalized, out hitInfo, vector.magnitude, layerMask);
        return hitInfo;
    }

    public static Vector3? GetMaskedCylinderContactPoint(Vector3 from, Vector3 to, Single r, Int32 layerMask) {
        Vector3 vector = to - from;
        RaycastHit hitInfo;
        if (Physics.CapsuleCast(from, from + new Vector3(0f, vector.magnitude, 0f), r, vector.normalized, out hitInfo, vector.magnitude, layerMask)) {
            return hitInfo.point;
        }

        return null;
    }

    public static Single GetVectorsYaw(Vector3 v1, Vector3 v2) {
        Vector3 vector = Vector3.ProjectOnPlane(v1, Vector3.up);
        Vector3 vector2 = Vector3.ProjectOnPlane(v2, Vector3.up);
        return Vector3.Angle(vector, vector2) * (Single)Math.Sign(Vector3.Cross(vector, vector2).y);
    }

    public static Vector3 ProjectOXZ(Vector3 v) {
        return Vector3.ProjectOnPlane(v, Vector3.up);
    }

    public static Vector2 Rotate2DPoint(Single x, Single y, Single angle) {
        return new Vector2(x * Mathf.Cos(angle) - y * Mathf.Sin(angle), x * Mathf.Sin(angle) + y * Mathf.Cos(angle));
    }

    public static Vector3 RotatePointAroundOY(Vector3 p, Single angle) {
        Vector2 vector = Rotate2DPoint(p.x, p.z, angle);
        return new Vector3(vector.x, p.y, vector.y);
    }

    public static Single ClampAngle(Single angle, Single min, Single max) {
        return Mathf.Clamp(ClampAngleTo360(angle), min, max);
    }

    public static Single ClampAngleTo360(Single angle) {
        return Mathf.Abs(angle) <= 360f ? angle : angle - (Int32)angle / 360 * 360;
    }

    public static Single ClampAngleTo180(Single angle) {
        if (Mathf.Abs(angle) <= 180f) {
            return angle;
        }

        return (!(angle > 0f)) ? (angle + 360f) : (angle - 360f);
    }

    public static Single CrossXZ(Vector3 a, Vector3 b) {
        return a.x * b.z - a.z * b.x;
    }

    public static Boolean PointInsideTriangleXZ(Vector3 p, Vector3 a, Vector3 b, Vector3 c) {
        Vector3 vector = a - c;
        Vector3 b2 = b - c;
        Single num = CrossXZ(vector, b2);
        Single num2 = (CrossXZ(p, b2) - CrossXZ(c, b2)) / num;
        Single num3 = (0f - (CrossXZ(p, vector) - CrossXZ(c, vector))) / num;
        return num2 > 0f && num3 > 0f && num2 + num3 < 1f;
    }

    public static Single PointToSegmentDistance(Vector3 p, Vector3 a, Vector3 b) {
        Vector3 vector = b - a;
        Single magnitude = vector.magnitude;
        vector /= magnitude;
        Single num = Mathf.Clamp(Vector3.Dot(p - a, vector), 0f, magnitude);
        Vector3 vector2 = a + num * vector;
        return (vector2 - p).magnitude;
    }

    public static Vector3 ClosestPointOnSegment(Vector3 p, Vector3 a, Vector3 b) {
        Vector3 vector = b - a;
        Single magnitude = vector.magnitude;
        vector /= magnitude;
        Single num = Mathf.Clamp(Vector3.Dot(p - a, vector), 0f, magnitude);
        return a + num * vector;
    }

    public static Single PointToTriangleDistanceXZ(Vector3 p, Vector3 a, Vector3 b, Vector3 c) {
        if (PointInsideTriangleXZ(p, a, b, c)) {
            return 0f;
        }

        return Mathf.Min(PointToSegmentDistance(p, a, b), PointToSegmentDistance(p, b, c), PointToSegmentDistance(p, c, a));
    }

    public static Vector3 ClosestPointOnTriangleXZ(Vector3 p, Vector3 a, Vector3 b, Vector3 c) {
        if (PointInsideTriangleXZ(p, a, b, c)) {
            return p;
        }

        Vector3 vector = ClosestPointOnSegment(p, a, b);
        Vector3 vector2 = ClosestPointOnSegment(p, b, c);
        Vector3 vector3 = ClosestPointOnSegment(p, c, a);
        Single magnitude = (vector - p).magnitude;
        Single magnitude2 = (vector2 - p).magnitude;
        Single magnitude3 = (vector3 - p).magnitude;
        if (magnitude <= magnitude2 && magnitude <= magnitude3) {
            return vector;
        }

        if (magnitude2 <= magnitude && magnitude2 <= magnitude3) {
            return vector2;
        }

        return vector3;
    }

    public static Boolean SegmentSegmentOverlap1D(Single minA, Single maxA, Single minB, Single maxB) {
        return Mathf.Max(minA, minB) < Mathf.Min(maxA, maxB);
    }

    public static Boolean PointOverlapAABBXZ(Vector3 p, Single minX, Single maxX, Single minZ, Single maxZ) {
        return p.x >= minX && p.z >= minZ && p.x < maxX && p.z < maxZ;
    }

    public static Boolean BoxBoxOverlapAABBXZ(Single minX1, Single maxX1, Single minZ1, Single maxZ1, Single minX2, Single maxX2, Single minZ2, Single maxZ2) {
        return SegmentSegmentOverlap1D(minX1, maxX1, minX2, maxX2) && SegmentSegmentOverlap1D(minZ1, maxZ1, minZ2, maxZ2);
    }

    public static Single PointToAABBXZDistance(Single px, Single pz, Single minX, Single minZ, Single maxX, Single maxZ) {
        Boolean flag = px >= minX && px < maxX;
        Boolean flag2 = pz >= minZ && pz < maxZ;
        if (flag && flag2) {
            return 0f;
        }

        if (flag) {
            return Mathf.Min(minZ - pz, pz - maxZ);
        }

        if (flag2) {
            return Mathf.Min(minX - px, px - maxX);
        }

        if (px < minX) {
            if (pz < minZ) {
                return Sqrt(minX - px, minZ - pz);
            }

            if (pz >= maxZ) {
                return Sqrt(minX - px, pz - maxZ);
            }
        }

        if (pz < minZ) {
            return Sqrt(px - maxX, minZ - pz);
        }

        return Sqrt(px - maxX, pz - maxZ);
    }

    public static Boolean TriangleOverlapAABBXZ(Vector3 a, Vector3 b, Vector3 c, Vector3 boxMinXZ, Vector3 boxMaxXZ) {
        Vector3 vector = Vector3.Min(Vector3.Min(a, b), c);
        Vector3 vector2 = Vector3.Max(Vector3.Max(a, b), c);
        if (!SegmentSegmentOverlap1D(vector.x, vector2.x, boxMinXZ.x, boxMaxXZ.x)) {
            return false;
        }

        if (!SegmentSegmentOverlap1D(vector.z, vector2.z, boxMinXZ.z, boxMaxXZ.z)) {
            return false;
        }

        Vector3 vector3 = new Vector3(boxMinXZ.x, 0f, boxMinXZ.z);
        Vector3 vector4 = new Vector3(boxMinXZ.x, 0f, boxMaxXZ.z);
        Vector3 vector5 = new Vector3(boxMaxXZ.x, 0f, boxMaxXZ.z);
        Vector3 vector6 = new Vector3(boxMaxXZ.x, 0f, boxMinXZ.z);
        Vector3 a2 = b - a;
        Single num = Mathf.Sign(CrossXZ(a2, c - a));
        if ((Single)Math.Sign(CrossXZ(a2, vector3 - a)) != num && (Single)Math.Sign(CrossXZ(a2, vector4 - a)) != num && (Single)Math.Sign(CrossXZ(a2, vector5 - a)) != num && (Single)Math.Sign(CrossXZ(a2, vector6 - a)) != num) {
            return false;
        }

        a2 = c - a;
        num = Mathf.Sign(CrossXZ(a2, b - a));
        if ((Single)Math.Sign(CrossXZ(a2, vector3 - a)) != num && (Single)Math.Sign(CrossXZ(a2, vector4 - a)) != num && (Single)Math.Sign(CrossXZ(a2, vector5 - a)) != num && (Single)Math.Sign(CrossXZ(a2, vector6 - a)) != num) {
            return false;
        }

        a2 = b - c;
        num = Mathf.Sign(CrossXZ(a2, a - c));
        if ((Single)Math.Sign(CrossXZ(a2, vector3 - c)) != num && (Single)Math.Sign(CrossXZ(a2, vector4 - c)) != num && (Single)Math.Sign(CrossXZ(a2, vector5 - c)) != num && (Single)Math.Sign(CrossXZ(a2, vector6 - c)) != num) {
            return false;
        }

        return true;
    }

    public static Int32 GetNextPointOnLine(List<Vector3> points, Int32 startIndex, Vector3 fromPoint, Single minDist, Single dAngle, Single maxDist = -1f) {
        if (startIndex >= points.Count || startIndex < 0) {
            return -1;
        }

        Int32 farEnoughPointIndex = GetFarEnoughPointIndex(points, startIndex, fromPoint, minDist);
        Vector3 vector = points[farEnoughPointIndex];
        if (Mathf.Approximately(fromPoint.x, vector.x)) {
            for (Int32 i = farEnoughPointIndex + 1; i < points.Count; i++) {
                vector = points[i];
                if (maxDist > 0f && (fromPoint - vector).magnitude > maxDist) {
                    return i - 1;
                }

                if (Mathf.Approximately(fromPoint.x, vector.x)) {
                    continue;
                }

                Single f = (fromPoint.z - vector.z) / (fromPoint.x - vector.x);
                Single num = Mathf.Atan(f) * 57.29578f;
                if (num < 180f) {
                    if (Mathf.Abs(90f - num) > dAngle) {
                        return i - 1;
                    }
                }
                else if (Mathf.Abs(270f - num) > dAngle) {
                    return i - 1;
                }
            }
        }
        else {
            Single f2 = (fromPoint.z - vector.z) / (fromPoint.x - vector.x);
            Single num2 = Mathf.Atan(f2) * 57.29578f;
            for (Int32 j = farEnoughPointIndex + 1; j < points.Count; j++) {
                vector = points[j];
                if (maxDist > 0f && (fromPoint - vector).magnitude > maxDist) {
                    return j - 1;
                }

                Single num3 = ((!(num2 < 180f)) ? 270f : 90f);
                if (!Mathf.Approximately(fromPoint.x, vector.x)) {
                    f2 = (fromPoint.z - vector.z) / (fromPoint.x - vector.x);
                    num3 = Mathf.Atan(f2) * 57.29578f;
                }

                if (Mathf.Abs(num3 - num2) > dAngle) {
                    return j - 1;
                }
            }
        }

        return points.Count - 1;
    }

    private static Int32 GetFarEnoughPointIndex(List<Vector3> points, Int32 startIndex, Vector3 fromPoint, Single minDist) {
        for (Int32 i = startIndex; i < points.Count; i++) {
            if ((fromPoint - points[i]).magnitude >= minDist) {
                return i;
            }
        }

        return points.Count - 1;
    }
}