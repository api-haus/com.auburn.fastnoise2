using System.IO;
using FastNoise2.Runtime.Bindings;
using FastNoise2.Runtime.Jobs;
using FastNoise2.Runtime.Jobs._2D;
using FastNoise2.Runtime.NativeTexture;
using NUnit.Framework;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace FastNoise2.Tests
{
	public class JobSystemTests
	{
		[BurstCompile]
		struct TestJobGenerateChunk : IJob
		{
			[NativeDisableUnsafePtrRestriction] public NativeTexture2D<float> Texture;
			[NativeDisableUnsafePtrRestriction] public FastNoise Noise;

			public void Execute()
			{
				Noise.GenUniformGrid2D(
					Texture,
					0, 0,
					Texture.Width, Texture.Height,
					0.02f, 1337);
			}
		}

		[Test]
		public void NativeTexture2DInJobSystem()
		{
			var nodeTree = FastNoise.FromEncodedNodeTree("DQAFAAAAAAAAQAgAAAAAAD8AAAAAAA==");

			var texture = new Texture2D(512, 512, TextureFormat.RFloat, false);
			using var nt = new NativeTexture2D<float>(new int2(texture.width, texture.height), Allocator.TempJob);

			JobHandle dependency = default;

			dependency = new TestJobGenerateChunk
			{
				Texture = nt,
				Noise = nodeTree,
			}.Schedule(dependency);

			dependency = new NormalizeTexture2DJob
			{
				Texture = nt,
			}.Schedule(nt.Length, nt.Width, dependency);

			dependency.Complete();

			nt.ApplyTo(texture);

			File.WriteAllBytes("texJobSystem.png", texture.EncodeToPNG());
			Object.DestroyImmediate(texture);
		}
	}
}
