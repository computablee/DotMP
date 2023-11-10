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

        /// <summary>
        /// Creates a GPU parallel for loop.
        /// The body of the kernel is run on a GPU target.
        /// This overload specifies that seven arrays are used on the GPU.
        /// </summary>
        /// <param name="start">The start of the loop, inclusive.</param>
        /// <param name="end">The end of the loop, exclusive.</param>
        /// <param name="buf1">The first buffer to run the kernel with.</param>
        /// <param name="buf2">The second buffer to run the kernel with.</param>
        /// <param name="buf3">The third buffer to run the kernel with.</param>
        /// <param name="buf4">The fourth buffer to run the kernel with.</param>
        /// <param name="buf5">The fifth buffer to run the kernel with.</param>
        /// <param name="buf6">The sixth buffer to run the kernel with.</param>
        /// <param name="buf7">The seventh buffer to run the kernel with.</param>
        /// <param name="action">The kernel to run on the GPU.</param>
        /// <typeparam name="T">The base type of the first argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="U">The base type of the second argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="V">The base type of the third argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="W">The base type of the fourth argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="X">The base type of the fifth argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="Y">The base type of the sixth argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="Z">The base type of the seventh argument. Must be an unmanaged type.</typeparam>
        public static void ParallelFor<T, U, V, W, X, Y, Z>(int start, int end, Buffer<T> buf1, Buffer<U> buf2, Buffer<V> buf3, Buffer<W> buf4, Buffer<X> buf5, Buffer<Y> buf6, Buffer<Z> buf7, Action<Index, GPUArray<T>, GPUArray<U>, GPUArray<V>, GPUArray<W>, GPUArray<X>, GPUArray<Y>, GPUArray<Z>> action)
            where T : unmanaged
            where U : unmanaged
            where V : unmanaged
            where W : unmanaged
            where X : unmanaged
            where Y : unmanaged
            where Z : unmanaged
        {
            var handler = new AcceleratorHandler();
            handler.DispatchKernel(start, end, buf1, buf2, buf3, buf4, buf5, buf6, buf7, action);
        }

        /// <summary>
        /// Creates a GPU parallel for loop.
        /// The body of the kernel is run on a GPU target.
        /// This overload specifies that eight arrays are used on the GPU.
        /// </summary>
        /// <param name="start">The start of the loop, inclusive.</param>
        /// <param name="end">The end of the loop, exclusive.</param>
        /// <param name="buf1">The first buffer to run the kernel with.</param>
        /// <param name="buf2">The second buffer to run the kernel with.</param>
        /// <param name="buf3">The third buffer to run the kernel with.</param>
        /// <param name="buf4">The fourth buffer to run the kernel with.</param>
        /// <param name="buf5">The fifth buffer to run the kernel with.</param>
        /// <param name="buf6">The sixth buffer to run the kernel with.</param>
        /// <param name="buf7">The seventh buffer to run the kernel with.</param>
        /// <param name="buf8">The eighth buffer to run the kernel with.</param>
        /// <param name="action">The kernel to run on the GPU.</param>
        /// <typeparam name="T">The base type of the first argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="U">The base type of the second argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="V">The base type of the third argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="W">The base type of the fourth argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="X">The base type of the fifth argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="Y">The base type of the sixth argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="Z">The base type of the seventh argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="A">The base type of the eighth argument. Must be an unmanaged type.</typeparam>
        public static void ParallelFor<T, U, V, W, X, Y, Z, A>(int start, int end, Buffer<T> buf1, Buffer<U> buf2, Buffer<V> buf3, Buffer<W> buf4, Buffer<X> buf5, Buffer<Y> buf6, Buffer<Z> buf7, Buffer<A> buf8, Action<Index, GPUArray<T>, GPUArray<U>, GPUArray<V>, GPUArray<W>, GPUArray<X>, GPUArray<Y>, GPUArray<Z>, GPUArray<A>> action)
            where T : unmanaged
            where U : unmanaged
            where V : unmanaged
            where W : unmanaged
            where X : unmanaged
            where Y : unmanaged
            where Z : unmanaged
            where A : unmanaged
        {
            var handler = new AcceleratorHandler();
            handler.DispatchKernel(start, end, buf1, buf2, buf3, buf4, buf5, buf6, buf7, buf8, action);
        }

        /// <summary>
        /// Creates a GPU parallel for loop.
        /// The body of the kernel is run on a GPU target.
        /// This overload specifies that nine arrays are used on the GPU.
        /// </summary>
        /// <param name="start">The start of the loop, inclusive.</param>
        /// <param name="end">The end of the loop, exclusive.</param>
        /// <param name="buf1">The first buffer to run the kernel with.</param>
        /// <param name="buf2">The second buffer to run the kernel with.</param>
        /// <param name="buf3">The third buffer to run the kernel with.</param>
        /// <param name="buf4">The fourth buffer to run the kernel with.</param>
        /// <param name="buf5">The fifth buffer to run the kernel with.</param>
        /// <param name="buf6">The sixth buffer to run the kernel with.</param>
        /// <param name="buf7">The seventh buffer to run the kernel with.</param>
        /// <param name="buf8">The eighth buffer to run the kernel with.</param>
        /// <param name="buf9">The ninth buffer to run the kernel with.</param>
        /// <param name="action">The kernel to run on the GPU.</param>
        /// <typeparam name="T">The base type of the first argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="U">The base type of the second argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="V">The base type of the third argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="W">The base type of the fourth argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="X">The base type of the fifth argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="Y">The base type of the sixth argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="Z">The base type of the seventh argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="A">The base type of the eighth argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="B">The base type of the ninth argument. Must be an unmanaged type.</typeparam>
        public static void ParallelFor<T, U, V, W, X, Y, Z, A, B>(int start, int end, Buffer<T> buf1, Buffer<U> buf2, Buffer<V> buf3, Buffer<W> buf4, Buffer<X> buf5, Buffer<Y> buf6, Buffer<Z> buf7, Buffer<A> buf8, Buffer<B> buf9, Action<Index, GPUArray<T>, GPUArray<U>, GPUArray<V>, GPUArray<W>, GPUArray<X>, GPUArray<Y>, GPUArray<Z>, GPUArray<A>, GPUArray<B>> action)
            where T : unmanaged
            where U : unmanaged
            where V : unmanaged
            where W : unmanaged
            where X : unmanaged
            where Y : unmanaged
            where Z : unmanaged
            where A : unmanaged
            where B : unmanaged
        {
            var handler = new AcceleratorHandler();
            handler.DispatchKernel(start, end, buf1, buf2, buf3, buf4, buf5, buf6, buf7, buf8, buf9, action);
        }

        /// <summary>
        /// Creates a GPU parallel for loop.
        /// The body of the kernel is run on a GPU target.
        /// This overload specifies that ten arrays are used on the GPU.
        /// </summary>
        /// <param name="start">The start of the loop, inclusive.</param>
        /// <param name="end">The end of the loop, exclusive.</param>
        /// <param name="buf1">The first buffer to run the kernel with.</param>
        /// <param name="buf2">The second buffer to run the kernel with.</param>
        /// <param name="buf3">The third buffer to run the kernel with.</param>
        /// <param name="buf4">The fourth buffer to run the kernel with.</param>
        /// <param name="buf5">The fifth buffer to run the kernel with.</param>
        /// <param name="buf6">The sixth buffer to run the kernel with.</param>
        /// <param name="buf7">The seventh buffer to run the kernel with.</param>
        /// <param name="buf8">The eighth buffer to run the kernel with.</param>
        /// <param name="buf9">The ninth buffer to run the kernel with.</param>
        /// <param name="buf10">The tenth buffer to run the kernel with.</param>
        /// <param name="action">The kernel to run on the GPU.</param>
        /// <typeparam name="T">The base type of the first argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="U">The base type of the second argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="V">The base type of the third argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="W">The base type of the fourth argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="X">The base type of the fifth argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="Y">The base type of the sixth argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="Z">The base type of the seventh argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="A">The base type of the eighth argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="B">The base type of the ninth argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="C">The base type of the tenth argument. Must be an unmanaged type.</typeparam>
        public static void ParallelFor<T, U, V, W, X, Y, Z, A, B, C>(int start, int end, Buffer<T> buf1, Buffer<U> buf2, Buffer<V> buf3, Buffer<W> buf4, Buffer<X> buf5, Buffer<Y> buf6, Buffer<Z> buf7, Buffer<A> buf8, Buffer<B> buf9, Buffer<C> buf10, Action<Index, GPUArray<T>, GPUArray<U>, GPUArray<V>, GPUArray<W>, GPUArray<X>, GPUArray<Y>, GPUArray<Z>, GPUArray<A>, GPUArray<B>, GPUArray<C>> action)
            where T : unmanaged
            where U : unmanaged
            where V : unmanaged
            where W : unmanaged
            where X : unmanaged
            where Y : unmanaged
            where Z : unmanaged
            where A : unmanaged
            where B : unmanaged
            where C : unmanaged
        {
            var handler = new AcceleratorHandler();
            handler.DispatchKernel(start, end, buf1, buf2, buf3, buf4, buf5, buf6, buf7, buf8, buf9, buf10, action);
        }

        /// <summary>
        /// Creates a GPU parallel for loop.
        /// The body of the kernel is run on a GPU target.
        /// This overload specifies that eleven arrays are used on the GPU.
        /// </summary>
        /// <param name="start">The start of the loop, inclusive.</param>
        /// <param name="end">The end of the loop, exclusive.</param>
        /// <param name="buf1">The first buffer to run the kernel with.</param>
        /// <param name="buf2">The second buffer to run the kernel with.</param>
        /// <param name="buf3">The third buffer to run the kernel with.</param>
        /// <param name="buf4">The fourth buffer to run the kernel with.</param>
        /// <param name="buf5">The fifth buffer to run the kernel with.</param>
        /// <param name="buf6">The sixth buffer to run the kernel with.</param>
        /// <param name="buf7">The seventh buffer to run the kernel with.</param>
        /// <param name="buf8">The eighth buffer to run the kernel with.</param>
        /// <param name="buf9">The ninth buffer to run the kernel with.</param>
        /// <param name="buf10">The tenth buffer to run the kernel with.</param>
        /// <param name="buf11">The eleventh buffer to run the kernel with.</param>
        /// <param name="action">The kernel to run on the GPU.</param>
        /// <typeparam name="T">The base type of the first argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="U">The base type of the second argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="V">The base type of the third argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="W">The base type of the fourth argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="X">The base type of the fifth argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="Y">The base type of the sixth argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="Z">The base type of the seventh argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="A">The base type of the eighth argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="B">The base type of the ninth argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="C">The base type of the tenth argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="D">The base type of the eleventh argument. Must be an unmanaged type.</typeparam>
        public static void ParallelFor<T, U, V, W, X, Y, Z, A, B, C, D>(int start, int end, Buffer<T> buf1, Buffer<U> buf2, Buffer<V> buf3, Buffer<W> buf4, Buffer<X> buf5, Buffer<Y> buf6, Buffer<Z> buf7, Buffer<A> buf8, Buffer<B> buf9, Buffer<C> buf10, Buffer<D> buf11, Action<Index, GPUArray<T>, GPUArray<U>, GPUArray<V>, GPUArray<W>, GPUArray<X>, GPUArray<Y>, GPUArray<Z>, GPUArray<A>, GPUArray<B>, GPUArray<C>, GPUArray<D>> action)
            where T : unmanaged
            where U : unmanaged
            where V : unmanaged
            where W : unmanaged
            where X : unmanaged
            where Y : unmanaged
            where Z : unmanaged
            where A : unmanaged
            where B : unmanaged
            where C : unmanaged
            where D : unmanaged
        {
            var handler = new AcceleratorHandler();
            handler.DispatchKernel(start, end, buf1, buf2, buf3, buf4, buf5, buf6, buf7, buf8, buf9, buf10, buf11, action);
        }

        /// <summary>
        /// Creates a GPU parallel for loop.
        /// The body of the kernel is run on a GPU target.
        /// This overload specifies that twelve arrays are used on the GPU.
        /// </summary>
        /// <param name="start">The start of the loop, inclusive.</param>
        /// <param name="end">The end of the loop, exclusive.</param>
        /// <param name="buf1">The first buffer to run the kernel with.</param>
        /// <param name="buf2">The second buffer to run the kernel with.</param>
        /// <param name="buf3">The third buffer to run the kernel with.</param>
        /// <param name="buf4">The fourth buffer to run the kernel with.</param>
        /// <param name="buf5">The fifth buffer to run the kernel with.</param>
        /// <param name="buf6">The sixth buffer to run the kernel with.</param>
        /// <param name="buf7">The seventh buffer to run the kernel with.</param>
        /// <param name="buf8">The eighth buffer to run the kernel with.</param>
        /// <param name="buf9">The ninth buffer to run the kernel with.</param>
        /// <param name="buf10">The tenth buffer to run the kernel with.</param>
        /// <param name="buf11">The eleventh buffer to run the kernel with.</param>
        /// <param name="buf12">The twelfth buffer to run the kernel with.</param>
        /// <param name="action">The kernel to run on the GPU.</param>
        /// <typeparam name="T">The base type of the first argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="U">The base type of the second argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="V">The base type of the third argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="W">The base type of the fourth argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="X">The base type of the fifth argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="Y">The base type of the sixth argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="Z">The base type of the seventh argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="A">The base type of the eighth argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="B">The base type of the ninth argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="C">The base type of the tenth argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="D">The base type of the eleventh argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="E">The base type of the twelfth argument. Must be an unmanaged type.</typeparam>
        public static void ParallelFor<T, U, V, W, X, Y, Z, A, B, C, D, E>(int start, int end, Buffer<T> buf1, Buffer<U> buf2, Buffer<V> buf3, Buffer<W> buf4, Buffer<X> buf5, Buffer<Y> buf6, Buffer<Z> buf7, Buffer<A> buf8, Buffer<B> buf9, Buffer<C> buf10, Buffer<D> buf11, Buffer<E> buf12, Action<Index, GPUArray<T>, GPUArray<U>, GPUArray<V>, GPUArray<W>, GPUArray<X>, GPUArray<Y>, GPUArray<Z>, GPUArray<A>, GPUArray<B>, GPUArray<C>, GPUArray<D>, GPUArray<E>> action)
            where T : unmanaged
            where U : unmanaged
            where V : unmanaged
            where W : unmanaged
            where X : unmanaged
            where Y : unmanaged
            where Z : unmanaged
            where A : unmanaged
            where B : unmanaged
            where C : unmanaged
            where D : unmanaged
            where E : unmanaged
        {
            var handler = new AcceleratorHandler();
            handler.DispatchKernel(start, end, buf1, buf2, buf3, buf4, buf5, buf6, buf7, buf8, buf9, buf10, buf11, buf12, action);
        }

        /// <summary>
        /// Creates a GPU parallel for loop.
        /// The body of the kernel is run on a GPU target.
        /// This overload specifies that thirteen arrays are used on the GPU.
        /// </summary>
        /// <param name="start">The start of the loop, inclusive.</param>
        /// <param name="end">The end of the loop, exclusive.</param>
        /// <param name="buf1">The first buffer to run the kernel with.</param>
        /// <param name="buf2">The second buffer to run the kernel with.</param>
        /// <param name="buf3">The third buffer to run the kernel with.</param>
        /// <param name="buf4">The fourth buffer to run the kernel with.</param>
        /// <param name="buf5">The fifth buffer to run the kernel with.</param>
        /// <param name="buf6">The sixth buffer to run the kernel with.</param>
        /// <param name="buf7">The seventh buffer to run the kernel with.</param>
        /// <param name="buf8">The eighth buffer to run the kernel with.</param>
        /// <param name="buf9">The ninth buffer to run the kernel with.</param>
        /// <param name="buf10">The tenth buffer to run the kernel with.</param>
        /// <param name="buf11">The eleventh buffer to run the kernel with.</param>
        /// <param name="buf12">The twelfth buffer to run the kernel with.</param>
        /// <param name="buf13">The thirteenth buffer to run the kernel with.</param>
        /// <param name="action">The kernel to run on the GPU.</param>
        /// <typeparam name="T">The base type of the first argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="U">The base type of the second argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="V">The base type of the third argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="W">The base type of the fourth argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="X">The base type of the fifth argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="Y">The base type of the sixth argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="Z">The base type of the seventh argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="A">The base type of the eighth argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="B">The base type of the ninth argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="C">The base type of the tenth argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="D">The base type of the eleventh argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="E">The base type of the twelfth argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="F">The base type of the thirteenth argument. Must be an unmanaged type.</typeparam>
        public static void ParallelFor<T, U, V, W, X, Y, Z, A, B, C, D, E, F>(int start, int end, Buffer<T> buf1, Buffer<U> buf2, Buffer<V> buf3, Buffer<W> buf4, Buffer<X> buf5, Buffer<Y> buf6, Buffer<Z> buf7, Buffer<A> buf8, Buffer<B> buf9, Buffer<C> buf10, Buffer<D> buf11, Buffer<E> buf12, Buffer<F> buf13, Action<Index, GPUArray<T>, GPUArray<U>, GPUArray<V>, GPUArray<W>, GPUArray<X>, GPUArray<Y>, GPUArray<Z>, GPUArray<A>, GPUArray<B>, GPUArray<C>, GPUArray<D>, GPUArray<E>, GPUArray<F>> action)
            where T : unmanaged
            where U : unmanaged
            where V : unmanaged
            where W : unmanaged
            where X : unmanaged
            where Y : unmanaged
            where Z : unmanaged
            where A : unmanaged
            where B : unmanaged
            where C : unmanaged
            where D : unmanaged
            where E : unmanaged
            where F : unmanaged
        {
            var handler = new AcceleratorHandler();
            handler.DispatchKernel(start, end, buf1, buf2, buf3, buf4, buf5, buf6, buf7, buf8, buf9, buf10, buf11, buf12, buf13, action);
        }

        /// <summary>
        /// Creates a GPU parallel for loop.
        /// The body of the kernel is run on a GPU target.
        /// This overload specifies that fourteen arrays are used on the GPU.
        /// </summary>
        /// <param name="start">The start of the loop, inclusive.</param>
        /// <param name="end">The end of the loop, exclusive.</param>
        /// <param name="buf1">The first buffer to run the kernel with.</param>
        /// <param name="buf2">The second buffer to run the kernel with.</param>
        /// <param name="buf3">The third buffer to run the kernel with.</param>
        /// <param name="buf4">The fourth buffer to run the kernel with.</param>
        /// <param name="buf5">The fifth buffer to run the kernel with.</param>
        /// <param name="buf6">The sixth buffer to run the kernel with.</param>
        /// <param name="buf7">The seventh buffer to run the kernel with.</param>
        /// <param name="buf8">The eighth buffer to run the kernel with.</param>
        /// <param name="buf9">The ninth buffer to run the kernel with.</param>
        /// <param name="buf10">The tenth buffer to run the kernel with.</param>
        /// <param name="buf11">The eleventh buffer to run the kernel with.</param>
        /// <param name="buf12">The twelfth buffer to run the kernel with.</param>
        /// <param name="buf13">The thirteenth buffer to run the kernel with.</param>
        /// <param name="buf14">The fourteenth buffer to run the kernel with.</param>
        /// <param name="action">The kernel to run on the GPU.</param>
        /// <typeparam name="T">The base type of the first argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="U">The base type of the second argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="V">The base type of the third argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="W">The base type of the fourth argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="X">The base type of the fifth argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="Y">The base type of the sixth argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="Z">The base type of the seventh argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="A">The base type of the eighth argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="B">The base type of the ninth argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="C">The base type of the tenth argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="D">The base type of the eleventh argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="E">The base type of the twelfth argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="F">The base type of the thirteenth argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="G">The base type of the fourteenth argument. Must be an unmanaged type.</typeparam>
        public static void ParallelFor<T, U, V, W, X, Y, Z, A, B, C, D, E, F, G>(int start, int end, Buffer<T> buf1, Buffer<U> buf2, Buffer<V> buf3, Buffer<W> buf4, Buffer<X> buf5, Buffer<Y> buf6, Buffer<Z> buf7, Buffer<A> buf8, Buffer<B> buf9, Buffer<C> buf10, Buffer<D> buf11, Buffer<E> buf12, Buffer<F> buf13, Buffer<G> buf14, Action<Index, GPUArray<T>, GPUArray<U>, GPUArray<V>, GPUArray<W>, GPUArray<X>, GPUArray<Y>, GPUArray<Z>, GPUArray<A>, GPUArray<B>, GPUArray<C>, GPUArray<D>, GPUArray<E>, GPUArray<F>, GPUArray<G>> action)
            where T : unmanaged
            where U : unmanaged
            where V : unmanaged
            where W : unmanaged
            where X : unmanaged
            where Y : unmanaged
            where Z : unmanaged
            where A : unmanaged
            where B : unmanaged
            where C : unmanaged
            where D : unmanaged
            where E : unmanaged
            where F : unmanaged
            where G : unmanaged
        {
            var handler = new AcceleratorHandler();
            handler.DispatchKernel(start, end, buf1, buf2, buf3, buf4, buf5, buf6, buf7, buf8, buf9, buf10, buf11, buf12, buf13, buf14, action);
        }

        /// <summary>
        /// Creates a GPU parallel for loop.
        /// The body of the kernel is run on a GPU target.
        /// This overload specifies that fifteen arrays are used on the GPU.
        /// </summary>
        /// <param name="start">The start of the loop, inclusive.</param>
        /// <param name="end">The end of the loop, exclusive.</param>
        /// <param name="buf1">The first buffer to run the kernel with.</param>
        /// <param name="buf2">The second buffer to run the kernel with.</param>
        /// <param name="buf3">The third buffer to run the kernel with.</param>
        /// <param name="buf4">The fourth buffer to run the kernel with.</param>
        /// <param name="buf5">The fifth buffer to run the kernel with.</param>
        /// <param name="buf6">The sixth buffer to run the kernel with.</param>
        /// <param name="buf7">The seventh buffer to run the kernel with.</param>
        /// <param name="buf8">The eighth buffer to run the kernel with.</param>
        /// <param name="buf9">The ninth buffer to run the kernel with.</param>
        /// <param name="buf10">The tenth buffer to run the kernel with.</param>
        /// <param name="buf11">The eleventh buffer to run the kernel with.</param>
        /// <param name="buf12">The twelfth buffer to run the kernel with.</param>
        /// <param name="buf13">The thirteenth buffer to run the kernel with.</param>
        /// <param name="buf14">The fourteenth buffer to run the kernel with.</param>
        /// <param name="buf15">The fifteenth buffer to run the kernel with.</param>
        /// <param name="action">The kernel to run on the GPU.</param>
        /// <typeparam name="T">The base type of the first argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="U">The base type of the second argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="V">The base type of the third argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="W">The base type of the fourth argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="X">The base type of the fifth argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="Y">The base type of the sixth argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="Z">The base type of the seventh argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="A">The base type of the eighth argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="B">The base type of the ninth argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="C">The base type of the tenth argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="D">The base type of the eleventh argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="E">The base type of the twelfth argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="F">The base type of the thirteenth argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="G">The base type of the fourteenth argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="H">The base type of the fifteenth argument. Must be an unmanaged type.</typeparam>
        public static void ParallelFor<T, U, V, W, X, Y, Z, A, B, C, D, E, F, G, H>(int start, int end, Buffer<T> buf1, Buffer<U> buf2, Buffer<V> buf3, Buffer<W> buf4, Buffer<X> buf5, Buffer<Y> buf6, Buffer<Z> buf7, Buffer<A> buf8, Buffer<B> buf9, Buffer<C> buf10, Buffer<D> buf11, Buffer<E> buf12, Buffer<F> buf13, Buffer<G> buf14, Buffer<H> buf15, Action<Index, GPUArray<T>, GPUArray<U>, GPUArray<V>, GPUArray<W>, GPUArray<X>, GPUArray<Y>, GPUArray<Z>, GPUArray<A>, GPUArray<B>, GPUArray<C>, GPUArray<D>, GPUArray<E>, GPUArray<F>, GPUArray<G>, GPUArray<H>> action)
            where T : unmanaged
            where U : unmanaged
            where V : unmanaged
            where W : unmanaged
            where X : unmanaged
            where Y : unmanaged
            where Z : unmanaged
            where A : unmanaged
            where B : unmanaged
            where C : unmanaged
            where D : unmanaged
            where E : unmanaged
            where F : unmanaged
            where G : unmanaged
            where H : unmanaged
        {
            var handler = new AcceleratorHandler();
            handler.DispatchKernel(start, end, buf1, buf2, buf3, buf4, buf5, buf6, buf7, buf8, buf9, buf10, buf11, buf12, buf13, buf14, buf15, action);
        }

        /// <summary>
        /// Creates a GPU parallel for loop.
        /// The body of the kernel is run on a GPU target.
        /// This overload specifies that sixteen arrays are used on the GPU.
        /// </summary>
        /// <param name="start">The start of the loop, inclusive.</param>
        /// <param name="end">The end of the loop, exclusive.</param>
        /// <param name="buf1">The first buffer to run the kernel with.</param>
        /// <param name="buf2">The second buffer to run the kernel with.</param>
        /// <param name="buf3">The third buffer to run the kernel with.</param>
        /// <param name="buf4">The fourth buffer to run the kernel with.</param>
        /// <param name="buf5">The fifth buffer to run the kernel with.</param>
        /// <param name="buf6">The sixth buffer to run the kernel with.</param>
        /// <param name="buf7">The seventh buffer to run the kernel with.</param>
        /// <param name="buf8">The eighth buffer to run the kernel with.</param>
        /// <param name="buf9">The ninth buffer to run the kernel with.</param>
        /// <param name="buf10">The tenth buffer to run the kernel with.</param>
        /// <param name="buf11">The eleventh buffer to run the kernel with.</param>
        /// <param name="buf12">The twelfth buffer to run the kernel with.</param>
        /// <param name="buf13">The thirteenth buffer to run the kernel with.</param>
        /// <param name="buf14">The fourteenth buffer to run the kernel with.</param>
        /// <param name="buf15">The fifteenth buffer to run the kernel with.</param>
        /// <param name="buf16">The sixteenth buffer to run the kernel with.</param>
        /// <param name="action">The kernel to run on the GPU.</param>
        /// <typeparam name="T">The base type of the first argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="U">The base type of the second argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="V">The base type of the third argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="W">The base type of the fourth argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="X">The base type of the fifth argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="Y">The base type of the sixth argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="Z">The base type of the seventh argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="A">The base type of the eighth argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="B">The base type of the ninth argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="C">The base type of the tenth argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="D">The base type of the eleventh argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="E">The base type of the twelfth argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="F">The base type of the thirteenth argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="G">The base type of the fourteenth argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="H">The base type of the fifteenth argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="I">The base type of the sixteenth argument. Must be an unmanaged type.</typeparam>
        public static void ParallelFor<T, U, V, W, X, Y, Z, A, B, C, D, E, F, G, H, I>(int start, int end, Buffer<T> buf1, Buffer<U> buf2, Buffer<V> buf3, Buffer<W> buf4, Buffer<X> buf5, Buffer<Y> buf6, Buffer<Z> buf7, Buffer<A> buf8, Buffer<B> buf9, Buffer<C> buf10, Buffer<D> buf11, Buffer<E> buf12, Buffer<F> buf13, Buffer<G> buf14, Buffer<H> buf15, Buffer<I> buf16, Action<Index, GPUArray<T>, GPUArray<U>, GPUArray<V>, GPUArray<W>, GPUArray<X>, GPUArray<Y>, GPUArray<Z>, GPUArray<A>, GPUArray<B>, GPUArray<C>, GPUArray<D>, GPUArray<E>, GPUArray<F>, GPUArray<G>, GPUArray<H>, GPUArray<I>> action)
            where T : unmanaged
            where U : unmanaged
            where V : unmanaged
            where W : unmanaged
            where X : unmanaged
            where Y : unmanaged
            where Z : unmanaged
            where A : unmanaged
            where B : unmanaged
            where C : unmanaged
            where D : unmanaged
            where E : unmanaged
            where F : unmanaged
            where G : unmanaged
            where H : unmanaged
            where I : unmanaged
        {
            var handler = new AcceleratorHandler();
            handler.DispatchKernel(start, end, buf1, buf2, buf3, buf4, buf5, buf6, buf7, buf8, buf9, buf10, buf11, buf12, buf13, buf14, buf15, buf16, action);
        }
    }
}