using System;

namespace OpenMP
{
    /// <summary>
    /// Exception thrown if a parallel-only construct is used outside of a parallel region.
    /// </summary>
    class NotInParallelRegionException : Exception
    {
        public NotInParallelRegionException() { }

        public NotInParallelRegionException(string msg) : base(msg) { }

        public NotInParallelRegionException(string msg, Exception ex) : base(msg, ex) { }
    }

    /// <summary>
    /// Exception thrown if a sections-only construct is used outside of a sections region.
    /// </summary>
    class NotInSectionsRegionException : Exception
    {
        public NotInSectionsRegionException() { }

        public NotInSectionsRegionException(string msg) : base(msg) { }

        public NotInSectionsRegionException(string msg, Exception ex) : base(msg, ex) { }
    }
}