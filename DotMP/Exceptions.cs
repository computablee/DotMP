using System;

namespace DotMP
{
    /// <summary>
    /// Exception thrown if a parallel-only construct is used outside of a parallel region.
    /// </summary>
    public class NotInParallelRegionException : Exception
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public NotInParallelRegionException() { }

        /// <summary>
        /// Constructor with a message.
        /// </summary>
        /// <param name="msg">The message to associate with the exception.</param>
        public NotInParallelRegionException(string msg) : base(msg) { }

        /// <summary>
        /// Constructor with a message and inner exception.
        /// </summary>
        /// <param name="msg">The message to associate with the exception.</param>
        /// <param name="ex">The inner exception.</param>
        public NotInParallelRegionException(string msg, Exception ex) : base(msg, ex) { }
    }

    /// <summary>
    /// Exception thrown if a Parallel.ParallelRegion is created inside of another Parallel.ParallelRegion.
    /// </summary>
    public class CannotPerformNestedParallelismException : Exception
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public CannotPerformNestedParallelismException() { }

        /// <summary>
        /// Constructor with a message.
        /// </summary>
        /// <param name="msg">The message to associate with the exception.</param>
        public CannotPerformNestedParallelismException(string msg) : base(msg) { }

        /// <summary>
        /// Constructor with a message and inner exception.
        /// </summary>
        /// <param name="msg">The message to associate with the exception.</param>
        /// <param name="ex">The inner exception.</param>
        public CannotPerformNestedParallelismException(string msg, Exception ex) : base(msg, ex) { }
    }

    /// <summary>
    /// Exception thrown if a Parallel.For or Parallel.ForReduction&lt;T&gt; is created inside of another Parallel.For or Parallel.ForReduction&lt;T&gt;.
    /// </summary>
    public class CannotPerformNestedForException : Exception
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public CannotPerformNestedForException() { }

        /// <summary>
        /// Constructor with a message.
        /// </summary>
        /// <param name="msg">The message to associate with the exception.</param>
        public CannotPerformNestedForException(string msg) : base(msg) { }

        /// <summary>
        /// Constructor with a message and inner exception.
        /// </summary>
        /// <param name="msg">The message to associate with the exception.</param>
        /// <param name="ex">The inner exception.</param>
        public CannotPerformNestedForException(string msg, Exception ex) : base(msg, ex) { }
    }

    /// <summary>
    /// Exception thrown if a Parallel.Single is created inside of a Parallel.For or Parallel.ForReduction&lt;T&gt;.
    /// </summary>
    public class CannotPerformNestedWorksharingException : Exception
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public CannotPerformNestedWorksharingException() { }

        /// <summary>
        /// Constructor with a message.
        /// </summary>
        /// <param name="msg">The message to associate with the exception.</param>
        public CannotPerformNestedWorksharingException(string msg) : base(msg) { }

        /// <summary>
        /// Constructor with a message and inner exception.
        /// </summary>
        /// <param name="msg">The message to associate with the exception.</param>
        /// <param name="ex">The inner exception.</param>
        public CannotPerformNestedWorksharingException(string msg, Exception ex) : base(msg, ex) { }
    }
}