using System.Threading;

namespace DotMP
{
    /// <summary>
    /// A lock that can be used in a parallel region.
    /// Also contains instance methods for locking.
    /// Available methods are Set, Unset, and Test.
    /// </summary>
    public sealed class Lock
    {
        /// <summary>
        /// The int acting as the lock.
        /// </summary>
        volatile internal int _lock;

        /// <summary>
        /// Constructs a new lock.
        /// </summary>
        public Lock()
        {
            _lock = 0;
        }

        /// <summary>
        /// Stalls the thread until the lock is set.
        /// </summary>
        public void Set()
        {
            while (Interlocked.CompareExchange(ref this._lock, 1, 0) == 1) ;
        }

        /// <summary>
        /// Unsets the lock.
        /// </summary>
        public void Unset()
        {
            Interlocked.Exchange(ref this._lock, 0);
        }

        /// <summary>
        /// Attempts to set the lock.
        /// Does not stall the thread.
        /// </summary>
        /// <returns>True if the lock was set, false otherwise.</returns>
        public bool Test()
        {
            return Interlocked.CompareExchange(ref this._lock, 1, 0) == 0;
        }
    }
}
