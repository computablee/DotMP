using System;
using System.Threading;

namespace DotMP
{
    /// <summary>
    /// Implementation of static scheduling.
    /// </summary>
    internal sealed class StaticScheduler : Schedule
    {
        /// <summary>
        /// The chunk size.
        /// </summary>
        internal uint chunk_size;
        /// <summary>
        /// Number of threads.
        /// </summary>
        internal uint num_threads;
        /// <summary>
        /// Start of the loop, inclusive.
        /// </summary>
        internal int start;
        /// <summary>
        /// End of the loop, exclusive.
        /// </summary>
        internal int end;
        /// <summary>
        /// Bookkeeping to check which iteration each thread is on.
        /// </summary>
        internal int[] curr_iters;

        /// <summary>
        /// Override method for LoopInit, is called when first starting a static loop.
        /// </summary>
        /// <param name="start">The start of the loop, inclusive.</param>
        /// <param name="end">The end of the loop, exclusive.</param>
        /// <param name="num_threads">The number of threads.</param>
        /// <param name="chunk_size">The chunk size.</param>
        public override void LoopInit(int start, int end, uint num_threads, uint chunk_size)
        {
            this.chunk_size = chunk_size;
            this.start = start;
            this.end = end;
            this.num_threads = num_threads;
            curr_iters = new int[num_threads];
            for (int i = 0; i < num_threads; i++)
                curr_iters[i] = start + ((int)chunk_size * i);
        }

        /// <summary>
        /// Override method for LoopNext, is called to get the bounds of the next chunk to execute.
        /// </summary>
        /// <param name="thread_id">The thread ID.</param>
        /// <param name="start">The start of the chunk, inclusive.</param>
        /// <param name="end">The end of the chunk, exclusive.</param>
        public override void LoopNext(int thread_id, out int start, out int end)
        {
            start = curr_iters[thread_id];
            end = Math.Min(start + (int)chunk_size, this.end);
            curr_iters[thread_id] += (int)(chunk_size * num_threads);
        }
    }

    /// <summary>
    /// Implementation of dynamic scheduling.
    /// </summary>
    internal sealed class DynamicScheduler : Schedule
    {
        /// <summary>
        /// The chunk size.
        /// </summary>
        internal uint chunk_size;
        /// <summary>
        /// Number of threads.
        /// </summary>
        internal uint num_threads;
        /// <summary>
        /// Start of the loop, inclusive.
        /// </summary>
        internal int start;
        /// <summary>
        /// End of the loop, exclusive.
        /// </summary>
        internal int end;

        /// <summary>
        /// Override method for LoopInit, is called when first starting a dynamic loop.
        /// </summary>
        /// <param name="start">The start of the loop, inclusive.</param>
        /// <param name="end">The end of the loop, exclusive.</param>
        /// <param name="num_threads">The number of threads.</param>
        /// <param name="chunk_size">The chunk size.</param>
        public override void LoopInit(int start, int end, uint num_threads, uint chunk_size)
        {
            this.chunk_size = chunk_size;
            this.start = start;
            this.end = end;
            this.num_threads = num_threads;
        }

        /// <summary>
        /// Override method for LoopNext, is called to get the bounds of the next chunk to execute.
        /// </summary>
        /// <param name="thread_id">The thread ID.</param>
        /// <param name="start">The start of the chunk, inclusive.</param>
        /// <param name="end">The end of the chunk, exclusive.</param>
        public override void LoopNext(int thread_id, out int start, out int end)
        {
            start = Interlocked.Add(ref this.start, (int)chunk_size) - (int)chunk_size;
            end = Math.Min(start + (int)chunk_size, this.end);
        }
    }

    /// <summary>
    /// Implementation of guided scheduling.
    /// </summary>
    internal sealed class GuidedScheduler : Schedule
    {
        /// <summary>
        /// The chunk size.
        /// </summary>
        internal uint chunk_size;
        /// <summary>
        /// Number of threads.
        /// </summary>
        internal uint num_threads;
        /// <summary>
        /// Start of the loop, inclusive.
        /// </summary>
        internal int start;
        /// <summary>
        /// End of the loop, exclusive.
        /// </summary>
        internal int end;
        /// <summary>
        /// Lock for scheduling purposes.
        /// </summary>
        internal object sched_lock;

        /// <summary>
        /// Override method for LoopInit, is called when first starting a guided loop.
        /// </summary>
        /// <param name="start">The start of the loop, inclusive.</param>
        /// <param name="end">The end of the loop, exclusive.</param>
        /// <param name="num_threads">The number of threads.</param>
        /// <param name="chunk_size">The chunk size.</param>
        public override void LoopInit(int start, int end, uint num_threads, uint chunk_size)
        {
            this.chunk_size = chunk_size;
            this.start = start;
            this.end = end;
            this.num_threads = num_threads;
            this.sched_lock = new object();
        }

        /// <summary>
        /// Override method for LoopNext, is called to get the bounds of the next chunk to execute.
        /// </summary>
        /// <param name="thread_id">The thread ID.</param>
        /// <param name="start">The start of the chunk, inclusive.</param>
        /// <param name="end">The end of the chunk, exclusive.</param>
        public override void LoopNext(int thread_id, out int start, out int end)
        {
            int chunk_size;

            lock (sched_lock)
            {
                start = this.start;
                chunk_size = (int)Math.Max(this.chunk_size, (this.end - start) / (num_threads * 2));

                this.start += chunk_size;
            }

            end = Math.Min(start + chunk_size, this.end);
        }
    }

    /// <summary>
    /// Placeholder for the runtime scheduler.
    /// Is not meant to be called directly. The Parallel.FixArgs method should detect its existence and swap it out for another scheduler with implementations.
    /// </summary>
    internal sealed class RuntimeScheduler : Schedule
    {
        /// <summary>
        /// Should not be called.
        /// </summary>
        /// <param name="start">Unused.</param>
        /// <param name="end">Unused.</param>
        /// <param name="num_threads">Unused.</param>
        /// <param name="chunk_size">Unused.</param>
        /// <exception cref="NotImplementedException">Always thrown.</exception>
        public override void LoopInit(int start, int end, uint num_threads, uint chunk_size)
        {
            throw new NotImplementedException("The runtime scheduler isn't meant to be called directly.");
        }

        /// <summary>
        /// Should not be called.
        /// </summary>
        /// <param name="thread_id">Unused.</param>
        /// <param name="start">Unused.</param>
        /// <param name="end">Unused.</param>
        /// <exception cref="NotImplementedException">Always thrown.</exception>
        public override void LoopNext(int thread_id, out int start, out int end)
        {
            throw new NotImplementedException("The runtime scheduler isn't meant to be called directly.");
        }
    }
}