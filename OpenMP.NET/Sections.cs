using System;
using System.Collections.Generic;

namespace OpenMP
{
    /// <summary>
    /// Static class that contains necessary information for sections.
    /// Sections allow for the user to submit multiple actions to be executed in parallel.
    /// A sections region contains a collection of actions to be executed, specified as Parallel.Section directives.
    /// More information can be found in the Parallel.Sections documentation.
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