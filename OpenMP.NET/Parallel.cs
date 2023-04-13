using System;
using OpenMP;
using System.Collections.Generic;
using System.Threading;

namespace OpenMP
{
    public delegate void ActionRef<T>(ref T a, int i);

    public static class Parallel
    {
        public enum Schedule { Static, Dynamic, Guided };
        private static volatile Dictionary<int, (int, object)> critical_lock = new Dictionary<int, (int, object)>();
        private static volatile Dictionary<int, int> single_thread = new Dictionary<int, int>();
        private static volatile Dictionary<int, int> ordered = new Dictionary<int, int>();
        private static volatile int found_criticals = 0;
        private static volatile Barrier barrier;
        private static volatile uint num_threads = 0;

        private static void FixArgs(int start, int end, Schedule sched, ref uint? chunk_size, uint num_threads)
        {
            if (num_threads == 0)
            {
                num_threads = (uint)GetNumProcs();
            }

            if (chunk_size == null)
            {
                switch (sched)
                {
                    case Schedule.Static:
                        chunk_size = (uint)((end - start) / num_threads);
                        break;
                    case Schedule.Dynamic:
                        chunk_size = (uint)((end - start) / num_threads) / 32;
                        if (chunk_size < 1) chunk_size = 1;
                        break;
                    case Schedule.Guided:
                        chunk_size = 1;
                        break;
                }
            }
        }

        public static void For(int start, int end, Action<int> action, Schedule schedule = Schedule.Static, uint? chunk_size = null)
        {
            FixArgs(start, end, schedule, ref chunk_size, Init.ws.num_threads);

            Master(() =>
            {
                Init.ws = new WorkShare((uint)GetNumThreads(), ForkedRegion.ws.threads);
                Init.ws.start = start;
                Init.ws.end = end;
                Init.ws.chunk_size = chunk_size.Value;
            });

            Barrier();

            switch (schedule)
            {
                case Schedule.Static:
                    Iter.StaticLoop<object>(GetThreadNum(), action, null, false);
                    break;
                case Schedule.Dynamic:
                    Iter.DynamicLoop<object>(GetThreadNum(), action, null, false);
                    break;
                case Schedule.Guided:
                    Iter.GuidedLoop<object>(GetThreadNum(), action, null, false);
                    break;
            }

            Barrier();

            Master(() => ordered.Clear());
        }

        public static void ForReduction<T>(int start, int end, Operations op, ref T reduce_to, ActionRef<T> action, Schedule schedule = Schedule.Static, uint? chunk_size = null)
        {
            FixArgs(start, end, schedule, ref chunk_size, Init.ws.num_threads);

            if (GetThreadNum() == 0)
            {
                Init.ws = new WorkShare((uint)GetNumThreads(), ForkedRegion.ws.threads);
                Init.ws.start = start;
                Init.ws.end = end;
                Init.ws.chunk_size = chunk_size.Value;
                Init.ws.op = op;
                Init.ws.reduction_list.Clear();
            }

            Barrier();

            switch (schedule)
            {
                case Schedule.Static:
                    Iter.StaticLoop(GetThreadNum(), null, action, true);
                    break;
                case Schedule.Dynamic:
                    Iter.DynamicLoop(GetThreadNum(), null, action, true);
                    break;
                case Schedule.Guided:
                    Iter.GuidedLoop(GetThreadNum(), null, action, true);
                    break;
            }

            Barrier();

            if (GetThreadNum() == 0)
            {
                foreach (T i in Init.ws.reduction_list)
                {
                    switch (Init.ws.op)
                    {
                        case Operations.Add:
                        case Operations.Subtract:
                            reduce_to += (dynamic)i;
                            break;
                        case Operations.Multiply:
                            reduce_to *= (dynamic)i;
                            break;
                        case Operations.BinaryAnd:
                            reduce_to &= (dynamic)i;
                            break;
                        case Operations.BinaryOr:
                            reduce_to |= (dynamic)i;
                            break;
                        case Operations.BinaryXor:
                            reduce_to ^= (dynamic)i;
                            break;
                        case Operations.BooleanAnd:
                            reduce_to = (dynamic)reduce_to && (dynamic)i;
                            break;
                        case Operations.BooleanOr:
                            reduce_to = (dynamic)reduce_to || (dynamic)i;
                            break;
                        case Operations.Min:
                            reduce_to = Math.Min((dynamic)reduce_to, (dynamic)i);
                            break;
                        case Operations.Max:
                            reduce_to = Math.Max((dynamic)reduce_to, (dynamic)i);
                            break;
                    }
                }

                ordered.Clear();
            }
        }

        public static void ParallelRegion(Action action, uint? num_threads = null)
        {
            if (num_threads == null && Parallel.num_threads == 0)
                num_threads = (uint)GetNumProcs();
            else
                num_threads ??= Parallel.num_threads;

            ForkedRegion.CreateThreadpool(num_threads.Value, action);
            barrier = new Barrier((int)num_threads.Value);
            ForkedRegion.StartThreadpool();
            ForkedRegion.ws.num_threads = 1;
            barrier = new Barrier(1);
        }

        public static void ParallelFor(int start, int end, Action<int> action, Schedule schedule = Schedule.Static, uint? chunk_size = null, uint? num_threads = null)
        {
            if (num_threads == null && Parallel.num_threads == 0)
                num_threads = (uint)GetNumProcs();
            else
                num_threads ??= Parallel.num_threads;

            ParallelRegion(num_threads: num_threads.Value, action: () =>
            {
                For(start, end, action, schedule, chunk_size);
            });
        }

        public static void ParallelForReduction<T>(int start, int end, Operations op, ref T reduce_to, ActionRef<T> action, Schedule schedule = Schedule.Static, uint? chunk_size = null, uint? num_threads = null)
        {
            if (num_threads == null && Parallel.num_threads == 0)
                num_threads = (uint)GetNumProcs();
            else
                num_threads ??= Parallel.num_threads;

            T local = reduce_to;

            ParallelRegion(num_threads: num_threads.Value, action: () =>
            {
                ForReduction(start, end, op, ref local, action, schedule, chunk_size);
            });

            reduce_to = local;
        }

        public static int Critical(int id, Action action)
        {
            object lock_obj;

            lock (critical_lock)
            {
                if (!critical_lock.ContainsKey(id))
                {
                    critical_lock.Add(id, (++found_criticals, new object()));
                }

                (id, lock_obj) = critical_lock[id];
            }

            lock (lock_obj)
            {
                action();
            }

            return id;
        }

        public static void Barrier()
        {
            barrier.SignalAndWait();
        }

        public static int GetNumProcs()
        {
            return Environment.ProcessorCount;
        }

        public static void Master(Action action)
        {
            if (GetThreadNum() == 0)
            {
                action();
            }
        }

        public static void Single(int id, Action action)
        {
            lock (single_thread)
            {
                if (!single_thread.ContainsKey(id))
                {
                    single_thread.Add(id, GetThreadNum());
                }
            }

            if (single_thread[id] == GetThreadNum())
            {
                action();
            }
        }

        public static void Ordered(int id, Action action)
        {
            int tid = GetThreadNum();

            lock (ordered)
            {
                if (!ordered.ContainsKey(id))
                {
                    ordered.Add(id, 0);
                }
                Thread.MemoryBarrier();
            }

            while (ordered[id] != Init.ws.threads[tid].working_iter)
            {
                ForkedRegion.ws.spin[tid].SpinOnce();
            }

            action();

            lock (ordered)
            {
                ordered[id]++;
            }
        }

        public static int GetNumThreads()
        {
            int num_threads = (int)ForkedRegion.ws.num_threads;

            if (num_threads == 0)
            {
                ForkedRegion.ws.num_threads = 1;
                return 1;
            }

            return num_threads;
        }

        public static int GetThreadNum()
        {
            return Convert.ToInt32(Thread.CurrentThread.Name);
        }

        public static void SetNumThreads(int num_threads)
        {
            Parallel.num_threads = (uint)num_threads;
        }

        public static int GetMaxThreads()
        {
            return Parallel.num_threads != 0 ? (int)Parallel.num_threads : GetNumProcs();
        }

        public static bool InParallel()
        {
            return ForkedRegion.in_parallel;
        }

        public static void SetDynamic()
        {
            Parallel.num_threads = 0;
        }

        public static bool GetDynamic()
        {
            return Parallel.num_threads == 0;
        }

        public static void SetNested(bool _)
        {
            throw new NotImplementedException();
        }

        public static bool GetNested() => false;

        public static double GetWTime()
        {
            return DateTime.Now.Ticks / 10000000.0;
        }

        public static void __reset_lambda_memory()
        {
            critical_lock.Clear();
            single_thread.Clear();
            found_criticals = 0;
        }
    }
}