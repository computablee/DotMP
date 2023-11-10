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