using FastNoise2.Runtime.NativeTexture;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using static FastNoise2.Runtime.Bindings.FastNoise;

namespace FastNoise2.Runtime.Bindings
{
    public static class FastNoise2NativeTextureExtensions
    {
        public static unsafe void GenUniformGrid2D(
            this FastNoise fn,
            NativeTexture2D<float> nativeTexture,
            int xStart, int yStart,
            int xSize, int ySize,
            float frequency,
            int seed)
        {
            nativeTexture.BoundsRef.Reset();
            fnGenUniformGrid2D(
                fn.mNodeHandle,
                nativeTexture.GetRawTextureData().GetUnsafePtr(),
                xStart, yStart,
                xSize, ySize,
                frequency, seed,
                nativeTexture.BoundsRef.GetUnsafePtr()
            );
        }

        public static unsafe void GenUniformGrid3D(
            this FastNoise fn,
            NativeTexture3D<float> nativeTexture,
            int xStart, int yStart, int zStart,
            int xSize, int ySize, int zSize,
            float frequency, int seed)
        {
            nativeTexture.BoundsRef.Reset();
            fnGenUniformGrid3D(
                fn.mNodeHandle,
                nativeTexture.GetRawTextureData().GetUnsafePtr(),
                xStart, yStart, zStart,
                xSize, ySize, zSize,
                frequency, seed,
                nativeTexture.BoundsRef.GetUnsafePtr()
            );
        }

        public static unsafe void GenUniformGrid4D(
            this FastNoise fn,
            NativeTexture4D<float> nativeTexture,
            int xStart, int yStart, int zStart, int wStart,
            int xSize, int ySize, int zSize, int wSize,
            float frequency, int seed)
        {
            nativeTexture.BoundsRef.Reset();
            fnGenUniformGrid4D(
                fn.mNodeHandle,
                nativeTexture.GetRawTextureData().GetUnsafePtr(),
                xStart, yStart, zStart, wStart,
                xSize, ySize, zSize, wSize,
                frequency,
                seed,
                nativeTexture.BoundsRef.GetUnsafePtr()
            );
        }

        public static unsafe void GenTileable2D(
            this FastNoise fn,
            NativeTexture2D<float> nativeTexture,
            int xSize, int ySize,
            float frequency, int seed)
        {
            nativeTexture.BoundsRef.Reset();
            fnGenTileable2D(
                fn.mNodeHandle,
                nativeTexture.GetRawTextureData().GetUnsafePtr(),
                xSize, ySize,
                frequency, seed,
                nativeTexture.BoundsRef.GetUnsafePtr()
            );
        }

        public static unsafe void GenPositionArray2D(
            this FastNoise fn,
            NativeTexture2D<float> nativeTexture,
            int positionCount,
            NativeArray<float> xPosArray,
            NativeArray<float> yPosArray,
            float xOffset, float yOffset,
            int seed)
        {
            nativeTexture.BoundsRef.Reset();
            fnGenPositionArray2D(
                fn.mNodeHandle,
                nativeTexture.GetRawTextureData().GetUnsafePtr(),
                positionCount,
                xPosArray.GetUnsafePtr(),
                yPosArray.GetUnsafePtr(),
                xOffset, yOffset,
                seed,
                nativeTexture.BoundsRef.GetUnsafePtr()
            );
        }

        public static unsafe void GenPositionArray3D(
            this FastNoise fn,
            NativeTexture3D<float> nativeTexture,
            int positionCount,
            NativeArray<float> xPosArray,
            NativeArray<float> yPosArray,
            NativeArray<float> zPosArray,
            float xOffset, float yOffset, float zOffset,
            int seed)
        {
            nativeTexture.BoundsRef.Reset();
            fnGenPositionArray3D(
                fn.mNodeHandle,
                nativeTexture.GetRawTextureData().GetUnsafePtr(),
                positionCount,
                xPosArray.GetUnsafePtr(),
                yPosArray.GetUnsafePtr(),
                zPosArray.GetUnsafePtr(),
                xOffset, yOffset, zOffset,
                seed,
                nativeTexture.BoundsRef.GetUnsafePtr()
            );
        }

        public static unsafe void GenPositionArray4D(
            this FastNoise fn,
            NativeTexture4D<float> nativeTexture,
            int positionCount,
            NativeArray<float> xPosArray,
            NativeArray<float> yPosArray,
            NativeArray<float> zPosArray,
            NativeArray<float> wPosArray,
            float xOffset, float yOffset, float zOffset, float wOffset,
            int seed)
        {
            nativeTexture.BoundsRef.Reset();
            fnGenPositionArray4D(
                fn.mNodeHandle,
                nativeTexture.GetRawTextureData().GetUnsafePtr(),
                positionCount,
                xPosArray.GetUnsafePtr(),
                yPosArray.GetUnsafePtr(),
                zPosArray.GetUnsafePtr(),
                wPosArray.GetUnsafePtr(),
                xOffset, yOffset, zOffset, wOffset,
                seed,
                nativeTexture.BoundsRef.GetUnsafePtr()
            );
        }
    }
}
