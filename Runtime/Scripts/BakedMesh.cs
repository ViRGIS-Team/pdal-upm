using g3;
using System;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Unity.Burst;


namespace Pdal
{
    /// <summary>
    /// Class for Holding and Manipulating Meshes Loaded Using PDAL
    /// </summary>
    public class BakedMesh
    {

        public DMesh3 Dmesh;
        public BakedMesh() { }

        /// <summary>
        /// Load a PointView as a Mesh
        /// </summary>
        /// <param name="view">Pointview</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async static Task<BakedMesh> Initialize(PointView view)
        {
            BakedPointCloud bpc = await BakedPointCloud.Initialize(view);
            NativeArray<byte> Data = new(view.GetPackedMesh(out ulong dataSize), Allocator.Persistent);
            if (dataSize == 0) throw new Exception("BakedMesh : No Mesh Found");

            ulong sizeTri = dataSize / 12;

            NativeArray<Int32> Tris = new(new Int32[sizeTri * 3], Allocator.Persistent);

            DecodeMesh dm = new() {
                Data = Data,
                Tris = Tris,
            };

            JobHandle jh = dm.Schedule((int)sizeTri, 100);
            jh.Complete();

            NativeArray<Color> positions = bpc.PositionMap.GetRawTextureData<Color>();
            NativeArray<Color32> colors = bpc.ColorMap.GetRawTextureData<Color32>();

            double[] vertices= new double[bpc.PointCount * 3 ];


            for (int i = 0; i < bpc.PointCount; i++)
            {
                Color position = positions[i];
                vertices[3 * i] = position.r;
                vertices[3 * i + 1] = position.g;
                vertices[3 * i + 2] =   position.b;
            }

            // Now we can make the Mesh
            BakedMesh bm = new()
            {
                Dmesh = DMesh3Builder.Build<double, int, int>(vertices, Tris)
            };
            if (colors.Length > 0)
            {
                bm.Dmesh.EnableVertexColors(new Vector3f());
                foreach (int idx in bm.Dmesh.VertexIndices())
                {
                    bm.Dmesh.SetVertexColor(idx, new Vector3f((float)colors[idx].r / 256, (float)colors[idx].g / 256, (float)colors[idx].b / 256));
                }
            }

            positions.Dispose();
            colors.Dispose();
            Data.Dispose();
            Tris.Dispose();
            return bm;
        }

        /// <summary>
        /// Job System Job that is used to iterate the Vertices
        /// </summary>
        [BurstCompile]
        public struct DecodeMesh : IJobParallelFor
        {
            [ReadOnly]
            public NativeArray<byte> Data;

            [NativeDisableParallelForRestriction]
            public NativeArray<Int32> Tris;

            public void Execute(int job)
            {
                int pointer = job * 3;
                int index = pointer * 4;

                if (index + 12 > Data.Length) 
                    return;
                Tris[pointer] = (int)Data.ReinterpretLoad<UInt32>(index);
                Tris[pointer + 1] = (int)Data.ReinterpretLoad<UInt32>(index + 4);
                Tris[pointer + 2] = (int)Data.ReinterpretLoad<UInt32>(index + 8);
            }
        }

        /// <summary>
        /// Extracts a UnityEngine.Mesh from the BakedMesh
        /// The DMesh3 must be compact. If neccesary - run Compactify first.
        /// </summary>
        /// <returns>UnityEngine.Mesh</returns>
        public Mesh ToMesh()
        {
            Mesh unityMesh = new Mesh();
            unityMesh.MarkDynamic();
            unityMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            Vector3[] vertices = new Vector3[Dmesh.VertexCount];
            Color[] colors = new Color[Dmesh.VertexCount];
            Vector2[] uvs = new Vector2[Dmesh.VertexCount];
            Vector3[] normals = new Vector3[Dmesh.VertexCount];
            NewVertexInfo data;
            for (int i = 0; i < Dmesh.VertexCount; i++)
            {
                if (Dmesh.IsVertex(i))
                {
                    data = Dmesh.GetVertexAll(i);
                    vertices[i] = (Vector3)data.v;
                    if (data.bHaveC)
                        colors[i] = (Color)data.c;
                    if (data.bHaveUV)
                        uvs[i] = (Vector2)data.uv;
                    if (data.bHaveN)
                        normals[i] = (Vector3)data.n;
                }
            }
            unityMesh.vertices = vertices;
            if (Dmesh.HasVertexColors) unityMesh.SetColors(colors);
            if (Dmesh.HasVertexUVs) unityMesh.SetUVs(0, uvs);
            if (Dmesh.HasVertexNormals) unityMesh.SetNormals(normals);
            int[] triangles = new int[Dmesh.TriangleCount * 3];
            int j = 0;
            foreach (Index3i tri in Dmesh.Triangles())
            {
                triangles[j * 3] = tri.a;
                triangles[j * 3 + 1] = tri.b;
                triangles[j * 3 + 2] = tri.c;
                j++;
            }
            unityMesh.triangles = triangles;
            return unityMesh;
        }
    }
}


