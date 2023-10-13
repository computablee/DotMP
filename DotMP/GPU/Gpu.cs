using System;

namespace DotMP
{
    /// <summary>
    /// Specifies a kernel with one data parameter.
    /// </summary>
    /// <typeparam name="T">The base type of the first argument. Must be an unmanaged type.</typeparam>
    /// <param name="d">The device handle.</param>
    /// <param name="o1">The first argument. Must be an array.</param>
    public delegate void ActionGPU<T>(DeviceHandle d, GPUArray<T> o1)
        where T : unmanaged;
    /// <summary>
    /// Specifies a kernel with one data parameter.
    /// </summary>
    /// <typeparam name="T">The base type of the first argument. Must be an unmanaged type.</typeparam>
    /// <typeparam name="U">The base type of the second argument. Must be an unmanaged type.</typeparam>
    /// <param name="d">The device handle.</param>
    /// <param name="o1">The first argument. Must be an array.</param>
    /// <param name="o2">The second argument. Must be an array.</param>
    public delegate void ActionGPU<T, U>(DeviceHandle d, GPUArray<T> o1, GPUArray<U> o2)
        where T : unmanaged
        where U : unmanaged;
    /// <summary>
    /// Specifies a kernel with one data parameter.
    /// </summary>
    /// <typeparam name="T">The base type of the first argument. Must be an unmanaged type.</typeparam>
    /// <typeparam name="U">The base type of the second argument. Must be an unmanaged type.</typeparam>
    /// <typeparam name="V">The base type of the third argument. Must be an unmanaged type.</typeparam>
    /// <param name="d">The device handle.</param>
    /// <param name="o1">The first argument. Must be an array.</param>
    /// <param name="o2">The second argument. Must be an array.</param>
    /// <param name="o3">The third argument. Must be an array.</param>
    public delegate void ActionGPU<T, U, V>(DeviceHandle d, GPUArray<T> o1, GPUArray<U> o2, GPUArray<V> o3)
        where T : unmanaged
        where U : unmanaged
        where V : unmanaged;
    /// <summary>
    /// Specifies a kernel with one data parameter.
    /// </summary>
    /// <typeparam name="T">The base type of the first argument. Must be an unmanaged type.</typeparam>
    /// <typeparam name="U">The base type of the second argument. Must be an unmanaged type.</typeparam>
    /// <typeparam name="V">The base type of the third argument. Must be an unmanaged type.</typeparam>
    /// <typeparam name="W">The base type of the fourth argument. Must be an unmanaged type.</typeparam>
    /// <param name="d">The device handle.</param>
    /// <param name="o1">The first argument. Must be an array.</param>
    /// <param name="o2">The second argument. Must be an array.</param>
    /// <param name="o3">The third argument. Must be an array.</param>
    /// <param name="o4">The fourth argument. Must be an array.</param>
    public delegate void ActionGPU<T, U, V, W>(DeviceHandle d, GPUArray<T> o1, GPUArray<U> o2, GPUArray<V> o3, GPUArray<W> o4)
        where T : unmanaged
        where U : unmanaged
        where V : unmanaged
        where W : unmanaged;
    /// <summary>
    /// The main class of DotMP's GPU API, powered by the ILGPU project.
    /// Contains all the main methods for constructing and running GPU kernels.
    /// The GPU API is not thread-safe at the current moment, so its methods should not be called from within a Parallel.ParallelRegion!
    /// </summary>
    public static class GPU
    {

        /// <summary>
        /// Creates a GPU kernel.
        /// The body of the kernel is run on a GPU target.
        /// This overload specifies that one array is used on the GPU.
        /// </summary>
        /// <param name="action">The kernel to run on the GPU.</param>
        /// <typeparam name="T">The base type of the first argument. Must be an unmanaged type.</typeparam>
        public static void Kernel<T>(ActionGPU<T> action)
            where T : unmanaged
        {
            var handler = new AcceleratorHandler();
            handler.DispatchKernel(action);
        }

        /// <summary>
        /// Creates a GPU kernel.
        /// The body of the kernel is run on a GPU target.
        /// This overload specifies that two arrays are used on the GPU.
        /// </summary>
        /// <param name="action">The kernel to run on the GPU.</param>
        /// <typeparam name="T">The base type of the first argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="U">The base type of the second argument. Must be an unmanaged type.</typeparam>
        public static void Kernel<T, U>(ActionGPU<T, U> action)
            where T : unmanaged
            where U : unmanaged
        {
            var handler = new AcceleratorHandler();
            handler.DispatchKernel(action);
        }

        /// <summary>
        /// Creates a GPU kernel.
        /// The body of the kernel is run on a GPU target.
        /// This overload specifies that three arrays are used on the GPU.
        /// </summary>
        /// <param name="action">The kernel to run on the GPU.</param>
        /// <typeparam name="T">The base type of the first argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="U">The base type of the second argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="V">The base type of the third argument. Must be an unmanaged type.</typeparam>
        public static void Kernel<T, U, V>(ActionGPU<T, U, V> action)
            where T : unmanaged
            where U : unmanaged
            where V : unmanaged
        {
            var handler = new AcceleratorHandler();
            handler.DispatchKernel(action);
        }

        /// <summary>
        /// Creates a GPU kernel.
        /// The body of the kernel is run on a GPU target.
        /// This overload specifies that four arrays are used on the GPU.
        /// </summary>
        /// <param name="action">The kernel to run on the GPU.</param>
        /// <typeparam name="T">The base type of the first argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="U">The base type of the second argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="V">The base type of the third argument. Must be an unmanaged type.</typeparam>
        /// <typeparam name="W">The base type of the fourth argument. Must be an unmanaged type.</typeparam>
        public static void Kernel<T, U, V, W>(ActionGPU<T, U, V, W> action)
            where T : unmanaged
            where U : unmanaged
            where V : unmanaged
            where W : unmanaged
        {
            var handler = new AcceleratorHandler();
            handler.DispatchKernel(action);
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

        /// <summary>
        /// Creates a GPU parallel for loop.
        /// A for loop created with For inside of a GPU kernel is executed in parallel, with iterations being distributed among the offload target, and potentially out-of-order.
        /// Unlike the CPU API, there are no parameters for setting a schedule or chunk size.
        /// </summary>
        /// <param name="d">The device handle.</param>
        /// <param name="start">The start of the for loop, inclusive.</param>
        /// <param name="end">The end of the for loop, exclusive.</param>
        /// <param name="action">The action to be performed in the loop.</param>
        public static void For(DeviceHandle d, int start, int end, Action<int> action)
        {
            for (int idx = d.GetIndex().X + start; idx < end; idx += d.GetIndex().Size)
                action(idx);
        }
    }
}