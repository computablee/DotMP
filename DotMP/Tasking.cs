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
        /// Getter for singleton queue TaskingContainer.tasks_pv.
        /// </summary>
        internal Queue<Action> tasks
        {
            get
            {
                return tasks_pv;
            }
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        internal TaskingContainer() { }
    }
}