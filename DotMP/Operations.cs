/*
 * DotMP - A collection of powerful abstractions for parallel programming in .NET with an OpenMP-like API. 
 * Copyright (C) 2023 Phillip Allen Lane
 *
 * This library is free software; you can redistribute it and/or modify it under the terms of the GNU Lesser
 * General Public License as published by the Free Software Foundation; either version 2.1 of the License, or
 * (at your option) any later version.

 * This library is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the
 * implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public
 * License for more details.

 * You should have received a copy of the GNU Lesser General Public License along with this library; if not,
 * write to the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.
 * Also add information on how to contact you by electronic and paper mail. 
 */

namespace DotMP
{
    /// <summary>
    /// Enum that represents the different operations that can be used in a for-reduction.
    /// The operations are Add, Subtract, Multiply, BinaryAnd, BinaryOr, BinaryXor, BooleanAnd, BooleanOr, Min, and Max.
    /// </summary>
    public enum Operations
    {
        /// <summary>
        /// Represents a reduction using the '+' operator.
        /// </summary>
        Add,
        /// <summary>
        /// Represents a reduction using the '-' operator.
        /// </summary>
        Subtract,
        /// <summary>
        /// Represents a reduction using the '*' operator.
        /// </summary>
        Multiply,
        /// <summary>
        /// Represents a reduction using the '&amp;' operator;
        /// </summary>
        BinaryAnd,
        /// <summary>
        /// Represents a reduction using the '|' operator.
        /// </summary>
        BinaryOr,
        /// <summary>
        /// Represents a reduction using the '^' operator.
        /// </summary>
        BinaryXor,
        /// <summary>
        /// Represents a reduction using the '&amp;&amp;' operator.
        /// </summary>
        BooleanAnd,
        /// <summary>
        /// Represents a reduction using the '||' operator.
        /// </summary>
        BooleanOr,
        /// <summary>
        /// Represents a reduction using the Math.Min() function.
        /// </summary>
        Min,
        /// <summary>
        /// Represents a reduction using the Math.Max() function.
        /// </summary>
        Max
    }
}