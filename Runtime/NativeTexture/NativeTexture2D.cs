using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace FastNoise2.Runtime.NativeTexture
{
	/// <summary>
	/// Native Texture2D Wrapper.
	/// </summary>
	[DebuggerDisplay("Length = {RawTextureData.m_Length}")]
	public struct NativeTexture2D<T> : INativeDisposable where T : unmanaged
	{
		#region Fields

		[ReadOnly] [NativeDisableUnsafePtrRestriction]
		readonly IntPtr TexturePtr;

		[NativeDisableContainerSafetyRestriction]
		internal NativeReference<float2> BoundsRef;

		internal NativeArray<T> RawTextureData;

		#endregion

		#region AutoProperties

		public readonly int2 Resolution;

		public readonly int Width => Resolution.x;
		public readonly int Height => Resolution.y;

		public readonly int Row1 => Width;

		public readonly bool IsCreated => RawTextureData.IsCreated;

		#endregion

		#region Array Accessors

		public int Length
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => RawTextureData.Length;
		}

		public T this[int x, int y]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => RawTextureData[y * Row1 + x];
			[WriteAccessRequired, MethodImpl(MethodImplOptions.AggressiveInlining)]
			set => RawTextureData[y * Row1 + x] = value;
		}

		public T this[int2 coord]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => this[coord.x, coord.y];
			[WriteAccessRequired, MethodImpl(MethodImplOptions.AggressiveInlining)]
			set => this[coord.x, coord.y] = value;
		}

		public T this[int index]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => ReadPixel(index);
			[WriteAccessRequired, MethodImpl(MethodImplOptions.AggressiveInlining)]
			set => RawTextureData[index] = value;
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Create NativeTexture2D From Texture2D.
		/// </summary>
		public NativeTexture2D(int2 resolution, Texture2D texture, Allocator allocator)
		{
			Resolution = resolution;
			TexturePtr = texture.GetNativeTexturePtr();
			RawTextureData = texture.GetRawTextureData<T>();
			BoundsRef = new NativeReference<float2>(
				new float2(float.PositiveInfinity, float.NegativeInfinity),
				allocator
			);
		}

		/// <summary>
		/// Create NativeTexture2D.
		/// </summary>
		public NativeTexture2D(int2 resolution, Allocator allocator)
		{
			Resolution = resolution;
			TexturePtr = IntPtr.Zero;
			RawTextureData = new NativeArray<T>(resolution.x * resolution.y, allocator);
			BoundsRef = new NativeReference<float2>(
				new float2(float.PositiveInfinity, float.NegativeInfinity),
				allocator
			);
		}

		#endregion

		#region NativeTexture API

		public NativeArray<T> GetRawTextureData()
		{
			return RawTextureData;
		}

		public NativeReference<float2> GetBounds()
		{
			return BoundsRef;
		}

		[BurstDiscard]
		public Texture2D ApplyTo(Texture2D texture, bool updateMipmaps = false)
		{
			if (texture.GetNativeTexturePtr() != TexturePtr)
			{
				var textureData = RawTextureData;
				var writeableTextureMemory = texture.GetRawTextureData<T>();

				textureData.CopyTo(writeableTextureMemory);
			}

			texture.Apply(updateMipmaps);
			return texture;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T ReadPixel(int pixelIndex)
		{
			return this[pixelIndex];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T ReadPixel(int pixelIndex, out int2 coord)
		{
			coord = new int2(
				pixelIndex % Width,
				pixelIndex / Width
			);

			return this[pixelIndex];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryReadPixel(int pixelIndex, out int2 coord, out T pixel)
		{
			if (!(pixelIndex >= 0 && pixelIndex < Length))
			{
				pixel = default;
				coord = default;
				return false;
			}

			pixel = ReadPixel(pixelIndex, out coord);
			return true;
		}

		#endregion

		#region IDisposable

		public void Dispose()
		{
			if (RawTextureData.IsCreated)
				RawTextureData.Dispose();
			if (BoundsRef.IsCreated)
				BoundsRef.Dispose();
		}

		[BurstCompile]
		struct DisposeJob : IJob
		{
			public NativeTexture2D<T> Texture;

			public void Execute()
			{
				Texture.Dispose();
			}
		}

		public JobHandle Dispose(JobHandle inputDeps)
		{
			return new DisposeJob
			{
				Texture = this,
			}.Schedule(inputDeps);
		}

		#endregion
	}
}
