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
	/// Native Texture2D Wrapper as 4D.
	/// </summary>
	[DebuggerDisplay("Length = {RawTextureData.m_Length}")]
	public struct NativeTexture4D<T> : IDisposable where T : unmanaged
	{
		#region Fields

		public readonly int4 Resolution;

		[NativeDisableContainerSafetyRestriction] [NativeDisableParallelForRestriction]
		internal NativeArray<T> RawTextureData;

		internal NativeReference<float2> ValueBounds;

		[NativeDisableUnsafePtrRestriction] internal readonly IntPtr TexturePtr;

		#endregion

		#region AutoProperties

		public readonly int Width => Resolution.x;
		public readonly int Height => Resolution.y;
		public readonly int Depth => Resolution.z;
		public readonly int World => Resolution.w;

		public readonly int Row3 => Width * Height * Depth;
		public readonly int Row2 => Width * Height;
		public readonly int Row1 => Width;

		public readonly bool IsCreated => RawTextureData.IsCreated;

		public readonly bool HasTexturePointer => TexturePtr == IntPtr.Zero;

		#endregion

		#region Array Accessors

		public int Length
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => RawTextureData.Length;
		}

		public T this[int x, int y, int z, int w]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => RawTextureData[w * Row3 + z * Row2 + y * Row1 + x];
			[WriteAccessRequired, MethodImpl(MethodImplOptions.AggressiveInlining)]
			set => RawTextureData[w * Row3 + z * Row2 + y * Row1 + x] = value;
		}

		public T this[int4 c]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => ReadPixel(c);
			[WriteAccessRequired, MethodImpl(MethodImplOptions.AggressiveInlining)]
			set => RawTextureData[c.w * Row3 + c.z * Row2 + c.y * Row1 + c.x] = value;
		}

		public T this[int index]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => RawTextureData[index];
			[WriteAccessRequired, MethodImpl(MethodImplOptions.AggressiveInlining)]
			set => RawTextureData[index] = value;
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Create NativeTexture4D From Texture2D.
		/// </summary>
		public NativeTexture4D(int4 resolution, Texture2D texture)
		{
			Resolution = resolution;
			TexturePtr = texture.GetNativeTexturePtr();
			RawTextureData = texture.GetRawTextureData<T>();
			ValueBounds = new NativeReference<float2>(
				new float2(float.PositiveInfinity, float.NegativeInfinity),
				Allocator.Persistent
			);
		}

		/// <summary>
		/// Create NativeTexture4D With Allocator.
		/// </summary>
		public NativeTexture4D(int4 resolution, Allocator allocator) : this(resolution,
			new NativeArray<T>(resolution.x * resolution.y * resolution.z * resolution.w, allocator))
		{
		}

		/// <summary>
		/// Create NativeTexture4D From existing data.
		/// </summary>
		public NativeTexture4D(int4 resolution, NativeArray<T> rawTextureData)
		{
			Resolution = resolution;
			TexturePtr = IntPtr.Zero;
			RawTextureData = rawTextureData;
			ValueBounds = new NativeReference<float2>(
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
			return ValueBounds;
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
			pixelIndex = math.clamp(pixelIndex, 0, Length);

			return this[pixelIndex];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T ReadPixel(int4 pixelCoord)
		{
			pixelCoord = math.clamp(pixelCoord, 0, Resolution - 1);

			return this[pixelCoord.w * Row3 + pixelCoord.z * Row2 + pixelCoord.y * Row1 + pixelCoord.x];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T ReadPixel(int pixelIndex, out int4 coord)
		{
			int w = pixelIndex / Row3;
			int remainderAfterW = pixelIndex % Row3;

			int z = remainderAfterW / Row2;
			int remainderAfterZ = remainderAfterW % Row2;

			int y = remainderAfterZ / Row1;
			int x = remainderAfterZ % Row1;

			coord = new int4(
				x, y, z, w
			);

			return this[pixelIndex];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryReadPixel(int pixelIndex, out int4 coord, out T pixel)
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
			if (HasTexturePointer && RawTextureData.IsCreated)
				RawTextureData.Dispose();
		}

		#endregion
	}
}
