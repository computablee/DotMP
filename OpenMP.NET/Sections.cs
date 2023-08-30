using System;
using System.Collections.Generic;

namespace OpenMP
{
    /// <summary>
    /// Static class that contains necessary information for sections.
    /// </summary>
    internal static class SectionHandler
    {
        internal static bool in_sections = false;

        internal static Queue<Action> actions = new Queue<Action>();

        internal static int num_actions = 0;

        internal static object actions_list_lock = new object();
    }
}