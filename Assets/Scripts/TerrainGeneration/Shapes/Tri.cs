using Unity.Mathematics;

namespace Stubblefield.Shapes
{
    public struct Tri
    {
        public float3x3 vertices;

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

        public Tri(float3 v0, float3 v1, float3 v2)
        {
            vertices = new(v0, v1, v2);
        }

        /// <returns>The triangle's normal not normalized.</returns>
        public float3 Normal()
        {
            return math.cross(V1 - V0, V2 - V0);
        }

        public bool IsDegenerate()
        {
            return math.all(Normal() == 0);
        }
    }
}