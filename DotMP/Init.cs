using System.Collections.Generic;
using System.Threading;

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
        internal Parallel.Schedule? schedule;

        /// <summary>
        /// Creates a WorkShare struct.
        /// </summary>
        /// <param name="num_threads">The number of threads to be used in the parallel for loop.</param>
        /// <param name="threads">The Thread objects to be used in the parallel for loop.</param>
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
            this.op = null;
            this.reduction_list = new List<dynamic>();
            this.schedule = null;
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
    }
}
