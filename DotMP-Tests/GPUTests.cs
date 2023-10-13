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
            double[] res = new double[65536];
            double[] res_cpu = new double[65536];

            random_init(a);
            random_init(x);
            random_init(y);

            DotMP.GPU.DataTo(a, x, y);
            DotMP.GPU.DataFrom(res);

            DotMP.GPU.Kernel<double, double, double, double>((h, a, x, y, res) =>
            {
                DotMP.GPU.For(h, 0, a.Length, i =>
                {
                    res[i] = a[i] * x[i] + y[i];
                });
            });

            for (int i = 0; i < a.Length; i++)
            {
                res_cpu[i] = a[i] * x[i] + y[i];
            }

            Assert.Equal(res_cpu, res);
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