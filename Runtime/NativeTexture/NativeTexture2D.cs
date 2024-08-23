using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

namespace FastNoise2.Runtime.NativeTexture
{
	/// <summary>
	/// Native Texture2D Wrapper.
	/// </summary>
	[DebuggerDisplay("Length = {RawTextureData.m_Length}")]
	public struct NativeTexture2D<T> : IDisposable where T : unmanaged
	{
		#region Fields

		[NativeDisableContainerSafetyRestriction] [NativeDisableParallelForRestriction]
		internal NativeReference<IntPtr> TexturePtr;

		[NativeDisableContainerSafetyRestriction] [NativeDisableParallelForRestriction]
		internal NativeReference<float2> BoundsRef;

		[NativeDisableContainerSafetyRestriction] [NativeDisableParallelForRestriction]
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
		public NativeTexture2D(Texture2D texture) : this(new int2(texture.width, texture.height), texture)
		{
		}

		public NativeTexture2D(int2 resolution, Texture2D texture) : this(resolution, texture.GetRawTextureData<T>())
		{
			TexturePtr = new NativeReference<IntPtr>(texture.GetNativeTexturePtr(), Allocator.Persistent);
		}

		/// <summary>
		/// Create NativeTexture2D With Allocator.
		/// </summary>
		public NativeTexture2D(int2 resolution, Allocator allocator) : this(resolution,
			new NativeArray<T>(resolution.x * resolution.y, allocator))
		{
		}

		/// <summary>
		/// Create NativeTexture2D From existing data.
		/// </summary>
		public NativeTexture2D(int2 resolution, NativeArray<T> rawTextureData)
		{
			Resolution = resolution;
			TexturePtr = new NativeReference<IntPtr>(IntPtr.Zero, Allocator.Persistent);
			RawTextureData = rawTextureData;
			BoundsRef = new NativeReference<float2>(
				new float2(float.PositiveInfinity, float.NegativeInfinity),
				Allocator.Persistent
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
			if (texture.GetNativeTexturePtr() != TexturePtr.Value)
			{
				var textureData = RawTextureData;
				var writeableTextureMemory = texture.GetRawTextureData<T>();

				UnityEngine.Debug.Log(
					$"t={textureData.Length},w={writeableTextureMemory.Length}, {typeof(T)}, {texture.format}, {texture.graphicsFormat}, {texture.width}x{texture.height}");

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
			if (TexturePtr.IsCreated)
				TexturePtr.Dispose();
		}

		#endregion
	}
}
