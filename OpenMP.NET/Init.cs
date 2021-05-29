using System;
using System.Collections.Generic;
using System.Threading;
using OpenMP;

namespace OpenMP
{

    internal class Thr
    {
        internal Thread thread;
        volatile internal int curr_iter;
    }

    internal struct WorkShare
    {
        internal Thr[] threads;
        internal int start;
        internal int end;
        internal uint chunk_size;
        internal uint num_threads;
        internal Action<int> omp_fn;
        volatile internal int threads_complete;

        internal WorkShare(uint num_threads, int start, int end, uint chunk_size, Action<int> omp_fn)
        {
            threads = new Thr[num_threads];
            for (int i = 0; i < num_threads; i++)
                threads[i] = new Thr();
            threads_complete = 0;
            this.start = start;
            this.end = end;
            this.chunk_size = chunk_size;
            this.num_threads = num_threads;
            this.omp_fn = omp_fn;
        }
    }

    internal static class Init
    {
        internal static WorkShare ws;

        internal static void CreateThreadpool(int start, int end, Parallel.Schedule sched, uint chunk_size, uint num_threads, Action<int> omp_fn)
        {
            ws = new WorkShare(num_threads, start, end, chunk_size, omp_fn);
            for (int i = 0; i < num_threads; i++)
            {
                ws.threads[i].thread = new Thread(Iter.StaticLoop);
                ws.threads[i].curr_iter = 0;
            }
        }

        internal static void StartThreadpool()
        {
            for (int i = 0; i < ws.num_threads; i++)
                ws.threads[i].thread.Start(i);

            for (int i = 0; i < ws.num_threads; i++)
                ws.threads[i].thread.Join();
        }
    }
}
