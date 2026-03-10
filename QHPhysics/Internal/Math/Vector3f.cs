using System;
using Newtonsoft.Json;
using UnityEngine;

public struct Vector3f
{
    public float x;

    public float y;

    public float z;

    [JsonIgnore]
    public static Vector3f Zero {
        get { return new Vector3f(0f, 0f, 0f); }
    }

    [JsonIgnore]
    public float Magnitude {
        get { return (float)Math.Sqrt(x * x + z * z + y * y); }
    }

    [JsonIgnore]
    public float SqrMagnitude {
        get { return x * x + z * z + y * y; }
    }

    public Vector3f(float x0, float y0, float z0) {
        x = x0;
        y = y0;
        z = z0;
    }

    public Vector3f(Vector3 v) {
        x = v.x;
        y = v.y;
        z = v.z;
    }

    public static Vector3f operator +(Vector3f left, Vector3f right) {
        return new Vector3f(left.x + right.x, left.y + right.y, left.z + right.z);
    }

    public static Vector3f operator -(Vector3f left, Vector3f right) {
        return new Vector3f(left.x - right.x, left.y - right.y, left.z - right.z);
    }

    public static Vector3f operator *(float k, Vector3f right) {
        return new Vector3f(k * right.x, k * right.y, k * right.z);
    }

    public static Vector3f operator *(Vector3f left, float k) {
        return new Vector3f(k * left.x, k * left.y, k * left.z);
    }

    public static bool operator ==(Vector3f left, Vector3f right) {
        return (double)(left.x - right.x) < 1E-06 && (double)(left.y - right.y) < 1E-06 && (double)(left.z - right.z) < 1E-06;
    }

    public static bool operator !=(Vector3f left, Vector3f right) {
        return !(left == right);
    }

    public Vector3 ToVector3() {
        return new Vector3(x, y, z);
    }
}