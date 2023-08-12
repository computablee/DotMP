using System.Threading;

namespace OpenMP
{
    public class Lock
    {
        volatile internal int _lock;

        public Lock()
        {
            _lock = 0;
        }
    }

    public static class Locking
    {
        public static void Set(Lock lck)
        {
            while (Interlocked.CompareExchange(ref lck._lock, 1, 0) == 1) ;
        }

        public static void Unset(Lock lck)
        {
            Interlocked.Exchange(ref lck._lock, 0);
        }

        public static bool Test(Lock lck)
        {
            return Interlocked.CompareExchange(ref lck._lock, 1, 0) == 0;
        }
    }
}
