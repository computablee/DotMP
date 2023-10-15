using System;

namespace DotMP
{
    /// <summary>
    /// Exception thrown if a parallel-only construct is used outside of a parallel region.
    /// </summary>
    public class NotInParallelRegionException : Exception
    {
        /// <summary>
        /// Constructor with a message.
        /// </summary>
        /// <param name="msg">The message to associate with the exception.</param>
        public NotInParallelRegionException(string msg) : base(msg) { }
    }

    /// <summary>
    /// Exception thrown if a Parallel.ParallelRegion is created inside of another Parallel.ParallelRegion.
    /// </summary>
    public class CannotPerformNestedParallelismException : Exception
    {
        /// <summary>
        /// Constructor with a message.
        /// </summary>
        /// <param name="msg">The message to associate with the exception.</param>
        public CannotPerformNestedParallelismException(string msg) : base(msg) { }
    }

    /// <summary>
    /// Exception thrown if a Parallel.Single is created inside of a Parallel.For or Parallel.ForReduction&lt;T&gt;.
    /// </summary>
    public class CannotPerformNestedWorksharingException : Exception
    {
        /// <summary>
        /// Constructor with a message.
        /// </summary>
        /// <param name="msg">The message to associate with the exception.</param>
        public CannotPerformNestedWorksharingException(string msg) : base(msg) { }
    }

    /// <summary>
    /// Exception thrown if a Parallel.Ordered is called outside of Parallel.For or Parallel.ForReduction.
    /// </summary>
    public class NotInForException : Exception
    {
        /// <summary>
        /// Constructor with a message.
        /// </summary>
        /// <param name="msg">The message to associate with the exception.</param>
        public NotInForException(string msg) : base(msg) { }
    }
}