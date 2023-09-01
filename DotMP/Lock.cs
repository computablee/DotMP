using System.Threading;

namespace DotMP
{
    /// <summary>
    /// A lock that can be used in a parallel region.
    /// Also contains static methods for locking.
    /// Available methods are Set, Unset, and Test.
    /// </summary>
    public class Lock
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
        /// <param name="lck">The lock to wait for.</param>
        public static void Set(Lock lck)
        {
            while (Interlocked.CompareExchange(ref lck._lock, 1, 0) == 1) ;
        }

        /// <summary>
        /// Unsets the lock.
        /// </summary>
        /// <param name="lck">The lock to unset.</param>
        public static void Unset(Lock lck)
        {
            Interlocked.Exchange(ref lck._lock, 0);
        }

        /// <summary>
        /// Attempts to set the lock.
        /// Does not stall the thread.
        /// </summary>
        /// <param name="lck">The lock to attempt to set.</param>
        /// <returns>True if the lock was set, false otherwise.</returns>
        public static bool Test(Lock lck)
        {
            return Interlocked.CompareExchange(ref lck._lock, 1, 0) == 0;
        }
    }
}
