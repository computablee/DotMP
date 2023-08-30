using System;
using System.Threading;

namespace OpenMP
{
    /// <summary>
    /// Contains all of the scheduling code for parallel for loops.
    /// </summary>
    internal static class Iter
    {
        /// <summary>
        /// Sets the local variable to the appropriate value based on the operation for parallel for reduction loops.
        /// </summary>
        /// <typeparam name="T">The type of the local variable.</typeparam>
        /// <param name="local">The local variable to be set.</param>
        /// <param name="op">The operation to be performed.</param>
        internal static void SetLocal<T>(ref T local, Operations? op)
        {
            switch (Init.ws.op)
            {
                case Operations.Add:
                case Operations.Subtract:
                    local = (T)Convert.ChangeType(0, typeof(T));
                    break;
                case Operations.Multiply:
                    local = (T)Convert.ChangeType(1, typeof(T));
                    break;
                case Operations.BinaryAnd:
                    local = (T)Convert.ChangeType(-1, typeof(T));
                    break;
                case Operations.BinaryOr:
                    local = (T)Convert.ChangeType(0, typeof(T));
                    break;
                case Operations.BinaryXor:
                    local = (T)Convert.ChangeType(0, typeof(T));
                    break;
                case Operations.BooleanAnd:
                    local = (T)Convert.ChangeType(true, typeof(T));
                    break;
                case Operations.BooleanOr:
                    local = (T)Convert.ChangeType(false, typeof(T));
                    break;
                case Operations.Min:
                    local = (T)Convert.ChangeType(int.MaxValue, typeof(T));
                    break;
                case Operations.Max:
                    local = (T)Convert.ChangeType(int.MinValue, typeof(T));
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Starts and controls a parallel for loop with static scheduling.
        /// </summary>
        /// <typeparam name="T">The type of the local variable for reductions.</typeparam>
        /// <param name="thread_id">The thread ID.</param>
        /// <param name="omp_fn">The function to be executed.</param>
        /// <param name="omp_fn_red">The function to be executed for reductions.</param>
        /// <param name="is_reduction">Whether or not the loop is a reduction loop.</param>
        internal static void StaticLoop<T>(object thread_id, Action<int> omp_fn, ActionRef<T> omp_fn_red, bool is_reduction)
        {
            int tid = (int)thread_id;
            Thr thr = Init.ws.threads[tid];

            thr.curr_iter = (int)(Init.ws.start + tid * Init.ws.chunk_size);
            int end = Init.ws.end;

            T local = default;
            SetLocal(ref local, Init.ws.op);

            while (thr.curr_iter < end)
                StaticNext(thr, tid, Init.ws.chunk_size, omp_fn, omp_fn_red, is_reduction, ref local);

            Interlocked.Add(ref Init.ws.threads_complete, 1);

            lock (Init.ws.reduction_list)
            {
                Init.ws.reduction_list.Add(local);
            }
        }

        /// <summary>
        /// Calculates the next chunk of iterations for a static scheduling parallel for loop and executes the appropriate function.
        /// </summary>
        /// <typeparam name="T">The type of the local variable for reductions.</typeparam>
        /// <param name="thr">The Thr object for the current thread.</param>
        /// <param name="thread_id">The thread ID.</param>
        /// <param name="chunk_size">The chunk size.</param>
        /// <param name="omp_fn">The function to be executed.</param>
        /// <param name="omp_fn_red">The function to be executed for reductions.</param>
        /// <param name="is_reduction">Whether or not the loop is a reduction loop.</param>
        /// <param name="local">The local variable for reductions.</param>
        private static void StaticNext<T>(Thr thr, int thread_id, uint chunk_size, Action<int> omp_fn, ActionRef<T> omp_fn_red, bool is_reduction, ref T local)
        {
            int start = thr.curr_iter;
            int end = (int)Math.Min(thr.curr_iter + chunk_size, Init.ws.end);

            ref int i = ref thr.working_iter;

            if (!is_reduction) for (i = start; i < end; i++)
                {
                    omp_fn(i);
                }
            else for (i = start; i < end; i++)
                {
                    omp_fn_red(ref local, i);
                }

            thr.curr_iter += (int)(Init.ws.num_threads * chunk_size);
        }

        /// <summary>
        /// Starts and controls a parallel for loop with dynamic scheduling.
        /// </summary>
        /// <typeparam name="T">The type of the local variable for reductions.</typeparam>
        /// <param name="thread_id">The thread ID.</param>
        /// <param name="omp_fn">The function to be executed.</param>
        /// <param name="omp_fn_red">The function to be executed for reductions.</param>
        /// <param name="is_reduction">Whether or not the loop is a reduction loop.</param>
        internal static void DynamicLoop<T>(object thread_id, Action<int> omp_fn, ActionRef<T> omp_fn_red, bool is_reduction)
        {
            int tid = (int)thread_id;
            Thr thr = Init.ws.threads[tid];
            int end = Init.ws.end;

            T local = default;
            SetLocal(ref local, Init.ws.op);

            while (Init.ws.start < end)
            {
                DynamicNext(thr, omp_fn, omp_fn_red, is_reduction, ref local);
            }

            Interlocked.Add(ref Init.ws.threads_complete, 1);

            lock (Init.ws.reduction_list)
            {
                Init.ws.reduction_list.Add(local);
            }
        }

        /// <summary>
        /// Calculates the next chunk of iterations for a dynamic scheduling parallel for loop and executes the appropriate function.
        /// </summary>
        /// <typeparam name="T">The type of the local variable for reductions.</typeparam>
        /// <param name="thr">The Thr object for the current thread.</param>
        /// <param name="omp_fn">The function to be executed.</param>
        /// <param name="omp_fn_red">The function to be executed for reductions.</param>
        /// <param name="is_reduction">Whether or not the loop is a reduction loop.</param>
        /// <param name="local">The local variable for reductions.</param>
        private static void DynamicNext<T>(Thr thr, Action<int> omp_fn, ActionRef<T> omp_fn_red, bool is_reduction, ref T local)
        {
            int chunk_start;

            lock (Init.ws.ws_lock)
            {
                chunk_start = Init.ws.start;
                Init.ws.start += (int)Init.ws.chunk_size;
            }

            int chunk_end = (int)Math.Min(chunk_start + Init.ws.chunk_size, Init.ws.end);

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
        /// </summary>
        /// <typeparam name="T">The type of the local variable for reductions.</typeparam>
        /// <param name="thread_id">The thread ID.</param>
        /// <param name="omp_fn">The function to be executed.</param>
        /// <param name="omp_fn_red">The function to be executed for reductions.</param>
        /// <param name="is_reduction">Whether or not the loop is a reduction loop.</param>
        internal static void GuidedLoop<T>(object thread_id, Action<int> omp_fn, ActionRef<T> omp_fn_red, bool is_reduction)
        {
            int tid = (int)thread_id;
            Thr thr = Init.ws.threads[tid];
            int end = Init.ws.end;

            T local = default;
            SetLocal(ref local, Init.ws.op);

            while (Init.ws.start < end)
            {
                GuidedNext(thr, omp_fn, omp_fn_red, is_reduction, ref local);
            }

            Interlocked.Add(ref Init.ws.threads_complete, 1);

            lock (Init.ws.reduction_list)
            {
                Init.ws.reduction_list.Add(local);
            }
        }

        /// <summary>
        /// Calculates the next chunk of iterations for a guided scheduling parallel for loop and executes the appropriate function.
        /// </summary>
        /// <typeparam name="T">The type of the local variable for reductions.</typeparam>
        /// <param name="thr">The Thr object for the current thread.</param>
        /// <param name="omp_fn">The function to be executed.</param>
        /// <param name="omp_fn_red">The function to be executed for reductions.</param>
        /// <param name="is_reduction">Whether or not the loop is a reduction loop.</param>
        /// <param name="local">The local variable for reductions.</param>
        private static void GuidedNext<T>(Thr thr, Action<int> omp_fn, ActionRef<T> omp_fn_red, bool is_reduction, ref T local)
        {
            int chunk_start, chunk_size;

            lock (Init.ws.ws_lock)
            {
                chunk_start = Init.ws.start;
                chunk_size = (int)Math.Max(Init.ws.chunk_size, (Init.ws.end - chunk_start) / Init.ws.num_threads);
                Init.ws.start += chunk_size;
            }

            int chunk_end = (int)Math.Min(chunk_start + chunk_size, Init.ws.end);

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
