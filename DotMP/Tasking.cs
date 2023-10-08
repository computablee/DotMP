using System;
using System.Linq;
using System.Threading;

namespace DotMP
{
    /// <summary>
    /// A simple container for a Queue&lt;Action&gt; for managing tasks.
    /// Will grow in complexity as dependencies are added and a dependency graph must be generated.
    /// </summary>
    internal class TaskingContainer
    {
        /// <summary>
        /// DAG of tasks that must execute.
        /// We use a DAG in order to maintain dependency chains.
        /// </summary>
        private static DAG<ulong, Action> dag = new DAG<ulong, Action>();

        /// <summary>
        /// Default constructor.
        /// </summary>
        internal TaskingContainer() { }

        /// <summary>
        /// Resets the DAG to a default state.
        /// Allows the garbage collector to collect unused data.
        /// </summary>
        internal void ResetDAG()
        {
            Parallel.Master(() =>
            {
                dag.Dispose();
                dag = new DAG<ulong, Action>();
            });
        }

        /// <summary>
        /// Gets the next task from the queue.
        /// </summary>
        /// <param name="action">The body of the task to be executed.</param>
        /// <param name="uuid">The UUID of the task to be executed.</param>
        /// <param name="tasks_remaining">The number of tasks remaining in the queue.</param>
        /// <returns>Whether or not the action was successful.</returns>
        internal bool GetNextTask(out Action action, out ulong uuid, out int tasks_remaining)
        {
            return dag.GetNextItem(out action, out uuid, out tasks_remaining);
        }

        /// <summary>
        /// Enqueues a task to the queue.
        /// </summary>
        /// <param name="action">The task to enqueue.</param>
        /// <param name="depends">List of task dependencies.</param>
        /// <returns>Task generated from the enqueue task action.</returns>
        internal TaskUUID EnqueueTask(Action action, TaskUUID[] depends)
        {
            TaskUUID taskUUID = new TaskUUID();
            ulong[] ids = (from d in depends select d.GetUUID()).ToArray();
            dag.AddItem(taskUUID.GetUUID(), action, ids);
            return taskUUID;
        }

        /// <summary>
        /// Enqueues a taskloop task to the task queue.
        /// </summary>
        /// <param name="start">The start of the current taskloop task, inclusive.</param>
        /// <param name="end">The end of the current taskloop task, exclusive.</param>
        /// <param name="action">The action to be executed.</param>
        /// <param name="depends">List of task dependencies.</param>
        /// <returns>Task generated from this iteration of the taskloop.</returns>
        internal TaskUUID EnqueueTaskloopTask(int start, int end, Action<int> action, TaskUUID[] depends)
        {
            Action loop_action = () =>
            {
                for (int i = start; i < end; i++)
                {
                    action(i);
                }
            };

            return EnqueueTask(loop_action, depends);
        }

        /// <summary>
        /// Mark task as completed to remove as a dependency in the DAG.
        /// </summary>
        /// <param name="uuid">UUID of task to remove as a dependency.</param>
        internal void CompleteTask(ulong uuid)
        {
            dag.CompleteItem(uuid);
        }
    }

    /// <summary>
    /// Task UUID as returned from Parallel.Task.
    /// </summary>
    public sealed class TaskUUID
    {
        /// <summary>
        /// Global counter for next UUID to be generated.
        /// </summary>
        private static ulong next_uuid = 0;
        /// <summary>
        /// This task's UUID.
        /// </summary>
        private readonly ulong uuid;

        /// <summary>
        /// Default constructor.
        /// Initializes this task's UUID to the next valid UUID.
        /// </summary>
        internal TaskUUID()
        {
            uuid = Interlocked.Increment(ref next_uuid);
        }

        /// <summary>
        /// Gets this task's UUID.
        /// </summary>
        /// <returns>This task's UUID.</returns>
        internal ulong GetUUID()
        {
            return uuid;
        }
    }
}