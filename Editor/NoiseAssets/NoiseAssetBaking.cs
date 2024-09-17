using System;
using FastNoise2.Authoring;
using FastNoise2.Runtime.Bindings;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace FastNoise2.Editor.NoiseAssets
{
	public static class NoiseAssetBaking
	{
		public static void BakeIntoAsset(this NoiseAsset noiseAsset, string assetPath)
		{
			var texture = AssetDatabase.LoadAssetAtPath<Texture>(assetPath);
			if (!Validate(noiseAsset, texture))
			{
				if (texture)
					AssetDatabase.RemoveObjectFromAsset(texture);

				texture = MakeTexture(noiseAsset);

				AssetDatabase.AddObjectToAsset(texture, assetPath);
				AssetDatabase.ImportAsset(assetPath);
			}

			GenerateNoiseTexture(noiseAsset, texture);
			EditorUtility.SetDirty(texture);
		}

		static void GenerateNoiseTexture(NoiseAsset noiseAsset, Texture texture)
		{
			switch (noiseAsset.textureOutput)
			{
				case NoiseAssetTextureOutput.Texture2D:
					GenerateNoiseTexture2D(noiseAsset, (Texture2D)texture);
					break;
				case NoiseAssetTextureOutput.Texture3D:
					GenerateNoiseTexture3D(noiseAsset, (Texture3D)texture);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(noiseAsset.textureOutput));
			}
		}

		static unsafe void GenerateNoiseTexture2D(NoiseAsset noiseAsset, Texture2D texture)
		{
			using var graph = noiseAsset.graph.Instantiate();

			NativeArray<float> buffer = AllocateNative(noiseAsset, texture, out NativeReference<float2> minMax);
			using NativeReference<float2> nativeReference = minMax;

			graph.GenUniformGrid2D(
				buffer.GetUnsafePtr(),
				minMax.GetUnsafePtr(),
				noiseAsset.offset.x,
				noiseAsset.offset.y,
				noiseAsset.resolution.x,
				noiseAsset.resolution.y,
				noiseAsset.frequency,
				noiseAsset.seed
			);

			texture.Apply(false);
		}

		static NativeArray<float> AllocateNative(NoiseAsset noiseAsset, Texture3D texture,
			out NativeReference<float2> minMax)
		{
			minMax = default;
			try
			{
				var buffer = texture.GetPixelData<float>(0);
				minMax = new NativeReference<float2>(
					new float2(float.PositiveInfinity,
						float.NegativeInfinity),
					Allocator.Temp
				);
				return buffer;
			}
			catch
			{
				if (minMax.IsCreated)
					minMax.Dispose();
				throw;
			}
		}

		static NativeArray<float> AllocateNative(NoiseAsset noiseAsset, Texture2D texture,
			out NativeReference<float2> minMax)
		{
			minMax = default;
			try
			{
				var buffer = texture.GetRawTextureData<float>();
				minMax = new NativeReference<float2>(
					new float2(float.PositiveInfinity,
						float.NegativeInfinity),
					Allocator.Temp
				);
				return buffer;
			}
			catch
			{
				if (minMax.IsCreated)
					minMax.Dispose();
				throw;
			}
		}

		static unsafe void GenerateNoiseTexture3D(NoiseAsset noiseAsset, Texture3D texture)
		{
			using var graph = noiseAsset.graph.Instantiate();

			NativeArray<float> buffer = AllocateNative(noiseAsset, texture, out NativeReference<float2> minMax);
			using NativeReference<float2> nativeReference = minMax;

			graph.GenUniformGrid3D(
				buffer.GetUnsafePtr(),
				minMax.GetUnsafePtr(),
				noiseAsset.offset.x,
				noiseAsset.offset.y,
				noiseAsset.offset.z,
				noiseAsset.resolution.x,
				noiseAsset.resolution.y,
				noiseAsset.resolution.z,
				noiseAsset.frequency,
				noiseAsset.seed
			);

			texture.Apply(false);
		}

		static bool Validate(NoiseAsset noiseAsset, Texture existingTexture)
		{
			if (existingTexture == null || !existingTexture)
				return false;
			if (existingTexture.dimension != noiseAsset.Dimension())
				return false;
			if (existingTexture.width != noiseAsset.resolution.x)
				return false;
			if (existingTexture.height != noiseAsset.resolution.y)
				return false;
			if (existingTexture.dimension == TextureDimension.Tex3D &&
			    ((Texture3D)existingTexture).depth != noiseAsset.resolution.z)
				return false;

			return true;
		}

		public static TextureDimension Dimension(this NoiseAsset noiseAsset)
		{
			switch (noiseAsset.textureOutput)
			{
				case NoiseAssetTextureOutput.Texture2D:
					return TextureDimension.Tex2D;
				case NoiseAssetTextureOutput.Texture3D:
					return TextureDimension.Tex3D;
				default:
					throw new ArgumentOutOfRangeException(nameof(noiseAsset.textureOutput));
			}
		}

		public static Texture MakeTexture(NoiseAsset noiseAsset)
		{
			switch (noiseAsset.textureOutput)
			{
				case NoiseAssetTextureOutput.Texture2D:
					return new Texture2D(
						noiseAsset.resolution.x,
						noiseAsset.resolution.y,
						GetTextureFormat(noiseAsset),
						false) { name = noiseAsset.name };
				case NoiseAssetTextureOutput.Texture3D:
					return new Texture3D(
						noiseAsset.resolution.x,
						noiseAsset.resolution.y,
						noiseAsset.resolution.z,
						GetTextureFormat(noiseAsset),
						false) { name = noiseAsset.name };
				default:
					throw new ArgumentOutOfRangeException(nameof(noiseAsset.textureOutput));
			}
		}

		public static TextureFormat GetTextureFormat(NoiseAsset noiseAsset)
		{
			switch (noiseAsset.textureFormat)
			{
				// case NoiseAssetTextureFormat.R8:
				// return TextureFormat.R8;
				// case NoiseAssetTextureFormat.R16:
				// return TextureFormat.R16;
				case NoiseAssetTextureFormat.R32:
					return TextureFormat.RFloat;
				default:
					throw new ArgumentOutOfRangeException(nameof(noiseAsset.textureFormat));
			}
		}
	}
}
