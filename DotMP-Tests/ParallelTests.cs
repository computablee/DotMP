using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using DotMP;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace DotMPTests
{
    /// <summary>
    /// Tests for the DotMP library.
    /// </summary>
    public class ParallelTests
    {
        private readonly ITestOutputHelper writer;
        public ParallelTests(ITestOutputHelper outputwriter)
        {
            writer = outputwriter;
        }


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
        /// Tests to make sure that DotMP.Parallel.ForCollapse produces correct results.
        /// </summary>
        [Fact]
        public void Collapse_works()
        {
            int[,] iters_hit = new int[1024, 1024];

            DotMP.Parallel.ParallelForCollapse((256, 512), (512, 600), num_threads: 8, chunk_size: 7, schedule: Schedule.Static, action: (i, j) =>
            {
                DotMP.Atomic.Inc(ref iters_hit[i, j]);
            });

            for (int i = 0; i < 1024; i++)
                for (int j = 0; j < 1024; j++)
                    if (i >= 256 && i < 512 && j >= 512 && j < 600)
                        iters_hit[i, j].Should().Be(1);
                    else
                        iters_hit[i, j].Should().Be(0);
        }

        /// <summary>
        /// Tests to make sure that taskloops produce correct results.
        /// </summary>
        [Fact]
        public void Taskloop_should_produce_correct_results()
        {
            int workload = 1_000_000;

            float[] x = new float[workload];
            float[] y = new float[workload];

            for (int i = 0; i < x.Length; i++)
            {
                x[i] = 1.0f;
                y[i] = 1.0f;
            }

            float[] z = saxpy_parallelregion_for_taskloop(2.0f, x, y, 6);

            for (int i = 0; i < z.Length; i++)
            {
                z[i].Should().Be(3.0f);
            }
        }

        /// <summary>
        /// Tests taskloop dependencies, and in turn, more complex dependency chaining.
        /// </summary>
        [Fact]
        public void Taskloop_dependencies_work()
        {
            int size = 1_000_000;
            uint grainsize = 128;

            int[] a = new int[size];
            int[] b = new int[size];

            DotMP.Parallel.ParallelMaster(num_threads: 6, action: () =>
            {
                var t1 = DotMP.Parallel.Taskloop(0, size, grainsize: grainsize, action: i =>
                {
                    a[i] += 1;
                });

                var t2 = DotMP.Parallel.Taskloop(0, size, grainsize: grainsize, depends: t1, action: i =>
                {
                    a[i] += 2;
                });

                DotMP.Parallel.Taskloop(0, size, grainsize: grainsize, depends: t2, action: i =>
                {
                    b[i] = a[i];
                });
            });

            for (int i = 0; i < size; i++)
            {
                b[i].Should().Be(3);
            }
        }

        /// <summary>
        /// Ensures that nested task dependencies work.
        /// </summary>
        [Fact]
        public void Nested_task_dependencies_work()
        {
            bool task_triggered = false;

            DotMP.Parallel.ParallelMaster(num_threads: 2, action: () =>
            {
                DotMP.Parallel.Task(() =>
                {
                    var t1 = DotMP.Parallel.Task(() => { });
                    Thread.Sleep(100);
                    DotMP.Parallel.Task(depends: t1, action: () =>
                    {
                        task_triggered = true;
                    });
                });
            });

            task_triggered.Should().BeTrue();
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
            int iters = 1024;
            int total = 0;

            DotMP.Parallel.ParallelRegion(num_threads: threads, action: () =>
            {
                for (int i = 0; i < iters; i++)
                    DotMP.Parallel.Critical(0, () => ++total);
            });

            total.Should().Be((int)threads * iters);

            double start = DotMP.Parallel.GetWTime();

            DotMP.Parallel.ParallelRegion(num_threads: 4, action: () =>
            {
                if (DotMP.Parallel.GetThreadNum() == 0) DotMP.Parallel.Critical(0, () => Thread.Sleep(1000));
                if (DotMP.Parallel.GetThreadNum() == 1) DotMP.Parallel.Critical(1, () => Thread.Sleep(1000));
                if (DotMP.Parallel.GetThreadNum() == 2) DotMP.Parallel.Critical(0, () => Thread.Sleep(1000));
                if (DotMP.Parallel.GetThreadNum() == 3) DotMP.Parallel.Critical(1, () => Thread.Sleep(1000));
            });

            double elapsed = DotMP.Parallel.GetWTime() - start;
            elapsed.Should().BeLessThan(2200);
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
                for (int i = 0; i < 10; i++)
                {
                    DotMP.Parallel.Single(0, () => DotMP.Atomic.Inc(ref total));
                }
            });

            total.Should().Be(1);

            total = 0;

            DotMP.Parallel.ParallelRegion(num_threads: threads, action: () =>
            {
                for (int i = 0; i < 10; i++)
                {
                    DotMP.Parallel.Single(0, () => DotMP.Atomic.Inc(ref total));
                }
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
            int[] int_totals = new int[6];
            long[] long_totals = new long[6];
            uint[] uint_totals = new uint[5];
            ulong[] ulong_totals = new ulong[5];

            uint_totals[1] = 1024;
            ulong_totals[1] = 1024;

            int_totals[3] = int.MaxValue;
            uint_totals[3] = uint.MaxValue;
            long_totals[3] = long.MaxValue;
            ulong_totals[3] = ulong.MaxValue;

            //inc
            DotMP.Parallel.ParallelRegion(num_threads: threads, action: () =>
            {
                DotMP.Parallel.Barrier();
                DotMP.Atomic.Inc(ref int_totals[0]);
                DotMP.Parallel.Barrier();
                DotMP.Atomic.Inc(ref uint_totals[0]);
                DotMP.Parallel.Barrier();
                DotMP.Atomic.Inc(ref long_totals[0]);
                DotMP.Parallel.Barrier();
                DotMP.Atomic.Inc(ref ulong_totals[0]);
            });

            int_totals[0].Should().Be((int)threads);
            uint_totals[0].Should().Be((uint)threads);
            long_totals[0].Should().Be((long)threads);
            ulong_totals[0].Should().Be((ulong)threads);

            //dec
            DotMP.Parallel.ParallelRegion(num_threads: threads, action: () =>
            {
                DotMP.Parallel.Barrier();
                DotMP.Atomic.Dec(ref int_totals[1]);
                DotMP.Parallel.Barrier();
                DotMP.Atomic.Dec(ref uint_totals[1]);
                DotMP.Parallel.Barrier();
                DotMP.Atomic.Dec(ref long_totals[1]);
                DotMP.Parallel.Barrier();
                DotMP.Atomic.Dec(ref ulong_totals[1]);
            });

            int_totals[1].Should().Be((int)-threads);
            uint_totals[1].Should().Be(0);
            long_totals[1].Should().Be((long)-threads);
            ulong_totals[1].Should().Be(0);

            //add
            DotMP.Parallel.ParallelRegion(num_threads: threads, action: () =>
            {
                DotMP.Parallel.Barrier();
                DotMP.Atomic.Add(ref int_totals[2], 2);
                DotMP.Parallel.Barrier();
                DotMP.Atomic.Add(ref uint_totals[2], 2);
                DotMP.Parallel.Barrier();
                DotMP.Atomic.Add(ref long_totals[2], 2);
                DotMP.Parallel.Barrier();
                DotMP.Atomic.Add(ref ulong_totals[2], 2);
            });

            int_totals[2].Should().Be((int)threads * 2);
            uint_totals[2].Should().Be((uint)threads * 2);
            long_totals[2].Should().Be((long)threads * 2);
            ulong_totals[2].Should().Be((ulong)threads * 2);

            //and
            DotMP.Parallel.ParallelRegion(num_threads: threads, action: () =>
            {
                int tid_int = (int)DotMP.Parallel.GetThreadNum();
                uint tid_uint = (uint)DotMP.Parallel.GetThreadNum();
                long tid_long = (long)DotMP.Parallel.GetThreadNum();
                ulong tid_ulong = (ulong)DotMP.Parallel.GetThreadNum();

                DotMP.Parallel.Barrier();
                DotMP.Atomic.And(ref int_totals[3], tid_int);
                DotMP.Parallel.Barrier();
                DotMP.Atomic.And(ref uint_totals[3], tid_uint);
                DotMP.Parallel.Barrier();
                DotMP.Atomic.And(ref long_totals[3], tid_long);
                DotMP.Parallel.Barrier();
                DotMP.Atomic.And(ref ulong_totals[3], tid_ulong);
            });

            int_totals[3].Should().Be(0);
            uint_totals[3].Should().Be(0);
            long_totals[3].Should().Be(0);
            ulong_totals[3].Should().Be(0);

            //or
            DotMP.Parallel.ParallelRegion(num_threads: threads, action: () =>
            {
                int tid_int = (int)DotMP.Parallel.GetThreadNum();
                uint tid_uint = (uint)DotMP.Parallel.GetThreadNum();
                long tid_long = (long)DotMP.Parallel.GetThreadNum();
                ulong tid_ulong = (ulong)DotMP.Parallel.GetThreadNum();

                DotMP.Parallel.Barrier();
                DotMP.Atomic.Or(ref int_totals[4], tid_int);
                DotMP.Parallel.Barrier();
                DotMP.Atomic.Or(ref uint_totals[4], tid_uint);
                DotMP.Parallel.Barrier();
                DotMP.Atomic.Or(ref long_totals[4], tid_long);
                DotMP.Parallel.Barrier();
                DotMP.Atomic.Or(ref ulong_totals[4], tid_ulong);
            });

            uint res = 0;
            for (uint i = 0; i < threads; i++)
                res |= i;

            int_totals[4].Should().Be((int)res);
            uint_totals[4].Should().Be((uint)res);
            long_totals[4].Should().Be((long)res);
            ulong_totals[4].Should().Be((ulong)res);

            //sub
            DotMP.Parallel.ParallelRegion(num_threads: threads, action: () =>
            {
                DotMP.Parallel.Barrier();
                DotMP.Atomic.Sub(ref int_totals[5], 2);
                DotMP.Parallel.Barrier();
                DotMP.Atomic.Sub(ref long_totals[5], 2);
            });

            int_totals[5].Should().Be((int)-threads * 2);
            long_totals[5].Should().Be((long)-threads * 2);
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

            DotMP.Parallel.ParallelForReduction(0, 1024, DotMP.Operations.Add, ref total, num_threads: 8, chunk_size: 1, schedule: DotMP.Schedule.Static, action: (ref int total, int i) =>
            {
                total += i;
            });

            total.Should().Be(1024 * 1023 / 2);

            long total_long = 0;

            DotMP.Parallel.ParallelForReduction(0, 1024, DotMP.Operations.Subtract, ref total_long, num_threads: 8, chunk_size: 1, schedule: DotMP.Schedule.Static, action: (ref long total, int i) =>
            {
                total -= (long)i;
            });

            total_long.Should().Be((long)-(1024 * 1023 / 2));

            float total_float = 1;

            DotMP.Parallel.ParallelForReduction(0, 48, DotMP.Operations.Multiply, ref total_float, num_threads: 8, chunk_size: 1, schedule: DotMP.Schedule.Static, action: (ref float total, int i) =>
            {
                total *= 2;
            });

            total_float.Should().Be(Convert.ToUInt64(Math.Pow(2, 48)));

            ulong total_ulong = 1023;

            DotMP.Parallel.ParallelForReduction(0, 1024, DotMP.Operations.BinaryAnd, ref total_ulong, num_threads: 8, chunk_size: 1, schedule: DotMP.Schedule.Static, action: (ref ulong total, int i) =>
            {
                total &= (ulong)i;
            });

            total_ulong.Should().Be(0);

            uint total_uint = 0;

            DotMP.Parallel.ParallelForReduction(0, 1024, DotMP.Operations.BinaryOr, ref total_uint, num_threads: 8, chunk_size: 1, schedule: DotMP.Schedule.Static, action: (ref uint total, int i) =>
            {
                total |= (uint)i;
            });

            total_uint.Should().Be(1023);

            byte total_byte = 0;

            DotMP.Parallel.ParallelForReduction(0, 256, DotMP.Operations.BinaryXor, ref total_byte, num_threads: 8, chunk_size: 1, schedule: DotMP.Schedule.Static, action: (ref byte total, int i) =>
            {
                total ^= (byte)i;
            });

            byte actual_byte = 0;

            for (short i = 0; i < 256; i++)
                actual_byte ^= (byte)i;

            total_byte.Should().Be(actual_byte);

            bool total_bool = true;

            DotMP.Parallel.ParallelForReduction(0, 1024, DotMP.Operations.BooleanAnd, ref total_bool, num_threads: 8, chunk_size: 1, schedule: DotMP.Schedule.Static, action: (ref bool total, int i) =>
            {
                total = total && (i != 768);
            });

            total_bool.Should().BeFalse();

            total_bool = false;

            DotMP.Parallel.ParallelForReduction(0, 1024, DotMP.Operations.BooleanOr, ref total_bool, num_threads: 8, chunk_size: 1, schedule: DotMP.Schedule.Static, action: (ref bool total, int i) =>
            {
                total = total || (i == 768);
            });

            total_bool.Should().BeTrue();

            double total_double = double.MaxValue;

            DotMP.Parallel.ParallelForReduction(0, 1024, DotMP.Operations.Min, ref total_double, num_threads: 8, chunk_size: 1, schedule: DotMP.Schedule.Static, action: (ref double total, int i) =>
            {
                total = Math.Min(total, (double)i);
            });

            total_double.Should().Be(0);

            total_double = double.MinValue;

            DotMP.Parallel.ParallelForReduction(0, 1024, DotMP.Operations.Max, ref total_double, num_threads: 8, chunk_size: 1, schedule: DotMP.Schedule.Static, action: (ref double total, int i) =>
            {
                total = Math.Max(total, (double)i);
            });

            total_double.Should().Be(1023);
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
                l.Set();
                Thread.Sleep(100);
                l.Unset();
            });

            double elapsed = DotMP.Parallel.GetWTime() - time;
            elapsed.Should().BeGreaterThan(1.6);

            l.Test().Should().BeTrue();
            l.Test().Should().BeFalse();
            l.Test().Should().BeFalse();
            l.Unset();
            l.Test().Should().BeTrue();
            l.Test().Should().BeFalse();
            l.Test().Should().BeFalse();
            l.Unset();
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
                            DotMP.Atomic.Inc(ref total_tasks_executed);
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
            elapsed.Should().BeLessThan(1.25 * 2.0 * (sleep_duration / 1000.0));

            tasks_thread_executed = new int[threads];
            int tasks_to_spawn = 100_000;

            DotMP.Parallel.ParallelRegion(num_threads: threads, action: () =>
            {
                DotMP.Parallel.Single(0, () =>
                {
                    for (int i = 0; i < tasks_to_spawn; i++)
                    {
                        DotMP.Parallel.Task(() =>
                        {
                            tasks_thread_executed[DotMP.Parallel.GetThreadNum()]++;
                        });
                    }
                });
            });

            tasks_thread_executed.Sum().Should().Be(tasks_to_spawn);
        }

        /// <summary>
        /// Test if the only_if clause works on taskloops.
        /// </summary>
        [Fact]
        public void Taskloop_only_if_works()
        {
            uint threads = 2;
            int[] executed_on_thread = new int[2];

            DotMP.Parallel.ParallelMasterTaskloop(0, (int)threads, num_threads: threads, only_if: true, grainsize: 1, action: i =>
            {
                executed_on_thread[DotMP.Parallel.GetThreadNum()]++;
                Thread.Sleep(100);
            });

            for (uint i = 0; i < threads; i++)
            {
                executed_on_thread[i].Should().Be(1);
                executed_on_thread[i] = 0;
            }

            DotMP.Parallel.ParallelMasterTaskloop(0, (int)threads, num_threads: threads, only_if: false, grainsize: 1, action: i =>
            {
                executed_on_thread[DotMP.Parallel.GetThreadNum()]++;
                Thread.Sleep(100);
            });

            executed_on_thread[0].Should().Be((int)threads);
            for (uint i = 1; i < threads; i++)
            {
                executed_on_thread[i].Should().Be(0);
            }
        }

        /// <summary>
        /// Checks to see if nested tasks work.
        /// </summary>
        [Fact]
        public void Nested_tasks_work()
        {
            uint threads = 6;
            double start = DotMP.Parallel.GetWTime();

            DotMP.Parallel.ParallelRegion(num_threads: threads, action: () =>
            {
                DotMP.Parallel.Single(0, () =>
                {
                    DotMP.Parallel.Task(() =>
                    {
                        Thread.Sleep(500);
                        for (int i = 0; i < threads; i++)
                        {
                            DotMP.Parallel.Task(() =>
                            {
                                Thread.Sleep(500);
                            });
                        }
                    });
                });
            });

            double elapsed = DotMP.Parallel.GetWTime() - start;
            elapsed.Should().BeGreaterThan(1.0);
            elapsed.Should().BeLessThan(1.25);
        }

        /// <summary>
        /// Test if taskloop dependencies work.
        /// </summary>
        [Fact]
        public void Task_dependencies_work()
        {
            List<int> order_completed;

            for (int i = 0; i < 100; i++)
            {
                order_completed = new List<int>();

                DotMP.Parallel.ParallelRegion(num_threads: 4, action: () =>
                {
                    DotMP.Parallel.Master(() =>
                    {
                        var t1 = DotMP.Parallel.Task(() =>
                        {
                            order_completed.Add(0);
                        });

                        var t2 = DotMP.Parallel.Task(() =>
                        {
                            lock (order_completed)
                                order_completed.Add(1);
                        }, t1);

                        var t3 = DotMP.Parallel.Task(() =>
                        {
                            lock (order_completed)
                                order_completed.Add(2);
                        }, t1);

                        var t4 = DotMP.Parallel.Task(() =>
                        {
                            order_completed.Add(3);
                        }, t2, t3);

                    });
                });

                order_completed.Count.Should().Be(4);
                order_completed[0].Should().Be(0);
                order_completed[1].Should().BeInRange(1, 2);
                order_completed[2].Should().BeInRange(1, 2);
                order_completed[3].Should().Be(3);
            }

            order_completed = new List<int>();

            DotMP.Parallel.ParallelRegion(num_threads: 8, action: () =>
            {
                DotMP.Parallel.Master(() =>
                {
                    var t1 = DotMP.Parallel.Taskloop(0, 4, num_tasks: 4, action: i =>
                    {
                        Thread.Sleep(100);
                        lock (order_completed)
                            order_completed.Add(0);
                    });

                    DotMP.Parallel.Taskloop(0, 4, num_tasks: 4, depends: t1, action: i =>
                    {
                        lock (order_completed)
                            order_completed.Add(1);
                    });
                });

                DotMP.Parallel.Taskwait();
            });

            order_completed.Count.Should().Be(8);
            for (int i = 0; i < 4; i++)
                order_completed[i].Should().Be(0);
            for (int i = 4; i < 8; i++)
                order_completed[i].Should().Be(1);
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

            DotMP.Parallel.ParallelRegion(num_threads: 6, action: () =>
            {
                DotMP.Parallel.For(0, x.Length, schedule: schedule, chunk_size: chunk_size, action: i =>
                {
                    z[i] += a * x[i] + y[i];
                });
            });

            return z;
        }

        /// <summary>
        /// A sample workload for saxpy, using taskloops.
        /// </summary>
        /// <param name="a">Scalar for saxpy.</param>
        /// <param name="x">Vector to multiply by the scalar.</param>
        /// <param name="y">Vector to add.</param>
        /// <param name="grainsize">Grainsize to use</param>
        /// <returns>Result of saxpy.</returns>
        float[] saxpy_parallelregion_for_taskloop(float a, float[] x, float[] y, uint? grainsize)
        {
            float[] z = new float[x.Length];

            DotMP.Parallel.ParallelRegion(num_threads: 6, action: () =>
            {
                DotMP.Parallel.MasterTaskloop(0, x.Length, grainsize: grainsize, action: i =>
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
    }
}
