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

using System;
using System.Runtime.CompilerServices;
using ILGPU;
using ILGPU.Runtime;

namespace DotMP.GPU
{
    namespace Buffer
    {
        /// <summary>
        /// Specifies the behavior of the buffer.
        /// </summary>
        public enum Behavior
        {
            /// <summary>
            /// Specifies that data should be transfered to the GPU, but not from it.
            /// </summary>
            To,
            /// <summary>
            /// Specifies that data should be transfered from the GPU, but not to it.
            /// </summary>
            From,
            /// <summary>
            /// Specifies that data should be transfered both to and from the GPU.
            /// </summary>
            ToFrom,
            /// <summary>
            /// Specifies that the data shouldn't be transfered to or from the GPU. For internal use.
            /// </summary>
            NoCopy
        }
    }

    /// <summary>
    /// Buffer to manage GPU memory. Should only be created on the CPU.
    /// </summary>
    public sealed class Buffer<T> : IDisposable
        where T : unmanaged
    {
        /// <summary>
        /// The ILGPU buffer for arrays.
        /// </summary>
        private MemoryBuffer1D<T, Stride1D.Dense> buf;

        /// <summary>
        /// Behavior of the data, as specified by Behavior.
        /// </summary>
        private Buffer.Behavior behavior;

        /// <summary>
        /// The CPU 1D array, so that we can copy the data back.
        /// </summary>
        private T[] data1d;

        /// <summary>
        /// The CPU 2D array, so that we can copy the data back.
        /// </summary>
        private T[,] data2d;

        /// <summary>
        /// The CPU 3D array, so that we can copy the data back.
        /// </summary>
        private T[,,] data3d;

        /// <summary>
        /// Stride of Y dimension.
        /// </summary>
        private int stride_y;

        /// <summary>
        /// Stride of Z dimension.
        /// </summary>
        private int stride_z;

        /// <summary>
        /// The number of dimensions in the array.
        /// </summary>
        internal int Dimensions { get; private set; }

        /// <summary>
        /// Constructor for buffer object. Allocates a 1D array on the GPU and makes it available for the next GPU kernel.
        /// </summary>
        /// <param name="data">The data to allocate on the GPU.</param>
        /// <param name="behavior">The behavior of the data, see Behavior.</param>
        public Buffer(T[] data, Buffer.Behavior behavior)
        {
            new AcceleratorHandler();

            this.behavior = behavior;
            this.data1d = data;

            switch (behavior)
            {
                case Buffer.Behavior.To:
                case Buffer.Behavior.ToFrom:
                    buf = AcceleratorHandler.accelerator.Allocate1D(data);
                    break;
                case Buffer.Behavior.From:
                case Buffer.Behavior.NoCopy:
                    buf = AcceleratorHandler.accelerator.Allocate1D<T>(data.Length);
                    break;
            }

            stride_y = 0;
            stride_z = 0;
            Dimensions = 1;
        }

        /// <summary>
        /// Constructor for buffer object. Allocates a 2D array on the GPU and makes it available for the next GPU kernel.
        /// </summary>
        /// <param name="data">The data to allocate on the GPU.</param>
        /// <param name="behavior">The behavior of the data, see Behavior.</param>
        public unsafe Buffer(T[,] data, Buffer.Behavior behavior)
        {
            new AcceleratorHandler();

            this.behavior = behavior;
            this.data2d = data;
            buf = AcceleratorHandler.accelerator.Allocate1D<T>(data.Length);

            /* jscpd:ignore-start */
            if (behavior == Buffer.Behavior.To || behavior == Buffer.Behavior.ToFrom)
            {
                fixed (T* data_ptr = data)
                {
                    ReadOnlySpan<T> data_span = new Span<T>(data_ptr, data.Length);
                    buf.View.BaseView.CopyFromCPU(data_span);
                }
            }
            /* jscpd:ignore-end */

            stride_y = data.GetLength(1);
            stride_z = 0;
            Dimensions = 2;
        }

        /// <summary>
        /// Constructor for buffer object. Allocates a 3D array on the GPU and makes it available for the next GPU kernel.
        /// </summary>
        /// <param name="data">The data to allocate on the GPU.</param>
        /// <param name="behavior">The behavior of the data, see Behavior.</param>
        public unsafe Buffer(T[,,] data, Buffer.Behavior behavior)
        {
            new AcceleratorHandler();

            this.behavior = behavior;
            this.data3d = data;
            buf = AcceleratorHandler.accelerator.Allocate1D<T>(data.Length);

            /* jscpd:ignore-start */
            if (behavior == Buffer.Behavior.To || behavior == Buffer.Behavior.ToFrom)
            {
                fixed (T* data_ptr = data)
                {
                    ReadOnlySpan<T> data_span = new Span<T>(data_ptr, data.Length);
                    buf.View.BaseView.CopyFromCPU(data_span);
                }
            }
            /* jscpd:ignore-end */

            stride_y = data.GetLength(1);
            stride_z = data.GetLength(2);
            Dimensions = 3;
        }

        /// <summary>
        /// Dispose of the buffer, freeing GPU memory and copying any relevant data back to the CPU.
        /// </summary>
        public void Dispose()
        {
            if (Dimensions == 1)
            {
                if (behavior == Buffer.Behavior.From || behavior == Buffer.Behavior.ToFrom)
                {
                    buf.GetAsArray1D().CopyTo(data1d, 0);
                }

                buf.Dispose();
            }
            else if (Dimensions == 2)
            {
                if (behavior == Buffer.Behavior.From || behavior == Buffer.Behavior.ToFrom)
                {
                    System.Buffer.BlockCopy(buf.GetAsArray1D(), 0, data2d, 0, Unsafe.SizeOf<T>() * data2d.Length);
                }

                buf.Dispose();
            }
            else if (Dimensions == 3)
            {
                if (behavior == Buffer.Behavior.From || behavior == Buffer.Behavior.ToFrom)
                {
                    System.Buffer.BlockCopy(buf.GetAsArray1D(), 0, data3d, 0, Unsafe.SizeOf<T>() * data3d.Length);
                }

                buf.Dispose();
            }
        }

        /// <summary>
        /// Get the view of the memory for the GPU.
        /// </summary>
        internal ArrayView1D<T, Stride1D.Dense> View { get => buf.View; }

        /// <summary>
        /// Gets the Y and Z strides of the array.
        /// </summary>
        internal ValueTuple<int, int> Strides { get => (stride_y, stride_z); }
    }
}