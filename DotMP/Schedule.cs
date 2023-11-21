/*
 * DotMP - A collection of powerful abstractions for parallel programming in .NET with an OpenMP-like API. 
 * Copyright (C) 2023 Phillip Allen Lane
 *
 * This library is free software; you can redistribute it and/or modify it under the terms of the GNU Lesser
 * General Public License as published by the Free Software Foundation; either version 2.1 of the License, or
 * (at your option) any later version.
 *
 * This library is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the
 * implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public
 * License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License along with this library; if not,
 * write to the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.
 */

using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace DotMP
{
    #region ScheduleInterface
    /// <summary>
    /// Interface for user-defined schedulers.
    /// </summary>
    public interface IScheduler
    {
        /// <summary>
        /// Called before each worksharing parallel-for loop.
        /// Used to instantiate scheduler variables.
        /// </summary>
        /// <param name="start">The start of the loop, inclusive.</param>
        /// <param name="end">The end of the loop, exclusive.</param>
        /// <param name="num_threads">The number of threads.</param>
        /// <param name="chunk_size">Provided chunk size.</param>
        public void LoopInit(int start, int end, uint num_threads, uint chunk_size);

        /// <summary>
        /// Called between each chunk to calculate the bounds of the next chunk.
        /// </summary>
        /// <param name="thread_id">The thread ID to provide a chunk to.</param>
        /// <param name="start">The start of the chunk, inclusive.</param>
        /// <param name="end">The end of the chunk, exclusive.</param>
        public void LoopNext(int thread_id, out int start, out int end);
    }
    #endregion

    #region ScheduleClass
    /// <summary>
    /// Represents the various scheduling strategies for parallel for loops.
    /// Detailed explanations of each scheduling strategy are provided alongside each getter.
    /// If no schedule is specified, the default is <see cref="Schedule.Static"/>.
    /// </summary>
    public abstract class Schedule : IScheduler
    {
        /// <summary>
        /// Internal holder for StaticScheduler object.
        /// </summary>
        private static Schedulers.StaticScheduler static_scheduler = new Schedulers.StaticScheduler();
        /// <summary>
        /// Internal holder for the DynamicScheduler object.
        /// </summary>
        private static Schedulers.DynamicScheduler dynamic_scheduler = new Schedulers.DynamicScheduler();
        /// <summary>
        /// Internal holder for the GuidedScheduler object.
        /// </summary>
        private static Schedulers.GuidedScheduler guided_scheduler = new Schedulers.GuidedScheduler();
        /// <summary>
        /// Internal holder for the RuntimeScheduler object.
        /// </summary>
        private static Schedulers.RuntimeScheduler runtime_scheduler = new Schedulers.RuntimeScheduler();
        /// <summary>
        /// Internal holder for the WorkStealingScheduler object.
        /// </summary>
        private static Schedulers.WorkStealingScheduler workstealing_scheduler = new Schedulers.WorkStealingScheduler();

        /// <summary>
        /// The static scheduling strategy.
        /// Iterations are divided amongst threads in round-robin fashion.
        /// Each thread gets a 'chunk' of iterations, determined by the chunk size.
        /// If no chunk size is specified, it's computed as total iterations divided by number of threads.
        /// 
        /// Pros:
        /// - Reduced overhead.
        /// 
        /// Cons:
        /// - Potential for load imbalance.
        /// 
        /// Note: This is the default strategy if none is chosen.
        /// </summary>
        public static Schedule Static { get => static_scheduler; }

        /// <summary>
        /// The dynamic scheduling strategy.
        /// Iterations are managed in a central queue.
        /// Threads fetch chunks of iterations from this queue when they have no assigned work.
        /// If no chunk size is defined, a basic heuristic is used to determine a chunk size.
        /// 
        /// Pros:
        /// - Better load balancing.
        /// 
        /// Cons:
        /// - Increased overhead.
        /// </summary>
        public static Schedule Dynamic { get => dynamic_scheduler; }

        /// <summary>
        /// The guided scheduling strategy.
        /// Similar to dynamic, but the chunk size starts larger and shrinks as iterations are consumed.
        /// The shrinking formula is based on the remaining iterations divided by the number of threads.
        /// The chunk size parameter sets a minimum chunk size.
        /// 
        /// Pros:
        /// - Adaptable to workloads.
        /// 
        /// Cons:
        /// - Might not handle loops with early heavy load imbalance efficiently.
        /// </summary>
        public static Schedule Guided { get => guided_scheduler; }

        /// <summary>
        /// Runtime-defined scheduling strategy.
        /// Schedule is determined by the 'OMP_SCHEDULE' environment variable.
        /// Expected format: "schedule[,chunk_size]", e.g., "static,128", "guided", or "dynamic,3".
        /// </summary>
        public static Schedule Runtime { get => runtime_scheduler; }

        /// <summary>
        /// The work-stealing scheduling strategy.
        /// Each thread gets its own local queue of iterations to execute.
        /// If a thread's queue is empty, it randomly selects another thread's queue as its "victim" and steals half of its remaining iterations.
        /// The chunk size parameter specifies how many iterations a thread should execute from its local queue at a time.
        /// 
        /// Pros:
        /// - Good approximation of optimal load balancing.
        /// - No contention over a shared queue.
        /// 
        /// Cons:
        /// - Stealing can be an expensive operation.
        /// </summary>
        public static Schedule WorkStealing { get => workstealing_scheduler; }

        /// <summary>
        /// Abstract method for builtin schedulers to override for implementing IScheduler.
        /// </summary>
        /// <param name="start">The start of the loop, inclusive.</param>
        /// <param name="end">The end of the loop, exclusive.</param>
        /// <param name="num_threads">The number of threads.</param>
        /// <param name="chunk_size">The chunk size.</param>
        public abstract void LoopInit(int start, int end, uint num_threads, uint chunk_size);

        /// <summary>
        /// Abstract method for builtin schedulers to override for implementing IScheduler.
        /// </summary>
        /// <param name="thread_id">The thread ID.</param>
        /// <param name="start">The start of the chunk, inclusive.</param>
        /// <param name="end">The end of the chunk, exclusive.</param>
        public abstract void LoopNext(int thread_id, out int start, out int end);
    }
    #endregion

    #region Schedulers
    namespace Schedulers
    {
    /// <summary>
    /// Implementation of static scheduling.
    /// </summary>
    internal sealed class StaticScheduler : Schedule
    {
        /// <summary>
        /// Struct to ensure that the curr_iter variables cannot reside on the same cache line.
        /// Avoids false sharing bottlenecks.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 64, Size = 64)]
        private struct IterWrapper
        {
            /// <summary>
            /// A thread's current iteration.
            /// </summary>
            internal int curr_iter;
        }
        /// <summary>
        /// The chunk size.
        /// </summary>
        private uint chunk_size;
        /// <summary>
        /// End of the loop, exclusive.
        /// </summary>
        private int end;
        /// <summary>
        /// Bookkeeping to check which iteration each thread is on.
        /// </summary>
        private IterWrapper[] curr_iters;
        /// <summary>
        /// How much to advance by after each chunk.
        /// </summary>
        private int advance_by;

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
            this.end = end;
            advance_by = (int)(chunk_size * num_threads);
            curr_iters = new IterWrapper[num_threads];
            for (int i = 0; i < num_threads; i++)
                curr_iters[i].curr_iter = start + ((int)chunk_size * i);
        }

        /// <summary>
        /// Override method for LoopNext, is called to get the bounds of the next chunk to execute.
        /// </summary>
        /// <param name="thread_id">The thread ID.</param>
        /// <param name="start">The start of the chunk, inclusive.</param>
        /// <param name="end">The end of the chunk, exclusive.</param>
        public override void LoopNext(int thread_id, out int start, out int end)
        {
            start = curr_iters[thread_id].curr_iter;
            end = Math.Min(start + (int)chunk_size, this.end);
            curr_iters[thread_id].curr_iter += advance_by;
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
        private uint chunk_size;
        /// <summary>
        /// Start of the loop, inclusive.
        /// </summary>
        private int start;
        /// <summary>
        /// End of the loop, exclusive.
        /// </summary>
        private int end;

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
        private uint chunk_size;
        /// <summary>
        /// Number of threads.
        /// </summary>
        private uint num_threads;
        /// <summary>
        /// Start of the loop, inclusive.
        /// </summary>
        private int start;
        /// <summary>
        /// End of the loop, exclusive.
        /// </summary>
        private int end;
        /// <summary>
        /// Lock for scheduling purposes.
        /// </summary>
        private object sched_lock;

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

    /// <summary>
    /// Implementation of work-stealing scheduling.
    /// </summary>
    internal sealed class WorkStealingScheduler : Schedule
    {
        /// <summary>
        /// Queue struct, ensuring that no two values share a cache line.
        /// This avoids false sharing issues.
        /// </summary>
        [StructLayout(LayoutKind.Explicit, Size = 256)]
        private struct Queue
        {
            /// <summary>
            /// Start of the queue.
            /// </summary>
            [FieldOffset(0)] internal int start;
            /// <summary>
            /// End of the queue.
            /// </summary>
            [FieldOffset(64)] internal int end;
            /// <summary>
            /// Whether or not there is work remaining in the queue.
            /// </summary>
            [FieldOffset(128)] internal bool work_remaining;
            /// <summary>
            /// Lock for this queue.
            /// </summary>
            [FieldOffset(192)] internal object qlock;
        }

        /// <summary>
        /// Each thread's queue.
        /// </summary>
        private Queue[] queues;
        /// <summary>
        /// Chunk size to use.
        /// </summary>
        private uint chunk_size;
        /// <summary>
        /// Number of threads.
        /// </summary>
        private uint num_threads;
        /// <summary>
        /// Counts the remaining threads with work so threads know when to stop attempting to steal.
        /// </summary>
        private volatile uint threads_with_remaining_work;

        /// <summary>
        /// Override method for LoopInit, is called when first starting a work-stealing loop.
        /// </summary>
        /// <param name="start">The start of the loop, inclusive.</param>
        /// <param name="end">The end of the loop, exclusive.</param>
        /// <param name="num_threads">The number of threads.</param>
        /// <param name="chunk_size">The chunk size.</param>
        public override void LoopInit(int start, int end, uint num_threads, uint chunk_size)
        {
            this.queues = new Queue[num_threads];
            this.chunk_size = chunk_size;
            this.threads_with_remaining_work = num_threads;
            this.num_threads = num_threads;

            int ctr = start;
            int div = (end - start) / (int)num_threads;

            for (int i = 0; i < num_threads - 1; i++)
            {
                queues[i].start = ctr;
                ctr += div;
                queues[i].end = ctr;
                queues[i].work_remaining = true;
                queues[i].qlock = new object();
            }

            queues[num_threads - 1].start = ctr;
            queues[num_threads - 1].end = end;
            queues[num_threads - 1].work_remaining = true;
            queues[num_threads - 1].qlock = new object();
        }

        /// <summary>
        /// Override method for LoopNext, is called to get the bounds of the next chunk to execute.
        /// </summary>
        /// <param name="thread_id">The thread ID.</param>
        /// <param name="start">The start of the chunk, inclusive.</param>
        /// <param name="end">The end of the chunk, exclusive.</param>
        public override void LoopNext(int thread_id, out int start, out int end)
        {
            do
            {
                lock (queues[thread_id].qlock)
                {
                    start = queues[thread_id].start;
                    end = Math.Min(start + (int)chunk_size, queues[thread_id].end);

                    if (start < end)
                    {
                        queues[thread_id].start += (int)chunk_size;
                        return;
                    }
                }

                StealHandler(thread_id);
            }
            while (threads_with_remaining_work > 0);
        }

        /// <summary>
        /// Handles whether or not to steal and how to process the results from a steal.
        /// </summary>
        /// <param name="thread_id">The thread ID.</param>
        private void StealHandler(int thread_id)
        {
            if (queues[thread_id].work_remaining)
            {
                Interlocked.Decrement(ref threads_with_remaining_work);
                queues[thread_id].work_remaining = false;
            }

            if (DoSteal(thread_id))
            {
                Interlocked.Increment(ref threads_with_remaining_work);
                queues[thread_id].work_remaining = true;
            }
        }

        /// <summary>
        /// Perform a steal.
        /// </summary>
        /// <param name="thread_id">The thread ID.</param>
        /// <returns>Whether or not the steal was successful.</returns>
        private bool DoSteal(int thread_id)
        {
            int rng = Random.Shared.Next((int)num_threads);
            int new_start, new_end;

            lock (queues[rng].qlock)
            {
                if (queues[rng].start < queues[rng].end)
                {
                    int steal_size = (queues[rng].end - queues[rng].start + 1) / 2;

                    new_start = queues[rng].start;
                    new_end = queues[rng].start + steal_size;

                    queues[rng].start = new_end;
                }
                else
                {
                    return false;
                }
            }

            lock (queues[thread_id].qlock)
            {
                queues[thread_id].start = new_start;
                queues[thread_id].end = new_end;
            }

            return true;
        }
    }
    }
    #endregion
}
