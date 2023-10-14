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
            double[] a = new double[65536];
            double[] x = new double[65536];
            double[] y = new double[65536];
            float[] res = new float[65536];
            float[] res_cpu = new float[65536];

            random_init(a);
            random_init(x);
            random_init(y);

            DotMP.GPU.Parallel.DataTo(a, x, y);
            DotMP.GPU.Parallel.DataFrom(res);
            DotMP.GPU.Parallel.ParallelFor<double, double, double, float>
                (0, a.Length, (i, h) =>
            {
                (int idx,
                DotMP.GPU.GPUArray<double> a,
                DotMP.GPU.GPUArray<double> x,
                DotMP.GPU.GPUArray<double> y,
                DotMP.GPU.GPUArray<float> res) = h.GetData(i);

                res[idx] = (float)(a[idx] * x[idx] + y[idx]);
            });

            for (int i = 0; i < a.Length; i++)
            {
                res_cpu[i] = (float)(a[i] * x[i] + y[i]);
            }

            Assert.Equal(res_cpu, res);

            double[] a_old = a.Select(a => a).ToArray();

            DotMP.GPU.Parallel.DataToFrom(a);
            DotMP.GPU.Parallel.ParallelFor<double>(0, a.Length, (i, h) =>
            {
                (int idx, DotMP.GPU.GPUArray<double> a) = h.GetData(i);

                a[idx]++;
            });

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