using System;
using System.Collections.Generic;

namespace OpenMP
{
    /// <summary>
    /// Static class that contains necessary information for sections.
    /// </summary>
    internal static class SectionHandler
    {
        /// <summary>
        /// Whether or not the current thread is in a sections region.
        /// </summary>
        internal static bool in_sections = false;

        /// <summary>
        /// The actions submitted by the individual `section` directives.
        /// </summary>
        internal static Queue<Action> actions = new Queue<Action>();

        /// <summary>
        /// The number of actions submitted by the individual `section` directives.
        /// </summary>
        internal static int num_actions = 0;

        /// <summary>
        /// The lock to be used when accessing the actions queue.
        /// </summary>
        internal static object actions_list_lock = new object();
    }
}