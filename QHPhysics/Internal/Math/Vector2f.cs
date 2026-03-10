using System;
using Newtonsoft.Json;
using UnityEngine;

[Serializable]
public struct Vector2f
{
    [JsonProperty] public float x;

    [JsonProperty] public float y;

    [JsonIgnore]
    public float Magnitude {
        get { return (float)Math.Sqrt(x * x + y * y); }
    }

    [JsonIgnore]
    public float SqrMagnitude {
        get { return x * x + y * y; }
    }

    public Vector2f(float x0, float y0) {
        x = x0;
        y = y0;
    }

    public Vector2f(Vector3 v, bool ignoreHeight = true) {
        x = v.x;
        y = ((!ignoreHeight) ? v.y : v.z);
    }

    public static Vector2f operator *(float left, Vector2f right) {
        return new Vector2f(left * right.x, left * right.y);
    }

    public static Vector2f operator *(Vector2f left, float right) {
        return new Vector2f(left.x * right, left.y * right);
    }

    public static Vector2f operator +(Vector2f left, Vector2f right) {
        return new Vector2f(left.x + right.x, left.y + right.y);
    }

    public static Vector2f operator -(Vector2f left, Vector2f right) {
        return new Vector2f(left.x - right.x, left.y - right.y);
    }

    public static bool operator ==(Vector2f left, Vector2f right) {
        return (double)(left.x - right.x) < 1E-06 && (double)(left.y - right.y) < 1E-06;
    }

    public static bool operator !=(Vector2f left, Vector2f right) {
        return !(left == right);
    }

    public Vector2 ToVector2() {
        return new Vector2(x, y);
    }
}