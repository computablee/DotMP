using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

#nullable enable

namespace DotMP
{
    /// <summary>
    /// Wrapper around integers as a reference type.
    /// </summary>
    internal class IntWrapper
    {
        /// <summary>
        /// Integer to keep track of.
        /// </summary>
        internal volatile int @int;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="int">Value to initialize integer to.</param>
        internal IntWrapper(int @int)
        {
            this.@int = @int;
        }
    }

    /// <summary>
    /// DAG for maintaining task dependencies.
    /// </summary>
    internal class DAG<T, U>
        where T : struct
        where U : class?
    {
        /// <summary>
        /// Counter for remaining tasks in queue.
        /// </summary>
        private volatile int tasks_remaining;
        /// <summary>
        /// Associations from T->U.
        /// </summary>
        private ConcurrentDictionary<T, U> associations;
        /// <summary>
        /// Counts the number of yet-unmet dependencies of a task.
        /// </summary>
        private Dictionary<T, IntWrapper> unmet_dependencies;
        /// <summary>
        /// Keeps track of what task satisfies what dependencies.
        /// </summary>
        private Dictionary<T, List<T>> satisfies_dependency;
        /// <summary>
        /// Bag of items with no dependencies.
        /// </summary>
        private ConcurrentBag<T> no_dependencies;
        /// <summary>
        /// RW lock for managing tasks.
        /// </summary>
        private volatile ReaderWriterLockSlim rw_lock;
        /// <summary>
        /// Keeps track of what items have been completed.
        /// </summary>
        private ConcurrentDictionary<T, T> completed;

        /// <summary>
        /// Default constructor.
        /// </summary>
        internal DAG()
        {
            associations = new ConcurrentDictionary<T, U>();
            unmet_dependencies = new Dictionary<T, IntWrapper>();
            satisfies_dependency = new Dictionary<T, List<T>>();
            no_dependencies = new ConcurrentBag<T>();
            rw_lock = new ReaderWriterLockSlim();
            completed = new ConcurrentDictionary<T, T>();
        }

        /// <summary>
        /// Adds an item to the DAG.
        /// </summary>
        /// <param name="id">The ID of the item to be added.</param>
        /// <param name="item">The item to be added.</param>
        /// <param name="dependencies">A list of ID dependencies.</param>
        internal void AddItem(T id, U item, T[] dependencies)
        {
            rw_lock.EnterWriteLock();

            int dependency_count = dependencies.Length;

            tasks_remaining++;
            associations.TryAdd(id, item);

            foreach (T d in dependencies)
                if (completed.ContainsKey(d))
                    dependency_count--;
                else
                    satisfies_dependency[d].Add(id);

            unmet_dependencies.TryAdd(id, new IntWrapper(dependency_count));

            satisfies_dependency.TryAdd(id, new List<T>());

            if (dependency_count == 0)
                no_dependencies.Add(id);

            rw_lock.ExitWriteLock();
        }

        /// <summary>
        /// Gets the next item from the DAG.
        /// </summary>
        /// <param name="item">The item returned from the DAG.</param>
        /// <param name="id">The ID of the item returned from the DAG.</param>
        /// <param name="tasks_remaining">The number of tasks remaining in the queue.</param>
        /// <returns>Whether or not there was an item to be returned.</returns>
        internal bool GetNextItem(out U? item, out T id, out int tasks_remaining)
        {
            tasks_remaining = this.tasks_remaining;

            if (no_dependencies.TryTake(out id) && associations.TryRemove(id, out item))
            {
                return true;
            }
            else
            {
                item = null;
                return false;
            }
        }

        /// <summary>
        /// Mark an item as completed, in order to remove as a dependency.
        /// </summary>
        /// <param name="id">The ID of the item to be marked completed.</param>
        internal void CompleteItem(T id)
        {
            rw_lock.EnterReadLock();

            Interlocked.Decrement(ref tasks_remaining);
            if (satisfies_dependency[id].Count > 0)
                foreach (T d in satisfies_dependency[id])
                    if (Interlocked.Decrement(ref unmet_dependencies[d].@int) == 0)
                        no_dependencies.Add(d);

            completed.TryAdd(id, id);

            rw_lock.ExitReadLock();
        }
    }
}