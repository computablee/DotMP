using System;
using ILGPU;

namespace DotMP
{
    /// <summary>
    /// The main class of DotMP's GPU API, powered by the ILGPU project.
    /// Contains all the main methods for constructing and running GPU kernels.
    /// The GPU API is not thread-safe at the current moment, so its methods should not be called from within a Parallel.ParallelRegion!
    /// </summary>
    public static class GPU
    {
        /// <summary>
        /// Creates a GPU parallel for loop.
        /// The body of the kernel is run on a GPU target.
        /// This overload specifies that one array is used on the GPU.
        /// </summary>
        /// <param name="start">The start of the loop, inclusive.</param>
        /// <param name="end">The end of the loop, exclusive.</param>
        /// <param name="action">The kernel to run on the GPU.</param>
        /// <typeparam name="T">The base type of the first argument. Must be an unmanaged type.</typeparam>
        public static void ParallelFor<T>(int start, int end, ActionGPU<T> action)
            where T : unmanaged
        {
            var handler = new AcceleratorHandler();
            handler.DispatchKernel(start, end, action);
        }

        /// <summary>
        /// Creates a GPU parallel for loop.
        /// The body of the kernel is run on a GPU target.
        /// This overload specifies that two arrays are used on the GPU.
        /// </summary>
        /// <param name="start">The start of the loop, inclusive.</param>
        /// <param name="end">The end of the loop, exclusive.</param>
        /// <param name="action">The kernel to run on the GPU.</param>
        /// <typeparam name="T">The base type of the first argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="U">The base type of the second argument. Must be an unmanaged type.</typeparam>
        public static void ParallelFor<T, U>(int start, int end, ActionGPU<T, U> action)
            where T : unmanaged
            where U : unmanaged
        {
            var handler = new AcceleratorHandler();
            handler.DispatchKernel(start, end, action);
        }

        /// <summary>
        /// Creates a GPU parallel for loop.
        /// The body of the kernel is run on a GPU target.
        /// This overload specifies that three arrays are used on the GPU.
        /// </summary>
        /// <param name="start">The start of the loop, inclusive.</param>
        /// <param name="end">The end of the loop, exclusive.</param>
        /// <param name="action">The kernel to run on the GPU.</param>
        /// <typeparam name="T">The base type of the first argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="U">The base type of the second argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="V">The base type of the third argument. Must be an unmanaged type.</typeparam>
        public static void ParallelFor<T, U, V>(int start, int end, ActionGPU<T, U, V> action)
            where T : unmanaged
            where U : unmanaged
            where V : unmanaged
        {
            var handler = new AcceleratorHandler();
            handler.DispatchKernel(start, end, action);
        }

        /// <summary>
        /// Creates a GPU parallel for loop.
        /// The body of the kernel is run on a GPU target.
        /// This overload specifies that four arrays are used on the GPU.
        /// </summary>
        /// <param name="start">The start of the loop, inclusive.</param>
        /// <param name="end">The end of the loop, exclusive.</param>
        /// <param name="action">The kernel to run on the GPU.</param>
        /// <typeparam name="T">The base type of the first argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="U">The base type of the second argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="V">The base type of the third argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="W">The base type of the fourth argument. Must be an unmanaged type.</typeparam>
        public static void ParallelFor<T, U, V, W>(int start, int end, ActionGPU<T, U, V, W> action)
            where T : unmanaged
            where U : unmanaged
            where V : unmanaged
            where W : unmanaged
        {
            var handler = new AcceleratorHandler();
            handler.DispatchKernel(start, end, action);
        }

        /// <summary>
        /// Specifies data movement to the GPU at the start of the kernel, but not back to the CPU at the end of the kernel.
        /// Can be called multiple times with different datatypes, but is cleared after a call to Kernel().
        /// </summary>
        /// <typeparam name="T">The base type of the data. Must be an unmanaged type.</typeparam>
        /// <param name="to_data">The data to move to the GPU.</param>
        public static void DataTo<T>(params T[][] to_data)
            where T : unmanaged
        {
            var handler = new AcceleratorHandler();
            handler.AllocateTo(to_data);
        }

        /// <summary>
        /// Specifies data movement back to the CPU at the end of the kernel, but not to the GPU at the start of the kernel..
        /// Can be called multiple times with different datatypes, but is cleared after a call to Kernel().
        /// </summary>
        /// <typeparam name="T">The base type of the data. Must be an unmanaged type.</typeparam>
        /// <param name="to_data">The data to move from the GPU.</param>
        public static void DataFrom<T>(params T[][] to_data)
            where T : unmanaged
        {
            var handler = new AcceleratorHandler();
            handler.AllocateFrom(to_data);
        }

        /// <summary>
        /// Specifies data movement to the GPU at the start of the kernel, and from the GPU back to the CPU at the end of the kernel.
        /// Can be called multiple times with different datatypes, but is cleared after a call to Kernel().
        /// </summary>
        /// <typeparam name="T">The base type of the data. Must be an unmanaged type.</typeparam>
        /// <param name="to_data">The data to move to and from the GPU.</param>
        public static void DataToFrom<T>(params T[][] to_data)
            where T : unmanaged
        {
            var handler = new AcceleratorHandler();
            handler.AllocateToFrom(to_data);
        }
    }
}