using System;
using OpenMP;

namespace OpenMP
{
    public static class Parallel
    {
        public enum Schedule { Static, Dynamic, Guided };

        private static void FixArgs(int start, int end, Schedule sched, ref uint? chunk_size, ref uint? num_threads)
        {
            if (num_threads == null)
                num_threads = (uint)GetNumProcs();


            if (chunk_size == null)
            {
                switch (sched)
                {
                    case Schedule.Static:
                        chunk_size = (uint)((end - start) / num_threads.Value);
                        break;
                    case Schedule.Dynamic:
                        chunk_size = 1;
                        break;
                    case Schedule.Guided:
                        chunk_size = 1;
                        break;
                }
            }
        }

        public static void For(int start, int end, Action<int> omp_fn, Schedule sched = Schedule.Static, uint? chunk_size = null, uint? num_threads = null)
        {
            FixArgs(start, end, sched, ref chunk_size, ref num_threads);

            Console.WriteLine("Executing for loop from {0} to {1} with {2},{3} scheduling on {4} threads.",
                start,
                end,
                sched == Schedule.Static ? "static" : sched == Schedule.Dynamic ? "dynamic" : "guided",
                chunk_size,
                num_threads);

            Init.CreateThreadpool(start, end, sched, chunk_size.Value, num_threads.Value, omp_fn);
            Init.StartThreadpool();
        }

        public static int GetNumProcs()
        {
            return Environment.ProcessorCount;
        }
    }
}
