using System.Runtime.CompilerServices;
using FastNoise2.Runtime.NativeTexture;
using Unity.Burst;
using Unity.Collections;
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
			public NativeReference<float2> Bounds;

			public void Execute()
			{
				Bounds.Optimise();
			}
		}

		[BurstCompile]
		struct ResetBoundsJob : IJob
		{
			[WriteOnly] public NativeReference<float2> Bounds;

			public void Execute()
			{
				Bounds.Value = new float2(float.PositiveInfinity, float.NegativeInfinity);
			}
		}

		[NativeDisableParallelForRestriction] public NativeArray<float> Texture;

		[NativeDisableParallelForRestriction] [ReadOnly]
		public NativeReference<float2> Bounds;

		[BurstCompile]
		public void Execute(int i)
		{
			Texture[i] = ReadPixelNormalized(i);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public float ReadPixelNormalized(int i)
		{
			return Bounds.NormalizeToBounds(Texture[i]);
		}

		public static JobHandle Schedule(
			NativeTexture2D<float> noiseDataNoiseOut,
			JobHandle dependency)
		{
			dependency = new OptimiseBoundsJob
			{
				Bounds = noiseDataNoiseOut.BoundsRef,
			}.Schedule(dependency);

			dependency = new NormalizeTexture2DJob
			{
				Texture = noiseDataNoiseOut.GetRawTextureData(),
				Bounds = noiseDataNoiseOut.BoundsRef,
			}.Schedule(noiseDataNoiseOut.Length, noiseDataNoiseOut.Width, dependency);

			dependency = new ResetBoundsJob
			{
				Bounds = noiseDataNoiseOut.BoundsRef,
			}.Schedule(dependency);

			return dependency;
		}
	}
}
