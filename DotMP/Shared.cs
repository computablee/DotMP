using System;
using System.Collections.Generic;

namespace DotMP
{
    /// <summary>
    /// A shared variable that can be used in a parallel region.
    /// This allows for a variable to be declared inside of a parallel region that is shared among all threads, which has some nice use cases.
    /// </summary>
    /// <typeparam name="T">The type of the shared variable.</typeparam>
    public class Shared<T> : IDisposable
    {
        /// <summary>
        /// The shared variables.
        /// </summary>
        protected static Dictionary<string, dynamic> shared = new Dictionary<string, dynamic>();

        /// <summary>
        /// The name of the shared variable.
        /// </summary>
        private string name;

        /// <summary>
        /// Whether or not the shared variable has been disposed.
        /// </summary>
        public bool Disposed { get; private set; }

        /// <summary>
        /// Creates a new shared variable with the given name and value.
        /// Must be called from all threads in the parallel region.
        /// Acts as a barrier.
        /// </summary>
        /// <param name="name">Name of the shared variable.</param>
        /// <param name="value">Initial starting value of the shared variable.</param>
        public Shared(string name, T value)
        {
            Parallel.Master(() =>
            {
                shared[name] = value;
            });
            this.name = name;
            this.Disposed = false;

            Parallel.Barrier();
        }

        /// <summary>
        /// Clears the shared variable from memory.
        /// Must be called from all threads in the parallel region.
        /// Acts as a barrier.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Clears the shared variable from memory.
        /// Virtual implementation for IDisposable interface.
        /// </summary>
        /// <param name="disposing">Whether or not to dispose of the shared variable.</param>
        public virtual void Dispose(bool disposing)
        {
            Parallel.Barrier();
            if (!Disposed)
            {
                if (disposing)
                {
                    Parallel.Master(() => shared.Remove(name));
                    Parallel.Barrier();
                }

                Disposed = true;
            }
        }

        /// <summary>
        /// Gets the value of the shared variable.
        /// </summary>
        /// <param name="shared">The shared variable to get the value from.</param>
        public static implicit operator T(Shared<T> shared)
        {
            return shared.Get();
        }

        /// <summary>
        /// Sets the value of the shared variable.
        /// Is not thread-safe, so user must ensure thread safety.
        /// </summary>
        /// <param name="value">The new value of the shared variable.</param>
        public void Set(T value)
        {
            shared[name] = value;
        }

        /// <summary>
        /// Gets the value of the shared variable.
        /// </summary>
        /// <returns>The value of the shared variable.</returns>
        public T Get()
        {
            return shared[name];
        }
    }

    /// <summary>
    /// Factory class for Shared&lt;T&gt;
    /// </summary>
    public static class Shared
    {
        /// <summary>
        /// Factory method for creating shared variables.
        /// </summary>
        /// <typeparam name="T">The type of the shared variable to create.</typeparam>
        /// <param name="name">The name of the shared variable.</param>
        /// <param name="value">The value of the shared variable.</param>
        /// <returns>A Shared object.</returns>
        public static Shared<T> Create<T>(string name, T value)
        {
            return new Shared<T>(name, value);
        }
    }

    /// <summary>
    /// A specialization of Shared for items that can be indexed with square brackets.
    /// The DotMP-parallelized Conjugate Gradient example shows this off fairly well inside of the SpMV function.
    /// </summary>
    /// <typeparam name="T">The type that the IList is containing.</typeparam>
    /// <typeparam name="U">The full IList type.</typeparam>
    public class SharedEnumerable<T, U> : Shared<U> where U : IList<T>
    {
        /// <summary>
        /// Constructs a new shared variable with the given name and value.
        /// </summary>
        /// <param name="name">The name of the shared variable.</param>
        /// <param name="value">The value of the shared variable.</param>
        public SharedEnumerable(string name, U value) : base(name, value) { }

        /// <summary>
        /// Allows for indexing into the shared variable with square brackets.
        /// </summary>
        /// <param name="index">The index to fetch.</param>
        /// <returns>The value at that index.</returns>
        public T this[int index]
        {
            get => Get()[index];
            set => Get()[index] = value;
        }

        /// <summary>
        /// Allows for implicit conversion to an array.
        /// </summary>
        /// <param name="shared">A SharedEnumerable object.</param>
        public static implicit operator U(SharedEnumerable<T, U> shared)
        {
            return shared.Get();
        }

        /// <summary>
        /// Clears the shared variable from memory.
        /// Must be called from all threads in the parallel region.
        /// Acts as a barrier.
        /// </summary>
        public new void Dispose()
        {
            base.Dispose();
        }

        /// <summary>
        /// Gets the value of the shared variable as an IList&lt;T&gt;.
        /// </summary>
        /// <returns>The value of the shared variable as an IList&lt;T&gt;.</returns>
        public new U Get()
        {
            return base.Get();
        }
    }

    /// <summary>
    /// The factory class for SharedEnumerable&lt;T, U&gt;
    /// </summary>
    public static class SharedEnumerable
    {
        /// <summary>
        /// Factory method for creating shared arrays.
        /// </summary>
        /// <typeparam name="T">The type the array contains.</typeparam>
        /// <param name="name">The name of the shared enumerable.</param>
        /// <param name="value">Initial starting value of the shared enumerable.</param>
        /// <returns>A SharedEnumerable object.</returns>
        public static SharedEnumerable<T, T[]> Create<T>(string name, T[] value)
        {
            return new SharedEnumerable<T, T[]>(name, value);
        }

        /// <summary>
        /// Factory method for creating shared Lists.
        /// </summary>
        /// <typeparam name="T">The type the array contains.</typeparam>
        /// <param name="name">The name of the shared enumerable.</param>
        /// <param name="value">Initial starting value of the shared enumerable.</param>
        /// <returns>A SharedEnumerable object.</returns>
        public static SharedEnumerable<T, List<T>> Create<T>(string name, List<T> value)
        {
            return new SharedEnumerable<T, List<T>>(name, value);
        }
    }
}