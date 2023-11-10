using System;
using DotMP.GPU;
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
            ToFrom
        }
    }

    /// <summary>
    /// Buffer to manage GPU memory. Should only be created on the CPU.
    /// </summary>
    public class Buffer<T> : IDisposable
        where T : unmanaged
    {

        /// <summary>
        /// The ILGPU buffer.
        /// </summary>
        private MemoryBuffer1D<T, ILGPU.Stride1D.Dense> buf;

        /// <summary>
        /// Behavior of the data, as specified by Behavior.
        /// </summary>
        private Buffer.Behavior behavior;

        /// <summary>
        /// The CPU array, so that we can copy the data back.
        /// </summary>
        private T[] data;

        /// <summary>
        /// Constructor for buffer object. Allocates data on the GPU and makes it available for the next GPU kernel.
        /// </summary>
        /// <param name="data">The data to allocate on the GPU.</param>
        /// <param name="behavior">The behavior of the data, see Behavior.</param>
        public Buffer(T[] data, Buffer.Behavior behavior)
        {
            new AcceleratorHandler();

            this.behavior = behavior;
            this.data = data;

            switch (behavior)
            {
                case Buffer.Behavior.To:
                case Buffer.Behavior.ToFrom:
                    buf = AcceleratorHandler.accelerator.Allocate1D(data);
                    break;
                case Buffer.Behavior.From:
                    buf = AcceleratorHandler.accelerator.Allocate1D<T>(data.Length);
                    break;
            }
        }

        /// <summary>
        /// Dispose of the buffer, freeing GPU memory and copying any relevant data back to the CPU.
        /// </summary>
        public void Dispose()
        {
            if (behavior == Buffer.Behavior.From || behavior == Buffer.Behavior.ToFrom)
            {
                buf.GetAsArray1D().CopyTo(data, 0);
            }

            buf.Dispose();
        }

        /// <summary>
        /// Get the view of the memory for the GPU.
        /// </summary>
        internal ArrayView1D<T, ILGPU.Stride1D.Dense> View { get => buf.View; }
    }
}