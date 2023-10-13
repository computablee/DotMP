using System;
using System.Linq;
using ILGPU;
using ILGPU.Runtime;

namespace DotMP
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
        private static Accelerator accelerator;
        /// <summary>
        /// The arrays going to the GPU.
        /// </summary>
        private static dynamic[] tos;
        /// <summary>
        /// The arrays coming back from the GPU.
        /// </summary>
        private static dynamic[] froms;
        /// <summary>
        /// The arrays going both to and from the GPU.
        /// </summary>
        private static dynamic[] tofroms;

        /// <summary>
        /// Default constructor. If this is the first time it's called, it initializes all relevant singleton data.
        /// </summary>
        internal AcceleratorHandler()
        {
            if (initialized) return;

            context = Context.CreateDefault();
            accelerator = context.Devices[0].CreateAccelerator(context);
            initialized = true;

            tos = new dynamic[0];
            froms = new dynamic[0];
            tofroms = new dynamic[0];
        }

        /// <summary>
        /// Aggregates the parameters into a single array.
        /// </summary>
        /// <returns>A dynamic array of all parameters.</returns>
        private dynamic[] AggregateParams()
        {
            object[] parameters = new object[0];
            parameters.Append(tos).Append(froms).Append(tofroms);

            return parameters;
        }

        /// <summary>
        /// Calculates the longest parameter to determine the number of GPU threads.
        /// </summary>
        /// <returns>The length of the longest parameter.</returns>
        private int LongestParam()
        {
            int max = int.MinValue;

            foreach (var t in tos)
                if (t.IntExtent > max)
                    max = t.IntExtent;

            foreach (var t in froms)
                if (t.IntExtent > max)
                    max = t.IntExtent;

            foreach (var t in tofroms)
                if (t.IntExtent > max)
                    max = t.IntExtent;

            return max;
        }

        /// <summary>
        /// Called if data should be moved to the device.
        /// Allocates data on GPU and copies the data from the CPU.
        /// </summary>
        /// <typeparam name="T">The type of data to allocate.</typeparam>
        /// <param name="values">The data to allocate.</param>
        internal void AllocateTo<T>(T[][] values)
            where T : unmanaged
        {
            var tos = values.Select(v => accelerator.Allocate1D(v)).ToArray();
            AcceleratorHandler.tos.Append(tos);

            for (int i = 0; i < tos.Length; i++)
                tos[i].CopyFromCPU(values[i]);
        }

        /// <summary>
        /// Called if data should be moved from the device.
        /// Allocates data on GPU.
        /// </summary>
        /// <typeparam name="T">The type of data to allocate.</typeparam>
        /// <param name="values">The data to allocate.</param>
        internal void AllocateFrom<T>(T[][] values)
            where T : unmanaged
        {
            var froms = values.Select(v => accelerator.Allocate1D<T>(v.Length)).ToArray();
            AcceleratorHandler.froms.Append(froms);
        }

        /// <summary>
        /// Called if data should be moved to and from the device.
        /// Allocates data on GPU and copies the data from the CPU.
        /// </summary>
        /// <typeparam name="T">The type of data to allocate.</typeparam>
        /// <param name="values">The data to allocate.</param>
        internal void AllocateToFrom<T>(T[][] values)
            where T : unmanaged
        {
            var tofroms = values.Select(v => accelerator.Allocate1D(v)).ToArray();
            AcceleratorHandler.tofroms.Append(tofroms);

            for (int i = 0; i < tos.Length; i++)
                tofroms[i].CopyFromCPU(values[i]);
        }

        /// <summary>
        /// Dispatches a kernel with one data parameter.
        /// </summary>
        /// <typeparam name="T">The type of the data parameter.</typeparam>
        /// <param name="action">The action to perform.</param>
        internal void DispatchKernel<T>(ActionGPU<T> action)
            where T : unmanaged
        {
            Action<Index1D, ArrayView<T>> disp_action = (index, i) =>
            {
                DeviceHandle deviceHandle = new DeviceHandle(index);
                action(deviceHandle, i);
            };

            var kernel = accelerator.LoadAutoGroupedStreamKernel(disp_action);
            dynamic[] parameters = AggregateParams();

            kernel(LongestParam(), parameters[0]);
        }

        /// <summary>
        /// Dispatches a kernel with two data parameters.
        /// </summary>
        /// <typeparam name="T">The type of the first data parameter.</typeparam>
        /// <typeparam name="U">The type of the second data parameter.</typeparam>
        /// <param name="action">The action to perform.</param>
        internal void DispatchKernel<T, U>(ActionGPU<T, U> action)
            where T : unmanaged
            where U : unmanaged
        {
            Action<Index1D, ArrayView<T>, ArrayView<U>> disp_action = (index, i, j) =>
            {
                DeviceHandle deviceHandle = new DeviceHandle(index);
                action(deviceHandle, i, j);
            };

            var kernel = accelerator.LoadAutoGroupedStreamKernel(disp_action);
            dynamic[] parameters = AggregateParams();

            kernel(LongestParam(), parameters[0], parameters[1]);
        }

        /// <summary>
        /// Dispatches a kernel with three data parameters.
        /// </summary>
        /// <typeparam name="T">The type of the first data parameter.</typeparam>
        /// <typeparam name="U">The type of the second data parameter.</typeparam>
        /// <typeparam name="V">The type of the third data parameter.</typeparam>
        /// <param name="action">The action to perform.</param>
        internal void DispatchKernel<T, U, V>(ActionGPU<T, U, V> action)
            where T : unmanaged
            where U : unmanaged
            where V : unmanaged
        {
            Action<Index1D, ArrayView<T>, ArrayView<U>, ArrayView<V>> disp_action = (index, i, j, k) =>
            {
                DeviceHandle deviceHandle = new DeviceHandle(index);
                action(deviceHandle, i, j, k);
            };

            var kernel = accelerator.LoadAutoGroupedStreamKernel(disp_action);
            dynamic[] parameters = AggregateParams();

            kernel(LongestParam(), parameters[0], parameters[1], parameters[2]);
        }

        /// <summary>
        /// The type of the first data parameter.
        /// </summary>
        /// <typeparam name="T">The type of the first data parameter.</typeparam>
        /// <typeparam name="U">The type of the second data parameter.</typeparam>
        /// <typeparam name="V">The type of the third data parameter.</typeparam>
        /// <typeparam name="W">The type of the fourth data parameter.</typeparam>
        /// <param name="action">The action to perform.</param>
        internal void DispatchKernel<T, U, V, W>(ActionGPU<T, U, V, W> action)
            where T : unmanaged
            where U : unmanaged
            where V : unmanaged
            where W : unmanaged
        {
            Action<Index1D, ArrayView<T>, ArrayView<U>, ArrayView<V>, ArrayView<W>> disp_action = (index, i, j, k, l) =>
            {
                DeviceHandle deviceHandle = new DeviceHandle(index);
                action(deviceHandle, i, j, k, l);
            };

            var kernel = accelerator.LoadAutoGroupedStreamKernel(disp_action);
            dynamic[] parameters = AggregateParams();

            kernel(LongestParam(), parameters[0], parameters[1], parameters[2], parameters[3]);
        }
    }
}