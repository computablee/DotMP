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
            var selectedDevice = context.Devices[0];

            foreach (var d in context.Devices)
            {
                Console.WriteLine("Detected {0} accelerator.", d.ToString());

                if (selectedDevice.AcceleratorType == AcceleratorType.CPU && d.AcceleratorType == AcceleratorType.OpenCL)
                    selectedDevice = d;
                if (selectedDevice.AcceleratorType != AcceleratorType.Cuda && d.AcceleratorType == AcceleratorType.Cuda)
                    selectedDevice = d;
            }

            accelerator = selectedDevice.CreateAccelerator(context);

            Console.WriteLine("Using {0} accelerator.", accelerator.AcceleratorType.ToString());
            initialized = true;
            block_size = accelerator.AcceleratorType == AcceleratorType.CPU ? 16 : 256;
        }

        /// <summary>
        /// Synchronize pending operations.
        /// </summary>
        private void Synchronize() => accelerator.Synchronize();

        /// <summary>
        /// Dispatches a kernel with one parameter.
        /// </summary>
        /// <param name="start">The start of the loop, inclusive.</param>
        /// <param name="end">The end of the loop, exclusive.</param>
        /// <param name="buf">The buffer to run the kernel with.</param>
        /// <param name="action">The kernel to run on the GPU.</param>
        /// <typeparam name="T">The base type of the first argument. Must be an unmanaged type.</typeparam>
        internal void DispatchKernel<T>(int start, int end, Buffer<T> buf, Action<Index, GPUArray<T>> action)
            where T : unmanaged
        {
            var idx = new Index(start);

            var kernel = accelerator.LoadStreamKernel(action);

            kernel(((end - start) / block_size, block_size), idx,
                new GPUArray<T>(buf));

            Synchronize();
        }

        /// <summary>
        /// Dispatches a kernel with two parameters.
        /// </summary>
        /// <param name="start">The start of the loop, inclusive.</param>
        /// <param name="end">The end of the loop, exclusive.</param>
        /// <param name="buf1">The first buffer to run the kernel with.</param>
        /// <param name="buf2">The second buffer to run the kernel with.</param>
        /// <param name="action">The kernel to run on the GPU.</param>
        /// <typeparam name="T">The base type of the first argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="U">The base type of the second argument. Must be an unmanaged type.</typeparam>
        internal void DispatchKernel<T, U>(int start, int end, Buffer<T> buf1, Buffer<U> buf2, Action<Index, GPUArray<T>, GPUArray<U>> action)
            where T : unmanaged
            where U : unmanaged
        {
            var idx = new Index(start);

            var kernel = accelerator.LoadStreamKernel(action);

            kernel(((end - start) / block_size, block_size), idx,
                new GPUArray<T>(buf1),
                new GPUArray<U>(buf2));

            Synchronize();
        }

        /// <summary>
        /// Dispatches a kernel with three parameters.
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
        internal void DispatchKernel<T, U, V>(int start, int end, Buffer<T> buf1, Buffer<U> buf2, Buffer<V> buf3, Action<Index, GPUArray<T>, GPUArray<U>, GPUArray<V>> action)
            where T : unmanaged
            where U : unmanaged
            where V : unmanaged
        {
            var idx = new Index(start);

            var kernel = accelerator.LoadStreamKernel(action);

            kernel(((end - start) / block_size, block_size), idx,
                new GPUArray<T>(buf1),
                new GPUArray<U>(buf2),
                new GPUArray<V>(buf3));

            Synchronize();
        }

        /// <summary>
        /// Dispatches a kernel with four parameters.
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
        internal void DispatchKernel<T, U, V, W>(int start, int end, Buffer<T> buf1, Buffer<U> buf2, Buffer<V> buf3, Buffer<W> buf4, Action<Index, GPUArray<T>, GPUArray<U>, GPUArray<V>, GPUArray<W>> action)
            where T : unmanaged
            where U : unmanaged
            where V : unmanaged
            where W : unmanaged
        {
            var idx = new Index(start);

            var kernel = accelerator.LoadStreamKernel(action);

            kernel(((end - start) / block_size, block_size), idx,
                new GPUArray<T>(buf1),
                new GPUArray<U>(buf2),
                new GPUArray<V>(buf3),
                new GPUArray<W>(buf4));

            Synchronize();
        }

        /// <summary>
        /// Dispatches a kernel with five parameters.
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
        internal void DispatchKernel<T, U, V, W, X>(int start, int end, Buffer<T> buf1, Buffer<U> buf2, Buffer<V> buf3, Buffer<W> buf4, Buffer<X> buf5, Action<Index, GPUArray<T>, GPUArray<U>, GPUArray<V>, GPUArray<W>, GPUArray<X>> action)
            where T : unmanaged
            where U : unmanaged
            where V : unmanaged
            where W : unmanaged
            where X : unmanaged
        {
            var idx = new Index(start);

            var kernel = accelerator.LoadStreamKernel(action);

            kernel(((end - start) / block_size, block_size), idx,
                new GPUArray<T>(buf1),
                new GPUArray<U>(buf2),
                new GPUArray<V>(buf3),
                new GPUArray<W>(buf4),
                new GPUArray<X>(buf5));

            Synchronize();
        }

        /// <summary>
        /// Dispatches a kernel with six parameters.
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
        internal void DispatchKernel<T, U, V, W, X, Y>(int start, int end, Buffer<T> buf1, Buffer<U> buf2, Buffer<V> buf3, Buffer<W> buf4, Buffer<X> buf5, Buffer<Y> buf6, Action<Index, GPUArray<T>, GPUArray<U>, GPUArray<V>, GPUArray<W>, GPUArray<X>, GPUArray<Y>> action)
            where T : unmanaged
            where U : unmanaged
            where V : unmanaged
            where W : unmanaged
            where X : unmanaged
            where Y : unmanaged
        {
            var idx = new Index(start);

            var kernel = accelerator.LoadStreamKernel(action);

            kernel(((end - start) / block_size, block_size), idx,
                new GPUArray<T>(buf1),
                new GPUArray<U>(buf2),
                new GPUArray<V>(buf3),
                new GPUArray<W>(buf4),
                new GPUArray<X>(buf5),
                new GPUArray<Y>(buf6));

            Synchronize();
        }

        /// <summary>
        /// Dispatches a kernel with seven parameters.
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
        internal void DispatchKernel<T, U, V, W, X, Y, Z>(int start, int end, Buffer<T> buf1, Buffer<U> buf2, Buffer<V> buf3, Buffer<W> buf4, Buffer<X> buf5, Buffer<Y> buf6, Buffer<Z> buf7, Action<Index, GPUArray<T>, GPUArray<U>, GPUArray<V>, GPUArray<W>, GPUArray<X>, GPUArray<Y>, GPUArray<Z>> action)
            where T : unmanaged
            where U : unmanaged
            where V : unmanaged
            where W : unmanaged
            where X : unmanaged
            where Y : unmanaged
            where Z : unmanaged
        {
            var idx = new Index(start);

            var kernel = accelerator.LoadStreamKernel(action);

            kernel(((end - start) / block_size, block_size), idx,
                new GPUArray<T>(buf1),
                new GPUArray<U>(buf2),
                new GPUArray<V>(buf3),
                new GPUArray<W>(buf4),
                new GPUArray<X>(buf5),
                new GPUArray<Y>(buf6),
                new GPUArray<Z>(buf7));

            Synchronize();
        }

        /// <summary>
        /// Dispatches a kernel with eight parameters.
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
        internal void DispatchKernel<T, U, V, W, X, Y, Z, A>(int start, int end, Buffer<T> buf1, Buffer<U> buf2, Buffer<V> buf3, Buffer<W> buf4, Buffer<X> buf5, Buffer<Y> buf6, Buffer<Z> buf7, Buffer<A> buf8, Action<Index, GPUArray<T>, GPUArray<U>, GPUArray<V>, GPUArray<W>, GPUArray<X>, GPUArray<Y>, GPUArray<Z>, GPUArray<A>> action)
            where T : unmanaged
            where U : unmanaged
            where V : unmanaged
            where W : unmanaged
            where X : unmanaged
            where Y : unmanaged
            where Z : unmanaged
            where A : unmanaged
        {
            var idx = new Index(start);

            var kernel = accelerator.LoadStreamKernel(action);

            kernel(((end - start) / block_size, block_size), idx,
                new GPUArray<T>(buf1),
                new GPUArray<U>(buf2),
                new GPUArray<V>(buf3),
                new GPUArray<W>(buf4),
                new GPUArray<X>(buf5),
                new GPUArray<Y>(buf6),
                new GPUArray<Z>(buf7),
                new GPUArray<A>(buf8));

            Synchronize();
        }

        /// <summary>
        /// Dispatches a kernel with nine parameters.
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
        internal void DispatchKernel<T, U, V, W, X, Y, Z, A, B>(int start, int end, Buffer<T> buf1, Buffer<U> buf2, Buffer<V> buf3, Buffer<W> buf4, Buffer<X> buf5, Buffer<Y> buf6, Buffer<Z> buf7, Buffer<A> buf8, Buffer<B> buf9, Action<Index, GPUArray<T>, GPUArray<U>, GPUArray<V>, GPUArray<W>, GPUArray<X>, GPUArray<Y>, GPUArray<Z>, GPUArray<A>, GPUArray<B>> action)
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
            var idx = new Index(start);

            var kernel = accelerator.LoadStreamKernel(action);

            kernel(((end - start) / block_size, block_size), idx,
                new GPUArray<T>(buf1),
                new GPUArray<U>(buf2),
                new GPUArray<V>(buf3),
                new GPUArray<W>(buf4),
                new GPUArray<X>(buf5),
                new GPUArray<Y>(buf6),
                new GPUArray<Z>(buf7),
                new GPUArray<A>(buf8),
                new GPUArray<B>(buf9));

            Synchronize();
        }

        /// <summary>
        /// Dispatches a kernel with ten parameters.
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
        internal void DispatchKernel<T, U, V, W, X, Y, Z, A, B, C>(int start, int end, Buffer<T> buf1, Buffer<U> buf2, Buffer<V> buf3, Buffer<W> buf4, Buffer<X> buf5, Buffer<Y> buf6, Buffer<Z> buf7, Buffer<A> buf8, Buffer<B> buf9, Buffer<C> buf10, Action<Index, GPUArray<T>, GPUArray<U>, GPUArray<V>, GPUArray<W>, GPUArray<X>, GPUArray<Y>, GPUArray<Z>, GPUArray<A>, GPUArray<B>, GPUArray<C>> action)
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
            var idx = new Index(start);

            var kernel = accelerator.LoadStreamKernel(action);

            kernel(((end - start) / block_size, block_size), idx,
                new GPUArray<T>(buf1),
                new GPUArray<U>(buf2),
                new GPUArray<V>(buf3),
                new GPUArray<W>(buf4),
                new GPUArray<X>(buf5),
                new GPUArray<Y>(buf6),
                new GPUArray<Z>(buf7),
                new GPUArray<A>(buf8),
                new GPUArray<B>(buf9),
                new GPUArray<C>(buf10));

            Synchronize();
        }

        /// <summary>
        /// Dispatches a kernel with eleven parameters.
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
        internal void DispatchKernel<T, U, V, W, X, Y, Z, A, B, C, D>(int start, int end, Buffer<T> buf1, Buffer<U> buf2, Buffer<V> buf3, Buffer<W> buf4, Buffer<X> buf5, Buffer<Y> buf6, Buffer<Z> buf7, Buffer<A> buf8, Buffer<B> buf9, Buffer<C> buf10, Buffer<D> buf11, Action<Index, GPUArray<T>, GPUArray<U>, GPUArray<V>, GPUArray<W>, GPUArray<X>, GPUArray<Y>, GPUArray<Z>, GPUArray<A>, GPUArray<B>, GPUArray<C>, GPUArray<D>> action)
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
            var idx = new Index(start);

            var kernel = accelerator.LoadStreamKernel(action);

            kernel(((end - start) / block_size, block_size), idx,
                new GPUArray<T>(buf1),
                new GPUArray<U>(buf2),
                new GPUArray<V>(buf3),
                new GPUArray<W>(buf4),
                new GPUArray<X>(buf5),
                new GPUArray<Y>(buf6),
                new GPUArray<Z>(buf7),
                new GPUArray<A>(buf8),
                new GPUArray<B>(buf9),
                new GPUArray<C>(buf10),
                new GPUArray<D>(buf11));

            Synchronize();
        }

        /// <summary>
        /// Dispatches a kernel with twelve parameters.
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
        internal void DispatchKernel<T, U, V, W, X, Y, Z, A, B, C, D, E>(int start, int end, Buffer<T> buf1, Buffer<U> buf2, Buffer<V> buf3, Buffer<W> buf4, Buffer<X> buf5, Buffer<Y> buf6, Buffer<Z> buf7, Buffer<A> buf8, Buffer<B> buf9, Buffer<C> buf10, Buffer<D> buf11, Buffer<E> buf12, Action<Index, GPUArray<T>, GPUArray<U>, GPUArray<V>, GPUArray<W>, GPUArray<X>, GPUArray<Y>, GPUArray<Z>, GPUArray<A>, GPUArray<B>, GPUArray<C>, GPUArray<D>, GPUArray<E>> action)
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
            var idx = new Index(start);

            var kernel = accelerator.LoadStreamKernel(action);

            kernel(((end - start) / block_size, block_size), idx,
                new GPUArray<T>(buf1),
                new GPUArray<U>(buf2),
                new GPUArray<V>(buf3),
                new GPUArray<W>(buf4),
                new GPUArray<X>(buf5),
                new GPUArray<Y>(buf6),
                new GPUArray<Z>(buf7),
                new GPUArray<A>(buf8),
                new GPUArray<B>(buf9),
                new GPUArray<C>(buf10),
                new GPUArray<D>(buf11),
                new GPUArray<E>(buf12));

            Synchronize();
        }

        /// <summary>
        /// Dispatches a kernel with thirteen parameters.
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
        internal void DispatchKernel<T, U, V, W, X, Y, Z, A, B, C, D, E, F>(int start, int end, Buffer<T> buf1, Buffer<U> buf2, Buffer<V> buf3, Buffer<W> buf4, Buffer<X> buf5, Buffer<Y> buf6, Buffer<Z> buf7, Buffer<A> buf8, Buffer<B> buf9, Buffer<C> buf10, Buffer<D> buf11, Buffer<E> buf12, Buffer<F> buf13, Action<Index, GPUArray<T>, GPUArray<U>, GPUArray<V>, GPUArray<W>, GPUArray<X>, GPUArray<Y>, GPUArray<Z>, GPUArray<A>, GPUArray<B>, GPUArray<C>, GPUArray<D>, GPUArray<E>, GPUArray<F>> action)
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
            var idx = new Index(start);

            var kernel = accelerator.LoadStreamKernel(action);

            kernel(((end - start) / block_size, block_size), idx,
                new GPUArray<T>(buf1),
                new GPUArray<U>(buf2),
                new GPUArray<V>(buf3),
                new GPUArray<W>(buf4),
                new GPUArray<X>(buf5),
                new GPUArray<Y>(buf6),
                new GPUArray<Z>(buf7),
                new GPUArray<A>(buf8),
                new GPUArray<B>(buf9),
                new GPUArray<C>(buf10),
                new GPUArray<D>(buf11),
                new GPUArray<E>(buf12),
                new GPUArray<F>(buf13));

            Synchronize();
        }
    }
}