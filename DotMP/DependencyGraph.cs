using System;
using System.Collections.Concurrent;
using DotMP;

/// <summary>
/// DAG for maintaining task dependencies.
/// </summary>
internal class DAG
{
    /// <summary>
    /// Associations from ulong->Action.
    /// </summary>
    private ConcurrentDictionary<ulong, Action> associations;
    /// <summary>
    /// Dictionary representing the relationship "X depends on Y[]".
    /// </summary>
    private ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, ulong>> depends_on;
    /// <summary>
    /// Dictionary representing the relationship "Y precedes X[]".
    /// </summary>
    private ConcurrentDictionary<ulong, ConcurrentBag<ulong>> precedes;
    /// <summary>
    /// Bag of items with no dependencies.
    /// </summary>
    private ConcurrentBag<ulong> no_dependencies;

    /// <summary>
    /// Default constructor.
    /// </summary>
    internal DAG()
    {
        associations = new ConcurrentDictionary<ulong, Action>();
        depends_on = new ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, ulong>>();
        precedes = new ConcurrentDictionary<ulong, ConcurrentBag<ulong>>();
        no_dependencies = new ConcurrentBag<ulong>();
    }

    /// <summary>
    /// Adds an item to the DAG.
    /// </summary>
    /// <param name="id">The ID of the item to be added.</param>
    /// <param name="item">The item to be added.</param>
    /// <param name="dependencies">A list of task dependencies.</param>
    internal void AddItem(ulong id, Action item, TaskUUID[] dependencies)
    {
        ConcurrentDictionary<ulong, ulong> dependency_list = new ConcurrentDictionary<ulong, ulong>();

        precedes.TryAdd(id, new ConcurrentBag<ulong>());

        foreach (TaskUUID d in dependencies)
        {
            dependency_list.TryAdd(d.GetUUID(), d.GetUUID());
            precedes[d.GetUUID()].Add(id);
        }
        depends_on.TryAdd(id, dependency_list);

        associations.TryAdd(id, item);

        if (dependencies.Length == 0)
        {
            no_dependencies.Add(id);
        }
    }

    /// <summary>
    /// Gets the next item from the DAG.
    /// </summary>
    /// <param name="item">The item returned from the DAG.</param>
    /// <param name="id">The ID of the item returned from the DAG.</param>
    /// <returns>Whether or not there was an item to be returned.</returns>
    internal bool GetNextItem(out Action item, out ulong id)
    {
        if (no_dependencies.TryTake(out id) && associations.TryGetValue(id, out item))
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
    internal void CompleteItem(ulong id)
    {
        foreach (ulong i in precedes[id])
        {
            depends_on[i].TryRemove(i, out _);
            if (depends_on[i].Count == 0)
            {
                no_dependencies.Add(i);
            }
        }

        precedes.TryRemove(id, out _);
        depends_on.TryRemove(id, out _);
        associations.TryRemove(id, out _);
    }
}