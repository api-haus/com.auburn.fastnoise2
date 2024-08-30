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
	/// Native Texture2D Wrapper as 3D.
	/// </summary>
	[DebuggerDisplay("Length = {RawTextureData.m_Length}")]
	public struct NativeTexture3D<T> : IDisposable where T : unmanaged
	{
		#region Fields

		public int3 Resolution;

		[ReadOnly] [NativeDisableUnsafePtrRestriction]
		readonly IntPtr TexturePtr;

		[NativeDisableContainerSafetyRestriction]
		internal NativeReference<float2> BoundsRef;

		internal NativeArray<T> RawTextureData;

		#endregion

		#region AutoProperties

		public readonly int Width => Resolution.x;
		public readonly int Height => Resolution.y;
		public readonly int Depth => Resolution.z;

		public readonly int Row2 => Width * Height;
		public readonly int Row1 => Width;

		public readonly bool IsCreated => RawTextureData.IsCreated;

		public readonly bool HasTexture2DPointer => TexturePtr == IntPtr.Zero;

		#endregion

		#region Array Accessors

		public int Length
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => RawTextureData.Length;
		}

		public T this[int x, int y, int z]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => RawTextureData[z * Row2 + y * Row1 + x];
			[WriteAccessRequired, MethodImpl(MethodImplOptions.AggressiveInlining)]
			set => RawTextureData[z * Row2 + y * Row1 + x] = value;
		}

		public T this[int3 c]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => ReadPixel(c);
			[WriteAccessRequired, MethodImpl(MethodImplOptions.AggressiveInlining)]
			set => RawTextureData[c.z * Row2 + c.y * Row1 + c.x] = value;
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
		/// Create NativeTexture2D From Texture2D.
		/// </summary>
		public NativeTexture3D(int3 resolution, Texture2D texture, Allocator allocator)
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
		/// Create NativeTexture3D With Allocator.
		/// </summary>
		public NativeTexture3D(int3 resolution, Allocator allocator)
		{
			Resolution = resolution;
			TexturePtr = IntPtr.Zero;
			RawTextureData = new NativeArray<T>(resolution.x * resolution.y * resolution.z, allocator);
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
			pixelIndex = math.clamp(pixelIndex, 0, Length);

			return this[pixelIndex];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T ReadPixel(int3 pixelCoord)
		{
			pixelCoord = math.clamp(pixelCoord, 0, Resolution - 1);

			return this[pixelCoord.z * Row2 + pixelCoord.y * Row1 + pixelCoord.x];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T ReadPixel(int pixelIndex, out int3 coord)
		{
			int z = pixelIndex / (Width * Height);
			int remainderAfterZ = pixelIndex % (Width * Height);
			int y = remainderAfterZ / Width;
			int x = remainderAfterZ % Width;

			coord = new int3(
				x, y, z
			);

			return this[pixelIndex];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryReadPixel(int pixelIndex, out int3 coord, out T pixel)
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
			if (HasTexture2DPointer && RawTextureData.IsCreated)
				RawTextureData.Dispose();
		}

		#endregion
	}
}
