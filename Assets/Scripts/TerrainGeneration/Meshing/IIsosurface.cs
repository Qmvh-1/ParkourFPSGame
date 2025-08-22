using Unity.Mathematics;

namespace Stubblefield.TerrainGen
{
    public interface IIsosurface
    {
        public float IsosurfaceValue(float3 position);
    }
}