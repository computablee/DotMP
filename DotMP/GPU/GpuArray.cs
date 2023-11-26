/*
* DotMP - A collection of powerful abstractions for parallel programming in .NET with an OpenMP-like API. 
* Copyright (C) 2023 Phillip Allen Lane
*
* This library is free software; you can redistribute it and/or modify it under the terms of the GNU Lesser
* General Public License as published by the Free Software Foundation; either version 2.1 of the License, or
* (at your option) any later version.
*
* This library is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the
* implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public
* License for more details.
*
* You should have received a copy of the GNU Lesser General Public License along with this library; if not,
* write to the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.
*/

using ILGPU;
using ILGPU.IR.Values;
using ILGPU.Runtime;
using System;
using System.Diagnostics.CodeAnalysis;

namespace DotMP.GPU
{
    /// <summary>
    /// Wrapper object for representing arrays on the GPU.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [ExcludeFromCodeCoverage]
    public struct GPUArray<T>
        where T : unmanaged
    {
        /// <summary>
        /// The ILGPU view for 1D arrays.
        /// </summary>
        private ArrayView1D<T, Stride1D.Dense> view1d;

        /// <summary>
        /// The ILGPU view for 2D arrays.
        /// </summary>
        private ArrayView2D<T, Stride2D.DenseY> view2d;

        /// <summary>
        /// The ILGPU view for 3D arrays.
        /// </summary>
        private ArrayView3D<T, Stride3D.DenseZY> view3d;

        /// <summary>
        /// Number of dimensions.
        /// </summary>
        private int dims;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="buf">The Buffer to create an array from.</param>
        internal GPUArray(Buffer<T> buf)
        {
            switch (buf.Dimensions)
            {
                default:
                case 1:
                    view1d = buf.View1D;
                    // BAND-AID FIX: Cannot use empty ArrayViews on OpenCL devices.
                    view2d = new Buffer<T>(new T[1, 1], Buffer.Behavior.NoCopy).View2D;
                    // BAND-AID FIX: Cannot use empty ArrayViews on OpenCL devices.
                    view3d = new Buffer<T>(new T[1, 1, 1], Buffer.Behavior.NoCopy).View3D;
                    break;
                case 2:
                    // BAND-AID FIX: Cannot use empty ArrayViews on OpenCL devices.
                    view1d = new Buffer<T>(new T[1], Buffer.Behavior.NoCopy).View1D;
                    view2d = buf.View2D;
                    // BAND-AID FIX: Cannot use empty ArrayViews on OpenCL devices.
                    view3d = new Buffer<T>(new T[1, 1, 1], Buffer.Behavior.NoCopy).View3D;
                    break;
                case 3:
                    // BAND-AID FIX: Cannot use empty ArrayViews on OpenCL devices.
                    view1d = new Buffer<T>(new T[1], Buffer.Behavior.NoCopy).View1D;
                    // BAND-AID FIX: Cannot use empty ArrayViews on OpenCL devices.
                    view2d = new Buffer<T>(new T[1, 1], Buffer.Behavior.NoCopy).View2D;
                    view3d = buf.View3D;
                    break;
            }

            dims = buf.Dimensions;
        }

        /// <summary>
        /// Overload for [] operator.
        /// </summary>
        /// <param name="idx">The ID to index into.</param>
        /// <returns>The data at that ID.</returns>
        public ref T this[int idx]
        {
            get => ref view1d[idx];
        }

        /// <summary>
        /// Overload for [,] operator.
        /// </summary>
        /// <param name="i">The first ID to index into.</param>
        /// <param name="j">The second ID to index into.</param>
        /// <returns>The data at that ID.</returns>
        public ref T this[int i, int j]
        {
            get => ref view2d[i, j];
        }

        /// <summary>
        /// Overload for [,,] operator.
        /// </summary>
        /// <param name="i">The first ID to index into.</param>
        /// <param name="j">The second ID to index into.</param>
        /// <param name="k">The third ID to index into.</param>
        /// <returns>The data at that ID.</returns>
        public ref T this[int i, int j, int k]
        {
            get => ref view3d[i, j, k];
        }

        /// <summary>
        /// Gets the length of the array.
        /// </summary>
        public int Length
        {
            get
            {
                switch (dims)
                {
                    case 1:
                    default:
                        return view1d.IntLength;
                    case 2:
                        return view2d.IntLength;
                    case 3:
                        return view3d.IntLength;
                }
            }
        }
    }
}
