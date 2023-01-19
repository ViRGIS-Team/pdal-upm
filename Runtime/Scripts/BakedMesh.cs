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

            NativeArray<Int32> Tris = new(new Int32[dataSize / 4 ], Allocator.Persistent);

            const int stride = 1000;

            DecodeMesh dm = new() {
                Data = Data,
                Width = stride,
                Tris = Tris,
            };

            JobHandle jh = dm.Schedule(Mathf.CeilToInt(dataSize /(12 * stride)) + 1, 1);
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
                    bm.Dmesh.SetVertexColor(idx, new Vector3f(colors[idx].r / 256, colors[idx].g / 256, colors[idx].b / 256));
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

            [ReadOnly]
            public int Width;

            [NativeDisableParallelForRestriction]
            public NativeArray<Int32> Tris;

            public void Execute(int job)
            {
                int pointer = job * Width * 3;
                int index = pointer * 4;

                //iterate through a row
                for (int i = 0; i < Width; i++)
                {
                    if (index + 12 > Data.Length) 
                        break;
                    Tris[pointer] = (int)Data.ReinterpretLoad<UInt32>(index);
                    Tris[pointer + 1] = (int)Data.ReinterpretLoad<UInt32>(index + 4);
                    Tris[pointer + 2] = (int)Data.ReinterpretLoad<UInt32>(index + 8);
                    index += 12;
                    pointer += 3;
                }
            }
        }
    }
}


