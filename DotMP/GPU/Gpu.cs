using System;
using ILGPU;

namespace DotMP.GPU
{
    /// <summary>
    /// The main class of DotMP's GPU API, powered by the ILGPU project.
    /// Contains all the main methods for constructing and running GPU kernels.
    /// The GPU API is not thread-safe at the current moment, so its methods should not be called from within a Parallel.ParallelRegion!
    /// </summary>
    public static class Parallel
    {
        /// <summary>
        /// Creates a GPU parallel for loop.
        /// The body of the kernel is run on a GPU target.
        /// This overload specifies that one array is used on the GPU.
        /// </summary>
        /// <param name="start">The start of the loop, inclusive.</param>
        /// <param name="end">The end of the loop, exclusive.</param>
        /// <param name="buf">The buffer to run the kernel with.</param>
        /// <param name="action">The kernel to run on the GPU.</param>
        /// <typeparam name="T">The base type of the first argument. Must be an unmanaged type.</typeparam>
        public static void ParallelFor<T>(int start, int end, Buffer<T> buf, Action<Index, GPUArray<T>> action)
            where T : unmanaged
        {
            var handler = new AcceleratorHandler();
            handler.DispatchKernel(start, end, buf, action);
        }

        /// <summary>
        /// Creates a GPU parallel for loop.
        /// The body of the kernel is run on a GPU target.
        /// This overload specifies that two arrays are used on the GPU.
        /// </summary>
        /// <param name="start">The start of the loop, inclusive.</param>
        /// <param name="end">The end of the loop, exclusive.</param>
        /// <param name="buf1">The first buffer to run the kernel with.</param>
        /// <param name="buf2">The second buffer to run the kernel with.</param>
        /// <param name="action">The kernel to run on the GPU.</param>
        /// <typeparam name="T">The base type of the first argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="U">The base type of the second argument. Must be an unmanaged type.</typeparam>
        public static void ParallelFor<T, U>(int start, int end, Buffer<T> buf1, Buffer<U> buf2, Action<Index, GPUArray<T>, GPUArray<U>> action)
            where T : unmanaged
            where U : unmanaged
        {
            var handler = new AcceleratorHandler();
            handler.DispatchKernel(start, end, buf1, buf2, action);
        }

        /// <summary>
        /// Creates a GPU parallel for loop.
        /// The body of the kernel is run on a GPU target.
        /// This overload specifies that three arrays are used on the GPU.
        /// </summary>
        /// <param name="start">The start of the loop, inclusive.</param>
        /// <param name="end">The end of the loop, exclusive.</param>
        /// <param name="buf1">The first buffer to run the kernel with.</param>
        /// <param name="buf2">The second buffer to run the kernel with.</param>
        /// <param name="buf3">The third buffer to run the kernel with.</param>
        /// <param name="action">The kernel to run on the GPU.</param>
        /// <typeparam name="T">The base type of the first argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="U">The base type of the second argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="V">The base type of the third argument. Must be an unmanaged type.</typeparam>
        public static void ParallelFor<T, U, V>(int start, int end, Buffer<T> buf1, Buffer<U> buf2, Buffer<V> buf3, Action<Index, GPUArray<T>, GPUArray<U>, GPUArray<V>> action)
            where T : unmanaged
            where U : unmanaged
            where V : unmanaged
        {
            var handler = new AcceleratorHandler();
            handler.DispatchKernel(start, end, buf1, buf2, buf3, action);
        }

        /// <summary>
        /// Creates a GPU parallel for loop.
        /// The body of the kernel is run on a GPU target.
        /// This overload specifies that four arrays are used on the GPU.
        /// </summary>
        /// <param name="start">The start of the loop, inclusive.</param>
        /// <param name="end">The end of the loop, exclusive.</param>
        /// <param name="buf1">The first buffer to run the kernel with.</param>
        /// <param name="buf2">The second buffer to run the kernel with.</param>
        /// <param name="buf3">The third buffer to run the kernel with.</param>
        /// <param name="buf4">The fourth buffer to run the kernel with.</param>
        /// <param name="action">The kernel to run on the GPU.</param>
        /// <typeparam name="T">The base type of the first argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="U">The base type of the second argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="V">The base type of the third argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="W">The base type of the fourth argument. Must be an unmanaged type.</typeparam>
        public static void ParallelFor<T, U, V, W>(int start, int end, Buffer<T> buf1, Buffer<U> buf2, Buffer<V> buf3, Buffer<W> buf4, Action<Index, GPUArray<T>, GPUArray<U>, GPUArray<V>, GPUArray<W>> action)
            where T : unmanaged
            where U : unmanaged
            where V : unmanaged
            where W : unmanaged
        {
            var handler = new AcceleratorHandler();
            handler.DispatchKernel(start, end, buf1, buf2, buf3, buf4, action);
        }

        /// <summary>
        /// Creates a GPU parallel for loop.
        /// The body of the kernel is run on a GPU target.
        /// This overload specifies that five arrays are used on the GPU.
        /// </summary>
        /// <param name="start">The start of the loop, inclusive.</param>
        /// <param name="end">The end of the loop, exclusive.</param>
        /// <param name="buf1">The first buffer to run the kernel with.</param>
        /// <param name="buf2">The second buffer to run the kernel with.</param>
        /// <param name="buf3">The third buffer to run the kernel with.</param>
        /// <param name="buf4">The fourth buffer to run the kernel with.</param>
        /// <param name="buf5">The fifth buffer to run the kernel with.</param>
        /// <param name="action">The kernel to run on the GPU.</param>
        /// <typeparam name="T">The base type of the first argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="U">The base type of the second argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="V">The base type of the third argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="W">The base type of the fourth argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="X">The base type of the fifth argument. Must be an unmanaged type.</typeparam>
        public static void ParallelFor<T, U, V, W, X>(int start, int end, Buffer<T> buf1, Buffer<U> buf2, Buffer<V> buf3, Buffer<W> buf4, Buffer<X> buf5, Action<Index, GPUArray<T>, GPUArray<U>, GPUArray<V>, GPUArray<W>, GPUArray<X>> action)
            where T : unmanaged
            where U : unmanaged
            where V : unmanaged
            where W : unmanaged
            where X : unmanaged
        {
            var handler = new AcceleratorHandler();
            handler.DispatchKernel(start, end, buf1, buf2, buf3, buf4, buf5, action);
        }

        /// <summary>
        /// Creates a GPU parallel for loop.
        /// The body of the kernel is run on a GPU target.
        /// This overload specifies that six arrays are used on the GPU.
        /// </summary>
        /// <param name="start">The start of the loop, inclusive.</param>
        /// <param name="end">The end of the loop, exclusive.</param>
        /// <param name="buf1">The first buffer to run the kernel with.</param>
        /// <param name="buf2">The second buffer to run the kernel with.</param>
        /// <param name="buf3">The third buffer to run the kernel with.</param>
        /// <param name="buf4">The fourth buffer to run the kernel with.</param>
        /// <param name="buf5">The fifth buffer to run the kernel with.</param>
        /// <param name="buf6">The sixth buffer to run the kernel with.</param>
        /// <param name="action">The kernel to run on the GPU.</param>
        /// <typeparam name="T">The base type of the first argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="U">The base type of the second argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="V">The base type of the third argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="W">The base type of the fourth argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="X">The base type of the fifth argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="Y">The base type of the sixth argument. Must be an unmanaged type.</typeparam>
        public static void ParallelFor<T, U, V, W, X, Y>(int start, int end, Buffer<T> buf1, Buffer<U> buf2, Buffer<V> buf3, Buffer<W> buf4, Buffer<X> buf5, Buffer<Y> buf6, Action<Index, GPUArray<T>, GPUArray<U>, GPUArray<V>, GPUArray<W>, GPUArray<X>, GPUArray<Y>> action)
            where T : unmanaged
            where U : unmanaged
            where V : unmanaged
            where W : unmanaged
            where X : unmanaged
            where Y : unmanaged
        {
            var handler = new AcceleratorHandler();
            handler.DispatchKernel(start, end, buf1, buf2, buf3, buf4, buf5, buf6, action);
        }
    }
}