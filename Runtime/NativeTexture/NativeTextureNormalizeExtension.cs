using Unity.Mathematics;

namespace FastNoise2.Runtime.NativeTexture
{
	public static class NativeTextureNormalizeExtension
	{
		public static float ReadPixelNormalized(this NativeTexture2D<float> tex, int2 pixelCoord)
		{
			var pixel = tex[pixelCoord.x, pixelCoord.y];

			return tex.BoundsRef.NormalizeToBounds(pixel);
		}
	}
}
