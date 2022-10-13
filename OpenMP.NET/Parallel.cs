using System;
using OpenMP;
using System.Threading;

namespace OpenMP
{
    public static class Parallel
    {
        public enum Schedule { Static, Dynamic, Guided };

        private static object critical_lock = new object();

        private static volatile bool init_is_finished = false;

        private static void FixArgs(int start, int end, Schedule sched, ref uint? chunk_size, int num_threads)
        {
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
            SpinWait spin = new SpinWait();
            FixArgs(start, end, schedule, ref chunk_size, GetNumThreads());

            if (GetThreadNum() == 0)
            {
                Init.ws = new WorkShare((uint)GetNumThreads(), ForkedRegion.ws.threads);
                Init.ws.start = start;
                Init.ws.end = end;
                Init.ws.chunk_size = chunk_size.Value;
                Init.ws.omp_fn = action;
                init_is_finished = true;
            }

            while (!init_is_finished) spin.SpinOnce();

            switch (schedule)
            {
                case Schedule.Static:
                    Iter.StaticLoop(GetThreadNum());
                    break;
                case Schedule.Dynamic:
                    Iter.DynamicLoop(GetThreadNum());
                    break;
                case Schedule.Guided:
                    Iter.GuidedLoop(GetThreadNum());
                    break;
            }

            while (Init.ws.threads_complete < GetNumThreads()) spin.SpinOnce();

            Init.ws.num_threads = 1;

            if (GetThreadNum() == 0) init_is_finished = false;
        }

        public static void ParallelRegion(Action action, uint? num_threads = null)
        {
            num_threads ??= (uint)GetNumProcs();

            ForkedRegion.CreateThreadpool(num_threads.Value, action);
            ForkedRegion.StartThreadpool();
        }

        public static void ParallelFor(int start, int end, Action<int> action, Schedule schedule = Schedule.Static, uint? chunk_size = null, uint? num_threads = null)
        {
            num_threads ??= (uint)GetNumProcs();

            ParallelRegion(num_threads: num_threads.Value, action: () =>
            {
                For(start, end, action, schedule, chunk_size);
            });
        }

        public static void Critical(Action action)
        {
            lock (critical_lock)
            {
                action();
            }
        }

        public static int GetNumProcs()
        {
            return Environment.ProcessorCount;
        }

        public static int GetNumThreads()
        {
            int num_threads = (int)ForkedRegion.ws.num_threads;

            if (num_threads == 0)
            {
                Init.ws.num_threads = 1;
                return 1;
            }

            return num_threads;
        }

        public static int GetThreadNum()
        {
            return Convert.ToInt32(Thread.CurrentThread.Name);
        }
    }
}