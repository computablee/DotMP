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
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace DotMP.GPU
{
    /// <summary>
    /// Represents an index passed as the first index argument.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public struct IndexI
    {
        /// <summary>
        /// The start of the first for loop, for index calculations.
        /// </summary>
        private int start1;

        /// <summary>
        /// The start of the second for loop, for index calculations.
        /// </summary>
        private int start2;

        /// <summary>
        /// The start of the third for loop, for index calculations.
        /// </summary>
        private int start3;

        /// <summary>
        /// The index to return.
        /// </summary>
        private int idx_prv;

        /// <summary>
        /// The difference between the second set of ranges.
        /// </summary>
        private int diff2;

        /// <summary>
        /// The difference between the third set of ranges.
        /// </summary>
        private int diff3;

        /// <summary>
        /// The offset, in case of a followup kernel.
        /// </summary>
        private int offset;

        /// <summary>
        /// The number of dimensions.
        /// </summary>
        private int dims;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="range">The range of the for loop.</param>
        /// <param name="offset">The offset for followup kernels.</param>
        internal IndexI((int, int) range, int offset = 0)
        {
            this.offset = offset;

            start1 = range.Item1;
            start2 = -1;
            start3 = -1;
            idx_prv = -1;
            diff2 = -1;
            diff3 = -1;
            dims = 1;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="range1">The outer range of the for loop.</param>
        /// <param name="range2">The inner range of the for loop.</param>
        /// <param name="offset">The offset for followup kernels.</param>
        internal IndexI((int, int) range1, (int, int) range2, int offset = 0)
        {
            this.offset = offset;

            start1 = range1.Item1;
            start2 = range2.Item1;
            start3 = -1;
            idx_prv = -1;
            diff2 = range2.Item2 - range2.Item1;
            diff3 = -1;
            dims = 2;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="range1">The outer range of the for loop.</param>
        /// <param name="range2">The middle range of the for loop.</param>
        /// <param name="range3">The inner range of the for loop.</param>
        /// <param name="offset">The offset for followup kernels.</param>
        internal IndexI((int, int) range1, (int, int) range2, (int, int) range3, int offset = 0)
        {
            this.offset = offset;

            start1 = range1.Item1;
            start2 = range2.Item1;
            start3 = range3.Item1;
            idx_prv = -1;
            diff2 = range2.Item2 - range2.Item1;
            diff3 = range3.Item2 - range3.Item1;
            dims = 3;
        }

        /// <summary>
        /// Casts an index to an int.
        /// </summary>
        /// <param name="h">The Index struct to cast.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator int(IndexI h)
        {
            switch (h.dims)
            {
                default:
                case 1:
                    if (h.idx_prv == -1)
                        h.idx_prv = Grid.GlobalLinearIndex + h.start1 + h.offset;

                    return h.idx_prv;

                case 2:
                    if (h.idx_prv == -1)
                    {
                        int idxoffset = Grid.GlobalLinearIndex + h.offset;
                        h.idx_prv = IntrinsicMath.DivRoundDown(idxoffset, h.diff2) + h.start1;
                    }

                    return h.idx_prv;

                case 3:
                    if (h.idx_prv == -1)
                    {

                    }

                    return h.idx_prv;
            }
        }
    }

    /// <summary>
    /// Represents an index passed as the second index argument.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public struct IndexJ
    {
        /// <summary>
        /// The start of the first for loop, for index calculations.
        /// </summary>
        private int start1;

        /// <summary>
        /// The start of the second for loop, for index calculations.
        /// </summary>
        private int start2;

        /// <summary>
        /// The start of the third for loop, for index calculations.
        /// </summary>
        private int start3;

        /// <summary>
        /// The index to return.
        /// </summary>
        private int idx_prv;

        /// <summary>
        /// The difference between the second set of ranges.
        /// </summary>
        private int diff2;

        /// <summary>
        /// The difference between the third set of ranges.
        /// </summary>
        private int diff3;

        /// <summary>
        /// The offset, in case of a followup kernel.
        /// </summary>
        private int offset;

        /// <summary>
        /// The number of dimensions.
        /// </summary>
        private int dims;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="range1">The outer range of the for loop.</param>
        /// <param name="range2">The inner range of the for loop.</param>
        /// <param name="offset">The offset for followup kernels.</param>
        internal IndexJ((int, int) range1, (int, int) range2, int offset = 0)
        {
            this.offset = offset;

            start1 = range1.Item1;
            start2 = range2.Item1;
            start3 = -1;
            idx_prv = -1;
            diff2 = range2.Item2 - range2.Item1;
            diff3 = -1;
            dims = 2;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="range1">The outer range of the for loop.</param>
        /// <param name="range2">The middle range of the for loop.</param>
        /// <param name="range3">The inner range of the for loop.</param>
        /// <param name="offset">The offset for followup kernels.</param>
        internal IndexJ((int, int) range1, (int, int) range2, (int, int) range3, int offset = 0)
        {
            this.offset = offset;

            start1 = range1.Item1;
            start2 = range2.Item1;
            start3 = range3.Item1;
            idx_prv = -1;
            diff2 = range2.Item2 - range2.Item1;
            diff3 = range3.Item2 - range3.Item1;
            dims = 3;
        }

        /// <summary>
        /// Casts an index to an int.
        /// </summary>
        /// <param name="h">The Index struct to cast.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator int(IndexJ h)
        {
            switch (h.dims)
            {
                default:
                case 2:
                    if (h.idx_prv == -1)
                    {
                        int idxoffset = Grid.GlobalLinearIndex + h.offset;
                        h.idx_prv = (idxoffset % h.diff2) + h.start2;
                    }

                    return h.idx_prv;

                case 3:
                    if (h.idx_prv == -1)
                    {

                    }

                    return h.idx_prv;
            }
        }
    }

    /// <summary>
    /// Represents an index passed as the third index argument.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public struct IndexK
    {
        /// <summary>
        /// The start of the first for loop, for index calculations.
        /// </summary>
        private int start1;

        /// <summary>
        /// The start of the second for loop, for index calculations.
        /// </summary>
        private int start2;

        /// <summary>
        /// The start of the third for loop, for index calculations.
        /// </summary>
        private int start3;

        /// <summary>
        /// The index to return.
        /// </summary>
        private int idx_prv;

        /// <summary>
        /// The difference between the second set of ranges.
        /// </summary>
        private int diff2;

        /// <summary>
        /// The difference between the third set of ranges.
        /// </summary>
        private int diff3;

        /// <summary>
        /// The offset, in case of a followup kernel.
        /// </summary>
        private int offset;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="range1">The outer range of the for loop.</param>
        /// <param name="range2">The middle range of the for loop.</param>
        /// <param name="range3">The inner range of the for loop.</param>
        /// <param name="offset">The offset for followup kernels.</param>
        internal IndexK((int, int) range1, (int, int) range2, (int, int) range3, int offset = 0)
        {
            this.offset = offset;

            start1 = range1.Item1;
            start2 = range2.Item1;
            start3 = range3.Item1;
            idx_prv = -1;
            diff2 = range2.Item2 - range2.Item1;
            diff3 = range3.Item2 - range3.Item1;
        }

        /// <summary>
        /// Casts an index to an int.
        /// </summary>
        /// <param name="h">The Index struct to cast.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator int(IndexK h)
        {
            if (h.idx_prv == -1)
            {

            }

            return h.idx_prv;
        }
    }
}