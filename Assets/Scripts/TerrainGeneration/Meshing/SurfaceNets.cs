using System.Diagnostics;
using System.Text;
using Stubblefield.Shapes;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using static Utility;

namespace Stubblefield.TerrainGen
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class SurfaceNets : MonoBehaviour
    {
        [SerializeField] int vertexCapacity = 1000000;
        [SerializeField] bool multithread = true;
        public bool refreshOnValidate;
        public float3 gridSize = 25;
        [Min(.1f)] public float cellSize = 1;
        public Noise[] noiseLayers;
        MeshBuilder meshBuilder;
        public TerrainTexturingRules texturingRules;

        MeshFilter MeshFilter => GetComponent<MeshFilter>();
        float3 gridMin => transform.position;
        int3 cellCounts => (int3)math.round(gridSize / cellSize);

        [ContextMenu(nameof(Build))]
        public void Build()
        {
            Stopwatch stopwatch = new();
            stopwatch.Start();
            StringBuilder sb = new($"Generating terrain... \ngridSize : {gridSize} | cellSize : {cellSize} | cellCounts : {cellCounts} | multithread : {multithread} | useBurst : {BurstCompiler.IsEnabled} \n");
            meshBuilder = new MeshBuilder();
            meshBuilder.mesh.name = "Generated Terrain";
            meshBuilder.Clear();
            NativeParallelHashMap<int3, Vertex> vertices = new(vertexCapacity, Allocator.TempJob);
            AddVerticesJob verticesJob = new()
            {
                cellSize = cellSize,
                gridMin = gridMin,
                gridSize = gridSize,
                layers = new NativeArray<Noise>(noiseLayers, Allocator.TempJob),
                vertices = vertices.AsParallelWriter(),
            };

            if (multithread)
            {
                verticesJob.Schedule(cellCounts.x * cellCounts.y * cellCounts.z, 64).Complete();
            }
            else
            {
                verticesJob.Run(cellCounts.x * cellCounts.y * cellCounts.z);
            }
            sb.Append($"vertices.Count {vertices.Count()} \n");
            sb.Append($"Adding vertices took {stopwatch.Elapsed.TotalSeconds} seconds \n");
            stopwatch.Restart();

            NativeArray<int3> keys = vertices.GetKeyArray(Allocator.Temp);
            for (int i = 0; i < keys.Length; i++)
            {
                Vertex vertex = vertices[keys[i]];
                vertex.index = i;
                vertices[keys[i]] = vertex;
                meshBuilder.positions.Add(vertex.position);
                meshBuilder.normals.Add(vertex.normal);
            }

            AddTriangles(vertices, meshBuilder.indices);
            sb.Append($"Adding triangles took {stopwatch.Elapsed.TotalSeconds} seconds \n");
            stopwatch.Restart();
            AddColors(meshBuilder.normals, meshBuilder.colors);
            sb.Append($"Adding colors took {stopwatch.Elapsed.TotalSeconds} seconds \n");
            stopwatch.Restart();
            meshBuilder.BuildMesh();
            sb.Append($"Building mesh took {stopwatch.Elapsed.TotalSeconds} seconds \n");
            stopwatch.Restart();

            if (gameObject.TryGetComponent(out MeshCollider meshCollider))
            {
                meshCollider.sharedMesh = meshBuilder.mesh;
                sb.Append($"Setting up mesh collider took {stopwatch.Elapsed.TotalSeconds} seconds \n");
                stopwatch.Restart();
            }
#if UNITY_EDITOR
            if (Application.isPlaying)
            {
                MeshFilter.sharedMesh = meshBuilder.mesh;
            }
            else
            {
                try
                {
                    UnityEditor.EditorApplication.delayCall += () => MeshFilter.sharedMesh = meshBuilder.mesh;
                }
                catch { }
            }
#else
            MeshFilter.sharedMesh = meshBuilder.mesh;
#endif
            sb.Append($"Setting up mesh filter took {stopwatch.Elapsed.TotalSeconds} seconds \n");
            print(sb.ToString());
            stopwatch.Stop();
            meshBuilder.Dispose();
            verticesJob.Dispose();
            vertices.Dispose();
        }

        void AddColors(NativeList<float3> inNormals, NativeList<Color32> outColors)
        {
            for (int i = 0; i < inNormals.Length; i++)
            {
                Color32 color = default;
                float slopeDegrees = Vector3.Angle(Vector3.up, inNormals[i]);
                color.r = (byte)Mathf.Round(texturingRules.slope0.Evaluate(slopeDegrees) * 255);
                color.g = (byte)Mathf.Round(texturingRules.slope1.Evaluate(slopeDegrees) * 255);
                outColors.Add(color);
            }
        }

        void AddTriangles(NativeParallelHashMap<int3, Vertex> vertices, NativeList<int> outIndices)
        {
            for (int z = 0; z < cellCounts.z; z++)
            {
                for (int y = 0; y < cellCounts.y; y++)
                {
                    for (int x = 0; x < cellCounts.x; x++)
                    {
                        int3 crds = new(x, y, z);
                        // right axis
                        TryAddQuad(vertices, outIndices, crds, crds + Forward(), crds + Up(), crds + Forward() + Up());
                        // up axis
                        TryAddQuad(vertices, outIndices, crds, crds + Right(), crds + Forward(), crds + Right() + Forward());
                        // forward axis
                        TryAddQuad(vertices, outIndices, crds, crds + Right(), crds + Up(), crds + Right() + Up());
                    }
                }
            }
        }

        static void TryAddQuad(NativeParallelHashMap<int3, Vertex> vertices, NativeList<int> outIndices, int3 crds0, int3 crds1, int3 crds2, int3 crds3)
        {
            if (vertices.TryGetValue(crds0, out Vertex v0)
                && vertices.TryGetValue(crds1, out Vertex v1)
                && vertices.TryGetValue(crds2, out Vertex v2)
                && vertices.TryGetValue(crds3, out Vertex v3))
            {
                AddTri(v0, v2, v3, outIndices);
                AddTri(v0, v3, v1, outIndices);
            }
        }

        static void AddTri(in Vertex a, in Vertex b, in Vertex c, NativeList<int> outIndices)
        {
            bool flip = !IsCorrectIndexOrder(a, b, c);
            outIndices.Add(a.index);
            outIndices.Add(flip ? c.index : b.index);
            outIndices.Add(flip ? b.index : c.index);
        }


        [BurstCompile]
        struct AddVerticesJob : IJobParallelFor
        {
            public float cellSize;
            public float3 gridMin;
            public float3 gridSize;
            [ReadOnly] public NativeArray<Noise> layers;
            public NativeParallelHashMap<int3, Vertex>.ParallelWriter vertices;
            int3 cellCounts => (int3)math.round(gridSize / cellSize);

            public void Dispose()
            {
                layers.Dispose();
            }

            public void Execute(int index)
            {
                // todo when grid scale y is half of grid scale x or z we see
                // that half of the cells on the y axis are missing, same with
                // oter ratios of x or z to y where y is smaler, may be due to
                // incorrect indecies, problem is likely accuring in the job and
                // not in the conversion process after!

                int3 crds = ToCrds(index, cellCounts);
                if (TryGetVertex(crds, out Vertex vertex))
                {
                    vertices.TryAdd(crds, vertex);
                }
            }

            bool TryGetVertex(int3 crds, out Vertex vertex)
            {
                float8 values = GetCornerValues(crds);
                if (AreAllSameSign(values))
                {
                    vertex = default;
                    return false;
                }
                else
                {
                    vertex = GenerateVertex(crds, values);
                    return true;
                }
            }

            float8 GetCornerValues(int3 crds)
            {
                float8 values = default;
                for (int i = 0; i < 8; i++)
                {
                    int3 octant = OctantCrds(i);
                    float3 position = GetLocalPosition(crds + octant);
                    for (int j = 0; j < layers.Length; j++)
                    {
                        values[i] += layers[j].IsosurfaceValue(position);
                    }
                }
                return values;
            }

            float3 GetLocalPosition(float3 crds) // tested
            {
                return crds * cellSize - gridMin;
            }

            Vertex GenerateVertex(int3 crds, in float8 cellCornerValues)
            {
                Vertex vertex = new();
                int surfaceEdgeCount = 0;
                for (int i = 0; i < EdgeCrds.Length / 2; i++)
                {
                    int3 octantCrdsA = EdgeCrds[i * 2];
                    int3 octantCrdsB = EdgeCrds[i * 2 + 1];
                    int octantIndexA = OctantIndex(octantCrdsA);
                    int octantIndexB = OctantIndex(octantCrdsB);
                    float valueA = cellCornerValues[octantIndexA];
                    float valueB = cellCornerValues[octantIndexB];
                    if ((valueA > 0 && valueB < 0) || (valueB > 0 && valueA < 0)) // different signs
                    {
                        surfaceEdgeCount++;
                        float3 pointA = (float3)octantCrdsA * cellSize;
                        float3 pointB = (float3)octantCrdsB * cellSize;
                        float t = math.abs(valueA) / (math.abs(valueA) + math.abs(valueB));
                        vertex.position += math.lerp(pointA, pointB, t);
                    }
                }
                vertex.position /= surfaceEdgeCount;
                vertex.position += GetLocalPosition(crds);
                vertex.normal = -math.normalize(new float3(
                    cellCornerValues[0] - cellCornerValues[1],
                    cellCornerValues[0] - cellCornerValues[2],
                    cellCornerValues[0] - cellCornerValues[4]));
                if (surfaceEdgeCount == 0)
                {
                    vertex.position = GetLocalPosition(crds + (float3).5f);
                }

                return vertex;
            }
        }


        static int OctantIndex(int3 crds)
        {
            return crds.z * 4 + crds.y * 2 + crds.x;
        }

        static int3 OctantCrds(int octantIndex)
        {
            return new int3(
                octantIndex & 1,
                (octantIndex >> 1) & 1,
                (octantIndex >> 2) & 1);
        }

        static bool IsCorrectIndexOrder(in Vertex a, in Vertex b, in Vertex c) // tested
        {
            float3 normalSum = a.normal + b.normal + c.normal;
            float3 triNormal = math.cross(b.position - a.position, c.position - a.position);
            float dot = math.dot(normalSum, triNormal);
            return dot > 0;
        }

        static bool AreAllSameSign(in float8 values) // tested
        {
            float sign = math.sign(values[0]);
            for (int i = 1; i < 8; i++)
            {
                if (math.sign(values[i]) != sign) return false;
            }
            return true;
        }

        static readonly int3[] EdgeCrds = {
            // along x axis
            new(0, 0, 0), new(1, 0, 0),
            new(0, 1, 0), new(1, 1, 0),
            new(0, 0, 1), new(1, 0, 1),
            new(0, 1, 1), new(1, 1, 1),
            // along y axis
            new(0, 0, 0), new(0, 1, 0),
            new(1, 0, 0), new(1, 1, 0),
            new(0, 0, 1), new(0, 1, 1),
            new(1, 0, 1), new(1, 1, 1),
            // along z axis
            new(0, 0, 0), new(0, 0, 1),
            new(1, 0, 0), new(1, 0, 1),
            new(0, 1, 0), new(0, 1, 1),
            new(1, 1, 0), new(1, 1, 1),
        };

        [System.Serializable]
        struct Vertex
        {
            public float3 position;
            public float3 normal;
            public int index;
        }

        static int3 Right() => new(1, 0, 0);
        static int3 Up() => new(0, 1, 0);
        static int3 Forward() => new(0, 0, 1);

        [System.Serializable]
        public class Sphere : IIsosurface
        {
            public float3 center;
            public float radius;

            public float IsosurfaceValue(float3 position)
            {
                return math.distance(center, position) - radius;
            }
        }

        void OnValidate()
        {
            if (refreshOnValidate) Build();
        }

        void OnDrawGizmosSelected()
        {
            float3 gridSize = cellCounts * (float3)cellSize;
            Gizmos.DrawWireCube(gridMin + gridSize * .5f, gridSize);
        }
    }
}