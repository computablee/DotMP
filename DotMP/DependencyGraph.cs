using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

#nullable enable

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
    private ConcurrentDictionary<T, IntWrapper> unmet_dependencies;
    /// <summary>
    /// Keeps track of what task satisfies what dependencies.
    /// </summary>
    private ConcurrentDictionary<T, List<T>> satisfies_dependency;
    /// <summary>
    /// Bag of items with no dependencies.
    /// </summary>
    private ConcurrentBag<T> no_dependencies;

    /// <summary>
    /// Default constructor.
    /// </summary>
    internal DAG()
    {
        associations = new ConcurrentDictionary<T, U>();
        unmet_dependencies = new ConcurrentDictionary<T, IntWrapper>();
        satisfies_dependency = new ConcurrentDictionary<T, List<T>>();
        no_dependencies = new ConcurrentBag<T>();
    }

    /// <summary>
    /// Adds an item to the DAG.
    /// </summary>
    /// <param name="id">The ID of the item to be added.</param>
    /// <param name="item">The item to be added.</param>
    /// <param name="dependencies">A list of ID dependencies.</param>
    internal void AddItem(T id, U item, T[] dependencies)
    {
        Interlocked.Increment(ref tasks_remaining);
        associations.TryAdd(id, item);
        unmet_dependencies.TryAdd(id, new IntWrapper(dependencies.Length));

        foreach (T d in dependencies)
            lock (satisfies_dependency[d])
                satisfies_dependency[d].Add(id);

        satisfies_dependency.TryAdd(id, new List<T>());

        if (dependencies.Length == 0)
            no_dependencies.Add(id);
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
        Interlocked.Decrement(ref tasks_remaining);
        if (satisfies_dependency[id].Count > 0)
            lock (satisfies_dependency[id])
                foreach (T d in satisfies_dependency[id])
                    if (Interlocked.Decrement(ref unmet_dependencies[d].@int) == 0)
                        no_dependencies.Add(d);
    }
}