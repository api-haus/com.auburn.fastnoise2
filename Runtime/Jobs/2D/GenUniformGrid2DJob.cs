using FastNoise2.Runtime.Bindings;
using FastNoise2.Runtime.NativeTexture;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

namespace FastNoise2.Runtime.Jobs._2D
{
	[BurstCompile]
	public struct GenUniformGrid2DJob : IJob
	{
		public NativeTexture2D<float> Texture;
		public FastNoise Noise;

		public int Seed;
		public int2 Offset;
		public float Frequency;

		public void Execute()
		{
			Noise.GenUniformGrid2D(
				Texture,
				Offset.x, Offset.y,
				Texture.Width, Texture.Height,
				Frequency, Seed);
		}
	}
}
