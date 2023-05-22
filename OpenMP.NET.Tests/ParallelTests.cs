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
            var actual = CreateRegion();

            actual.Should().Be((uint)OpenMP.Parallel.GetMaxThreads());
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

        [Fact]
        public void GetWTime_works()
        {
            double start = OpenMP.Parallel.GetWTime();
            Thread.Sleep(1000);
            double end = OpenMP.Parallel.GetWTime();
            (end - start).Should().BeGreaterOrEqualTo(1.0);
            (end - start).Should().BeLessThan(1.1);
        }

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
                s.Clear();
            });
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

        private static uint CreateRegion()
        {
            uint threads_spawned = 0;

            Parallel.ParallelRegion(() =>
            {
                Interlocked.Add(ref threads_spawned, 1);
            });

            return threads_spawned;
        }

        float[] saxpy_parallelregion_for(float a, float[] x, float[] y)
        {
            float[] z = new float[x.Length];

            Parallel.ParallelRegion(() =>
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

            Parallel.ParallelFor(0, x.Length, schedule: Parallel.Schedule.Guided, action: i =>
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