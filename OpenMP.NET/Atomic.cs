using System.Threading;

namespace OpenMP
{
    /// <summary>
    /// Provides atomic operations for integral types as a wrapper around the Interlocked class.
    /// Adds support for signed and unsigned 32- and 64-bit integers.
    /// Supports addition, subtraction (for signed types), increment, decrement, bitwise And, and bitwise Or.
    /// </summary>
    public static class Atomic
    {
        #region Int32

        /// <summary>
        /// Adds two 32-bit integers and replaces the first integer with the sum, as an atomic operation.
        /// </summary>
        /// <param name="target">The destination integer to be replaced.</param>
        /// <param name="value">The value to add to the destination integer.</param>
        /// <returns>The new value stored as a result of the operation.</returns>
        public static int Add(ref int target, int value)
        {
            return Interlocked.Add(ref target, value);
        }

        /// <summary>
        /// Subtracts two 32-bit integers and replaces the first integer with the difference, as an atomic operation.
        /// </summary>
        /// <param name="target">The destination integer to be replaced.</param>
        /// <param name="value">The value to subtract from the destination integer.</param>
        /// <returns>The new value stored as a result of the operation.</returns>
        public static int Sub(ref int target, int value)
        {
            return Interlocked.Add(ref target, -value);
        }

        /// <summary>
        /// Increments a specified 32-bit integer by one and stores the result, as an atomic operation.
        /// </summary>
        /// <param name="target">The destination integer to be incremented.</param>
        /// <returns>The new value stored as a result of the operation.</returns>
        public static int Inc(ref int target)
        {
            return Interlocked.Increment(ref target);
        }

        /// <summary>
        /// Decrements a specified 32-bit integer by one and stores the result, as an atomic operation.
        /// </summary>
        /// <param name="target">The destination integer to be decremented.</param>
        /// <returns>The new value stored as a result of the operation.</returns>
        public static int Dec(ref int target)
        {
            return Interlocked.Decrement(ref target);
        }

        /// <summary>
        /// Performs a bitwise And operation on two specified 32-bit integers and replaces the first integer with the result, as an atomic operation.
        /// </summary>
        /// <param name="target">The destination integer to be replaced.</param>
        /// <param name="value">The value to And with the destination integer.</param>
        /// <returns>The new value stored as a result of the operation.</returns>
        public static int And(ref int target, int value)
        {
            return Interlocked.And(ref target, value);
        }

        /// <summary>
        /// Performs a bitwise Or operation on two specified 32-bit integers and replaces the first integer with the result, as an atomic operation.
        /// </summary>
        /// <param name="target">The destination integer to be replaced.</param>
        /// <param name="value">The value to Or with the destination integer.</param>
        /// <returns>The new value stored as a result of the operation.</returns>
        public static int Or(ref int target, int value)
        {
            return Interlocked.Or(ref target, value);
        }
        #endregion

        #region UInt32
        /// <summary>
        /// Adds two 32-bit unsigned integers and replaces the first integer with the sum, as an atomic operation.
        /// </summary>
        /// <param name="target">The destination integer to be replaced.</param>
        /// <param name="value">The value to add to the destination integer.</param>
        /// <returns>The new value stored as a result of the operation.</returns>
        public static uint Add(ref uint target, uint value)
        {
            return Interlocked.Add(ref target, value);
        }

        /// <summary>
        /// Increments a specified 32-bit unsigned integer by one and stores the result, as an atomic operation.
        /// </summary>
        /// <param name="target">The destination integer to be incremented.</param>
        /// <returns>The new value stored as a result of the operation.</returns>
        public static uint Inc(ref uint target)
        {
            return Interlocked.Increment(ref target);
        }

        /// <summary>
        /// Decrements a specified 32-bit unsigned integer by one and stores the result, as an atomic operation.
        /// </summary>
        /// <param name="target">The destination integer to be decremented.</param>
        /// <returns>The new value stored as a result of the operation.</returns>
        public static uint Dec(ref uint target)
        {
            return Interlocked.Decrement(ref target);
        }

        /// <summary>
        /// Performs a bitwise And operation on two specified 32-bit unsigned integers and replaces the first integer with the result, as an atomic operation.
        /// </summary>
        /// <param name="target">The destination integer to be replaced.</param>
        /// <param name="value">The value to And with the destination integer.</param>
        /// <returns>The new value stored as a result of the operation.</returns>
        public static uint And(ref uint target, uint value)
        {
            return Interlocked.And(ref target, value);
        }

        /// <summary>
        /// Performs a bitwise Or operation on two specified 32-bit unsigned integers and replaces the first integer with the result, as an atomic operation.
        /// </summary>
        /// <param name="target">The destination integer to be replaced.</param>
        /// <param name="value">The value to Or with the destination integer.</param>
        /// <returns>The new value stored as a result of the operation.</returns>
        public static uint Or(ref uint target, uint value)
        {
            return Interlocked.Or(ref target, value);
        }
        #endregion

        #region Int64
        /// <summary>
        /// Adds two 64-bit integers and replaces the first integer with the sum, as an atomic operation.
        /// </summary>
        /// <param name="target">The destination integer to be replaced.</param>
        /// <param name="value">The value to add to the destination integer.</param>
        /// <returns>The new value stored as a result of the operation.</returns>
        public static long Add(ref long target, long value)
        {
            return Interlocked.Add(ref target, value);
        }

        /// <summary>
        /// Subtracts two 64-bit integers and replaces the first integer with the difference, as an atomic operation.
        /// </summary>
        /// <param name="target">The destination integer to be replaced.</param>
        /// <param name="value">The value to subtract from the destination integer.</param>
        /// <returns>The new value stored as a result of the operation.</returns>
        public static long Sub(ref long target, long value)
        {
            return Interlocked.Add(ref target, -value);
        }

        /// <summary>
        /// Increments a specified 64-bit integer by one and stores the result, as an atomic operation.
        /// </summary>
        /// <param name="target">The destination integer to be incremented.</param>
        /// <returns>The new value stored as a result of the operation.</returns>
        public static long Inc(ref long target)
        {
            return Interlocked.Increment(ref target);
        }

        /// <summary>
        /// Decrements a specified 64-bit integer by one and stores the result, as an atomic operation.
        /// </summary>
        /// <param name="target">The destination integer to be decremented.</param>
        /// <returns>The new value stored as a result of the operation.</returns>
        public static long Dec(ref long target)
        {
            return Interlocked.Decrement(ref target);
        }

        /// <summary>
        /// Performs a bitwise And operation on two specified 64-bit integers and replaces the first integer with the result, as an atomic operation.
        /// </summary>
        /// <param name="target">The destination integer to be replaced.</param>
        /// <param name="value">The value to And with the destination integer.</param>
        /// <returns>The new value stored as a result of the operation.</returns>
        public static long And(ref long target, long value)
        {
            return Interlocked.And(ref target, value);
        }

        /// <summary>
        /// Performs a bitwise Or operation on two specified 64-bit integers and replaces the first integer with the result, as an atomic operation.
        /// </summary>
        /// <param name="target">The destination integer to be replaced.</param>
        /// <param name="value">The value to Or with the destination integer.</param>
        /// <returns>The new value stored as a result of the operation.</returns>
        public static long Or(ref long target, long value)
        {
            return Interlocked.Or(ref target, value);
        }
        #endregion

        #region UInt64
        /// <summary>
        /// Adds two 64-bit unsigned integers and replaces the first integer with the sum, as an atomic operation.
        /// </summary>
        /// <param name="target">The destination integer to be replaced.</param>
        /// <param name="value">The value to add to the destination integer.</param>
        /// <returns>The new value stored as a result of the operation.</returns>
        public static ulong Add(ref ulong target, ulong value)
        {
            return Interlocked.Add(ref target, value);
        }

        /// <summary>
        /// Increments a specified 64-bit unsigned integer by one and stores the result, as an atomic operation.
        /// </summary>
        /// <param name="target">The destination integer to be incremented.</param>
        /// <returns>The new value stored as a result of the operation.</returns>
        public static ulong Inc(ref ulong target)
        {
            return Interlocked.Increment(ref target);
        }

        /// <summary>
        /// Decrements a specified 64-bit unsigned integer by one and stores the result, as an atomic operation.
        /// </summary>
        /// <param name="target">The destination integer to be decremented.</param>
        /// <returns>The new value stored as a result of the operation.</returns>
        public static ulong Dec(ref ulong target)
        {
            return Interlocked.Decrement(ref target);
        }

        /// <summary>
        /// Performs a bitwise And operation on two specified 64-bit unsigned integers and replaces the first integer with the result, as an atomic operation.
        /// </summary>
        /// <param name="target">The destination integer to be replaced.</param>
        /// <param name="value">The value to And with the destination integer.</param>
        /// <returns>The new value stored as a result of the operation.</returns>
        public static ulong And(ref ulong target, ulong value)
        {
            return Interlocked.And(ref target, value);
        }

        /// <summary>
        /// Performs a bitwise Or operation on two specified 64-bit unsigned integers and replaces the first integer with the result, as an atomic operation.
        /// </summary>
        /// <param name="target">The destination integer to be replaced.</param>
        /// <param name="value">The value to Or with the destination integer.</param>
        /// <returns>The new value stored as a result of the operation.</returns>
        public static ulong Or(ref ulong target, ulong value)
        {
            return Interlocked.Or(ref target, value);
        }
        #endregion
    }
}