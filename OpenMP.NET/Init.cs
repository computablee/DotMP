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

        internal Thr(Thread thread)
        {
            this.thread = thread;
            curr_iter = 0;
        }
    }

    internal struct WorkShare
    {
        internal Thr[] threads;
        internal int start;
        internal object ws_lock;
        internal int end;
        internal uint chunk_size;
        internal uint num_threads;
        internal Action<int> omp_fn;
        volatile internal int threads_complete;

        internal WorkShare(uint num_threads, Thread[] threads)
        {
            this.threads = new Thr[num_threads];
            for (int i = 0; i < num_threads; i++)
                this.threads[i] = new Thr(threads[i]);
            threads_complete = 0;
            ws_lock = new object();
            this.start = 0;
            this.end = 0;
            this.chunk_size = 0;
            this.num_threads = num_threads;
            this.omp_fn = null;
        }
    }

    internal static class Init
    {
        internal static WorkShare ws;
    }
}
