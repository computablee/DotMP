using DotMP;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Xunit;

namespace DotMPTests
{
    /// <summary>
    /// Tests for the DotMP library.
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
        /// Tests to make sure that DotMP.Parallel.ParallelRegion()'s are actually created.
        /// </summary>
        [Fact]
        public void Parallel_should_work()
        {
            var actual = CreateRegion();

            actual.Should().Be((uint)DotMP.Parallel.GetMaxThreads());
        }

        /// <summary>
        /// Tests the functionality of DotMP.Parallel.For().
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

            float[] z = saxpy_parallelregion_for(2.0f, x, y, Schedule.Static, null);
            float[] z2 = saxpy_parallelfor(2.0f, x, y);

            for (int i = 0; i < z.Length; i++)
            {
                z[i].Should().Be(z2[i]);
            }
        }

        /// <summary>
        /// Tests to make sure that DotMP.Schedule.Guided produces correct results.
        /// </summary>
        [Fact]
        public void Guided_should_produce_correct_results()
        {
            int workload = 1_000_000;

            float[] x = new float[workload];
            float[] y = new float[workload];

            for (int i = 0; i < x.Length; i++)
            {
                x[i] = 1.0f;
                y[i] = 1.0f;
            }

            float[] z = saxpy_parallelregion_for(2.0f, x, y, Schedule.Guided, 3);

            for (int i = 0; i < z.Length; i++)
            {
                z[i].Should().Be(3.0f);
            }
        }

        /// <summary>
        /// Tests to make sure that DotMP.Schedule.Static produces correct results.
        /// </summary>
        [Fact]
        public void Static_should_produce_correct_results()
        {
            int workload = 1_000_000;

            float[] x = new float[workload];
            float[] y = new float[workload];

            for (int i = 0; i < x.Length; i++)
            {
                x[i] = 1.0f;
                y[i] = 1.0f;
            }

            float[] z = saxpy_parallelregion_for(2.0f, x, y, Schedule.Static, 1024);

            for (int i = 0; i < z.Length; i++)
            {
                z[i].Should().Be(3.0f);
            }
        }

        /// <summary>
        /// Tests to make sure that DotMP.Schedule.Dynamic produces correct results.
        /// </summary>
        [Fact]
        public void Dynamic_should_produce_correct_results()
        {
            int workload = 1_000_000;

            float[] x = new float[workload];
            float[] y = new float[workload];

            for (int i = 0; i < x.Length; i++)
            {
                x[i] = 1.0f;
                y[i] = 1.0f;
            }

            float[] z = saxpy_parallelregion_for(2.0f, x, y, Schedule.Dynamic, 16);

            for (int i = 0; i < z.Length; i++)
            {
                z[i].Should().Be(3.0f);
            }
        }

        /// <summary>
        /// Tests to make sure that DotMP.Parallel.Schedule.Runtime properly reads values from the environment variable.
        /// </summary>
        [Fact]
        public void Schedule_runtime_works()
        {
            Environment.SetEnvironmentVariable("OMP_SCHEDULE", "guided,2");
            DotMP.Parallel.ParallelFor(0, 1024, schedule: DotMP.Schedule.Runtime, action: i =>
            {
                DotMP.Parallel.GetSchedule().Should().Be(DotMP.Schedule.Guided);
                DotMP.Parallel.GetChunkSize().Should().Be(2);
            });

            Environment.SetEnvironmentVariable("OMP_SCHEDULE", "dynamic,4");
            DotMP.Parallel.ParallelFor(0, 1024, schedule: DotMP.Schedule.Runtime, action: i =>
            {
                DotMP.Parallel.GetSchedule().Should().Be(DotMP.Schedule.Dynamic);
                DotMP.Parallel.GetChunkSize().Should().Be(4);
            });
        }

        /// <summary>
        /// Tests to make sure that DotMP.Parallel.Critical() works.
        /// </summary>
        [Fact]
        public void Critical_works()
        {
            uint threads = 1024;
            int total = 0;
            int one = critical_ids(false);
            int two = critical_ids(true);

            DotMP.Parallel.ParallelRegion(num_threads: threads, action: () =>
            {
                int id = DotMP.Parallel.Critical(5, () => ++total);
                id.Should().Be(3);
            });

            one.Should().Be(1);
            two.Should().Be(2);
            threads.Should().Be((uint)total);
        }

        /// <summary>
        /// Tests to make sure that DotMP.Parallel.Master() works.
        /// </summary>
        [Fact]
        public void Master_works()
        {
            uint threads = 1024;
            int total = 0;

            DotMP.Parallel.ParallelRegion(num_threads: threads, action: () =>
            {
                DotMP.Parallel.Master(() => ++total);
            });

            total.Should().Be(1);
        }

        /// <summary>
        /// Tests to make sure that DotMP.Parallel.Single() works.
        /// </summary>
        [Fact]
        public void Single_works()
        {
            uint threads = 1024;
            int total = 0;

            DotMP.Parallel.ParallelRegion(num_threads: threads, action: () =>
            {
                DotMP.Parallel.Single(0, () => ++total);
            });

            total.Should().Be(1);
        }

        /// <summary>
        /// Tests to make sure that the DotMP.Atomic class works.
        /// </summary>
        [Fact]
        public void Atomic_works()
        {
            uint threads = 1024;
            uint total = 0;
            long total2 = 0;

            DotMP.Parallel.ParallelRegion(num_threads: threads, action: () =>
            {
                DotMP.Atomic.Inc(ref total);
                DotMP.Atomic.Sub(ref total2, 2);
            });

            total.Should().Be(threads);
            total2.Should().Be(-threads * 2);
        }

        /// <summary>
        /// Tests to make sure that DotMP.Parallel.Ordered() works.
        /// </summary>
        [Fact]
        public void Ordered_works()
        {
            uint threads = 8;
            int[] incrementing = new int[1024];

            DotMP.Parallel.ParallelFor(0, 1024, schedule: DotMP.Schedule.Static,
                                        num_threads: threads, action: i =>
            {
                DotMP.Parallel.Ordered(0, () => incrementing[i] = i);
            });

            for (int i = 0; i < incrementing.Length; i++)
            {
                incrementing[i].Should().Be(i);
            }
        }

        /// <summary>
        /// Tests to make sure that DotMP.Parallel.ForReduction<T>() works.
        /// </summary>
        [Fact]
        public void Reduction_works()
        {
            int total = 0;

            DotMP.Parallel.ParallelForReduction(0, 1024, DotMP.Operations.Add, ref total, num_threads: 8, schedule: DotMP.Schedule.Static, action: (ref int total, int i) =>
            {
                total += i;
            });

            DotMP.Parallel.ParallelForReduction(0, 1024, DotMP.Operations.Add, ref total, num_threads: 8, schedule: DotMP.Schedule.Static, action: (ref int total, int i) =>
            {
                total += i;
            });

            total.Should().Be(1024 * 1023);
        }

        /// <summary>
        /// Tests to make sure that DotMP.Parallel.SetNumThreads() works.
        /// </summary>
        [Fact]
        public void SetNumThreads_works()
        {
            DotMP.Parallel.GetMaxThreads().Should().Be(DotMP.Parallel.GetNumProcs());

            DotMP.Parallel.ParallelRegion(() =>
            {
                DotMP.Parallel.GetNumThreads().Should().Be(DotMP.Parallel.GetNumProcs());
            });

            DotMP.Parallel.ParallelRegion(num_threads: 2, action: () =>
            {
                DotMP.Parallel.GetNumThreads().Should().Be(2);
            });

            DotMP.Parallel.SetNumThreads(15);
            DotMP.Parallel.GetMaxThreads().Should().Be(15);

            DotMP.Parallel.ParallelRegion(() =>
            {
                DotMP.Parallel.GetNumThreads().Should().Be(15);
            });
        }

        /// <summary>
        /// Tests to make sure that DotMP.Parallel.InParallel() works.
        /// </summary>
        [Fact]
        public void InParallel_works()
        {
            DotMP.Parallel.InParallel().Should().BeFalse();

            DotMP.Parallel.ParallelRegion(() =>
            {
                DotMP.Parallel.InParallel().Should().BeTrue();
            });

            DotMP.Parallel.InParallel().Should().BeFalse();
        }

        /// <summary>
        /// Tests to make sure that DotMP.Parallel.SetDynamic() works.
        /// </summary>
        [Fact]
        public void SetDynamic_works()
        {
            DotMP.Parallel.SetNumThreads(2);
            DotMP.Parallel.GetDynamic().Should().BeFalse();
            DotMP.Parallel.SetDynamic();
            DotMP.Parallel.GetDynamic().Should().BeTrue();
            DotMP.Parallel.ParallelRegion(() =>
            {
                DotMP.Parallel.GetNumThreads().Should().Be(DotMP.Parallel.GetNumProcs());
            });
            DotMP.Parallel.GetDynamic().Should().BeTrue();
            DotMP.Parallel.SetNumThreads(DotMP.Parallel.GetNumProcs());
            DotMP.Parallel.GetDynamic().Should().BeFalse();
        }

        /// <summary>
        /// Tests to make sure that DotMP.Parallel.GetWTime() works.
        /// </summary>
        [Fact]
        public void GetWTime_works()
        {
            double start = DotMP.Parallel.GetWTime();
            Thread.Sleep(1000);
            double end = DotMP.Parallel.GetWTime();
            (end - start).Should().BeGreaterOrEqualTo(1.0);
            (end - start).Should().BeLessThan(1.1);
        }

        /// <summary>
        /// Tests to make sure that DotMP.Parallel.GetNested() and DotMP.Parallel.SetNested() work.
        /// </summary>
        [Fact]
        public void GetNested_works()
        {
            DotMP.Parallel.GetNested().Should().BeFalse();
            try
            {
                DotMP.Parallel.SetNested(true);
                true.Should().BeFalse();
            }
            catch (NotImplementedException e)
            {
                e.Should().NotBeNull();
            }
        }

        /// <summary>
        /// Tests to make sure that the DotMP.Locking and DotMP.Lock classes work.
        /// </summary>
        [Fact]
        public void Locks_work()
        {
            uint threads = 16;
            DotMP.Lock l = new DotMP.Lock();

            double time = DotMP.Parallel.GetWTime();

            DotMP.Parallel.ParallelRegion(num_threads: threads, action: () =>
            {
                DotMP.Lock.Set(l);
                Thread.Sleep(100);
                DotMP.Lock.Unset(l);
            });

            double elapsed = DotMP.Parallel.GetWTime() - time;
            elapsed.Should().BeGreaterThan(1.6);

            DotMP.Lock.Test(l).Should().BeTrue();
            DotMP.Lock.Test(l).Should().BeFalse();
            DotMP.Lock.Test(l).Should().BeFalse();
            DotMP.Lock.Unset(l);
            DotMP.Lock.Test(l).Should().BeTrue();
            DotMP.Lock.Test(l).Should().BeFalse();
            DotMP.Lock.Test(l).Should().BeFalse();
            DotMP.Lock.Unset(l);
        }

        /// <summary>
        /// Tests to make sure the DotMP.Shared class works.
        /// </summary>
        [Fact]
        public void Shared_works()
        {
            DotMP.Parallel.ParallelRegion(() =>
            {
                DotMP.Shared<int> s;
                using (s = DotMP.Shared.Create("s", 6))
                {
                    s.Get().Should().Be(6);
                    (s + 1).Should().Be(7);
                    DotMP.Parallel.Barrier();
                    DotMP.Parallel.Master(() => s.Set(7));
                    DotMP.Parallel.Barrier();
                    s.Get().Should().Be(7);
                    DotMP.Parallel.Barrier();
                }
                s.Disposed.Should().BeTrue();
            });
        }

        /// <summary>
        /// Tests to make sure the DotMP.SharedEnumerable class works.
        /// </summary>
        [Fact]
        public void SharedEnumerable_works()
        {
            double[] returnVector = new double[0];

            DotMP.Parallel.ParallelRegion(() =>
            {
                DotMP.SharedEnumerable<double, double[]> vec;
                using (vec = DotMP.SharedEnumerable.Create("vec", new double[1024]))
                {
                    DotMP.Parallel.For(0, 1024, i =>
                    {
                        vec[i] = 1.0;
                    });

                    returnVector = vec;
                }
                vec.Disposed.Should().BeTrue();
            });

            for (int i = 0; i < returnVector.Length; i++)
            {
                returnVector[i].Should().Be(1.0);
            }

            DotMP.Parallel.ParallelRegion(() =>
            {
                var a = DotMP.SharedEnumerable.Create("a", new double[1024]);
                var x = DotMP.SharedEnumerable.Create("x", new List<double>(new double[1024]));

                a[0] = x[0];
                double[] a_arr = a;
                List<double> x_arr = x;

                DotMP.Parallel.Barrier();
                a.Dispose();
                x.Dispose();
                a.Disposed.Should().BeTrue();
                x.Disposed.Should().BeTrue();
            });
        }

        /// <summary>
        /// Tests to make sure that DotMP.Parallel.Sections() and DotMP.Parallel.Section() work.
        /// </summary>
        [Fact]
        public void Sections_works()
        {
            uint num_threads = 4;
            bool[] threads_used = new bool[num_threads];

            for (int i = 0; i < num_threads; i++)
                threads_used[i] = false;

            double start = DotMP.Parallel.GetWTime();

            DotMP.Parallel.ParallelSections(num_threads: num_threads, () =>
            {
                threads_used[DotMP.Parallel.GetThreadNum()] = true;
                Thread.Sleep(100);
            }, () =>
            {
                threads_used[DotMP.Parallel.GetThreadNum()] = true;
                Thread.Sleep(100);
            }, () =>
            {
                threads_used[DotMP.Parallel.GetThreadNum()] = true;
                Thread.Sleep(100);
            }, () =>
            {
                threads_used[DotMP.Parallel.GetThreadNum()] = true;
                Thread.Sleep(100);
            });

            double end = DotMP.Parallel.GetWTime() - start;

            for (int i = 0; i < num_threads; i++)
                threads_used[i].Should().Be(true);

            end.Should().BeLessThan(0.15);
        }

        /// <summary>
        /// Tests to see if the basics of tasking work.
        /// </summary>
        [Fact]
        public void Tasking_works()
        {
            uint threads = 6;
            int sleep_duration = 100;
            double start = DotMP.Parallel.GetWTime();
            int[] tasks_thread_executed = new int[threads];
            int total_tasks_executed = 0;

            DotMP.Parallel.ParallelRegion(num_threads: threads, action: () =>
            {
                DotMP.Parallel.Single(0, () =>
                {
                    for (int i = 0; i < threads * 2; i++)
                    {
                        DotMP.Parallel.Task(() =>
                        {
                            Thread.Sleep(sleep_duration);
                            total_tasks_executed++;
                            tasks_thread_executed[DotMP.Parallel.GetThreadNum()]++;
                        });
                    }
                });
            });

            double elapsed = DotMP.Parallel.GetWTime() - start;

            total_tasks_executed.Should().Be((int)threads * 2);
            foreach (int i in tasks_thread_executed)
            {
                i.Should().Be(2);
            }
            elapsed.Should().BeGreaterThan(2.0 * (sleep_duration / 1000.0));
            elapsed.Should().BeLessThan(threads * 2.0 * (sleep_duration / 1000.0));
        }

        /// <summary>
        /// A sample workload for DotMP.Parallel.ParallelFor().
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
                    DotMP.Parallel.ParallelFor(0, WORKLOAD, schedule: DotMP.Schedule.Guided,
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

            DotMP.Parallel.ParallelRegion(() =>
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
        /// <param name="schedule">Schedule to use.</param>
        /// <param name="chunk_size">Chunk size to use.</param>
        /// <returns>Result of saxpy.</returns>
        float[] saxpy_parallelregion_for(float a, float[] x, float[] y, Schedule schedule, uint? chunk_size)
        {
            float[] z = new float[x.Length];

            DotMP.Parallel.ParallelRegion(() =>
            {
                DotMP.Parallel.For(0, x.Length, schedule: schedule, chunk_size: chunk_size, action: i =>
                {
                    z[i] += a * x[i] + y[i];
                });
            });

            return z;
        }

        /// <summary>
        /// Same as saxpy_parallelregion_for, but uses DotMP.Parallel.ParallelFor() instead of DotMP.Parallel.ParallelRegion() and DotMP.Parallel.For().
        /// </summary>
        /// <param name="a">Scalar for saxpy.</param>
        /// <param name="x">Vector to multiply by the scalar.</param>
        /// <param name="y">Vector to add.</param>
        /// <returns>Result of saxpy.</returns>
        float[] saxpy_parallelfor(float a, float[] x, float[] y)
        {
            float[] z = new float[x.Length];

            DotMP.Parallel.ParallelFor(0, x.Length, schedule: DotMP.Schedule.Guided, action: i =>
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
                DotMP.Parallel.ParallelRegion(num_threads: 4, action: () =>
                {
                    int id1 = DotMP.Parallel.Critical(0, () => ++x);
                    int id2 = -1;

                    DotMP.Parallel.For(0, 100, schedule: DotMP.Schedule.Static, action: j =>
                    {
                        if (two_regions)
                        {
                            id2 = DotMP.Parallel.Critical(1, () => ++y);
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