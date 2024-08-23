using System.Runtime.CompilerServices;
using FastNoise2.Runtime.NativeTexture;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

namespace FastNoise2.Runtime.Jobs._2D
{
	[BurstCompile]
	public struct NormalizeTexture2DJob : IJobParallelFor
	{
		[BurstCompile]
		public struct OptimiseBoundsJob : IJob
		{
			[NativeDisableUnsafePtrRestriction] public NativeTexture2D<float> Texture;

			public void Execute()
			{
				Texture.BoundsRef.Optimise();
			}
		}

		[BurstCompile]
		struct ResetBoundsJob : IJob
		{
			[NativeDisableUnsafePtrRestriction] public NativeTexture2D<float> Texture;

			public void Execute()
			{
				Texture.BoundsRef.Value = new float2(0, 1);
			}
		}

		[NativeDisableUnsafePtrRestriction] public NativeTexture2D<float> Texture;

		[BurstCompile]
		public void Execute(int i)
		{
			Texture[i] = ReadPixelNormalized(i);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public float ReadPixelNormalized(int i)
		{
			return Texture.BoundsRef.NormalizeToBounds(Texture[i]);
		}

		public static JobHandle Schedule(
			NativeTexture2D<float> noiseDataNoiseOut,
			JobHandle dependency)
		{
			var optimiseJob = new OptimiseBoundsJob
			{
				Texture = noiseDataNoiseOut,
			}.Schedule(dependency);

			var normalizeJob = new NormalizeTexture2DJob
			{
				Texture = noiseDataNoiseOut,
			}.Schedule(noiseDataNoiseOut.Length, noiseDataNoiseOut.Width, optimiseJob);

			var resetJob = new ResetBoundsJob
			{
				Texture = noiseDataNoiseOut,
			}.Schedule(normalizeJob);

			return resetJob;
		}
	}
}
