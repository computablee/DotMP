using FluentAssertions;
using System;
using System.Diagnostics;
using System.Threading;
using Xunit;

namespace OpenMP.NET.Tests
{
    public class ParallelTests
    {
        [Fact]
        public void Parallel_performance_should_be_higher()
        {
            var elapsedParallel = Workload(true);
            var elapsedSeries = Workload(false);

            elapsedParallel.Should().BeLessThan(elapsedSeries);
        }

        [Fact]
        public void Parallel_should_work()
        {
            var expected = 1024u;
            var actual = CreateRegion(expected);

            actual.Should().Be(expected);
        }

        [Fact]
        public void Parallelfor_should_work()
        {
            int workload = 4096;

            float[] x = new float[workload];
            float[] y = new float[workload];

            for (int i = 0; i < x.Length; i++)
            {
                x[i] = 1.0f;
                y[i] = 1.0f;
            }

            float[] z = saxpy_parallelregion_for(2.0f, x, y);
            float[] z2 = saxpy_parallelfor(2.0f, x, y);

            for (int i = 0; i < z.Length; i++)
            {
                z[i].Should().Be(z2[i]);
            }
        }

        [Fact]
        public void Static_should_produce_correct_results()
        {
            int workload = 4096;

            float[] x = new float[workload];
            float[] y = new float[workload];

            for (int i = 0; i < x.Length; i++)
            {
                x[i] = 1.0f;
                y[i] = 1.0f;
            }

            float[] z = saxpy_parallelregion_for(2.0f, x, y);

            for (int i = 0; i < z.Length; i++)
            {
                z[i].Should().Be(3.0f);
            }
        }

        [Fact]
        public void Critical_works()
        {
            uint threads = 1024;
            int total = 0;
            int one = critical_ids(false);
            Parallel.__reset_lambda_memory();
            int two = critical_ids(true);
            Parallel.__reset_lambda_memory();

            OpenMP.Parallel.ParallelRegion(num_threads: threads, action: () =>
            {
                int id = OpenMP.Parallel.Critical(() => ++total);
                id.Should().Be(1);
            });
            Parallel.__reset_lambda_memory();

            one.Should().Be(1);
            two.Should().Be(2);
            threads.Should().Be((uint)total);
        }

        private static long Workload(bool inParallel)
        {
            const int WORKLOAD = 1_000_000;
            const int FORITERS = 20;
            float[] a = new float[WORKLOAD];
            float[] b = new float[WORKLOAD];
            float[] c = new float[WORKLOAD];
            Random r = new Random();

            for (int i = 0; i < WORKLOAD; i++)
            {
                a[i] = (float)r.NextDouble();
                b[i] = (float)r.NextDouble();
                c[i] = (float)r.NextDouble();
            }

            Console.WriteLine("Starting test.");

            Stopwatch s = new Stopwatch();
            s.Start();

            for (int i = 0; i < FORITERS; i++)
            {
                if (inParallel)
                {
                    Parallel.ParallelFor(0, WORKLOAD, schedule: Parallel.Schedule.Guided,
                        action: j => InnerWorkload(j, a, b, c));
                }
                else
                {
                    for (int j = 0; j < WORKLOAD; j++)
                    {
                        InnerWorkload(j, a, b, c);
                    }
                }
            }

            s.Stop();
            return s.ElapsedMilliseconds;
        }

        private static void InnerWorkload(int j, float[] a, float[] b, float[] c)
        {
            a[j] = (a[j] * b[j] + c[j]) / c[j];
            while (a[j] > 1000 || a[j] < -1000)
                a[j] /= 10;
            int temp = Convert.ToInt32(a[j]);
            for (int i = 0; i < temp; i++)
                a[j] += i;
        }

        private static uint CreateRegion(uint threads)
        {
            uint threads_spawned = 0;

            Parallel.ParallelRegion(num_threads: threads, action: () =>
            {
                Interlocked.Add(ref threads_spawned, 1);
            });

            return threads_spawned;
        }

        float[] saxpy_parallelregion_for(float a, float[] x, float[] y)
        {
            float[] z = new float[x.Length];

            Parallel.ParallelRegion(num_threads: 4, action: () =>
            {
                Parallel.For(0, x.Length, schedule: Parallel.Schedule.Guided, action: i =>
                {
                    z[i] = a * x[i] + y[i];
                });
            });

            return z;
        }

        float[] saxpy_parallelfor(float a, float[] x, float[] y)
        {
            float[] z = new float[x.Length];

            Parallel.ParallelFor(0, x.Length, schedule: Parallel.Schedule.Guided, num_threads: 4, action: i =>
            {
                z[i] = a * x[i] + y[i];
            });

            return z;
        }

        int critical_ids(bool two_regions)
        {
            object mylock = new object();
            int found_critical_regions = 0;

            int x, y;
            x = y = 0;

            for (int i = 0; i < 5; i++)
            {
                OpenMP.Parallel.ParallelRegion(num_threads: 4, action: () =>
                {
                    int id1 = OpenMP.Parallel.Critical(() => ++x);
                    int id2 = -1;

                    if (two_regions)
                    {
                        id2 = OpenMP.Parallel.Critical(() => ++y);
                    }

                    lock (mylock)
                    {
                        found_critical_regions = Math.Max(found_critical_regions, id1);
                        found_critical_regions = Math.Max(found_critical_regions, id2);
                    }
                });
            }

            return found_critical_regions;
        }
    }
}