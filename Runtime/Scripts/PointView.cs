/******************************************************************************
 * Copyright (c) 2019, Simverge Software LLC. All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following
 * conditions are met:
 *
 * 1. Redistributions of source code must retain the above copyright notice,
 *    this list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright notice,
      this list of conditions and the following disclaimer in the documentation
 *    and/or other materials provided with the distribution.
 * 3. Neither the name of Simverge Software LLC nor the names of its
 *    contributors may be used to endorse or promote products derived from this
 *    software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
 * LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

using g3;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Unity.Burst;

namespace Pdal
 {
    public class PointView : IDisposable
    {
        private const string PDALC_LIBRARY = "pdalc";
        private const int BUFFER_SIZE = 1024;
        private IntPtr mNative = IntPtr.Zero;

        [DllImport(PDALC_LIBRARY, EntryPoint = "PDALDisposePointView")]
        private static extern void dispose(IntPtr view);

        [DllImport(PDALC_LIBRARY, EntryPoint = "PDALGetPointViewId")]
        private static extern int id(IntPtr view);

        [DllImport(PDALC_LIBRARY, EntryPoint = "PDALGetPointViewSize")]
        private static extern ulong size(IntPtr view);

        [DllImport(PDALC_LIBRARY, EntryPoint = "PDALIsPointViewEmpty")]
        private static extern bool empty(IntPtr view);

        [DllImport(PDALC_LIBRARY, EntryPoint = "PDALClonePointView")]
        private static extern IntPtr clone(IntPtr view);

        [DllImport(PDALC_LIBRARY, EntryPoint = "PDALGetPointViewProj4")]
        private static extern uint getProj4(IntPtr view, StringBuilder buffer, uint size);

        [DllImport(PDALC_LIBRARY, EntryPoint = "PDALGetPointViewWkt")]
        private static extern uint getWkt(IntPtr view, StringBuilder buffer, uint size, bool pretty);

        [DllImport(PDALC_LIBRARY, EntryPoint = "PDALGetPointViewLayout")]
        private static extern IntPtr getLayout(IntPtr view);

        [DllImport(PDALC_LIBRARY, EntryPoint = "PDALGetPackedPoint")]
        private static extern uint getPackedPoint(IntPtr view, IntPtr types, ulong idx, [MarshalAs(UnmanagedType.LPArray)] byte[] buf);


        [DllImport(PDALC_LIBRARY, EntryPoint = "PDALGetAllPackedPoints")]
        private static extern ulong getAllPackedPoints(IntPtr view, IntPtr types, [MarshalAs(UnmanagedType.LPArray)] byte[] buf);


        [DllImport(PDALC_LIBRARY, EntryPoint = "PDALGetMeshSize")]
        private static extern ulong meshSize(IntPtr view);

        [DllImport(PDALC_LIBRARY, EntryPoint = "PDALGetAllTriangles")]
        private static extern ulong getAllTriangles(IntPtr view, [MarshalAs(UnmanagedType.LPArray)] byte[] buf);

        public PointView(IntPtr nativeView)
        {
            mNative = nativeView;
        }

        public void Dispose()
        {
            dispose(mNative);
            mNative = IntPtr.Zero;
        }

        public int Id
        {
            get { return id(mNative); }
        }

        public ulong Size
        {
            get { return size(mNative); }
        }

        public ulong MeshSize
        {
            get { return meshSize(mNative); }
        }

        public bool Empty
        {
            get { return empty(mNative); }
        }

        public string Proj4
        {
            get
            {
                StringBuilder buffer = new(BUFFER_SIZE);
                getProj4(mNative, buffer, (uint)buffer.Capacity);
                return buffer.ToString();
            }
        }

        public string Wkt
        {
            get
            {
                StringBuilder buffer = new(BUFFER_SIZE);
                getWkt(mNative, buffer, (uint)buffer.Capacity, false);
                return buffer.ToString();
            }
        }

        public string PrettyWkt
        {
            get
            {
                StringBuilder buffer = new(BUFFER_SIZE);
                getWkt(mNative, buffer, (uint)buffer.Capacity, true);
                return buffer.ToString();
            }
        }

        public PointLayout Layout
        {
            get
            {
                PointLayout layout = null;
                IntPtr nativeLayout = getLayout(mNative);

                if (nativeLayout != IntPtr.Zero)
                {
                    layout = new PointLayout(nativeLayout);
                }
                return layout;
            }
        }

        public byte[] GetAllPackedPoints(DimTypeList dims, out ulong size)
        {
            byte[] data = null;
            size = 0;

            if (this.Size > 0 && dims != null && dims.Size > 0)
            {
                ulong byteCount = this.Size * dims.ByteCount;
                data = new byte[byteCount];
                size = getAllPackedPoints(mNative, dims.Native, data);
            }
            return data;
        }

        public byte[] GetPackedPoint(DimTypeList dims, ulong idx)
        {
            byte[] data = null;

            if (this.Size > idx && dims != null && dims.Size > 0)
            {
                data = new byte[dims.ByteCount];
                getPackedPoint(mNative, dims.Native, idx, data);
            }

            return data;
        }

        public byte[] GetPackedMesh(out ulong size)
        {
            byte[] data = null;
            size = 0;

            if (meshSize(mNative) > 0)
            {
                ulong byteCount = meshSize(mNative) * 12;
                data = new byte[byteCount];
                size = getAllTriangles(mNative, data);
            }

            return data;
        }

        public PointView Clone()
        {
            PointView clonedView = null;
            IntPtr nativeClone = clone(mNative);

            if (nativeClone != IntPtr.Zero)
            {
                clonedView = new PointView(nativeClone);
            }

            return clonedView;
        }

        /// <summary>
        /// Job System Job to Decode the Point CLoud Vertices
        /// </summary>
        //[BurstCompile]
        public struct DecodeView : IJobParallelFor
        {
            [ReadOnly]
            public NativeArray<UInt32> Types;

            [ReadOnly]
            public NativeArray<UInt32> Indexes;

            [ReadOnly]
            public NativeArray<byte> Data;

            [ReadOnly]
            public uint PointSize;

            [ReadOnly]
            public int Width;

            [NativeDisableParallelForRestriction]
            public NativeArray<Color32> Colors;

            [NativeDisableParallelForRestriction]
            public NativeArray<Color> Positions;

            public void Execute(int job)
            {
                int pointer = job * Width;
                long index = pointer * PointSize;

                //iterate through a row
                for (int i = 0; i < Width; i++)
                {
                    if (index + PointSize > Data.Length) break;
                    /// Get the raw data for this point
                    double X = ParseDouble(Data, Types[0], (int)(index + Indexes[0]));
                    double Y = ParseDouble(Data, Types[1], (int)(index + Indexes[1]));
                    double Z = ParseDouble(Data, Types[2], (int)(index + Indexes[2]));
                    Positions[pointer] = new Color((float)X, (float)Y, (float)Z);

                    if (Types[3] != 0)
                    {
                        byte Red = ParseColor(Data, Types[3], (int)(index + Indexes[3]));
                        byte Green = ParseColor(Data, Types[4], (int)(index + Indexes[4]));
                        byte Blue = ParseColor(Data, Types[5], (int)(index + Indexes[5]));
                        Colors[pointer] = new Color32(Red, Green, Blue, 0xFF);
                    }
                    index += PointSize;
                    pointer += 1;
                }
            }

            public DecodeAwaiter GetAwaiter()
            {
                return new DecodeAwaiter(this);
            }

        }

        /// <summary>
        /// Used to Get Double Values from the Pointview as Byte Stream
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="type"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private static double ParseDouble(NativeArray<Byte>  buffer, UInt32 type, int position)
        {
            if (type == 10)
            {
                return buffer.ReinterpretLoad<double>(position);
            }
            else if (type == 9)
            {
                return buffer.ReinterpretLoad<float>(position);
            }
            else if (type == 7)
            {
                return buffer.ReinterpretLoad<UInt64>(position);
            }
            else if (type == 5)
            {
                return buffer.ReinterpretLoad<UInt32>(position);
            }
            else if (type == 3)
            {
                return buffer.ReinterpretLoad<UInt16>(position);
            }
            else if (type == 1)
            {
                return buffer[position];
            }
            else if (type == 8)
            {
                return buffer.ReinterpretLoad<Int64>(position);
            }
            else if (type == 6)
            {
                return buffer.ReinterpretLoad<Int32>(position);
            }
            else if (type == 4)
            {
                return buffer.ReinterpretLoad<Int16>(position);
            }
            else if (type == 2)
            {
                return ((sbyte)buffer[position]);
            }
            else
            {
                throw new Exception("Invalid PDAL type id");
            }
        }

        /// <summary>
        /// Used to get Byte Values from the PointView as Byte Stream
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="type"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private static byte ParseColor(NativeArray<Byte> buffer, UInt32 type, int position)
        {
            if (type == 1 || type == 2) return (byte)ParseDouble(buffer, type, position);
            else if (type == 3 | type == 4) return (byte)(ParseDouble(buffer, type, position));
            else if (type == 9 || type == 10)
            {
                double value = ParseDouble(buffer, type, position);
                if (value < 1) value *= 256;
                return (byte)value;
            }
            throw new Exception("PDAL - Unsupported Color type");
        }

        /// <summary>
        /// Used to Get PDAL type as a number
        /// </summary>
        /// <param name="InterpretationName"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static UInt32 TypeValue(string InterpretationName)
        {
            if (InterpretationName == "double")
            {
                return 10;
            }
            else if (InterpretationName == "float")
            {
                return 9;
            }
            else if (InterpretationName.StartsWith("uint64"))
            {
                return 7;
            }
            else if (InterpretationName.StartsWith("uint32"))
            {
                return 5;
            }
            else if (InterpretationName.StartsWith("uint16"))
            {
                return 3;
            }
            else if (InterpretationName.StartsWith("uint8"))
            {
                return 1;
            }
            else if (InterpretationName.StartsWith("int64"))
            {
                return 8;
            }
            else if (InterpretationName.StartsWith("int32"))
            {
                return 6;
            }
            else if (InterpretationName.StartsWith("int16"))
            {
                return 4;
            }
            else if (InterpretationName.StartsWith("int8"))
            {
                return 2;
            }
            throw new Exception("invalid PDAL type name");
        }

        /// <summary>
        /// Custom Awaiter for the Decode View
        /// </summary>
        public struct DecodeAwaiter : INotifyCompletion
        {
            JobHandle jh;
            private Action continuation;
        
            public DecodeAwaiter(DecodeView dv) {
                jh = dv.Schedule(dv.Width, 1);
                continuation = null;
            }

            public bool IsCompleted
            {
                get {
                    return jh.IsCompleted;
                }
            }

            public void OnCompleted(Action continuation)
            {
                {
                    ScheduleContinuation(continuation);
                    RunToCompletion();
                }
            }

            public void GetResult() { }

            internal void ScheduleContinuation(Action action)
            {
                continuation = action;
            }

            internal void RunToCompletion()
            {
                var wait = new SpinWait();
                while (! IsCompleted)
                    wait.SpinOnce();
                continuation();
            }
        }
    }
 }