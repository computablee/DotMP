/*
 * DotMP - A collection of powerful abstractions for parallel programming in .NET with an OpenMP-like API. 
 * Copyright (C) 2023 Phillip Allen Lane
 *
 * This library is free software; you can redistribute it and/or modify it under the terms of the GNU Lesser
 * General Public License as published by the Free Software Foundation; either version 2.1 of the License, or
 * (at your option) any later version.
 *
 * This library is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the
 * implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public
 * License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License along with this library; if not,
 * write to the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.
 */

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
    internal class DAG<T, U> : IDisposable
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

        /// <summary>
        /// Override to implement IDisposable, disposes of the read-write lock.
        /// </summary>
        public void Dispose()
        {
            rw_lock.Dispose();
        }

        /// <summary>
        /// Determines if a task has been completed.
        /// </summary>
        /// <param name="id">The ID of the task to check completion.</param>
        /// <returns>Whether or not the task has been completed.</returns>
        internal bool TaskIsComplete(T id)
        {
            return completed.ContainsKey(id);
        }
    }
}
