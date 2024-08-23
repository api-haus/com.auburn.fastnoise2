using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Mathematics;

namespace FastNoise2.Runtime.NativeTexture
{
	/// <summary>
	/// This extension adds usage of NativeReference of float2 as MinMax Bounds.
	/// </summary>
	public static class NativeReferenceBoundsExtension
	{
		/// <summary>
		/// Not called at all. Just an example of unoptimised call.
		///
		/// We bake Scale into [1] parameter of bounds during Optimise().
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float NormalizeToBoundsUnoptimised(this NativeReference<float2> bounds, float value)
		{
			var (min, max) = (bounds.Value[0], bounds.Value[1]);
			var scale = max - min;

			return (value - min) * scale;
		}

		/// <summary>
		/// Called during normalization.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float NormalizeToBounds(this NativeReference<float2> bounds, float value)
		{
			var (min, scale) = (bounds.Value[0], bounds.Value[1]);

			return (value - min) * scale;
		}

		/// <summary>
		/// Called before normalization.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Optimise(this NativeReference<float2> bounds)
		{
			var (min, max) = (bounds.Value[0], bounds.Value[1]);
			var scale = max - min;

			bounds.Value = new float2(min, scale);
		}

		/// <summary>
		/// Called before noise generation.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Reset(this NativeReference<float2> bounds)
		{
			bounds.Value = new(float.PositiveInfinity, float.NegativeInfinity);
		}
	}
}
