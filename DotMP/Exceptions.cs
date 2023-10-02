using System;

namespace DotMP
{
    /// <summary>
    /// Exception thrown if a parallel-only construct is used outside of a parallel region.
    /// </summary>
    public class NotInParallelRegionException : Exception
    {
        public NotInParallelRegionException() { }

        public NotInParallelRegionException(string msg) : base(msg) { }

        public NotInParallelRegionException(string msg, Exception ex) : base(msg, ex) { }
    }

    /// <summary>
    /// Exception thrown if a sections-only construct is used outside of a sections region.
    /// </summary>
    public class NotInSectionsRegionException : Exception
    {
        public NotInSectionsRegionException() { }

        public NotInSectionsRegionException(string msg) : base(msg) { }

        public NotInSectionsRegionException(string msg, Exception ex) : base(msg, ex) { }
    }

    /// <summary>
    /// Exception thrown if a Parallel.ParallelRegion is created inside of another Parallel.ParallelRegion.
    /// </summary>
    public class CannotPerformNestedParallelismException : Exception
    {
        public CannotPerformNestedParallelismException() { }

        public CannotPerformNestedParallelismException(string msg) : base(msg) { }

        public CannotPerformNestedParallelismException(string msg, Exception ex) : base(msg, ex) { }
    }

    /// <summary>
    /// Exception thrown if a Parallel.For or Parallel.ForReduction<T> is created inside of another Parallel.For or Parallel.ForReduction<T>.
    /// </summary>
    public class CannotPerformNestedForException : Exception
    {
        public CannotPerformNestedForException() { }

        public CannotPerformNestedForException(string msg) : base(msg) { }

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