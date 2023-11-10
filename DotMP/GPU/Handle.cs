using ILGPU;
using System;

namespace DotMP.GPU
{
    /// <summary>
    /// Handle for a GPU kernel to retrieve its kernel variables.
    /// </summary>
    public struct Index
    {
        /// <summary>
        /// The start of the for loop, for index calculations.
        /// </summary>
        private int start;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="start">The start of the parallel for loop.</param>
        internal Index(int start)
        {
            this.start = start;
        }

        /// <summary>
        /// Gets the index of the loop.
        /// </summary>
        /// <param name="h">Unused.</param>
        public static implicit operator int(Index h)
        {
            return Grid.GlobalIndex.X + h.start;
        }
    }
}