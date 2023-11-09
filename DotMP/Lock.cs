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

using System.Threading;

namespace DotMP
{
    /// <summary>
    /// A lock that can be used in a parallel region.
    /// Also contains instance methods for locking.
    /// Available methods are Set, Unset, and Test.
    /// </summary>
    public sealed class Lock
    {
        /// <summary>
        /// The int acting as the lock.
        /// </summary>
        volatile private int _lock;

        /// <summary>
        /// Constructs a new lock.
        /// </summary>
        public Lock()
        {
            _lock = 0;
        }

        /// <summary>
        /// Stalls the thread until the lock is set.
        /// </summary>
        public void Set()
        {
            while (Interlocked.CompareExchange(ref this._lock, 1, 0) == 1) ;
        }

        /// <summary>
        /// Unsets the lock.
        /// </summary>
        public void Unset()
        {
            Interlocked.Exchange(ref this._lock, 0);
        }

        /// <summary>
        /// Attempts to set the lock.
        /// Does not stall the thread.
        /// </summary>
        /// <returns>True if the lock was set, false otherwise.</returns>
        public bool Test()
        {
            return Interlocked.CompareExchange(ref this._lock, 1, 0) == 0;
        }
    }
}
