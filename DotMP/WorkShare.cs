﻿/*
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

using System.Collections.Generic;
using System.Threading;
using System;
using DotMP.Exceptions;

namespace DotMP
{
    /// <summary>
    /// Contains all relevant information about a parallel for loop.
    /// Contains a collection of Thr objects, the loop's start and end iterations, the chunk size, the number of threads, and the number of threads that have completed their work.
    /// </summary>
    internal class WorkShare
    {
        /// <summary>
        /// The threads to be used in the parallel for loop.
        /// </summary>
        private static Thread[] threads;
        /// <summary>
        /// The working iterations of each thread.
        /// </summary>
        private static int[] working_iters;
        /// <summary>
        /// Get Thr object based on current thread ID.
        /// </summary>
        internal int working_iter
        {
            get
            {
                return working_iters[Parallel.GetThreadNum()];
            }
        }
        /// <summary>
        /// The starting iteration of the parallel for loop, inclusive.
        /// </summary>
        private static volatile int start_pv;
        /// <summary>
        /// Getter and setter for the singleton integer WorkShare.start_pv.
        /// </summary>
        internal int start
        {
            get
            {
                return start_pv;
            }
        }
        /// <summary>
        /// The ending iteration of the parallel for loop, exclusive.
        /// </summary>
        internal int end { get; private set; }
        /// <summary>
        /// The chunk size to be used with the selected scheduler.
        /// </summary>
        private static uint chunk_size_pv;
        /// <summary>
        /// Getter and setter for singleton uint WorkShare.chunk_size_pv.
        /// </summary>
        internal uint chunk_size
        {
            get
            {
                return chunk_size_pv;
            }
            private set
            {
                chunk_size_pv = value;
            }
        }
        /// <summary>
        /// The number of threads to be used in the parallel for loop.
        /// </summary>
        internal uint num_threads { get; private set; }
        /// <summary>
        /// The operation to be performed if doing a reduction.
        /// </summary>
        internal Operations? op { get; private set; }
        /// <summary>
        /// The list of reduction variables from each thread.
        /// </summary>
        private static volatile List<dynamic> reduction_list;
        /// <summary>
        /// Getter for WorkShare.reduction_list.
        /// </summary>
        internal List<dynamic> reduction_values
        {
            get
            {
                return reduction_list;
            }
        }
        /// <summary>
        /// The schedule to be used in the parallel for loop.
        /// </summary>
        private static IScheduler schedule_pv;
        /// <summary>
        /// Getter and setter for singleton object WorkShare.schedule_pv.
        /// </summary>
        internal IScheduler schedule
        {
            get
            {
                return schedule_pv;
            }
            private set
            {
                schedule_pv = value;
            }
        }
        /// <summary>
        /// Booleans per-thread to check if we're currently in a Parallel.For or Parallel.ForReduction&lt;T&gt;.
        /// </summary>
        private static bool[] in_for_pv;
        /// <summary>
        /// Getter and setter for this thread's value in WorkShare.in_for_pv.
        /// </summary>
        internal bool in_for
        {
            get
            {
                int tid = Parallel.GetThreadNum();

                if (in_for_pv == null || tid >= in_for_pv.Length)
                {
                    return false;
                }

                return in_for_pv[tid];
            }
            set
            {
                in_for_pv[Parallel.GetThreadNum()] = value;
            }
        }

        /// <summary>
        /// The constructor for a WorkShare object.
        /// </summary>
        /// <param name="num_threads">The number of threads in the WorkShare.</param>
        /// <param name="threads">The Thread[] array of threads.</param>
        /// <param name="start">Starting iteration, inclusive.</param>
        /// <param name="end">Ending iteration, exclusive.</param>
        /// <param name="chunk_size">The chunk size to use.</param>
        /// <param name="op">The operation for reduction, null if not a reduction.</param>
        /// <param name="schedule">The Parallel.Schedule to use.</param>
        internal WorkShare(uint num_threads, Thread[] threads, int start, int end, uint chunk_size, Operations? op, IScheduler schedule)
        {
            this.end = end;
            this.num_threads = num_threads;
            this.op = op;
            Parallel.Master(() =>
            {
                WorkShare.threads = threads;
                working_iters = new int[num_threads];
                reduction_list = new List<dynamic>();
                in_for_pv = new bool[num_threads];
                start_pv = start;
                this.chunk_size = chunk_size;
                this.schedule = schedule;
            });
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        internal WorkShare() { }

        /// <summary>
        /// Add a value to reduction_list.
        /// </summary>
        /// <param name="value">The value to add to reduction_list.</param>
        internal void AddReductionValue(dynamic value)
        {
            lock (reduction_list)
            {
                reduction_list.Add(value);
            }
        }

        /// <summary>
        /// Sets the local variable to the appropriate value based on the operation for parallel for reduction loops.
        /// For addition and subtraction, the initial starting value is 0.
        /// For multiplication, the initial starting value is 1.
        /// For binary And, the initial starting value is the bitwise negation of 0.
        /// For binary Or and Xor, the initial starting value is 0.
        /// For boolean And, the initial starting value is true.
        /// For boolean Or, the initial starting value is false.
        /// For min, the initial starting value is int.MaxValue.
        /// For max, the initial starting value is int.MinValue.
        /// </summary>
        /// <typeparam name="T">The type of the local variable.</typeparam>
        /// <param name="local">The local variable to be set.</param>
        internal void SetLocal<T>(ref T local)
        {
            switch (op)
            {
                case Operations.Add:
                case Operations.Subtract:
                    local = (T)Convert.ChangeType(0, typeof(T));
                    break;
                case Operations.Multiply:
                    local = (T)Convert.ChangeType(1, typeof(T));
                    break;
                case Operations.BinaryAnd:
                    local = ~(dynamic)Convert.ChangeType(0, typeof(T));
                    break;
                case Operations.BinaryOr:
                    local = (T)Convert.ChangeType(0, typeof(T));
                    break;
                case Operations.BinaryXor:
                    local = (T)Convert.ChangeType(0, typeof(T));
                    break;
                case Operations.BooleanAnd:
                    local = (T)Convert.ChangeType(true, typeof(T));
                    break;
                case Operations.BooleanOr:
                    local = (T)Convert.ChangeType(false, typeof(T));
                    break;
                case Operations.Min:
                    local = (T)Convert.ChangeType(int.MaxValue, typeof(T));
                    break;
                case Operations.Max:
                    local = (T)Convert.ChangeType(int.MinValue, typeof(T));
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Performs a parallel for loop according to the scheduling policy provided.
        /// </summary>
        /// <typeparam name="T">The type of reductions, if applicable.</typeparam>
        /// <param name="forAction">The function to be executed.</param>
        /// <exception cref="InternalSchedulerException">Thrown if the internal schedulers throw an exception.</exception> 
        internal void PerformLoop<T>(ForAction<T> forAction)
        {
            int start = this.start;
            int end = this.end;
            uint num_threads = this.num_threads;
            uint chunk_size = this.chunk_size;
            IScheduler scheduler = schedule;
            int thread_id = Parallel.GetThreadNum();

            T local = default;
            if (forAction.IsReduction)
                SetLocal(ref local);

            try
            {
                Parallel.Master(() => scheduler.LoopInit(start, end, num_threads, chunk_size));
                Parallel.Barrier();

                int chunk_start, chunk_end;
                ref int curr_iter = ref working_iters[thread_id];

                do
                {
                    scheduler.LoopNext(thread_id, out chunk_start, out chunk_end);
                    if (chunk_start < chunk_end)
                        forAction.PerformLoop(ref curr_iter, chunk_start, chunk_end, ref local);
                }
                while (chunk_start < chunk_end);
            }
            catch (OverflowException)
            {
                throw new InternalSchedulerException(string.Format("An internal overflow exception has occurred within the loop scheduler. This most often happens when the upper bound of the loop is too close to {0}.", int.MaxValue));
            }

            if (forAction.IsReduction)
                AddReductionValue(local);
        }
    }
}
