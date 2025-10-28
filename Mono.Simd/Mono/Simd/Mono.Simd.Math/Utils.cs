using System;

namespace Mono.Simd.Math
{
	public static class Utils
	{
		public const float E = (float)System.Math.E;

		public const float Log10E = 0.4342945f;

		public const float Log2E = 1.442695f;

		public const float Pi = (float)System.Math.PI;

		public const float PiOver2 = (float)System.Math.PI / 2f;

		public const float PiOver4 = (float)System.Math.PI / 4f;

		public const float TwoPi = (float)System.Math.PI * 2f;

		public static bool MinimalSimdSupport()
		{
			return SimdRuntime.IsMethodAccelerated(typeof(Vector4f), "op_Addition") && SimdRuntime.IsMethodAccelerated(typeof(Vector4f), "op_Multiply") && SimdRuntime.IsMethodAccelerated(typeof(VectorOperations), "Shuffle", typeof(Vector4f), typeof(ShuffleSel));
		}

		public static bool EnhancedSimdSupport()
		{
			return SimdRuntime.IsMethodAccelerated(typeof(VectorOperations), "HorizontalAdd", typeof(Vector4f), typeof(Vector4f)) && SimdRuntime.IsMethodAccelerated(typeof(Vector4f), "op_Multiply");
		}

		public static float Barycentric(float value1, float value2, float value3, float amount1, float amount2)
		{
			return value1 + (value2 - value1) * amount1 + (value3 - value1) * amount2;
		}

		public static float CatmullRom(float value1, float value2, float value3, float value4, float amount)
		{
			double num = amount * amount;
			double num2 = num * (double)amount;
			return (float)(0.5 * (2.0 * (double)value2 + (double)((value3 - value1) * amount) + (2.0 * (double)value1 - 5.0 * (double)value2 + 4.0 * (double)value3 - (double)value4) * num + (3.0 * (double)value2 - (double)value1 - 3.0 * (double)value3 + (double)value4) * num2));
		}

		public static float Clamp(float value, float min, float max)
		{
			value = ((!(value > max)) ? value : max);
			value = ((!(value < min)) ? value : min);
			return value;
		}

		public static float Distance(float value1, float value2)
		{
			return System.Math.Abs(value1 - value2);
		}

		public static float Hermite(float value1, float tangent1, float value2, float tangent2, float amount)
		{
			double num = value1;
			double num2 = value2;
			double num3 = tangent1;
			double num4 = tangent2;
			double num5 = amount;
			double num6 = num5 * num5 * num5;
			double num7 = num5 * num5;
			double num8 = ((amount == 0f) ? ((double)value1) : ((amount != 1f) ? ((2.0 * num - 2.0 * num2 + num4 + num3) * num6 + (3.0 * num2 - 3.0 * num - 2.0 * num3 - num4) * num7 + num3 * num5 + num) : ((double)value2)));
			return (float)num8;
		}

		public static bool IsFinite(float value)
		{
			return !float.IsNaN(value) && !float.IsInfinity(value);
		}

		public static float Lerp(float value1, float value2, float amount)
		{
			return value1 + (value2 - value1) * amount;
		}

		public static float Max(float value1, float value2)
		{
			return System.Math.Max(value1, value2);
		}

		public static float Min(float value1, float value2)
		{
			return System.Math.Min(value1, value2);
		}

		public static float SmoothStep(float value1, float value2, float amount)
		{
			float amount2 = Clamp(amount, 0f, 1f);
			return Hermite(value1, 0f, value2, 0f, amount2);
		}

		public static float ToDegrees(float radians)
		{
			return (float)((double)radians * (180.0 / System.Math.PI));
		}

		public static float ToRadians(float degrees)
		{
			return (float)((double)degrees * (System.Math.PI / 180.0));
		}
	}
}
