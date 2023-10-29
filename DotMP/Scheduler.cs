namespace DotMP
{
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
}