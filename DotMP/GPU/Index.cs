/*
* DotMP - A collection of powerful abstractions for parallel programming in .NET with an OpenMP-like API. 
* Copyright (C) 2023 Phillip Allen Lane
*
* This library is free software; you can redistribute it and/or modify it under the terms of the GNU Lesser
* General Public License as published by the Free Software Foundation; either version 2.1 of the License, or
* (at your option) any later version.
*
* This library is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the
* implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public
* License for more details.
*
* You should have received a copy of the GNU Lesser General Public License along with this library; if not,
* write to the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.
*/

using ILGPU;
using ILGPU.Runtime;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml;

namespace DotMP.GPU
{
    /// <summary>
    /// Represents an index passed as the first index argument.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public struct Index
    {
        /// <summary>
        /// Lookup table for indices.
        /// </summary>
        private ArrayView1D<int, Stride1D.Dense> lookup;
        /// <summary>
        /// Offset for followup kernels.
        /// </summary>
        private int offset;
        /// <summary>
        /// Cached index.
        /// </summary>
        private int idx;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="buf">Buffer representing the indices.</param>
        internal Index(Buffer<int> buf)
        {
            lookup = buf.View;
            offset = 0;
            idx = -1;
        }

        /// <summary>
        /// Adds an offset in preperation for a followup kernel.
        /// </summary>
        /// <param name="offset">The offset to set.</param>
        internal void AddOffset(int offset)
        {
            this.offset = offset;
        }

        /// <summary>
        /// Calculates the index and caches for future use.
        /// </summary>
        /// <param name="i">The Index object to cast to int.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator int(Index i)
        {
            if (i.idx == -1)
                i.idx = i.lookup[Grid.GlobalLinearIndex + i.offset];

            return i.idx;
        }
    }
}
