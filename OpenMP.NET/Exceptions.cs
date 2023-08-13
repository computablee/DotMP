using System;

namespace OpenMP
{
    class NotInParallelRegionException : Exception
    {
        public NotInParallelRegionException() { }

        public NotInParallelRegionException(string msg) : base(msg) { }

        public NotInParallelRegionException(string msg, Exception ex) : base(msg, ex) { }
    }

    class NotInSectionsRegionException : Exception
    {
        public NotInSectionsRegionException() { }

        public NotInSectionsRegionException(string msg) : base(msg) { }

        public NotInSectionsRegionException(string msg, Exception ex) : base(msg, ex) { }
    }
}