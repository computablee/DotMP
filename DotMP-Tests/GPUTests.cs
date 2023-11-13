using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using DotMP;
using DotMP.GPU;
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

        /// <summary>
        /// Tests to make sure that DotMP.GPU.Parallel.ForCollapse produces correct results.
        /// </summary>
        [Fact]
        public void Collapse_works()
        {
            int[,] iters_hit = new int[1024, 1024];

            using (var buf = new Buffer<int>(iters_hit, DotMP.GPU.Buffer.Behavior.ToFrom))
            {
                DotMP.GPU.Parallel.ParallelForCollapse((258, 512), (512, 600), buf, action: (i, j, iters_hit) =>
                {
                    iters_hit[i, j]++;
                });
            }

            for (int i = 0; i < 1024; i++)
                for (int j = 0; j < 1024; j++)
                    if (i >= 258 && i < 512 && j >= 512 && j < 600)
                        iters_hit[i, j].Should().Be(1);
                    else
                        iters_hit[i, j].Should().Be(0);

            /*iters_hit = null;

            int[,,] iters_hit_3 = new int[128, 128, 64];

            DotMP.Parallel.ParallelForCollapse((35, 64), (16, 100), (10, 62), num_threads: 8, chunk_size: 3, schedule: Schedule.Dynamic, action: (i, j, k) =>
            {
                DotMP.Atomic.Inc(ref iters_hit_3[i, j, k]);
            });

            for (int i = 0; i < 128; i++)
                for (int j = 0; j < 128; j++)
                    for (int k = 0; k < 64; k++)
                        if (i >= 35 && i < 64 && j >= 16 && j < 100 && k >= 10 && k < 62)
                            iters_hit_3[i, j, k].Should().Be(1);
                        else
                            iters_hit_3[i, j, k].Should().Be(0);

            iters_hit_3 = null;*/
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
