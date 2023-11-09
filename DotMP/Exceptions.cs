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

using System;

namespace DotMP
{
    /// <summary>
    /// Exception thrown if a parallel-only construct is used outside of a parallel region.
    /// </summary>
    public class NotInParallelRegionException : Exception
    {
        /// <summary>
        /// Constructor with a message.
        /// </summary>
        /// <param name="msg">The message to associate with the exception.</param>
        public NotInParallelRegionException(string msg) : base(msg) { }
    }

    /// <summary>
    /// Exception thrown if a Parallel.ParallelRegion is created inside of another Parallel.ParallelRegion.
    /// </summary>
    public class CannotPerformNestedParallelismException : Exception
    {
        /// <summary>
        /// Constructor with a message.
        /// </summary>
        /// <param name="msg">The message to associate with the exception.</param>
        public CannotPerformNestedParallelismException(string msg) : base(msg) { }
    }

    /// <summary>
    /// Exception thrown if a Parallel.Single is created inside of a Parallel.For or Parallel.ForReduction&lt;T&gt;.
    /// </summary>
    public class CannotPerformNestedWorksharingException : Exception
    {
        /// <summary>
        /// Constructor with a message.
        /// </summary>
        /// <param name="msg">The message to associate with the exception.</param>
        public CannotPerformNestedWorksharingException(string msg) : base(msg) { }
    }

    /// <summary>
    /// Exception thrown if invalid arguments are specified to DotMP functions.
    /// </summary>
    public class InvalidArgumentsException : Exception
    {
        /// <summary>
        /// Constructor with a message.
        /// </summary>
        /// <param name="msg">The message to associate with the exception.</param>
        public InvalidArgumentsException(string msg) : base(msg) { }
    }
}