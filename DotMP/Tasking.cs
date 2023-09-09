using System;
using System.Collections.Generic;

namespace DotMP
{
    /// <summary>
    /// A simple container for a Queue<Action> for managing tasks.
    /// Will grow in complexity as dependencies are added and a dependency graph must be generated.
    /// </summary>
    internal class TaskingContainer
    {
        /// <summary>
        /// Queue of tasks that must execute.
        /// </summary>
        private static Queue<Action> tasks_pv = new Queue<Action>();

        /// <summary>
        /// Default constructor.
        /// </summary>
        internal TaskingContainer() { }

        /// <summary>
        /// Gets the next task from the queue.
        /// </summary>
        /// <returns>
        /// An (Action, bool) tuple.
        /// The Action is the next task to execute if there are tasks remaining, otherwise null.
        /// The bool represents whether or not there was a task remaining to return.
        /// </returns>
        internal (Action, bool) GetNextTask()
        {
            lock (tasks_pv)
            {
                if (tasks_pv.Count > 0)
                    return (tasks_pv.Dequeue(), true);
                else
                    return (null, false);
            }
        }

        /// <summary>
        /// Enqueues a task to the queue.
        /// </summary>
        /// <param name="action">The task to enqueue.</param>
        internal void EnqueueTask(Action action)
        {
            tasks_pv.Enqueue(action);
        }

        /// <summary>
        /// Enqueues a taskloop task to the task queue.
        /// </summary>
        /// <param name="start">The start of the current taskloop task, inclusive.</param>
        /// <param name="end">The end of the current taskloop task, exclusive.</param>
        /// <param name="action">The action to be executed.</param>
        internal void EnqueueTaskloopTask(int start, int end, Action<int> action)
        {
            Action loop_action = () =>
            {
                for (int i = start; i < end; i++)
                {
                    action(i);
                }
            };

            EnqueueTask(loop_action);
        }
    }
}