//using Unity.Collections;
//using Unity.Mathematics;

//namespace Stubblefield.TerrainGen
//{
//    public struct NoiseStack : IIsosurface
//    {
//        public NativeArray<Noise> layers;

//        public NoiseStack(Noise[] items, Allocator allocator)
//        {
//            layers = new NativeArray<Noise>(items, allocator);
//        }

//        public float IsosurfaceValue(float3 position)
//        {
//            float value = 0;
//            for (int i = 0; i < layers.Length; i++)
//            {
//                if (!layers[i].enabled) continue;
//                value += layers[i].IsosurfaceValue(position);
//            }
//            return value;
//        }
//    }
//}