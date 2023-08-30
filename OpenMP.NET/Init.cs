using System;
using System.Collections.Generic;
using System.Threading;
using OpenMP;

namespace OpenMP
{
    /// <summary>
    /// Encapsulates a Thread object with information about its progress through a parallel for loop.
    /// </summary>
    internal class Thr
    {
        internal Thread thread;
        volatile internal int curr_iter;
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
    /// </summary>
    internal struct WorkShare
    {
        internal Thr[] threads;
        internal int start;
        internal object ws_lock;
        internal int end;
        internal uint chunk_size;
        internal uint num_threads;
        volatile internal int threads_complete;
        internal Operations? op;
        internal List<dynamic> reduction_list;

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
        }
    }

    /// <summary>
    /// Contains the WorkShare struct.
    /// Surely there's a better way to do this. What was I thinking?
    /// </summary>
    internal static class Init
    {
        internal static WorkShare ws;
    }
}
