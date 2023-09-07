using System.Collections.Generic;
using System.Threading;
using System;

namespace DotMP
{
    /// <summary>
    /// Encapsulates a Thread object with information about its progress through a parallel for loop.
    /// For keeping track of its progress through a parallel for loop, we keep track of the current next iteration of the loop to be worked on, and the iteration the current thread is currently working on.
    /// </summary>
    internal class Thr
    {
        /// <summary>
        /// The Thread object to be encapsulated.
        /// </summary>
        internal Thread thread;
        /// <summary>
        /// The current iteration of the parallel for loop.
        /// </summary>
        volatile internal int curr_iter;
        /// <summary>
        /// The iteration the thread is currently working on.
        /// </summary>
        internal int working_iter;

        /// <summary>
        /// Creates a Thr object with the specified Thread object.
        /// </summary>
        /// <param name="thread">The Thread object to be encapsulated.</param>
        internal Thr(Thread thread)
        {
            this.thread = thread;
            curr_iter = 0;
            working_iter = 0;
        }
    }

    /// <summary>
    /// Contains all relevant information about a parallel for loop.
    /// Contains a collection of Thr objects, the loop's start and end iterations, the chunk size, the number of threads, and the number of threads that have completed their work.
    /// </summary>
    internal struct WorkShare
    {
        /// <summary>
        /// The threads to be used in the parallel for loop.
        /// </summary>
        internal Thr[] threads;
        /// <summary>
        /// The starting iteration of the parallel for loop, inclusive.
        /// </summary>
        internal int start;
        /// <summary>
        /// A generic lock to be used within the parallel for loop.
        /// </summary>
        internal object ws_lock;
        /// <summary>
        /// The ending iteration of the parallel for loop, exclusive.
        /// </summary>
        internal int end;
        /// <summary>
        /// The chunk size to be used with the selected scheduler.
        /// </summary>
        internal uint chunk_size;
        /// <summary>
        /// The number of threads to be used in the parallel for loop.
        /// </summary>
        internal uint num_threads;
        /// <summary>
        /// The number of threads that have completed their work.
        /// </summary>
        volatile internal int threads_complete;
        /// <summary>
        /// The operation to be performed if doing a reduction.
        /// </summary>
        internal Operations? op;
        /// <summary>
        /// The list of reduction variables from each thread.
        /// </summary>
        internal List<dynamic> reduction_list;
        /// <summary>
        /// The schedule to be used in the parallel for loop.
        /// </summary>
        internal Schedule? schedule;

        /// <summary>
        /// The constructor for a WorkShare object.
        /// </summary>
        /// <param name="num_threads">The number of threads in the WorkShare.</param>
        /// <param name="threads">The Thread[] array of threads.</param>
        /// <param name="start">Starting iteration, inclusive.</param>
        /// <param name="end">Ending iteration, exclusive.</param>
        /// <param name="chunk_size">The chunk size to use.</param>
        /// <param name="op">The operation for reduction, null if not a reduction.</param>
        /// <param name="schedule">The Parallel.Schedule to use.</param>
        internal WorkShare(uint num_threads, Thread[] threads, int start, int end, uint chunk_size, Operations? op, Schedule schedule)
        {
            this.threads = new Thr[num_threads];
            for (int i = 0; i < num_threads; i++)
                this.threads[i] = new Thr(threads[i]);
            threads_complete = 0;
            ws_lock = new object();
            this.start = start;
            this.end = end;
            this.chunk_size = chunk_size;
            this.num_threads = num_threads;
            this.op = op;
            this.reduction_list = new List<dynamic>();
            this.schedule = schedule;
        }
    }

    /// <summary>
    /// Contains the WorkShare struct.
    /// Surely there's a better way to do this. What was I thinking?
    /// </summary>
    internal static class Init
    {
        /// <summary>
        /// The WorkShare struct being encapsulated by the Init static class.
        /// </summary>
        internal static WorkShare ws;

        /// <summary>
        /// Sets the local variable to the appropriate value based on the operation for parallel for reduction loops.
        /// For addition and subtraction, the initial starting value is 0.
        /// For multiplication, the initial starting value is 1.
        /// For binary And, the initial starting value is -1.
        /// For binary Or and Xor, the initial starting value is 0.
        /// For boolean And, the initial starting value is true.
        /// For boolean Or, the initial starting value is false.
        /// For min, the initial starting value is int.MaxValue.
        /// For max, the initial starting value is int.MinValue.
        /// </summary>
        /// <typeparam name="T">The type of the local variable.</typeparam>
        /// <param name="local">The local variable to be set.</param>
        internal static void SetLocal<T>(ref T local)
        {
            switch (ws.op)
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
    }
}
