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
 */

using System;
using System.Threading;

namespace DotMP
{
    /// <summary>
    /// Contains relevant internal information about parallel regions, including the threads and the function to be executed.
    /// Provides a region-wide lock and SpinWait objects for each thread.
    /// </summary>
    internal class Region
    {
        /// <summary>
        /// The threads to be created and executed.
        /// </summary>
        internal Thread[] threads;
        /// <summary>
        /// Generic lock to be used within the workspace.
        /// </summary>
        internal object ws_lock;
        /// <summary>
        /// The number of threads in play.
        /// </summary>
        internal uint num_threads;
        /// <summary>
        /// The function to be executed.
        /// </summary>
        internal Action omp_fn;
        /// <summary>
        /// Exception caught from threads.
        /// </summary>
        internal Exception ex;

        /// <summary>
        /// Factory for creating threads in the threadpool.
        /// </summary>
        /// <param name="omp_fn">The function to execute.</param>
        /// <param name="tid">The thread ID.</param>
        /// <param name="num_threads">The total number of threads.</param>
        /// <returns>The created Thread object.</returns>
        internal Thread CreateThread(Action omp_fn, int tid, uint num_threads)
        {
            return new Thread(() =>
            {
                try
                {
                    omp_fn();
                    Parallel.Taskwait();
                }
                catch (Exception ex)
                {
                    this.ex ??= ex;
                    Parallel.canceled = true;

                    if (ex is not ThreadInterruptedException)
                        for (int i = 0; i < num_threads; i++)
                            if (i != tid)
                                threads[i].Interrupt();
                }
            });
        }

        /// <summary>
        /// Creates a specified number of threads available to the parallel region, and sets the function to be executed.
        /// Also sets other relevant data for the parallel region.
        /// </summary>
        /// <param name="num_threads">The number of threads to be created.</param>
        /// <param name="omp_fn">The function to be executed.</param>
        internal Region(uint num_threads, Action omp_fn)
        {
            threads = new Thread[num_threads];
            for (int i = 0; i < num_threads; i++)
                threads[i] = CreateThread(omp_fn, i, num_threads);
            ws_lock = new object();
            this.num_threads = num_threads;
            this.omp_fn = omp_fn;
            this.ex = null;
        }
    }

    /// <summary>
    /// Contains the Region object and controls for creating and starting a parallel region.
    /// </summary>
    internal class ForkedRegion
    {
        /// <summary>
        /// The contained Region object.
        /// </summary>
        private static Region reg_pv;
        /// <summary>
        /// Getter for singleton object ForkedRegion.reg_pv.
        /// </summary>
        internal Region reg
        {
            get
            {
                return reg_pv;
            }
        }
        /// <summary>
        /// Whether or not the program is currently in a parallel region.
        /// </summary>
        private static bool in_parallel_prv;
        /// <summary>
        /// Getter and setter for singleton bool ForkedRegion.in_parallel_prv.
        /// </summary>
        internal bool in_parallel
        {
            get
            {
                return in_parallel_prv;
            }
            private set
            {
                in_parallel_prv = value;
            }
        }
        /// <summary>
        /// Whether or not the program is currently in a worksharing region (>0 meaning the program is in said region).
        /// </summary>
        private static uint in_workshare_prv = 0;
        /// <summary>
        /// Getter and setter for singleton bool ForkedRegion.in_workshare_prv.
        /// </summary>
        internal ref uint in_workshare
        {
            get
            {
                return ref in_workshare_prv;
            }
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        internal ForkedRegion() { }

        /// <summary>
        /// Initializes the threadpool with the specified number of threads and function to be executed, as well as setting the thread names.
        /// </summary>
        /// <param name="num_threads">The number of threads to be created.</param>
        /// <param name="omp_fn">The function to be executed.</param>
        internal ForkedRegion(uint num_threads, Action omp_fn)
        {
            reg_pv = new Region(num_threads, omp_fn);
            for (int i = 0; i < num_threads; i++)
                reg.threads[i].Name = i.ToString();

            in_parallel_prv = false;
            in_workshare_prv = 0;
        }

        /// <summary>
        /// Starts the threadpool and waits for all threads to complete before returning.
        /// </summary>
        /// <exception cref="Exception">Thrown if an exception is caught in the threadpool. If multiple are thrown, the first one thrown is returned.</exception>
        internal void StartThreadpool()
        {
            in_parallel = true;
            Parallel.canceled = false;

            for (int i = 0; i < reg.num_threads; i++)
                reg.threads[i].Start();

            for (int i = 0; i < reg.num_threads; i++)
                reg.threads[i].Join();

            in_parallel = false;

            if (reg.ex is not null)
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(reg.ex).Throw();
        }
    }
}
