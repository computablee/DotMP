using System;
using System.Collections.Generic;
using System.Threading;

namespace OpenMP
{
    internal static class Iter
    {
        internal static void StaticLoop(object thread_id)
        {
            int tid = (int)thread_id;
            Thr thr = Init.ws.threads[tid];

            thr.curr_iter = (int)(Init.ws.start + tid * Init.ws.chunk_size);

            while (thr.curr_iter < Init.ws.end)
                StaticNext(thr, tid);

            Interlocked.Add(ref Init.ws.threads_complete, 1);
        }

        private static void StaticNext(Thr thr, int thread_id)
        {
            for (int i = thr.curr_iter; i < Math.Min(thr.curr_iter + Init.ws.chunk_size, Init.ws.end); i++)
            {
                //Console.WriteLine("Executing iteration {0} on thread {1}.", i, thread_id);
                Init.ws.omp_fn(i);
            }

            thr.curr_iter += (int)(Init.ws.num_threads * Init.ws.chunk_size);
        }
    }
}
