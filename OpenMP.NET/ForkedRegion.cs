using System;
using System.Threading;

namespace OpenMP
{
    internal struct Region
    {
        internal Thread[] threads;
        internal object ws_lock;
        internal uint num_threads;
        internal Action omp_fn;

        internal Region(uint num_threads, Action omp_fn)
        {
            threads = new Thread[num_threads];
            for (int i = 0; i < num_threads; i++)
                threads[i] = new Thread(() => omp_fn());
            ws_lock = new object();
            this.num_threads = num_threads;
            this.omp_fn = omp_fn;
        }
    }

    internal static class ForkedRegion
    {
        internal static Region ws;

        internal static void CreateThreadpool(uint num_threads, Action omp_fn)
        {
            ws = new Region(num_threads, omp_fn);
            for (int i = 0; i < num_threads; i++)
                ws.threads[i].Name = i.ToString();
        }

        internal static void StartThreadpool()
        {
            for (int i = 0; i < ws.num_threads; i++)
                ws.threads[i].Start();

            for (int i = 0; i < ws.num_threads; i++)
                ws.threads[i].Join();
        }
    }
}
