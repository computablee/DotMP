using System;
using System.Linq;
using ILGPU;
using ILGPU.Runtime;

namespace DotMP.GPU
{
    /// <summary>
    /// The handler class managing GPU acceleration.
    /// </summary>
    internal class AcceleratorHandler
    {
        /// <summary>
        /// Determines if a GPU context has been initialized yet.
        /// </summary>
        private static bool initialized = false;
        /// <summary>
        /// The GPU context.
        /// </summary>
        private static Context context;
        /// <summary>
        /// The accelerator object.
        /// </summary>
        internal static Accelerator accelerator;
        /// <summary>
        /// 
        /// </summary>
        private static int block_size;

        /// <summary>
        /// Default constructor. If this is the first time it's called, it initializes all relevant singleton data.
        /// </summary>
        internal AcceleratorHandler()
        {
            if (initialized) return;

            context = Context.CreateDefault();
            accelerator = context.Devices[0].CreateAccelerator(context);
            Console.WriteLine("Using {0} accelerator.", accelerator.AcceleratorType.ToString());
            initialized = true;
            block_size = accelerator.AcceleratorType == AcceleratorType.CPU ? 16 : 256;
        }

        /// <summary>
        /// Synchronize pending operations.
        /// </summary>
        private void Synchronize() => accelerator.Synchronize();

        /// <summary>
        /// Dispatches a kernel with one data parameter.
        /// </summary>
        /// <typeparam name="T">The type of the data parameter.</typeparam>
        /// <param name="start">The start of the loop, inclusive.</param>
        /// <param name="end">The end of the loop, exclusive.</param>
        /// <param name="buf">The buffer to run the kernel with.</param>
        /// <param name="action">The action to perform.</param>
        internal void DispatchKernel<T>(int start, int end, Buffer<T> buf, Action<Index, GPUArray<T>> action)
            where T : unmanaged
        {
            var idx = new Index();

            var kernel = accelerator.LoadStreamKernel(action);

            kernel(((end - start) / block_size, block_size), idx,
                new GPUArray<T>(buf.View));

            Synchronize();
        }

        /// <summary>
        /// Dispatches a kernel with two data parameters.
        /// </summary>
        /// <typeparam name="T">The type of the first data parameter.</typeparam>
        /// <typeparam name="U">The type of the second data parameter.</typeparam>
        /// <param name="start">The start of the loop, inclusive.</param>
        /// <param name="end">The end of the loop, exclusive.</param>
        /// <param name="buf1">The first buffer to run the kernel with.</param>
        /// <param name="buf2">The second buffer to run the kernel with.</param>
        /// <param name="action">The action to perform.</param>
        internal void DispatchKernel<T, U>(int start, int end, Buffer<T> buf1, Buffer<U> buf2, Action<Index, GPUArray<T>, GPUArray<U>> action)
            where T : unmanaged
            where U : unmanaged
        {
            var idx = new Index();

            var kernel = accelerator.LoadStreamKernel(action);

            kernel(((end - start) / block_size, block_size), idx,
                new GPUArray<T>(buf1.View),
                new GPUArray<U>(buf2.View));

            Synchronize();
        }

        /// <summary>
        /// Dispatches a kernel with three data parameters.
        /// </summary>
        /// <typeparam name="T">The type of the first data parameter.</typeparam>
        /// <typeparam name="U">The type of the second data parameter.</typeparam>
        /// <typeparam name="V">The type of the third data parameter.</typeparam>
        /// <param name="start">The start of the loop, inclusive.</param>
        /// <param name="end">The end of the loop, exclusive.</param>
        /// <param name="buf1">The first buffer to run the kernel with.</param>
        /// <param name="buf2">The second buffer to run the kernel with.</param>
        /// <param name="buf3">The third buffer to run the kernel with.</param>
        /// <param name="action">The action to perform.</param>
        internal void DispatchKernel<T, U, V>(int start, int end, Buffer<T> buf1, Buffer<U> buf2, Buffer<V> buf3, Action<Index, GPUArray<T>, GPUArray<U>, GPUArray<V>> action)
            where T : unmanaged
            where U : unmanaged
            where V : unmanaged
        {
            var idx = new Index();

            var kernel = accelerator.LoadStreamKernel(action);

            kernel(((end - start) / block_size, block_size), idx,
                new GPUArray<T>(buf1.View),
                new GPUArray<U>(buf2.View),
                new GPUArray<V>(buf3.View));

            Synchronize();
        }

        /// <summary>
        /// Dispatches a kernel with four parameters.
        /// </summary>
        /// <typeparam name="T">The type of the first data parameter.</typeparam>
        /// <typeparam name="U">The type of the second data parameter.</typeparam>
        /// <typeparam name="V">The type of the third data parameter.</typeparam>
        /// <typeparam name="W">The type of the fourth data parameter.</typeparam>
        /// <param name="start">The start of the loop, inclusive.</param>
        /// <param name="end">The end of the loop, exclusive.</param>
        /// <param name="buf1">The first buffer to run the kernel with.</param>
        /// <param name="buf2">The second buffer to run the kernel with.</param>
        /// <param name="buf3">The third buffer to run the kernel with.</param>
        /// <param name="buf4">The fourth buffer to run the kernel with.</param>
        /// <param name="action">The action to perform.</param>
        internal void DispatchKernel<T, U, V, W>(int start, int end, Buffer<T> buf1, Buffer<U> buf2, Buffer<V> buf3, Buffer<W> buf4, Action<Index, GPUArray<T>, GPUArray<U>, GPUArray<V>, GPUArray<W>> action)
            where T : unmanaged
            where U : unmanaged
            where V : unmanaged
            where W : unmanaged
        {
            var idx = new Index();

            var kernel = accelerator.LoadStreamKernel(action);

            kernel(((end - start) / block_size, block_size), idx,
                new GPUArray<T>(buf1.View),
                new GPUArray<U>(buf2.View),
                new GPUArray<V>(buf3.View),
                new GPUArray<W>(buf4.View));

            Synchronize();
        }

        /// <summary>
        /// Dispatches a kernel with five parameters.
        /// </summary>
        /// <typeparam name="T">The type of the first data parameter.</typeparam>
        /// <typeparam name="U">The type of the second data parameter.</typeparam>
        /// <typeparam name="V">The type of the third data parameter.</typeparam>
        /// <typeparam name="W">The type of the fourth data parameter.</typeparam>
        /// <typeparam name="X">The type of the fifth data parameter.</typeparam>
        /// <param name="start">The start of the loop, inclusive.</param>
        /// <param name="end">The end of the loop, exclusive.</param>
        /// <param name="buf1">The first buffer to run the kernel with.</param>
        /// <param name="buf2">The second buffer to run the kernel with.</param>
        /// <param name="buf3">The third buffer to run the kernel with.</param>
        /// <param name="buf4">The fourth buffer to run the kernel with.</param>
        /// <param name="buf5">The fifth buffer to run the kernel with.</param>
        /// <param name="action">The action to perform.</param>
        internal void DispatchKernel<T, U, V, W, X>(int start, int end, Buffer<T> buf1, Buffer<U> buf2, Buffer<V> buf3, Buffer<W> buf4, Buffer<X> buf5, Action<Index, GPUArray<T>, GPUArray<U>, GPUArray<V>, GPUArray<W>, GPUArray<X>> action)
            where T : unmanaged
            where U : unmanaged
            where V : unmanaged
            where W : unmanaged
            where X : unmanaged
        {
            var idx = new Index();

            var kernel = accelerator.LoadStreamKernel(action);

            kernel(((end - start) / block_size, block_size), idx,
                new GPUArray<T>(buf1.View),
                new GPUArray<U>(buf2.View),
                new GPUArray<V>(buf3.View),
                new GPUArray<W>(buf4.View),
                new GPUArray<X>(buf5.View));

            Synchronize();
        }

        /// <summary>
        /// Dispatches a kernel with six parameters.
        /// </summary>
        /// <typeparam name="T">The type of the first data parameter.</typeparam>
        /// <typeparam name="U">The type of the second data parameter.</typeparam>
        /// <typeparam name="V">The type of the third data parameter.</typeparam>
        /// <typeparam name="W">The type of the fourth data parameter.</typeparam>
        /// <typeparam name="X">The type of the fifth data parameter.</typeparam>
        /// <typeparam name="Y">The type of the sixth data parameter.</typeparam>
        /// <param name="start">The start of the loop, inclusive.</param>
        /// <param name="end">The end of the loop, exclusive.</param>
        /// <param name="buf1">The first buffer to run the kernel with.</param>
        /// <param name="buf2">The second buffer to run the kernel with.</param>
        /// <param name="buf3">The third buffer to run the kernel with.</param>
        /// <param name="buf4">The fourth buffer to run the kernel with.</param>
        /// <param name="buf5">The fifth buffer to run the kernel with.</param>
        /// <param name="buf6">The sixth buffer to run the kernel with.</param>
        /// <param name="action">The action to perform.</param>
        internal void DispatchKernel<T, U, V, W, X, Y>(int start, int end, Buffer<T> buf1, Buffer<U> buf2, Buffer<V> buf3, Buffer<W> buf4, Buffer<X> buf5, Buffer<Y> buf6, Action<Index, GPUArray<T>, GPUArray<U>, GPUArray<V>, GPUArray<W>, GPUArray<X>, GPUArray<Y>> action)
            where T : unmanaged
            where U : unmanaged
            where V : unmanaged
            where W : unmanaged
            where X : unmanaged
            where Y : unmanaged
        {
            var idx = new Index();

            var kernel = accelerator.LoadStreamKernel(action);

            kernel(((end - start) / block_size, block_size), idx,
                new GPUArray<T>(buf1.View),
                new GPUArray<U>(buf2.View),
                new GPUArray<V>(buf3.View),
                new GPUArray<W>(buf4.View),
                new GPUArray<X>(buf5.View),
                new GPUArray<Y>(buf6.View));

            Synchronize();
        }
    }
}