using System;
using System.Threading;
using System.Linq.Expressions;

namespace OpenMP
{
    public static class Atomic
    {
        #region Int32
        public static int Add(ref int target, int value)
        {
            return Interlocked.Add(ref target, value);
        }

        public static int Sub(ref int target, int value)
        {
            return Interlocked.Add(ref target, -value);
        }

        public static int Inc(ref int target)
        {
            return Interlocked.Increment(ref target);
        }

        public static int Dec(ref int target)
        {
            return Interlocked.Decrement(ref target);
        }

        public static int And(ref int target, int value)
        {
            return Interlocked.And(ref target, value);
        }

        public static int Or(ref int target, int value)
        {
            return Interlocked.Or(ref target, value);
        }
        #endregion

        #region UInt32
        public static uint Add(ref uint target, uint value)
        {
            return Interlocked.Add(ref target, value);
        }

        public static uint Inc(ref uint target)
        {
            return Interlocked.Increment(ref target);
        }

        public static uint Dec(ref uint target)
        {
            return Interlocked.Decrement(ref target);
        }

        public static uint And(ref uint target, uint value)
        {
            return Interlocked.And(ref target, value);
        }

        public static uint Or(ref uint target, uint value)
        {
            return Interlocked.Or(ref target, value);
        }
        #endregion

        #region Int64
        public static long Add(ref long target, long value)
        {
            return Interlocked.Add(ref target, value);
        }

        public static long Sub(ref long target, long value)
        {
            return Interlocked.Add(ref target, -value);
        }

        public static long Inc(ref long target)
        {
            return Interlocked.Increment(ref target);
        }

        public static long Dec(ref long target)
        {
            return Interlocked.Decrement(ref target);
        }

        public static long And(ref long target, long value)
        {
            return Interlocked.And(ref target, value);
        }

        public static long Or(ref long target, long value)
        {
            return Interlocked.Or(ref target, value);
        }
        #endregion

        #region UInt64
        public static ulong Add(ref ulong target, ulong value)
        {
            return Interlocked.Add(ref target, value);
        }

        public static ulong Inc(ref ulong target)
        {
            return Interlocked.Increment(ref target);
        }

        public static ulong Dec(ref ulong target)
        {
            return Interlocked.Decrement(ref target);
        }

        public static ulong And(ref ulong target, ulong value)
        {
            return Interlocked.And(ref target, value);
        }

        public static ulong Or(ref ulong target, ulong value)
        {
            return Interlocked.Or(ref target, value);
        }
        #endregion
    }
}