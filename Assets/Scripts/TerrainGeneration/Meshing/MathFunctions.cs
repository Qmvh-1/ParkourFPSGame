using Unity.Mathematics;

namespace Stubblefield.TerrainGen
{
    public static class MathFunctions
    {
        public static int OctantIndex(int3 octantCrds)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if ((uint)octantCrds.x >= 2
                | (uint)octantCrds.y >= 2
                | (uint)octantCrds.z >= 2)
                throw new System.ArgumentException("index must be between[0...7]");
#endif
            return octantCrds.x + octantCrds.y * 2 + octantCrds.z * 4;
        }

        public static int3 OctantCrds(int octantIndex)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if ((uint)octantIndex >= 8)
                throw new System.ArgumentException("index must be between[0...7]");
#endif
            return new int3(
                octantIndex & 1,
                (octantIndex >> 1) & 1,
                (octantIndex >> 2) & 1);
        }
        
        public static int CellIndex(int3 crds, int3 counts)
        {
            return crds.x + crds.y * counts.x + crds.z * counts.x * counts.y;
        }

        public static int3 CellCrds(int index, int3 counts)
        {
            return new int3(
                index % counts.x,
                index % (counts.x * counts.y) / counts.x,
                index / (counts.x * counts.y));
        }

        public static int3 CellCrds(float3 point, float3 min, float3 cellSize)
        {
            float3 localPoint = point - min;
            float3 crds = localPoint / cellSize;
            return (int3)math.floor(crds);
        }

        public static float3 CellCenter(int3 crds, float3 min, float3 cellSize)
        {
            return min + ((float3)crds + .5f) * cellSize;
        }

        public static float3 CellMin(int3 crds, float3 min, float3 cellSize)
        {
            return min + crds * cellSize;
        }
    }
}