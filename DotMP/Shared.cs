using System;
using System.Collections.Generic;

namespace DotMP
{
    /// <summary>
    /// A shared variable that can be used in a parallel region.
    /// This allows for a variable to be declared inside of a parallel region that is shared among all threads, which has some nice use cases.
    /// The DotMP-parallelized Conjugate Gradient example shows this off fairly well inside of the SpMV function.
    /// </summary>
    /// <typeparam name="T">The type of the shared variable.</typeparam>
    public class Shared<T> : IDisposable
    {
        /// <summary>
        /// The shared variables.
        /// </summary>
        private static Dictionary<string, dynamic> shared = new Dictionary<string, dynamic>();

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
            DotMP.Parallel.Master(() =>
            {
                shared[name] = value;
            });
            this.name = name;
            this.Disposed = false;

            DotMP.Parallel.Barrier();
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
            if (!Disposed)
            {
                if (disposing)
                {
                    DotMP.Parallel.Master(() => shared.Remove(name));
                    DotMP.Parallel.Barrier();
                }

                Disposed = true;
            }
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
}