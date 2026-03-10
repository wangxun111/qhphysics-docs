using System;
using UnityEngine;

namespace Mono.Simd
{
	public struct Vector3d
	{
		public double X;

		public double Y;

		public double Z;

		public static readonly Vector3d up = new Vector3d(0.0, 1.0, 0.0);

		public static readonly Vector3d down = new Vector3d(0.0, -1.0, 0.0);

		public static readonly Vector3d right = new Vector3d(1.0, 0.0, 0.0);

		public static readonly Vector3d left = new Vector3d(-1.0, 0.0, 0.0);

		public static readonly Vector3d forward = new Vector3d(0.0, 0.0, 1.0);

		public static readonly Vector3d back = new Vector3d(0.0, 0.0, -1.0);

		public static readonly Vector3d Zero = new Vector3d(0.0);

		public static readonly Vector3d one = new Vector3d(1.0);

		public static readonly Vector3d two = new Vector3d(2.0);

		public static readonly Vector3d half = new Vector3d(0.5);

		public static readonly Vector3d onethird = new Vector3d(1.0 / 3.0);

		public static readonly Vector3d quarter = new Vector3d(0.25);

		public Vector3d(double x, double y, double z)
		{
			X = x;
			Y = y;
			Z = z;
		}

		public Vector3d(double f)
		{
			X = f;
			Y = f;
			Z = f;
		}

		public Vector3d(float x, float y, float z)
		{
			X = x;
			Y = y;
			Z = z;
		}

		public Vector3d(float f)
		{
			X = f;
			Y = f;
			Z = f;
		}

		public Vector3d(Vector3 v)
		{
			X = v.x;
			Y = v.y;
			Z = v.z;
		}

		public Vector3d(Vector4f v)
		{
			X = v.X;
			Y = v.Y;
			Z = v.Z;
		}

		public static Vector3d operator +(Vector3d a, Vector3d b)
		{
			return new Vector3d(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
		}

		public static Vector3d operator +(Vector3d a, Vector4f b)
		{
			return new Vector3d(a.X + (double)b.X, a.Y + (double)b.Y, a.Z + (double)b.Z);
		}

		public static Vector3d operator -(Vector3d a, Vector3d b)
		{
			return new Vector3d(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
		}

		public static Vector3d operator -(Vector3d a, Vector4f b)
		{
			return new Vector3d(a.X - (double)b.X, a.Y - (double)b.Y, a.Z - (double)b.Z);
		}

		public static Vector3d operator *(Vector3d a, Vector3d b)
		{
			return new Vector3d(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
		}

		public static Vector3d operator *(Vector3d a, Vector4f b)
		{
			return new Vector3d(a.X * (double)b.X, a.Y * (double)b.Y, a.Z * (double)b.Z);
		}

		public static Vector3d operator *(Vector3d a, double f)
		{
			return new Vector3d(a.X * f, a.Y * f, a.Z * f);
		}

		public static Vector3d operator /(Vector3d a, Vector3d b)
		{
			return new Vector3d(a.X / b.X, a.Y / b.Y, a.Z / b.Z);
		}

		public static Vector3d operator /(Vector3d a, double f)
		{
			return new Vector3d(a.X / f, a.Y / f, a.Z / f);
		}

		public void Negate()
		{
			X = 0.0 - X;
			Y = 0.0 - Y;
			Z = 0.0 - Z;
		}

		public Vector3d Negative()
		{
			return new Vector3d(0.0 - X, 0.0 - Y, 0.0 - Z);
		}

		public void Normalize()
		{
			double num = Magnitude();
			X /= num;
			Y /= num;
			Z /= num;
		}

		public Vector3d Normalized()
		{
			double num = Magnitude();
			return new Vector3d(X / num, Y / num, Z / num);
		}

		public double Magnitude()
		{
			return System.Math.Sqrt(X * X + Y * Y + Z * Z);
		}

		public double SqrMagnitude()
		{
			return X * X + Y * Y + Z * Z;
		}

		public double Distance(Vector3d other)
		{
			return (this - other).Magnitude();
		}

		public Vector3 AsVector3()
		{
			return new Vector3((float)X, (float)Y, (float)Z);
		}

		public static Vector3d Cross(Vector3d a, Vector3d b)
		{
			return new Vector3d(a.Y * b.Z - a.Z * b.Y, a.Z * b.X - a.X * b.Z, a.X * b.Y - a.Y * b.X);
		}

		public static double Dot(Vector3d a, Vector3d b)
		{
			return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
		}

		public static Vector3d FromVector3(Vector3 v)
		{
			return new Vector3d(v.x, v.y, v.z);
		}

		public static Vector3d FromVector4f(Vector4f v)
		{
			return new Vector3d(v.X, v.Y, v.Z);
		}

		public static Vector3d Lerp(Vector3d a, Vector3d b, double t)
		{
			return a * (1.0 - t) + b * t;
		}

		public static Vector3d ProjectPointOnPlane(Vector3d planeNormal, Vector3d planePoint, Vector3d point)
		{
			return point - planeNormal * Dot(planeNormal, point - planePoint);
		}
	}
}
