// Pcx - Point cloud importer & renderer for Unity
// https://github.com/keijiro/Pcx

// This is a derivative work of the original that was authored by Keijiro and published UnLicensed.

using UnityEngine;
using System.Diagnostics;
using System;
using Unity.Collections;
using Unity.Jobs;
using System.Threading.Tasks;


namespace Pdal
{
    
    /// A container class for texture-baked point clouds.
    public class BakedPointCloud 
    {

        /// Number of points
        public int PointCount { get { return (int)m_PointCount; } }

        /// Width
        public int Width { get { return (int)m_Width; } }
        public int Height { get { return (int)m_Width; } } // yeah - it is a square so height and width are the same

        /// Position map texture
        public Texture2D PositionMap { get { return m_PositionMap; } }

        /// Color map texture
        public Texture2D ColorMap { get { return m_ColorMap; } }

        [SerializeField] ulong m_PointCount;
        [SerializeField] int m_Width;
        [SerializeField] Texture2D m_PositionMap;
        [SerializeField] Texture2D m_ColorMap;


        public BakedPointCloud(ulong size ) {
            m_PointCount = size;

            m_Width = Mathf.CeilToInt(Mathf.Sqrt(m_PointCount));

            m_PositionMap = new Texture2D(Width, Height, TextureFormat.RGBAFloat, false)
            {
                name = "Position Map",
                filterMode = FilterMode.Point
            };

            m_ColorMap = new Texture2D(Width, Height, TextureFormat.RGBA32, false)
            {
                name = "Color Map",
                filterMode = FilterMode.Point
            };
        }

        /// <summary>
        /// Load and initialize a Pouintview as a Pointcloud
        /// </summary>
        /// <param name="view">Pointview</param>
        /// <returns></returns>
        public async static Task<BakedPointCloud> Initialize(PointView view)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            //Get the PDAL data
            PointLayout layout = view.Layout;
            ulong size = view.Size;
            DimTypeList typelist = layout.Types;
            byte[] _data = view.GetAllPackedPoints(typelist, out ulong dataSize);



            // Createb the BPC
            BakedPointCloud bpc = new(size);

            // These are the native memory arrays for the texture components
            NativeArray<Color> positions = bpc.m_PositionMap.GetRawTextureData<Color>();
            NativeArray<Color32> colors = bpc.m_ColorMap.GetRawTextureData<Color32>();

            // Get the PDAL Metadata

            UInt32[] _indexs = new UInt32[6];
            UInt32[] _types = new UInt32[6];

            UInt32 count = 0;

            for (uint j = 0; j < typelist.Size; j++)
            {
                DimType type = typelist.at(j);
                UInt32 typeId = PointView.TypeValue(type.InterpretationName);
                int interpretationByteCount = type.InterpretationByteCount;
                string name = type.IdName;

                switch (name)
                {
                    case "X":
                        _types[0] = typeId;
                        _indexs[0] = count;
                        break;
                    case "Y":
                        _types[1] = typeId;
                        _indexs[1] = count;
                        break;
                    case "Z":
                        _types[2] = typeId;
                        _indexs[2] = count;
                        break;
                    case "Red":
                        _types[3] = typeId;
                        _indexs[3] = count;
                        break;
                    case "Green":
                        _types[4] = typeId;
                        _indexs[4] = count;
                        break;
                    case "Blue":
                        _types[5] = typeId;
                        _indexs[5] = count;
                        break;
                }

                count += (UInt32)interpretationByteCount;
            }

            NativeArray<UInt32> types = new(_types, Allocator.Persistent);
            NativeArray<UInt32> indexes = new(_indexs, Allocator.Persistent);
            NativeArray<byte> data = new(_data, Allocator.Persistent);
            uint pointSize = layout.PointSize;

            // Run the job

            PointView.DecodeView dv = new()
            {
                Types = types,
                Indexes = indexes,
                Data = data,
                PointSize = pointSize,
                Positions = positions,
                Colors = colors
            };

            JobHandle jh = dv.Schedule(bpc.PointCount, 100);
            jh.Complete();
            //await PointView.RunDVAsync(dv);

            // Cleanup

            bpc.m_PositionMap.Apply(false, false);
            bpc.m_ColorMap.Apply(false, false);

            types.Dispose();
            indexes.Dispose();
            data.Dispose();


            UnityEngine.Debug.Log($"BakedPointCloud took {stopwatch.Elapsed.TotalSeconds}");
            return bpc;
        }
    }
}
