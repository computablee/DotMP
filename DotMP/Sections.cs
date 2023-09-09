using System;
using System.Collections.Generic;

namespace DotMP
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
        /// The actions submitted by the individual `section` directives.
        /// </summary>
        internal static Queue<Action> actions = new Queue<Action>();
    }
}