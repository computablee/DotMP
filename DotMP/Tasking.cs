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
        /// Counter for coordinating Parallel.Taskwait(), ensures that all threads have agreed that no more work is to be done before progressing to the barrier.
        /// </summary>
        private static volatile int threads_complete_pv = 0;
        /// <summary>
        /// Ref getter for TaskingContainer.threads_complete_pv.
        /// </summary>
        internal ref int threads_complete
        {
            get
            {
                return ref threads_complete_pv;
            }
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        internal TaskingContainer() { }

        /// <summary>
        /// Gets the next task from the queue.
        /// </summary>
        /// <returns>The next task to execute if there are tasks remaining, otherwise null.</returns>
        internal Action GetNextTask()
        {
            lock (tasks_pv)
            {
                if (tasks_pv.Count > 0)
                    return tasks_pv.Dequeue();
                else
                    return null;
            }
        }

        /// <summary>
        /// Enqueues a task to the queue.
        /// </summary>
        /// <param name="action">The task to enqueue.</param>
        internal void EnqueueTask(Action action)
        {
            lock (tasks_pv)
            {
                tasks_pv.Enqueue(action);
            }
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