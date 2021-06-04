// Pcx - Point cloud importer & renderer for Unity
// https://github.com/keijiro/Pcx

// This is a derivative work of the original that was authored by Keijiro and published UnLicensed.

using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using g3;
namespace Pdal
{
    
    public struct BpcData
    {
        public IEnumerable<Vector3d> positions;
        public IEnumerable<Vector3f> colors;
        public ulong size;
    }
    
    /// A container class for texture-baked point clouds.
    public sealed class BakedPointCloud 
    {

        /// Number of points
        public int pointCount { get { return (int)_pointCount; } }

        /// Position map texture
        public Texture2D positionMap { get { return _positionMap; } }

        /// Color map texture
        public Texture2D colorMap { get { return _colorMap; } }


        [SerializeField] ulong _pointCount;
        [SerializeField] Texture2D _positionMap;
        [SerializeField] Texture2D _colorMap;



        public static BakedPointCloud Initialize(BpcData data)
        {
            BakedPointCloud bpc = new BakedPointCloud();
            
            bpc._pointCount = data.size;

            int width = Mathf.CeilToInt(Mathf.Sqrt(bpc._pointCount));

            bpc._positionMap = new Texture2D(width, width, TextureFormat.RGBAHalf, false);
            bpc._positionMap.name = "Position Map";
            bpc._positionMap.filterMode = FilterMode.Point;

            bpc._colorMap = new Texture2D(width, width, TextureFormat.RGBA32, false);
            bpc._colorMap.name = "Color Map";
            bpc._colorMap.filterMode = FilterMode.Point;

            int i1 = 0;
            uint i2 = 0U;

            IEnumerator<Vector3d> position = data.positions.GetEnumerator();
            IEnumerator<Vector3f> color = data.colors.GetEnumerator();

            position.MoveNext();
            color.MoveNext();


            for (int y = 0; y < width; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int i = i1 < bpc.pointCount ? i1 : (int)(i2 % bpc._pointCount);

                    Vector3d p = position.Current;
                    Vector3f c = color.Current;

                    bpc._positionMap.SetPixel(x, y, new Color((float)p.x, (float)p.y, (float)p.z));
                    bpc._colorMap.SetPixel(x, y, new Color(c.x, c.y, c.z));

                    i1 ++;
                    i2 += 132049U; // prime

                    position.MoveNext();
                    color.MoveNext();
                }
            }

            bpc._positionMap.Apply(false, true);
            bpc._colorMap.Apply(false, true);
            return bpc;
        }

        public static Task<BakedPointCloud> InitializeAsync(BpcData data) {
            Task<BakedPointCloud> t1 = new Task<BakedPointCloud>(() =>
            {
                return Initialize(data);
            });
            t1.Start(TaskScheduler.FromCurrentSynchronizationContext());
            return t1;
        }
    }
}
