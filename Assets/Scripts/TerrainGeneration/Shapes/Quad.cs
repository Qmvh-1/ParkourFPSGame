using System;
using Unity.Mathematics;

namespace Stubblefield.Shapes
{
    public struct Quad
    {
        public float3x4 vertices;

        public const int i0 = 0;
        public const int i1 = 2;
        public const int i2 = 3;
        public const int i3 = 0;
        public const int i4 = 3;
        public const int i5 = 1;

        public float3 V0
        {
            get => vertices.c0;
            set => vertices.c0 = value;
        }

        public float3 V1
        {
            get => vertices.c1;
            set => vertices.c1 = value;
        }

        public float3 V2
        {
            get => vertices.c2;
            set => vertices.c2 = value;
        }

        public float3 V3
        {
            get => vertices.c3;
            set => vertices.c3 = value;
        }

        public Tri Tri0 => new(vertices[i0], vertices[i1], vertices[i2]);
        
        public Tri Tri1 => new(vertices[i3], vertices[i4], vertices[i5]);
        
        public Quad(float3 v0, float3 v1, float3 v2, float3 v3)
        {
            vertices = new float3x4(v0, v1, v2, v3);
        }
    }
}