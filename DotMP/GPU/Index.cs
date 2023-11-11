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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;

namespace DotMP.GPU
{
    /// <summary>
    /// Handle for a GPU kernel to retrieve its kernel variables.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public struct Index
    {
        /// <summary>
        /// The start of the for loop, for index calculations.
        /// </summary>
        private int start1;
        private int start2;

        private int i_prv;
        private int j_prv;

        private int diff;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="start">The start of the parallel for loop.</param>
        internal Index(int start)
        {
            this.start1 = start;
            this.start2 = 0;
            i_prv = -1;
            j_prv = -1;
            diff = 0;
        }

        internal Index((int, int)[] ranges)
        {
            start1 = ranges[0].Item1;
            start2 = ranges[1].Item1;
            i_prv = -1;
            j_prv = -1;
            diff = ranges[1].Item2 - ranges[1].Item1;
        }

        /// <summary>
        /// Gets the index of the loop.
        /// </summary>
        /// <param name="h">Unused.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator int(Index h)
        {
            return Grid.GlobalLinearIndex + h.start1;
        }

        public int i
        {
            get
            {
                if (i_prv == -1)
                {
                    i_prv = IntrinsicMath.DivRoundDown(Grid.GlobalLinearIndex, diff);
                    j_prv = Grid.GlobalLinearIndex - i_prv * diff;
                    i_prv += start1;
                    j_prv += start2;
                }

                return i_prv;
            }
        }

        public int j
        {
            get
            {
                if (j_prv == -1)
                {
                    i_prv = IntrinsicMath.DivRoundDown(Grid.GlobalLinearIndex, diff);
                    j_prv = Grid.GlobalLinearIndex - i_prv * diff;
                    i_prv += start1;
                    j_prv += start2;
                }

                return j_prv;
            }
        }
    }
}