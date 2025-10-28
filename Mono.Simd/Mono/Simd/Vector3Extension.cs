using UnityEngine;

namespace Mono.Simd
{
	public static class Vector3Extension
	{
		public static Vector3d AsVector3d(this Vector3 vec)
		{
			return new Vector3d(vec.x, vec.y, vec.z);
		}

		public static Vector3d AsVector3d(this Vector4f vec)
		{
			return new Vector3d(vec.X, vec.Y, vec.Z);
		}

		public static Vector4f AsVector4f(this Vector3d vec)
		{
			return new Vector4f((float)vec.X, (float)vec.Y, (float)vec.Z, 0f);
		}

		public static Vector3 AsVector3(this Vector3d vec)
		{
			return new Vector3((float)vec.X, (float)vec.Y, (float)vec.Z);
		}
	}
}
