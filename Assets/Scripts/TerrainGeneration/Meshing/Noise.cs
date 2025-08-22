using Unity.Mathematics;
using Unity.Burst;
using UnityEngine;

namespace Stubblefield.TerrainGen
{
    [BurstCompile]
    [System.Serializable]
    public struct Noise : IIsosurface
    {
        public float3 offset;
        public float3 scale;
        public float2 range;
        public NoiseType noiseType;
        [Range(-1, 1)] public float weight;
        [SerializeField] byte _enabled;
        [SerializeField] byte _invert;

        public bool enabled
        {
            readonly get => _enabled != 0;
            set => _enabled = (byte)(value ? 1 : 0);
        }

        public bool invert
        {
            readonly get => _invert != 0;
            set => _invert = (byte)(value ? 1 : 0);
        }

        public static Noise Identity() => new()
        {
            scale = new float3(1),
            range = new float2(-1, 1),
        };

        public float IsosurfaceValue(float3 position) => ComputeBurst(position, this);

        readonly float Compute(float3 position)
        {
            if (!enabled) return 0;
            float3 pos = position;
            pos /= scale; // scale first because we want offsets to be the same between noise layers
            pos += offset;
            float value = noiseType switch
            {
                NoiseType.Simplex => noise.snoise(pos),
                NoiseType.Simplex2d => noise.snoise(pos.xz),
                NoiseType.Cellular => noise.cellular(pos).x,
                _ => range.x,
            };
            value = (value + 1) * .5f; // remap from [-1, 1] to [0, 1]
            value = range.x + value * (range.y - range.x);
            if (noiseType == NoiseType.Simplex2d) value = pos.y - value;
            return invert ? -value : value;
        }
        
        [BurstCompile] 
        static float ComputeBurst(in float3 position, in Noise n) => n.Compute(position);
        
        
        public enum NoiseType : byte
        {
            Simplex = 0,
            Simplex2d = 1,
            Cellular = 2,
        }
    }
}