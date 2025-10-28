using UnityEngine;

namespace Mono.Simd.Math
{
	public static class Vector3Extension
	{
		public static Vector4f AsVector4f(this Vector3 vec)
		{
			return new Vector4f(vec.x, vec.y, vec.z, 0f);
		}

		public static bool Approximately(Vector3 a, Vector3 b)
		{
			return Mathf.Approximately(a.x, b.x) && Mathf.Approximately(a.y, b.y) && Mathf.Approximately(a.z, b.z);
		}

		public static Vector4f AsPhyVector(this Vector3 vec)
		{
			return new Vector4f(vec.x, vec.y, vec.z, 0f);
		}

		public static Vector4f AsPhyVector(this Vector3 vec, ref Vector4f repr)
		{
			return new Vector4f(vec.x, vec.y, vec.z, 0f);
		}

		public static Vector3d AsPhyVector(this Vector3 vec, ref Vector3d repr)
		{
			return new Vector3d(vec.x, vec.y, vec.z);
		}

		public static bool IsNaN(Vector3 v)
		{
			return float.IsNaN(v.x) || float.IsNaN(v.y) || float.IsNaN(v.z);
		}

		public static bool IsInfinity(Vector3 v)
		{
			return float.IsInfinity(v.x) || float.IsInfinity(v.y) || float.IsInfinity(v.z);
		}

		public static bool IsZero(Vector3 v)
		{
			return v.x == 0f && v.y == 0f && v.z == 0f;
		}
	}
}
