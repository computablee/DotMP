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
    public class Buffer<T> : IDisposable
        where T : unmanaged
    {
        /// <summary>
        /// The ILGPU buffer for 1D arrays.
        /// </summary>
        private MemoryBuffer1D<T, Stride1D.Dense> buf1d;

        /// <summary>
        /// The ILGPU buffer for 2D arrays.
        /// </summary>
        private MemoryBuffer2D<T, Stride2D.DenseY> buf2d;

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
        /// Handler int for the number of dimensions in the array.
        /// </summary>
        private int dims;

        /// <summary>
        /// The number of dimensions in the array.
        /// </summary>
        internal int Dimensions
        {
            get
            {
                return dims;
            }

            private set
            {
                if (value < 1 || value > 3)
                    throw new ArgumentOutOfRangeException("Number of dimensions must be between 1 and 3.");

                dims = value;
            }
        }

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
                    buf1d = AcceleratorHandler.accelerator.Allocate1D(data);
                    break;
                case Buffer.Behavior.From:
                case Buffer.Behavior.NoCopy:
                    buf1d = AcceleratorHandler.accelerator.Allocate1D<T>(data.Length);
                    break;
            }

            Dimensions = 1;
        }

        /// <summary>
        /// Constructor for buffer object. Allocates a 2D array on the GPU and makes it available for the next GPU kernel.
        /// </summary>
        /// <param name="data">The data to allocate on the GPU.</param>
        /// <param name="behavior">The behavior of the data, see Behavior.</param>
        public Buffer(T[,] data, Buffer.Behavior behavior)
        {
            new AcceleratorHandler();

            this.behavior = behavior;
            this.data2d = data;

            switch (behavior)
            {
                case Buffer.Behavior.To:
                case Buffer.Behavior.ToFrom:
                    buf2d = AcceleratorHandler.accelerator.Allocate2DDenseY(data);
                    break;
                case Buffer.Behavior.From:
                case Buffer.Behavior.NoCopy:
                    buf2d = AcceleratorHandler.accelerator.Allocate2DDenseY<T>((data.GetLength(0), data.GetLength(1)));
                    break;
            }

            Dimensions = 2;
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
                    buf1d.GetAsArray1D().CopyTo(data1d, 0);
                }

                buf1d.Dispose();
            }
            else if (Dimensions == 2)
            {
                if (behavior == Buffer.Behavior.From || behavior == Buffer.Behavior.ToFrom)
                {
                    System.Buffer.BlockCopy(buf2d.GetAsArray2D(), 0, data2d, 0, Unsafe.SizeOf<T>() * data2d.Length);
                }

                buf2d.Dispose();
            }
        }

        /// <summary>
        /// Get the view of the memory for the GPU.
        /// </summary>
        internal ArrayView1D<T, Stride1D.Dense> View1D { get => buf1d.View; }

        /// <summary>
        /// Get the view of the memory for the GPU.
        /// </summary>
        internal ArrayView2D<T, Stride2D.DenseY> View2D { get => buf2d.View; }
    }
}