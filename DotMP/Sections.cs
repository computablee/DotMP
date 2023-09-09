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
    internal class SectionsContainer
    {
        /// <summary>
        /// The actions submitted by the individual `section` directives.
        /// </summary>
        private static Queue<Action> actions_pv;
        /// <summary>
        /// Getter for singleton queue SectionsContainer.actions_pv.
        /// </summary>
        internal Queue<Action> actions
        {
            get
            {
                return actions_pv;
            }
        }

        /// <summary>
        /// Constructor which takes a list of actions and ensures the master thread assigns to SectionsContainer.actions_pv.
        /// </summary>
        /// <param name="actions">The actions that the Parallel.Sections region will perform.</param>
        internal SectionsContainer(IEnumerable<Action> actions) =>
            Parallel.Master(() => actions_pv = new Queue<Action>(actions));
    }
}