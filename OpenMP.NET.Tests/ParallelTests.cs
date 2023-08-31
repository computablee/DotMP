using FluentAssertions;
using System;
using System.Diagnostics;
using System.Threading;
using Xunit;

namespace OmpNETTests
{
    /// <summary>
    /// Tests for the OpenMP.NET library.
    /// </summary>
    public class ParallelTests
    {
        /// <summary>
        /// Tests to make sure that parallel performance is higher than sequential performance.
        /// </summary>
        [Fact]
        public void Parallel_performance_should_be_higher()
        {
            var elapsedParallel = Workload(true);
            var elapsedSeries = Workload(false);

            elapsedParallel.Should().BeLessThan(elapsedSeries);
        }

        /// <summary>
        /// Tests to make sure that OpenMP.Parallel.ParallelRegion()'s are actually created.
        /// </summary>
        [Fact]
        public void Parallel_should_work()
        {
            var actual = CreateRegion();

            actual.Should().Be((uint)OpenMP.Parallel.GetMaxThreads());
        }

        /// <summary>
        /// Tests the functionality of OpenMP.Parallel.For().
        /// </summary>
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

        /// <summary>
        /// Tests to make sure that OpenMP.Parallel.Schedule.Static produces correct results.
        /// </summary>
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

        /// <summary>
        /// Tests to make sure that OpenMP.Parallel.Critical() works.
        /// </summary>
        [Fact]
        public void Critical_works()
        {
            uint threads = 1024;
            int total = 0;
            int one = critical_ids(false);
            int two = critical_ids(true);

            OpenMP.Parallel.ParallelRegion(num_threads: threads, action: () =>
            {
                int id = OpenMP.Parallel.Critical(5, () => ++total);
                id.Should().Be(3);
            });

            one.Should().Be(1);
            two.Should().Be(2);
            threads.Should().Be((uint)total);
        }

        /// <summary>
        /// Tests to make sure that OpenMP.Parallel.Master() works.
        /// </summary>
        [Fact]
        public void Master_works()
        {
            uint threads = 1024;
            int total = 0;

            OpenMP.Parallel.ParallelRegion(num_threads: threads, action: () =>
            {
                OpenMP.Parallel.Master(() => ++total);
            });

            total.Should().Be(1);
        }

        /// <summary>
        /// Tests to make sure that OpenMP.Parallel.Single() works.
        /// </summary>
        [Fact]
        public void Single_works()
        {
            uint threads = 1024;
            int total = 0;

            OpenMP.Parallel.ParallelRegion(num_threads: threads, action: () =>
            {
                OpenMP.Parallel.Single(0, () => ++total);
            });

            total.Should().Be(1);
        }

        /// <summary>
        /// Tests to make sure that the OpenMP.Atomic class works.
        /// </summary>
        [Fact]
        public void Atomic_works()
        {
            uint threads = 1024;
            uint total = 0;
            long total2 = 0;

            OpenMP.Parallel.ParallelRegion(num_threads: threads, action: () =>
            {
                OpenMP.Atomic.Inc(ref total);
                OpenMP.Atomic.Sub(ref total2, 2);
            });

            total.Should().Be(threads);
            total2.Should().Be(-threads * 2);
        }

        /// <summary>
        /// Tests to make sure that OpenMP.Parallel.Ordered() works.
        /// </summary>
        [Fact]
        public void Ordered_works()
        {
            uint threads = 8;
            int[] incrementing = new int[1024];

            OpenMP.Parallel.ParallelFor(0, 1024, schedule: OpenMP.Parallel.Schedule.Static,
                                        num_threads: threads, action: i =>
            {
                OpenMP.Parallel.Ordered(0, () => incrementing[i] = i);
            });

            for (int i = 0; i < incrementing.Length; i++)
            {
                incrementing[i].Should().Be(i);
            }
        }

        /// <summary>
        /// Tests to make sure that OpenMP.Parallel.ForReduction<T>() works.
        /// </summary>
        [Fact]
        public void Reduction_works()
        {
            int total = 0;

            OpenMP.Parallel.ParallelForReduction(0, 1024, OpenMP.Operations.Add, ref total, num_threads: 8, schedule: OpenMP.Parallel.Schedule.Static, action: (ref int total, int i) =>
            {
                total += i;
            });

            OpenMP.Parallel.ParallelForReduction(0, 1024, OpenMP.Operations.Add, ref total, num_threads: 8, schedule: OpenMP.Parallel.Schedule.Static, action: (ref int total, int i) =>
            {
                total += i;
            });

            total.Should().Be(1024 * 1023);
        }

        /// <summary>
        /// Tests to make sure that OpenMP.Parallel.SetNumThreads() works.
        /// </summary>
        [Fact]
        public void SetNumThreads_works()
        {
            OpenMP.Parallel.GetMaxThreads().Should().Be(OpenMP.Parallel.GetNumProcs());

            OpenMP.Parallel.ParallelRegion(() =>
            {
                OpenMP.Parallel.GetNumThreads().Should().Be(OpenMP.Parallel.GetNumProcs());
            });

            OpenMP.Parallel.ParallelRegion(num_threads: 2, action: () =>
            {
                OpenMP.Parallel.GetNumThreads().Should().Be(2);
            });

            OpenMP.Parallel.SetNumThreads(15);
            OpenMP.Parallel.GetMaxThreads().Should().Be(15);

            OpenMP.Parallel.ParallelRegion(() =>
            {
                OpenMP.Parallel.GetNumThreads().Should().Be(15);
            });
        }

        /// <summary>
        /// Tests to make sure that OpenMP.Parallel.InParallel() works.
        /// </summary>
        [Fact]
        public void InParallel_works()
        {
            OpenMP.Parallel.InParallel().Should().BeFalse();

            OpenMP.Parallel.ParallelRegion(() =>
            {
                OpenMP.Parallel.InParallel().Should().BeTrue();
            });

            OpenMP.Parallel.InParallel().Should().BeFalse();
        }

        /// <summary>
        /// Tests to make sure that OpenMP.Parallel.SetDynamic() works.
        /// </summary>
        [Fact]
        public void SetDynamic_works()
        {
            OpenMP.Parallel.SetNumThreads(2);
            OpenMP.Parallel.GetDynamic().Should().BeFalse();
            OpenMP.Parallel.SetDynamic();
            OpenMP.Parallel.GetDynamic().Should().BeTrue();
            OpenMP.Parallel.ParallelRegion(() =>
            {
                OpenMP.Parallel.GetNumThreads().Should().Be(OpenMP.Parallel.GetNumProcs());
            });
            OpenMP.Parallel.GetDynamic().Should().BeTrue();
            OpenMP.Parallel.SetNumThreads(OpenMP.Parallel.GetNumProcs());
            OpenMP.Parallel.GetDynamic().Should().BeFalse();
        }

        /// <summary>
        /// Tests to make sure that OpenMP.Parallel.GetWTime() works.
        /// </summary>
        [Fact]
        public void GetWTime_works()
        {
            double start = OpenMP.Parallel.GetWTime();
            Thread.Sleep(1000);
            double end = OpenMP.Parallel.GetWTime();
            (end - start).Should().BeGreaterOrEqualTo(1.0);
            (end - start).Should().BeLessThan(1.1);
        }

        /// <summary>
        /// Tests to make sure that OpenMP.Parallel.GetNested() and OpenMP.Parallel.SetNested() work.
        /// </summary>
        [Fact]
        public void GetNested_works()
        {
            OpenMP.Parallel.GetNested().Should().BeFalse();
            try
            {
                OpenMP.Parallel.SetNested(true);
                true.Should().BeFalse();
            }
            catch (NotImplementedException e)
            {
                e.Should().NotBeNull();
            }
        }

        /// <summary>
        /// Tests to make sure that the OpenMP.Locking and OpenMP.Lock classes work.
        /// </summary>
        [Fact]
        public void Locks_work()
        {
            uint threads = 16;
            OpenMP.Lock l = new OpenMP.Lock();

            double time = OpenMP.Parallel.GetWTime();

            OpenMP.Parallel.ParallelRegion(num_threads: threads, action: () =>
            {
                OpenMP.Locking.Set(l);
                Thread.Sleep(100);
                OpenMP.Locking.Unset(l);
            });

            double elapsed = OpenMP.Parallel.GetWTime() - time;
            elapsed.Should().BeGreaterThan(1.6);

            OpenMP.Locking.Test(l).Should().BeTrue();
            OpenMP.Locking.Test(l).Should().BeFalse();
            OpenMP.Locking.Test(l).Should().BeFalse();
            OpenMP.Locking.Unset(l);
            OpenMP.Locking.Test(l).Should().BeTrue();
            OpenMP.Locking.Test(l).Should().BeFalse();
            OpenMP.Locking.Test(l).Should().BeFalse();
            OpenMP.Locking.Unset(l);
        }

        /// <summary>
        /// Tests to make sure the OpenMP.Shared class works.
        /// </summary>
        [Fact]
        public void Shared_works()
        {
            OpenMP.Parallel.ParallelRegion(() =>
            {
                OpenMP.Shared<int> s = new OpenMP.Shared<int>("s", 6);
                s.Get().Should().Be(6);
                OpenMP.Parallel.Master(() => s.Set(7));
                OpenMP.Parallel.Barrier();
                s.Get().Should().Be(7);
                OpenMP.Parallel.Barrier();
                s.Clear();
            });
        }

        /// <summary>
        /// Tests to make sure that OpenMP.Parallel.Sections() and OpenMP.Parallel.Section() work.
        /// </summary>
        [Fact]
        public void Sections_works()
        {
            uint num_threads = 6;
            bool[] threads_used = new bool[num_threads];

            for (int i = 0; i < num_threads; i++)
                threads_used[i] = false;

            double start = OpenMP.Parallel.GetWTime();

            OpenMP.Parallel.ParallelSections(num_threads: num_threads, action: () =>
            {
                for (int i = 0; i < num_threads; i++)
                {
                    OpenMP.Parallel.Section(() =>
                    {
                        threads_used[OpenMP.Parallel.GetThreadNum()] = true;
                        Thread.Sleep(100);
                    });
                }
            });

            double end = OpenMP.Parallel.GetWTime() - start;

            for (int i = 0; i < num_threads; i++)
                threads_used[i].Should().Be(true);

            end.Should().BeLessThan(0.15);
        }

        /// <summary>
        /// A sample workload for OpenMP.Parallel.ParallelFor().
        /// </summary>
        /// <param name="inParallel">Whether or not to enable parallelism.</param>
        /// <returns>Elapsed milliseconds of the test.</returns>
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
                    OpenMP.Parallel.ParallelFor(0, WORKLOAD, schedule: OpenMP.Parallel.Schedule.Guided,
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

        /// <summary>
        /// A heavy workload for tests.
        /// </summary>
        /// <param name="j">Index to use into a, b, and c.</param>
        /// <param name="a">Float array 1 (destination).</param>
        /// <param name="b">Float array 2 (source).</param>
        /// <param name="c">Float array 3 (source).</param>
        private static void InnerWorkload(int j, float[] a, float[] b, float[] c)
        {
            a[j] = (a[j] * b[j] + c[j]) / c[j];
            while (a[j] > 1000 || a[j] < -1000)
                a[j] /= 10;
            int temp = Convert.ToInt32(a[j]);
            for (int i = 0; i < temp; i++)
                a[j] += i;
        }

        /// <summary>
        /// Creates a parallel region and returns the number of threads spawned.
        /// </summary>
        /// <returns></returns>
        private static uint CreateRegion()
        {
            uint threads_spawned = 0;

            OpenMP.Parallel.ParallelRegion(() =>
            {
                Interlocked.Add(ref threads_spawned, 1);
            });

            return threads_spawned;
        }

        /// <summary>
        /// A sample workload for saxpy.
        /// </summary>
        /// <param name="a">Scalar for saxpy.</param>
        /// <param name="x">Vector to multiply by the scalar.</param>
        /// <param name="y">Vector to add.</param>
        /// <returns>Result of saxpy.</returns>
        float[] saxpy_parallelregion_for(float a, float[] x, float[] y)
        {
            float[] z = new float[x.Length];

            OpenMP.Parallel.ParallelRegion(() =>
            {
                OpenMP.Parallel.For(0, x.Length, schedule: OpenMP.Parallel.Schedule.Guided, action: i =>
                {
                    z[i] = a * x[i] + y[i];
                });
            });

            return z;
        }

        /// <summary>
        /// Same as saxpy_parallelregion_for, but uses OpenMP.Parallel.ParallelFor() instead of OpenMP.Parallel.ParallelRegion() and OpenMP.Parallel.For().
        /// </summary>
        /// <param name="a">Scalar for saxpy.</param>
        /// <param name="x">Vector to multiply by the scalar.</param>
        /// <param name="y">Vector to add.</param>
        /// <returns>Result of saxpy.</returns>
        float[] saxpy_parallelfor(float a, float[] x, float[] y)
        {
            float[] z = new float[x.Length];

            OpenMP.Parallel.ParallelFor(0, x.Length, schedule: OpenMP.Parallel.Schedule.Guided, action: i =>
            {
                z[i] = a * x[i] + y[i];
            });

            return z;
        }

        /// <summary>
        /// Convoluted test to calculate how many critical regions were found. Outdated, can probably be removed.
        /// </summary>
        /// <param name="two_regions">Whether or not to spawn 2 regions or 1.</param>
        /// <returns>The number of encountered critical regions.</returns>
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
                    int id1 = OpenMP.Parallel.Critical(0, () => ++x);
                    int id2 = -1;

                    OpenMP.Parallel.For(0, 100, schedule: OpenMP.Parallel.Schedule.Static, action: j =>
                    {
                        if (two_regions)
                        {
                            id2 = OpenMP.Parallel.Critical(1, () => ++y);
                        }

                        lock (mylock)
                        {
                            found_critical_regions = Math.Max(found_critical_regions, id1);
                            found_critical_regions = Math.Max(found_critical_regions, id2);
                        }
                    });
                });
            }

            return found_critical_regions;
        }
    }
}