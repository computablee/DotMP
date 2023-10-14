using ILGPU;
using System;

namespace DotMP.GPU
{
    /// <summary>
    /// Handle for a GPU kernel to retrieve its kernel variables.
    /// </summary>
    /// <typeparam name="T">The type of the first data parameter.</typeparam>
    public struct Handle<T>
        where T : unmanaged
    {
        /// <summary>
        /// The first data parameter.
        /// </summary>
        private readonly GPUArray<T> o1;

        /// <summary>
        /// Constructor to be called from the CPU.
        /// </summary>
        /// <param name="o1">The first data parameter.</param>
        internal Handle(ArrayView<T> o1)
        {
            this.o1 = o1;
        }

        /// <summary>
        /// Gets the data for the GPU kernel.
        /// </summary>
        /// <param name="idx">The Index1D parameter passed to the kernel.</param>
        /// <returns>A tuple containing the index of the for loop plus all of the data.</returns>
        public readonly ValueTuple<int, GPUArray<T>> GetData(Index1D idx)
        {
            return (idx.X, o1);
        }
    }


    /// <summary>
    /// Handle for a GPU kernel to retrieve its kernel variables.
    /// </summary>
    /// <typeparam name="T">The type of the first data parameter.</typeparam>
    /// <typeparam name="U">The type of the second data parameter.</typeparam>
    public struct Handle<T, U>
        where T : unmanaged
        where U : unmanaged
    {
        /// <summary>
        /// The first data parameter.
        /// </summary>
        private readonly GPUArray<T> o1;
        /// <summary>
        /// The second data parameter.
        /// </summary>
        private readonly GPUArray<U> o2;

        /// <summary>
        /// Constructor to be called from the CPU.
        /// </summary>
        /// <param name="o1">The first data parameter.</param>
        /// <param name="o2">The second data parameter.</param>
        internal Handle(ArrayView<T> o1, ArrayView<U> o2)
        {
            this.o1 = o1;
            this.o2 = o2;
        }

        /// <summary>
        /// Gets the data for the GPU kernel.
        /// </summary>
        /// <param name="idx">The Index1D parameter passed to the kernel.</param>
        /// <returns>A tuple containing the index of the for loop plus all of the data.</returns>
        public readonly ValueTuple<int, GPUArray<T>, GPUArray<U>> GetData(Index1D idx)
        {
            return (idx.X, o1, o2);
        }
    }


    /// <summary>
    /// Handle for a GPU kernel to retrieve its kernel variables.
    /// </summary>
    /// <typeparam name="T">The type of the first data parameter.</typeparam>
    /// <typeparam name="U">The type of the second data parameter.</typeparam>
    /// <typeparam name="V">The type of the third data parameter.</typeparam>
    public struct Handle<T, U, V>
        where T : unmanaged
        where U : unmanaged
        where V : unmanaged
    {
        /// <summary>
        /// The first data parameter.
        /// </summary>
        private readonly GPUArray<T> o1;
        /// <summary>
        /// The second data parameter.
        /// </summary>
        private readonly GPUArray<U> o2;
        /// <summary>
        /// The third data parameter.
        /// </summary>
        private readonly GPUArray<V> o3;

        /// <summary>
        /// Constructor to be called from the CPU.
        /// </summary>
        /// <param name="o1">The first data parameter.</param>
        /// <param name="o2">The second data parameter.</param>
        /// <param name="o3">The third data parameter.</param>
        internal Handle(ArrayView<T> o1, ArrayView<U> o2, ArrayView<V> o3)
        {
            this.o1 = o1;
            this.o2 = o2;
            this.o3 = o3;
        }

        /// <summary>
        /// Gets the data for the GPU kernel.
        /// </summary>
        /// <param name="idx">The Index1D parameter passed to the kernel.</param>
        /// <returns>A tuple containing the index of the for loop plus all of the data.</returns>
        public readonly ValueTuple<int, GPUArray<T>, GPUArray<U>, GPUArray<V>> GetData(Index1D idx)
        {
            return (idx.X, o1, o2, o3);
        }
    }

    /// <summary>
    /// Handle for a GPU kernel to retrieve its kernel variables.
    /// </summary>
    /// <typeparam name="T">The type of the first data parameter.</typeparam>
    /// <typeparam name="U">The type of the second data parameter.</typeparam>
    /// <typeparam name="V">The type of the third data parameter.</typeparam>
    /// <typeparam name="W">The type of the fourth data parameter.</typeparam>
    public struct Handle<T, U, V, W>
        where T : unmanaged
        where U : unmanaged
        where V : unmanaged
        where W : unmanaged
    {
        /// <summary>
        /// The first data parameter.
        /// </summary>
        private readonly GPUArray<T> o1;
        /// <summary>
        /// The second data parameter.
        /// </summary>
        private readonly GPUArray<U> o2;
        /// <summary>
        /// The third data parameter.
        /// </summary>
        private readonly GPUArray<V> o3;
        /// <summary>
        /// The fourth data parameter.
        /// </summary>
        private readonly GPUArray<W> o4;

        /// <summary>
        /// Constructor to be called from the CPU.
        /// </summary>
        /// <param name="o1">The first data parameter.</param>
        /// <param name="o2">The second data parameter.</param>
        /// <param name="o3">The third data parameter.</param>
        /// <param name="o4">The fourth data parameter.</param>
        internal Handle(ArrayView<T> o1, ArrayView<U> o2, ArrayView<V> o3, ArrayView<W> o4)
        {
            this.o1 = o1;
            this.o2 = o2;
            this.o3 = o3;
            this.o4 = o4;
        }

        /// <summary>
        /// Gets the data for the GPU kernel.
        /// </summary>
        /// <param name="idx">The Index1D parameter passed to the kernel.</param>
        /// <returns>A tuple containing the index of the for loop plus all of the data.</returns>
        public readonly ValueTuple<int, GPUArray<T>, GPUArray<U>, GPUArray<V>, GPUArray<W>> GetData(Index1D idx)
        {
            return (idx.X, o1, o2, o3, o4);
        }
    }
}