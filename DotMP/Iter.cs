namespace DotMP
{
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
}
