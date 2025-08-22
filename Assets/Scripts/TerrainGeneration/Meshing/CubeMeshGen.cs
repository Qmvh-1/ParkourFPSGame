//using System.Collections.Generic;
//using Stubblefield.Shapes;
//using Unity.Collections;
//using Unity.Mathematics;
//using UnityEngine;
//using UnityEngine.Rendering;
//using Random = Unity.Mathematics.Random;

//namespace Stubblefield.TerrainGen
//{
//    [RequireComponent(typeof(MeshFilter))]
//    [RequireComponent(typeof(MeshRenderer))]
//    public class CubeMeshGen : MonoBehaviour
//    {
//        public float3 cellSize = new(1);
//        public int3 counts = new(25);
//        public float3 gridMin;
//        public NoiseStack noiseStack = new();
//        MeshBuilder meshBuilder;

//        MeshFilter MeshFilter => GetComponent<MeshFilter>();

//        [ContextMenu(nameof(Test))]
//        void Test()
//        {
//            BuildMesh();
//        }

//        [ContextMenu(nameof(CreateBox))]
//        void CreateBox()
//        {
//            meshBuilder ??= new MeshBuilder();
//            MeshFilter.sharedMesh = meshBuilder.mesh;
//            meshBuilder.Clear();
//            meshBuilder.AddBox(Box.MinSize(new float3(0, 0, 0), 1));
//            meshBuilder.AddBox(Box.MinSize(new float3(1, 0, 0), 1));
//            meshBuilder.AddBox(Box.MinSize(new float3(3, 0, 0), 1));
//            meshBuilder.BuildMesh();
//        }

//        void BuildMesh()
//        {
//            meshBuilder ??= new MeshBuilder();
//            MeshFilter.sharedMesh = meshBuilder.mesh;
//            meshBuilder.Clear();
//            for (int z = 0; z < counts.z; z++)
//            {
//                for (int y = 0; y < counts.y; y++)
//                {
//                    for (int x = 0; x < counts.x; x++)
//                    {
//                        float3 position = GetPosition(new int3(x, y, z));
//                        float value = noiseStack.IsosurfaceValue(position);
//                        if (value > 0)
//                        {
//                            Box box = Box.GridCell(new int3(x, y, z), default, cellSize);
//                            meshBuilder.AddBox(box);
//                        }
//                    }
//                }
//            }
//            meshBuilder.BuildMesh();
//        }

//        float3 GetPosition(int3 crds)
//        {
//            return ((float3)crds + .5f) * cellSize - gridMin;
//        }
//    }
//}