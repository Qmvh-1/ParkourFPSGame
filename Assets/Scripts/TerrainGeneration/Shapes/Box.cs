using System;
using Unity.Mathematics;

namespace Stubblefield.Shapes
{
    public struct Box
    {
        public float3 center;
        public float3 extents;
        public readonly float3 Size => extents * 2;
        public readonly float3 Min => center - extents;
        public readonly float3 Max => center + extents;

        public static Box MinSize(float3 min, float3 size)
        {
            return new Box(min + size * .5f, size * .5f);
        }

        public static Box CenterExtents(float3 center, float3 extents)
        {
            return new Box(center, extents);
        }
        
        public static Box GridCell(int3 crds, float3 gridOrigin, float3 cellSize)
        {
            return MinSize(gridOrigin + crds * cellSize, cellSize);
        }

        Box(float3 center, float3 extents)
        {
            this.center = center;
            this.extents = extents;
        }

        public readonly float3 GetCorner(int index)
        {
            int3 crds = CornerCrds(index);
            return Min + Size * crds;
        }

        /// <summary>
        /// Returns the quad that corresponds with index. Index should be in range [0, 5].
        /// 0 = left, 1 = right, 2 = down, 3 = up, 4 = back, 5 = forward
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public readonly Quad GetQuad(int index)
        {
            return CreateQuad(QuadCorners(index));
        }

        static int4 QuadCorners(int index)
        {
            return (index) switch
            {
                0 => new int4(4, 0, 6, 2),
                1 => new int4(1, 5, 3, 7),
                2 => new int4(4, 5, 0, 1),
                3 => new int4(2, 3, 6, 7),
                4 => new int4(0, 1, 2, 3),
                5 => new int4(5, 4, 7, 6),
                _ => throw new ArgumentOutOfRangeException(nameof(index), index, null)
            };
        }

        readonly Quad CreateQuad(int4 cornerIndices)
        {
            return new Quad(
                GetCorner(cornerIndices[0]),
                GetCorner(cornerIndices[1]),
                GetCorner(cornerIndices[2]),
                GetCorner(cornerIndices[3]));
        }
        
        static int3 CornerCrds(int index)
        {
            // return new int3(
            //     index % 2,
            //     index % 4 / 2,
            //     index % 4);
            return new int3(
                index & 0b_001,
                (index & 0b_010) >> 1,
                (index & 0b_100) >> 2);
        }
    }
}