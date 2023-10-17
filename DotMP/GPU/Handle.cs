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
        /// Gets the index of the loop.
        /// </summary>
        /// <param name="h">Unused.</param>
        public static implicit operator int(Index h)
        {
            return Grid.GlobalIndex.X;
        }
    }
}