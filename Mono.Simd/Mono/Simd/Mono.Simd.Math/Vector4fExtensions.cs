using UnityEngine;

namespace Mono.Simd.Math
{
	public static class Vector4fExtensions
	{
		public static readonly Vector4f up = new Vector4f(0f, 1f, 0f, 0f);

		public static readonly Vector4f down = new Vector4f(0f, -1f, 0f, 0f);

		public static readonly Vector4f right = new Vector4f(1f, 0f, 0f, 0f);

		public static readonly Vector4f left = new Vector4f(-1f, 0f, 0f, 0f);

		public static readonly Vector4f forward = new Vector4f(0f, 0f, 1f, 0f);

		public static readonly Vector4f back = new Vector4f(0f, 0f, -1f, 0f);

		public static readonly Vector4f Zero = new Vector4f(0f);

		public static readonly Vector4f one3 = new Vector4f(1f, 1f, 1f, 0f);

		public static readonly Vector4f two3 = new Vector4f(2f, 2f, 2f, 0f);

		public static readonly Vector4f half3 = new Vector4f(0.5f, 0.5f, 0.5f, 0f);

		public static readonly Vector4f onethird3 = new Vector4f(1f / 3f, 1f / 3f, 1f / 3f, 0f);

		public static readonly Vector4f quarter3 = new Vector4f(0.25f, 0.25f, 0.25f, 0f);

		public static readonly Vector4f one = new Vector4f(1f, 1f, 1f, 1f);

		public static readonly Vector4f two = new Vector4f(2f, 2f, 2f, 2f);

		public static readonly Vector4f half = new Vector4f(0.5f, 0.5f, 0.5f, 0.5f);

		public static readonly Vector4f onethird = new Vector4f(1f / 3f, 1f / 3f, 1f / 3f, 1f / 3f);

		public static readonly Vector4f quarter = new Vector4f(0.25f, 0.25f, 0.25f, 0.25f);

		public static void CheckNaN(Vector4f v, Vector4f v_src)
		{
			if (float.IsNaN(v.X) || float.IsNaN(v.X) || float.IsNaN(v.X) || float.IsNaN(v.X))
			{
				Debug.LogError("CheckNan " + v_src);
			}
		}

		public static bool CheckInvalid(Vector4f v)
		{
			return float.IsNaN(v.X) || float.IsNaN(v.Y) || float.IsNaN(v.X) || float.IsNaN(v.X) || float.IsInfinity(v.X) || float.IsInfinity(v.Y) || float.IsInfinity(v.X) || float.IsInfinity(v.X);
		}

		public static bool CheckInvalid(Vector3 v)
		{
			return float.IsNaN(v.x) || float.IsNaN(v.y) || float.IsNaN(v.z) || float.IsInfinity(v.x) || float.IsInfinity(v.y) || float.IsInfinity(v.z);
		}

		public static bool CheckInvalid(float x)
		{
			return float.IsNaN(x) || float.IsInfinity(x);
		}

		public static Vector4f FromVector3(Vector3 v)
		{
			return new Vector4f(v.x, v.y, v.z, 0f);
		}

		public static Vector3 AsVector3(this Vector4f vec)
		{
			return new Vector3(vec.X, vec.Y, vec.Z);
		}

		public static void Negate(this Vector4f vec)
		{
			vec *= Vector4f.MinusOne;
		}

		public static Vector4f Negative(this Vector4f vec)
		{
			return vec * Vector4f.MinusOne;
		}

		public static void Normalize(this Vector4f vec)
		{
			if (vec.SqrMagnitude() <= 1E-10f)
			{
				vec = Vector4f.Zero;
			}
			Vector4f vector4f = vec * vec;
			vector4f = vector4f.HorizontalAdd(vector4f);
			vector4f = vector4f.HorizontalAdd(vector4f);
			vector4f = vector4f.InvSqrt();
			vec *= vector4f;
		}

		public static Vector4f Normalized(this Vector4f vec)
		{
			if (vec.SqrMagnitude() <= 1E-10f)
			{
				return Vector4f.Zero;
			}
			Vector4f vector4f = vec * vec;
			vector4f = vector4f.HorizontalAdd(vector4f);
			vector4f = vector4f.HorizontalAdd(vector4f);
			vector4f = vector4f.InvSqrt();
			return vec * vector4f;
		}

		public static float Magnitude(this Vector4f vec)
		{
			Vector4f vector4f = vec * vec;
			vector4f = vector4f.HorizontalAdd(vector4f);
			vector4f = vector4f.HorizontalAdd(vector4f);
			return vector4f.Sqrt().X;
		}

		public static float SqrMagnitude(this Vector4f vec)
		{
			Vector4f vector4f = vec * vec;
			vector4f = vector4f.HorizontalAdd(vector4f);
			return vector4f.HorizontalAdd(vector4f).X;
		}

		public static void Decompose(this Vector4f vec, out Vector4f normalized, out float magnitude)
		{
			Vector4f vector4f = vec * vec;
			vector4f = vector4f.HorizontalAdd(vector4f);
			vector4f = vector4f.HorizontalAdd(vector4f);
			magnitude = vector4f.Sqrt().X;
			normalized = vec * new Vector4f(1f / magnitude);
		}

		public static float Decompose(this Vector4f vec)
		{
			Vector4f vector4f = vec * vec;
			vector4f = vector4f.HorizontalAdd(vector4f);
			vector4f = vector4f.HorizontalAdd(vector4f);
			float x = vector4f.Sqrt().X;
			vec *= new Vector4f(1f / x);
			return x;
		}

		public static float Distance(this Vector4f vec, Vector4f other)
		{
			return (vec - other).Magnitude();
		}

		public static float LengthSquared(this Vector4f vec)
		{
			Vector4f vector4f = vec * vec;
			vector4f = vector4f.HorizontalAdd(vector4f);
			return vector4f.HorizontalAdd(vector4f).X;
		}

		public static bool ApproxEquals(this Vector4f vec, Vector4f vector, float tolerance)
		{
			Vector4f vec2 = vec - vector;
			return vec2.Magnitude() <= tolerance;
		}

		public static bool Approximately(this Vector4f vec, Vector4f other)
		{
			return Mathf.Approximately(vec.X, other.X) && Mathf.Approximately(vec.Y, other.Y) && Mathf.Approximately(vec.Z, other.Z) && Mathf.Approximately(vec.W, other.W);
		}

		public static bool IsFinite(this Vector4f vec)
		{
			return Utils.IsFinite(vec.X) && Utils.IsFinite(vec.Y) && Utils.IsFinite(vec.Z) && Utils.IsFinite(vec.W);
		}

		public static bool IsZero(this Vector4f vec)
		{
			return vec.X == 0f && vec.Y == 0f && vec.Z == 0f && vec.W == 0f;
		}

		public static float Dot(Vector4f a, Vector4f b)
		{
			Vector4f vector4f = a * b;
			vector4f = vector4f.HorizontalAdd(vector4f);
			return vector4f.HorizontalAdd(vector4f).X;
		}

		public static Vector4f Cross(Vector4f a, Vector4f b)
		{
			Vector4f v = a;
			Vector4f v2 = b;
			v.W = v.X;
			v = v.Shuffle(ShuffleSel.RotateRight);
			v2.W = v2.Z;
			v2 = v2.Shuffle(ShuffleSel.RotateLeft);
			Vector4f v3 = a;
			Vector4f v4 = b;
			v3.W = v3.Z;
			v3 = v3.Shuffle(ShuffleSel.RotateLeft);
			v4.W = v4.X;
			v4 = v4.Shuffle(ShuffleSel.RotateRight);
			Vector4f result = v * v2 - v3 * v4;
			result.W = 0f;
			return result;
		}

		public static Vector4f Lerp(Vector4f a, Vector4f b, float t)
		{
			Vector4f vector4f = new Vector4f(t);
			return a * (Vector4f.One - vector4f) + b * vector4f;
		}

		public static Vector4f Lerp(Vector4f a, Vector4f b, Vector4f t)
		{
			return a * (Vector4f.One - t) + b * t;
		}

		public static Vector4f ProjectPointOnPlane(Vector4f planeNormal, Vector4f planePoint, Vector4f point)
		{
			Vector4f vector4f = new Vector4f(0f - Dot(planeNormal, point - planePoint));
			return point + planeNormal * vector4f;
		}

		public static Quaternion NormalizeQuaternion(Quaternion q)
		{
			Vector4f vector4f = new Vector4f(q.x, q.y, q.z, q.w);
			Vector4f vector4f2 = vector4f * vector4f;
			vector4f2 = vector4f2.HorizontalAdd(vector4f2);
			vector4f *= new Vector4f(1f / Mathf.Sqrt(vector4f2.HorizontalAdd(vector4f2).X));
			return new Quaternion(vector4f.X, vector4f.Y, vector4f.Z, vector4f.W);
		}
	}
}
