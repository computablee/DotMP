using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using DotMP;
using DotMP.GPU;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;


namespace DotMPTests
{
    /// <summary>
    /// GPU tests for the DotMP library.
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
#if NET6_0_OR_GREATER
                using var a_gpu = new DotMP.GPU.Buffer<double>(a, DotMP.GPU.Buffer.Behavior.To);
                using var x_gpu = new DotMP.GPU.Buffer<double>(x, DotMP.GPU.Buffer.Behavior.To);
                using var y_gpu = new DotMP.GPU.Buffer<double>(y, DotMP.GPU.Buffer.Behavior.To);
                using var res_gpu = new DotMP.GPU.Buffer<float>(res, DotMP.GPU.Buffer.Behavior.From);
#else
                var a_gpu = new DotMP.GPU.Buffer<double>(a, DotMP.GPU.Buffer.Behavior.To);
                var x_gpu = new DotMP.GPU.Buffer<double>(x, DotMP.GPU.Buffer.Behavior.To);
                var y_gpu = new DotMP.GPU.Buffer<double>(y, DotMP.GPU.Buffer.Behavior.To);
                var res_gpu = new DotMP.GPU.Buffer<float>(res, DotMP.GPU.Buffer.Behavior.From);
#endif

                DotMP.GPU.Parallel.ParallelFor(0, a.Length, a_gpu, x_gpu, y_gpu, res_gpu,
                    (i, a_kernel, x_kernel, y_kernel, res_kernel) =>
                {
                    res_kernel[i] = (float)(a_kernel[i] * x_kernel[i] + y_kernel[i]);
                });
#if !NET6_0_OR_GREATER
                a_gpu.Dispose();
                x_gpu.Dispose();
                y_gpu.Dispose();
                res_gpu.Dispose();
#endif
            }

            for (int i = 0; i < a.Length; i++)
            {
                res_cpu[i] = (float)(a[i] * x[i] + y[i]);
            }

            Assert.Equal(res_cpu, res);

            double[] a_old = a.Select(a_l => a_l).ToArray();

            using (var a_gpu = new DotMP.GPU.Buffer<double>(a, DotMP.GPU.Buffer.Behavior.ToFrom))
            {
                DotMP.GPU.Parallel.ParallelFor(0, a.Length, a_gpu, (i, a_kernel) =>
                {
                    a_kernel[i]++;
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
                DotMP.GPU.Parallel.ParallelForCollapse((258, 512), (512, 600), buf, (i, j, iters_hit_kernel) =>
                {
                    iters_hit_kernel[i, j]++;
                });
            }

            for (int i = 0; i < 1024; i++)
                for (int j = 0; j < 1024; j++)
                    if (i >= 258 && i < 512 && j >= 512 && j < 600)
                        iters_hit[i, j].Should().Be(1);
                    else
                        iters_hit[i, j].Should().Be(0);

            iters_hit = null;

            int[,,] iters_hit_3 = new int[128, 128, 64];

            using (var buf = new Buffer<int>(iters_hit_3, DotMP.GPU.Buffer.Behavior.ToFrom))
            {
                DotMP.GPU.Parallel.ParallelForCollapse((35, 64), (16, 100), (10, 62), buf, (i, j, k, iters_hit_3_kernel) =>
                {
                    iters_hit_3_kernel[i, j, k]++;
                });
            }

            for (int i = 0; i < 128; i++)
                for (int j = 0; j < 128; j++)
                    for (int k = 0; k < 64; k++)
                        if (i >= 35 && i < 64 && j >= 16 && j < 100 && k >= 10 && k < 62)
                            iters_hit_3[i, j, k].Should().Be(1);
                        else
                            iters_hit_3[i, j, k].Should().Be(0);

            iters_hit_3 = null;
        }

        /* jscpd:ignore-start */
        /// <summary>
        /// Tests to ensure that 1D buffers work.
        /// </summary>
        [Fact]
        public void One_dimensional_buffer_works()
        {
            float[] to_set = new float[1000];

            using (var to_set_buffer = new Buffer<float>(to_set, DotMP.GPU.Buffer.Behavior.ToFrom))
            {
                DotMP.GPU.Parallel.ParallelFor(0, to_set.Length, to_set_buffer, (i, to_set_kernel) =>
                {
                    to_set_kernel[i] = 4;
                });
            }

            to_set.Should().AllBeEquivalentTo(4);
        }

        /// <summary>
        /// Tests to ensure that 2D buffers work.
        /// </summary>
        [Fact]
        public void Two_dimensional_buffer_works()
        {
            float[,] to_set = new float[500, 20];

            using (var to_set_buffer = new Buffer<float>(to_set, DotMP.GPU.Buffer.Behavior.ToFrom))
            {
                DotMP.GPU.Parallel.ParallelForCollapse((0, 500), (0, 20), to_set_buffer, (i, j, to_set_kernel) =>
                {
                    to_set_kernel[i, j] = 5;
                });
            }

            for (int i = 0; i < 500; i++)
                for (int j = 0; j < 20; j++)
                    to_set[i, j].Should().Be(5);

            to_set = new float[20, 500];

            using (var to_set_buffer = new Buffer<float>(to_set, DotMP.GPU.Buffer.Behavior.ToFrom))
            {
                DotMP.GPU.Parallel.ParallelForCollapse((0, 20), (0, 500), to_set_buffer, (i, j, to_set_kernel) =>
                {
                    to_set_kernel[i, j] = 6;
                });
            }

            for (int i = 0; i < 20; i++)
                for (int j = 0; j < 500; j++)
                    to_set[i, j].Should().Be(6);
        }

        /// <summary>
        /// Tests to ensure that 3D buffers work.
        /// </summary>
        [Fact]
        public void Three_dimensional_buffer_works()
        {
            float[,,] to_set = new float[200, 200, 15];

            using (var to_set_buffer = new Buffer<float>(to_set, DotMP.GPU.Buffer.Behavior.ToFrom))
            {
                DotMP.GPU.Parallel.ParallelForCollapse((0, 200), (0, 200), (0, 15), to_set_buffer, (i, j, k, to_set_kernel) =>
                {
                    to_set_kernel[i, j, k] = 7;
                });
            }

            for (int i = 0; i < 200; i++)
                for (int j = 0; j < 200; j++)
                    for (int k = 0; k < 15; k++)
                        to_set[i, j, k].Should().Be(7);

            to_set = new float[200, 15, 200];

            using (var to_set_buffer = new Buffer<float>(to_set, DotMP.GPU.Buffer.Behavior.ToFrom))
            {
                DotMP.GPU.Parallel.ParallelForCollapse((0, 200), (0, 15), (0, 200), to_set_buffer, (i, j, k, to_set_kernel) =>
                {
                    to_set_kernel[i, j, k] = 8;
                });
            }

            for (int i = 0; i < 200; i++)
                for (int j = 0; j < 15; j++)
                    for (int k = 0; k < 200; k++)
                        to_set[i, j, k].Should().Be(8);

            to_set = new float[15, 200, 200];

            using (var to_set_buffer = new Buffer<float>(to_set, DotMP.GPU.Buffer.Behavior.ToFrom))
            {
                DotMP.GPU.Parallel.ParallelForCollapse((0, 15), (0, 200), (0, 200), to_set_buffer, (i, j, k, to_set_kernel) =>
                {
                    to_set_kernel[i, j, k] = 9;
                });
            }

            for (int i = 0; i < 15; i++)
                for (int j = 0; j < 200; j++)
                    for (int k = 0; k < 200; k++)
                        to_set[i, j, k].Should().Be(9);
        }
        /* jscpd:ignore-end */

        /// <summary>
        /// Randomly initialize an array of type T.
        /// </summary>
        /// <typeparam name="T">The type to initialize to.</typeparam>
        /// <param name="arr">The allocated array to store values into.</param>
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
