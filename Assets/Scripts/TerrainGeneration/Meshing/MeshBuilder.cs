using System.Collections.Generic;
using Stubblefield.Shapes;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Stubblefield.TerrainGen
{
    public class MeshBuilder
    {
        public readonly Mesh mesh = new();
        public readonly NativeList<float3> positions = new(Allocator.TempJob);
        public readonly NativeList<float3> normals = new(Allocator.TempJob);
        public readonly NativeList<int> indices = new(Allocator.TempJob);
        public readonly NativeList<Color32> colors = new(Allocator.TempJob);

        public void Dispose()
        {
            positions.Dispose();
            normals.Dispose();
            indices.Dispose();
            colors.Dispose();
        }

        public void Clear()
        {
            mesh.Clear();
            positions.Clear();
            normals.Clear();
            indices.Clear();
            colors.Clear();
        }

        public void BuildMesh()
        {
            mesh.indexFormat = IndexFormat.UInt32;
            mesh.SetVertices(new List<Vector3>(positions.AsArray().Reinterpret<Vector3>()));
            mesh.SetNormals(normals.AsArray());
            mesh.SetIndices(indices.AsArray(), MeshTopology.Triangles, 0);
            mesh.SetColors(colors.AsArray());
        }
        
        public void AddBox(in Box box)
        {
            for (int i = 0; i < 6; i++)
            {
                int start = positions.Length;
                Quad quad = box.GetQuad(i);
                positions.Add(quad.V0);
                positions.Add(quad.V1);
                positions.Add(quad.V2);
                positions.Add(quad.V3);
                float3 normal = math.normalize(quad.Tri0.Normal());
                normals.Add(normal);
                normals.Add(normal);
                normals.Add(normal);
                normals.Add(normal);
                indices.Add(start + Quad.i0);
                indices.Add(start + Quad.i1);
                indices.Add(start + Quad.i2);
                indices.Add(start + Quad.i3);
                indices.Add(start + Quad.i4);
                indices.Add(start + Quad.i5);
            }
        }
    }    
}