using System;
using System.Collections.Generic;
using ILGPU;
using ILGPU.Runtime;

namespace DotMP.GPU
{
    /// <summary>
    /// Handles the code for performing reductions.
    /// </summary>
    internal static class ReductionKernels
    {
        /// <summary>
        /// The reduce action after being translated to the GPU.
        /// </summary>
        private static Dictionary<Type, Delegate> reduce_action = new Dictionary<Type, Delegate>();

        /// <summary>
        /// Gets the greatest power of 2 less than or equal to x.
        /// </summary>
        /// <param name="x">The value to compute from.</param>
        /// <returns>The computed value.</returns>
        internal static int Power2RoundDown(int x)
        {
            x |= x >> 1;
            x |= x >> 2;
            x |= x >> 4;
            x |= x >> 8;
            x |= x >> 16;
            return x ^ (x >> 1);
        }

        /// <summary>
        /// The reduction kernel to run on the GPU.
        /// </summary>
        /// <param name="arr">The array of values to reduce over.</param>
        /// <param name="idx">The Index variable representing a thread's global index.</param>
        /// <param name="len">The length of the array.</param>
        private static void Reduce<T>(ArrayView1D<T, Stride1D.Dense> arr, Index idx, int len)
            where T : unmanaged
        {
            int start_from = Power2RoundDown(len);

            if (idx + start_from < len)
                arr[idx] = arr[idx + start_from];

            for (int i = start_from >> 1; i > 0; i >>= 1)
            {
                arr[(int)idx] += arr[idx + i];
            }
        }

        /// <summary>
        /// Gets the reduction kernel, caching it for future use.
        /// </summary>
        /// <returns>The reduction kernel, translated to a CUDA or OpenCL kernel.</returns>
        internal static Action<KernelConfig, ArrayView1D<T, Stride1D.Dense>, Index, int> GetReduce<T>()
            where T : unmanaged
        {
            if (!reduce_action.ContainsKey(typeof(T)))
            {
                reduce_action.Add(typeof(T), AcceleratorHandler.accelerator.LoadStreamKernel(Reduce));
            }

            return (Action<KernelConfig, ArrayView1D<T, Stride1D.Dense>, Index, int>)reduce_action[typeof(T)];
        }
    }
}