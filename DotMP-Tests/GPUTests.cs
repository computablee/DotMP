using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using DotMP;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;


namespace DotMPTests
{
    /// <summary>
    /// CPU tests for the DotMP library.
    /// </summary>
    public class GPUTests
    {
        /// <summary>
        /// Tests to make sure that for loops work in GPU kernels.
        /// </summary>
        [Fact]
        public void GPU_for_works()
        {
            double[] a = new double[50000];
            double[] x = new double[50000];
            double[] y = new double[50000];
            float[] res = new float[50000];
            float[] res_cpu = new float[50000];

            random_init(a);
            random_init(x);
            random_init(y);

            {
                using var a_gpu = new DotMP.GPU.Buffer<double>(a, DotMP.GPU.Buffer.Behavior.To);
                using var x_gpu = new DotMP.GPU.Buffer<double>(x, DotMP.GPU.Buffer.Behavior.To);
                using var y_gpu = new DotMP.GPU.Buffer<double>(y, DotMP.GPU.Buffer.Behavior.To);
                using var res_gpu = new DotMP.GPU.Buffer<float>(res, DotMP.GPU.Buffer.Behavior.From);

                DotMP.GPU.Parallel.ParallelFor(0, a.Length, a_gpu, x_gpu, y_gpu, res_gpu,
                    (i, a, x, y, res) =>
                {
                    res[i] = (float)(a[i] * x[i] + y[i]);
                });
            }

            for (int i = 0; i < a.Length; i++)
            {
                res_cpu[i] = (float)(a[i] * x[i] + y[i]);
            }

            Assert.Equal(res_cpu, res);

            double[] a_old = a.Select(a => a).ToArray();

            using (var a_gpu = new DotMP.GPU.Buffer<double>(a, DotMP.GPU.Buffer.Behavior.ToFrom))
            {
                DotMP.GPU.Parallel.ParallelFor(0, a.Length, a_gpu, (i, a) =>
                {
                    a[i]++;
                });
            }

            for (int i = 0; i < a.Length; i++)
            {
                a_old[i]++;
            }

            Assert.Equal(a, a_old);
        }

        private void random_init<T>(T[] arr)
        {
            Random r = new Random();

            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = (T)Convert.ChangeType(r.NextDouble() * 128, typeof(T));
            }
        }
    }
}
