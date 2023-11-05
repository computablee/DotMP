using System;
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
        private static StaticScheduler static_scheduler = new StaticScheduler();
        /// <summary>
        /// Internal holder for the DynamicScheduler object.
        /// </summary>
        private static DynamicScheduler dynamic_scheduler = new DynamicScheduler();
        /// <summary>
        /// Internal holder for the GuidedScheduler object.
        /// </summary>
        private static GuidedScheduler guided_scheduler = new GuidedScheduler();
        /// <summary>
        /// Internal holder for the RuntimeScheduler object.
        /// </summary>
        private static RuntimeScheduler runtime_scheduler = new RuntimeScheduler();

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
    #endregion
}