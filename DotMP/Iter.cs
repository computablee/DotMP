using System;
using System.Threading;

namespace DotMP
{
    /// <summary>
    /// The different types of schedules for a parallel for loop.
    /// The default schedule if none is specified is static.
    /// Detailed explanations of each schedule can be found in the Iter class.
    /// The Runtime schedule simply fetches the schedule from the OMP_SCHEDULE environment variable.
    /// </summary>
    public enum Schedule { Static, Dynamic, Guided, Runtime };

    /// <summary>
    /// Contains all of the scheduling code for parallel for loops.
    /// </summary>
    internal static class Iter
    {
        /// <summary>
        /// Starts and controls a parallel for loop with static scheduling.
        /// Static scheduling works as follows:
        /// Iterations are submitted to threads via round-robin scheduling. Unless the chunk size is 1, each thread will receive a chunk of iterations to work on.
        /// This chunk is specified by the chunk_size variable, found in Init.ws.chunk_size.
        /// By default, if no chunk size is specified, the chunk size is the number of iterations divided by the number of threads.
        /// There is a tradeoff with having a large vs. small chunk size.
        /// A large chunk size will result in less overhead, but may result in load imbalance.
        /// A small chunk size will result in more overhead, but will result in better load balancing.
        /// Static scheduling is the default scheduling method for parallel for loops if none is specified.
        /// </summary>
        /// <typeparam name="T">The type of the local variable for reductions.</typeparam>
        /// <param name="ws">The WorkShare object for state.</param>
        /// <param name="thread_id">The thread ID.</param>
        /// <param name="omp_fn">The function to be executed.</param>
        /// <param name="omp_fn_red">The function to be executed for reductions.</param>
        /// <param name="is_reduction">Whether or not the loop is a reduction loop.</param>
        internal static void StaticLoop<T>(WorkShare ws, int thread_id, Action<int> omp_fn, ActionRef<T> omp_fn_red, bool is_reduction)
        {
            int tid = thread_id;
            Thr thr = ws.thread;

            thr.curr_iter = (int)(ws.start + tid * ws.chunk_size);
            int end = ws.end;

            T local = default;
            ws.SetLocal(ref local);

            while (thr.curr_iter < end)
                StaticNext(ws, thr, ws.chunk_size, omp_fn, omp_fn_red, is_reduction, ref local);

            ws.Finished();

            ws.AddReductionValue(local);
        }

        /// <summary>
        /// Calculates the next chunk of iterations for a static scheduling parallel for loop and executes the appropriate function.
        /// Each time this function is called, the calling thread receives a chunk of iterations to work on, as specified in the Iter.StaticLoop&lt;T&gt; documentation.
        /// </summary>
        /// <typeparam name="T">The type of the local variable for reductions.</typeparam>
        /// <param name="ws">The WorkShare object for state.</param>
        /// <param name="thr">The Thr object for the current thread.</param>
        /// <param name="chunk_size">The chunk size.</param>
        /// <param name="omp_fn">The function to be executed.</param>
        /// <param name="omp_fn_red">The function to be executed for reductions.</param>
        /// <param name="is_reduction">Whether or not the loop is a reduction loop.</param>
        /// <param name="local">The local variable for reductions.</param>
        private static void StaticNext<T>(WorkShare ws, Thr thr, uint chunk_size, Action<int> omp_fn, ActionRef<T> omp_fn_red, bool is_reduction, ref T local)
        {
            int start = thr.curr_iter;
            int end = (int)Math.Min(thr.curr_iter + chunk_size, ws.end);

            ref int i = ref thr.working_iter;

            if (!is_reduction) for (i = start; i < end; i++)
                {
                    omp_fn(i);
                }
            else for (i = start; i < end; i++)
                {
                    omp_fn_red(ref local, i);
                }

            thr.curr_iter += (int)(ws.num_threads * chunk_size);
        }

        /// <summary>
        /// Starts and controls a parallel for loop with dynamic scheduling.
        /// Dynamic scheduling works as follows:
        /// Iterations are thrown into a central queue.
        /// When a thread has no current assigned work, it will grab a chunk of iterations from the queue.
        /// This chunk is specified by the chunk_size variable, found in Init.ws.chunk_size.
        /// By default, if no chunk size is specified, the Parallel.FixArgs method will calulate a chunk size based on a simple heuristic.
        /// When the central queue is empty, each thread will wait at a barrier until all other threads have completed their work.
        /// There is a tradeoff with having a large vs. small chunk size.
        /// A large chunk size will result in less overhead, but may result in load imbalance.
        /// A small chunk size will result in more overhead, but will result in better load balancing.
        /// </summary>
        /// <typeparam name="T">The type of the local variable for reductions.</typeparam>
        /// <param name="ws">The WorkShare object for state.</param>
        /// <param name="omp_fn">The function to be executed.</param>
        /// <param name="omp_fn_red">The function to be executed for reductions.</param>
        /// <param name="is_reduction">Whether or not the loop is a reduction loop.</param>
        internal static void DynamicLoop<T>(WorkShare ws, Action<int> omp_fn, ActionRef<T> omp_fn_red, bool is_reduction)
        {
            Thr thr = ws.thread;
            int end = ws.end;

            T local = default;
            ws.SetLocal(ref local);

            while (ws.start < end)
            {
                DynamicNext(ws, thr, omp_fn, omp_fn_red, is_reduction, ref local);
            }

            ws.Finished();

            ws.AddReductionValue(local);
        }

        /// <summary>
        /// Calculates the next chunk of iterations for a dynamic scheduling parallel for loop and executes the appropriate function.
        /// Each time this function is called, the calling thread receives a chunk of iterations to work on, as specified in the Iter.DynamicLoop&lt;T&gt; documentation.
        /// </summary>
        /// <typeparam name="T">The type of the local variable for reductions.</typeparam>
        /// <param name="ws">The WorkShare object for state.</param>
        /// <param name="thr">The Thr object for the current thread.</param>
        /// <param name="omp_fn">The function to be executed.</param>
        /// <param name="omp_fn_red">The function to be executed for reductions.</param>
        /// <param name="is_reduction">Whether or not the loop is a reduction loop.</param>
        /// <param name="local">The local variable for reductions.</param>
        private static void DynamicNext<T>(WorkShare ws, Thr thr, Action<int> omp_fn, ActionRef<T> omp_fn_red, bool is_reduction, ref T local)
        {
            int chunk_start;

            lock (ws.ws_lock)
            {
                chunk_start = ws.start;
                ws.Advance((int)ws.chunk_size);
            }

            int chunk_end = (int)Math.Min(chunk_start + ws.chunk_size, ws.end);

            ref int i = ref thr.working_iter;

            if (!is_reduction) for (i = chunk_start; i < chunk_end; i++)
                {
                    omp_fn(i);
                }
            else for (i = chunk_start; i < chunk_end; i++)
                {
                    omp_fn_red(ref local, i);
                }
        }

        /// <summary>
        /// Starts and controls a parallel for loop with guided scheduling.
        /// Guided scheduling works as follows:
        /// Iterations are thrown into a central queue.
        /// When a thread has no current assigned work, it will grab a chunk of iterations from the queue.
        /// This chunk is starts large and decreases in size as the loop progresses.
        /// The chunk size is equal to the number of remaining iterations divided by the number of threads.
        /// The chunk_size variable, found in Init.ws.chunk_size, is used as a minimum chunk size.
        /// When the central queue is empty, each thread will wait at a barrier until all other threads have completed their work.
        /// There is a tradeoff with having a large vs. small chunk size.
        /// A large chunk size will result in less overhead, but may result in load imbalance.
        /// A small chunk size will result in more overhead, but will result in better load balancing.
        /// Guided scheduling is usually a great default choice for parallel for loops,
        /// but may not be adequate if a loop is irregular with heavy load imbalance biased towards the start of the loop.
        /// </summary>
        /// <typeparam name="T">The type of the local variable for reductions.</typeparam>
        /// <param name="ws">The WorkShare object for state.</param>
        /// <param name="omp_fn">The function to be executed.</param>
        /// <param name="omp_fn_red">The function to be executed for reductions.</param>
        /// <param name="is_reduction">Whether or not the loop is a reduction loop.</param>
        internal static void GuidedLoop<T>(WorkShare ws, Action<int> omp_fn, ActionRef<T> omp_fn_red, bool is_reduction)
        {
            Thr thr = ws.thread;
            int end = ws.end;

            T local = default;
            ws.SetLocal(ref local);

            while (ws.start < end)
            {
                GuidedNext(ws, thr, omp_fn, omp_fn_red, is_reduction, ref local);
            }

            ws.Finished();

            ws.AddReductionValue(local);
        }

        /// <summary>
        /// Calculates the next chunk of iterations for a guided scheduling parallel for loop and executes the appropriate function.
        /// Each time this function is called, the calling thread receives a chunk of iterations to work on, as specified in the Iter.GuidedLoop&lt;T&gt; documentation.
        /// </summary>
        /// <typeparam name="T">The type of the local variable for reductions.</typeparam>
        /// <param name="ws">The WorkShare object for state.</param>
        /// <param name="thr">The Thr object for the current thread.</param>
        /// <param name="omp_fn">The function to be executed.</param>
        /// <param name="omp_fn_red">The function to be executed for reductions.</param>
        /// <param name="is_reduction">Whether or not the loop is a reduction loop.</param>
        /// <param name="local">The local variable for reductions.</param>
        private static void GuidedNext<T>(WorkShare ws, Thr thr, Action<int> omp_fn, ActionRef<T> omp_fn_red, bool is_reduction, ref T local)
        {
            int chunk_start, chunk_size;

            lock (ws.ws_lock)
            {
                chunk_start = ws.start;
                chunk_size = (int)Math.Max(ws.chunk_size, (ws.end - chunk_start) / ws.num_threads);

                ws.Advance(chunk_size);
            }

            int chunk_end = Math.Min(chunk_start + chunk_size, ws.end);

            ref int i = ref thr.working_iter;

            if (!is_reduction) for (i = chunk_start; i < chunk_end; i++)
                {
                    omp_fn(i);
                }
            else for (i = chunk_start; i < chunk_end; i++)
                {
                    omp_fn_red(ref local, i);
                }
        }
    }
}
