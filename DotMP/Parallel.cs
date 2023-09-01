using System;
using System.Collections.Generic;
using System.Threading;

namespace DotMP
{
    /// <summary>
    /// Action delegate that takes an int and a ref T as parameters.
    /// </summary>
    /// <typeparam name="T">Type of the ref parameter.</typeparam>
    /// <param name="a">The ref parameter.</param>
    /// <param name="i">The int parameter.</param>
    public delegate void ActionRef<T>(ref T a, int i);

    /// <summary>
    /// The main class of DotMP.
    /// Contains all the main methods for parallelism.
    /// For users, this is the main class you want to worry about, along with Lock, Shared, and Atomic
    /// </summary>
    public static class Parallel
    {
        /// <summary>
        /// The different types of schedules for a parallel for loop.
        /// The default schedule if none is specified is static.
        /// Detailed explanations of each schedule can be found in the Iter class.
        /// The Runtime schedule simply fetches the schedule from the OMP_SCHEDULE environment variable.
        /// </summary>
        public enum Schedule { Static, Dynamic, Guided, Runtime };
        /// <summary>
        /// The dictionary for critical regions.
        /// </summary>
        private static volatile Dictionary<int, (int, object)> critical_lock = new Dictionary<int, (int, object)>();
        /// <summary>
        /// The dictionary for single regions.
        /// </summary>
        private static volatile Dictionary<int, int> single_thread = new Dictionary<int, int>();
        /// <summary>
        /// The dictionary for ordered regions.
        /// </summary>
        private static volatile Dictionary<int, int> ordered = new Dictionary<int, int>();
        /// <summary>
        /// The number of critical regions found.
        /// </summary>
        private static volatile int found_criticals = 0;
        /// <summary>
        /// Barrier object for DotMP.Parallel.Barrier()
        /// </summary>
        private static volatile Barrier barrier;
        /// <summary>
        /// Number of threads to be used in the next parallel region, where 0 means that it will be determined on-the-fly.
        /// </summary>
        private static volatile uint num_threads = 0;

        /// <summary>
        /// Fixes the arguments for a parallel for loop.
        /// If a Schedule is set to Static, Dynamic, or Guided, then the function simply calculates chunk size if none is given.
        /// If a Schedule is set to Runtime, then the function checks the OMP_SCHEDULE environment variable and sets the appropriate values.
        /// </summary>
        /// <param name="start">The start of the loop.</param>
        /// <param name="end">The end of the loop.</param>
        /// <param name="sched">The schedule of the loop.</param>
        /// <param name="chunk_size">The chunk size of the loop.</param>
        /// <param name="num_threads">The number of threads to be used in the loop.</param>
        private static void FixArgs(int start, int end, ref Schedule sched, ref uint? chunk_size, uint num_threads)
        {
            if (num_threads == 0)
            {
                num_threads = (uint)GetNumProcs();
            }

            if (sched == Schedule.Runtime)
            {
                string schedule = Environment.GetEnvironmentVariable("OMP_SCHEDULE");

                if (schedule == null)
                {
                    sched = Schedule.Static;
                }
                else
                {
                    string[] parts = schedule.Split(',');

                    switch (parts[0].ToLower())
                    {
                        case "dynamic":
                            sched = Schedule.Dynamic;
                            break;
                        case "guided":
                            sched = Schedule.Guided;
                            break;
                        case "static":
                            sched = Schedule.Static;
                            break;
                        default:
                            Console.WriteLine("Invalid schedule specified by OMP_SCHEDULE, defaulting to static.");
                            sched = Schedule.Static;
                            break;
                    }

                    if (parts.Length > 1)
                    {
                        uint try_chunk_size;
                        if (uint.TryParse(parts[1], out try_chunk_size))
                        {
                            chunk_size = try_chunk_size;
                        }
                        else
                        {
                            chunk_size = null;
                        }
                    }
                }
            }

            if (chunk_size == null)
            {
                switch (sched)
                {
                    case Schedule.Static:
                        chunk_size = (uint)((end - start) / num_threads);
                        break;
                    case Schedule.Dynamic:
                        chunk_size = (uint)((end - start) / num_threads) / 32;
                        if (chunk_size < 1) chunk_size = 1;
                        break;
                    case Schedule.Guided:
                        chunk_size = 1;
                        break;
                }
            }
        }

        /// <summary>
        /// Creates a for loop inside a parallel region.
        /// A for loop created with For inside of a parallel region is executed in parallel, with iterations being distributed among the threads, and potentially out-of-order.
        /// A schedule is provided to inform the runtime how to distribute iterations of the loop to threads.
        /// Available schedules are specified by the Schedule enum, and have detailed documentation in the Iter class.
        /// Acts as an implicit Barrier().
        /// </summary>
        /// <param name="start">The start of the loop, inclusive.</param>
        /// <param name="end">The end of the loop, exclusive.</param>
        /// <param name="action">The action to be performed in the loop.</param>
        /// <param name="schedule">The schedule of the loop, defaulting to static.</param>
        /// <param name="chunk_size">The chunk size of the loop, defaulting to null. If null, will be calculated on-the-fly.</param>
        /// <exception cref="NotInParallelRegionException">Thrown when not in a parallel region.</exception>
        public static void For(int start, int end, Action<int> action, Schedule schedule = Schedule.Static, uint? chunk_size = null)
        {
            if (!ForkedRegion.in_parallel)
            {
                throw new NotInParallelRegionException();
            }

            FixArgs(start, end, ref schedule, ref chunk_size, Init.ws.num_threads);

            Master(() =>
            {
                Init.ws = new WorkShare((uint)GetNumThreads(), ForkedRegion.ws.threads);
                Init.ws.start = start;
                Init.ws.end = end;
                Init.ws.chunk_size = chunk_size.Value;
                Init.ws.schedule = schedule;
            });

            Barrier();

            switch (schedule)
            {
                case Schedule.Static:
                    Iter.StaticLoop<object>(GetThreadNum(), action, null, false);
                    break;
                case Schedule.Dynamic:
                    Iter.DynamicLoop<object>(GetThreadNum(), action, null, false);
                    break;
                case Schedule.Guided:
                    Iter.GuidedLoop<object>(GetThreadNum(), action, null, false);
                    break;
            }

            Barrier();

            Master(() => ordered.Clear());
        }

        /// <summary>
        /// Creates a for loop inside a parallel region with a reduction.
        /// This is similar to For(), but the reduction allows multiple threads to reduce their work down to a single variable.
        /// Using ForReduction<T> allows the runtime to perform this operation much more efficiently than a naive approach using the Locking or Atomic classes.
        /// Each thread gets a thread-local version of the reduction variable, and the runtime performs a global reduction at the end of the loop.
        /// Since the global reduction only involves as many variables as there are threads, it is much more efficient than a naive approach.
        /// Acts as an implicit Barrier().
        /// </summary>
        /// <typeparam name="T">The type of the reduction.</typeparam>
        /// <param name="start">The start of the loop, inclusive.</param>
        /// <param name="end">The end of the loop, exclusive.</param>
        /// <param name="op">The operation to be performed on the reduction.</param>
        /// <param name="reduce_to">The variable to reduce to.</param>
        /// <param name="action">The action to be performed in the loop.</param>
        /// <param name="schedule">The schedule of the loop, defaulting to static.</param>
        /// <param name="chunk_size">The chunk size of the loop, defaulting to null. If null, will be calculated on-the-fly.</param>
        /// <exception cref="NotInParallelRegionException">Thrown when not in a parallel region.</exception>
        public static void ForReduction<T>(int start, int end, Operations op, ref T reduce_to, ActionRef<T> action, Schedule schedule = Schedule.Static, uint? chunk_size = null)
        {
            if (!ForkedRegion.in_parallel)
            {
                throw new NotInParallelRegionException();
            }

            FixArgs(start, end, ref schedule, ref chunk_size, Init.ws.num_threads);

            if (GetThreadNum() == 0)
            {
                Init.ws = new WorkShare((uint)GetNumThreads(), ForkedRegion.ws.threads);
                Init.ws.start = start;
                Init.ws.end = end;
                Init.ws.chunk_size = chunk_size.Value;
                Init.ws.op = op;
                Init.ws.schedule = schedule;
                Init.ws.reduction_list.Clear();
            }

            Barrier();

            switch (schedule)
            {
                case Schedule.Static:
                    Iter.StaticLoop(GetThreadNum(), null, action, true);
                    break;
                case Schedule.Dynamic:
                    Iter.DynamicLoop(GetThreadNum(), null, action, true);
                    break;
                case Schedule.Guided:
                    Iter.GuidedLoop(GetThreadNum(), null, action, true);
                    break;
            }

            Barrier();

            if (GetThreadNum() == 0)
            {
                foreach (T i in Init.ws.reduction_list)
                {
                    switch (Init.ws.op)
                    {
                        case Operations.Add:
                        case Operations.Subtract:
                            reduce_to += (dynamic)i;
                            break;
                        case Operations.Multiply:
                            reduce_to *= (dynamic)i;
                            break;
                        case Operations.BinaryAnd:
                            reduce_to &= (dynamic)i;
                            break;
                        case Operations.BinaryOr:
                            reduce_to |= (dynamic)i;
                            break;
                        case Operations.BinaryXor:
                            reduce_to ^= (dynamic)i;
                            break;
                        case Operations.BooleanAnd:
                            reduce_to = (dynamic)reduce_to && (dynamic)i;
                            break;
                        case Operations.BooleanOr:
                            reduce_to = (dynamic)reduce_to || (dynamic)i;
                            break;
                        case Operations.Min:
                            reduce_to = Math.Min((dynamic)reduce_to, (dynamic)i);
                            break;
                        case Operations.Max:
                            reduce_to = Math.Max((dynamic)reduce_to, (dynamic)i);
                            break;
                    }
                }

                ordered.Clear();
            }

            Barrier();
        }

        /// <summary>
        /// Creates a parallel region.
        /// The body of a parallel region is executed by as many threads as specified by the num_threads parameter.
        /// If the num_threads parameter is absent, then the runtime checks if SetNumThreads has been called.
        /// If so, it will use that many threads. If not, the runtime will try to use as many threads as there are logical processors.
        /// </summary>
        /// <param name="action">The action to be performed in the parallel region.</param>
        /// <param name="num_threads">The number of threads to be used in the parallel region, defaulting to null. If null, will be calculated on-the-fly.</param>
        /// <exception cref="CannotPerformNestedParallelismException">Thrown if ParallelRegion is called from within another ParallelRegion.</exception>
        public static void ParallelRegion(Action action, uint? num_threads = null)
        {
            if (InParallel())
                throw new CannotPerformNestedParallelismException();

            if (num_threads == null && Parallel.num_threads == 0)
                num_threads = (uint)GetNumProcs();
            else
                num_threads ??= Parallel.num_threads;

            ForkedRegion.CreateThreadpool(num_threads.Value, action);
            barrier = new Barrier((int)num_threads.Value);
            ForkedRegion.StartThreadpool();
            ForkedRegion.ws.num_threads = 1;
            barrier = new Barrier(1);
        }

        /// <summary>
        /// Creates a parallel for loop. Contains all of the parameters from ParallelRegion() and For().
        /// This is simply a convenience method for creating a parallel region and a for loop inside of it.
        /// </summary>
        /// <param name="start">The start of the loop, inclusive.</param>
        /// <param name="end">The end of the loop, exclusive.</param>
        /// <param name="action">The action to be performed in the loop.</param>
        /// <param name="schedule">The schedule of the loop, defaulting to static.</param>
        /// <param name="chunk_size">The chunk size of the loop, defaulting to null. If null, will be calculated on-the-fly.</param>
        /// <param name="num_threads">The number of threads to be used in the loop, defaulting to null. If null, will be calculated on-the-fly.</param>
        public static void ParallelFor(int start, int end, Action<int> action, Schedule schedule = Schedule.Static, uint? chunk_size = null, uint? num_threads = null)
        {
            if (num_threads == null && Parallel.num_threads == 0)
                num_threads = (uint)GetNumProcs();
            else
                num_threads ??= Parallel.num_threads;

            ParallelRegion(num_threads: num_threads.Value, action: () =>
            {
                For(start, end, action, schedule, chunk_size);
            });
        }

        /// <summary>
        /// Creates a parallel for loop with a reduction. Contains all of the parameters from ParallelRegion() and ForReduction<T>().
        /// This is simply a convenience method for creating a parallel region and a for loop with a reduction inside of it.
        /// </summary>
        /// <typeparam name="T">The type of the reduction.</typeparam>
        /// <param name="start">The start of the loop, inclusive.</param>
        /// <param name="end">The end of the loop, exclusive.</param>
        /// <param name="op">The operation to be performed on the reduction.</param>
        /// <param name="reduce_to">The variable to reduce to.</param>
        /// <param name="action">The action to be performed in the loop.</param>
        /// <param name="schedule">The schedule of the loop, defaulting to static.</param>
        /// <param name="chunk_size">The chunk size of the loop, defaulting to null. If null, will be calculated on-the-fly.</param>
        /// <param name="num_threads">The number of threads to be used in the loop, defaulting to null. If null, will be calculated on-the-fly.</param>
        public static void ParallelForReduction<T>(int start, int end, Operations op, ref T reduce_to, ActionRef<T> action, Schedule schedule = Schedule.Static, uint? chunk_size = null, uint? num_threads = null)
        {
            if (num_threads == null && Parallel.num_threads == 0)
                num_threads = (uint)GetNumProcs();
            else
                num_threads ??= Parallel.num_threads;

            T local = reduce_to;

            ParallelRegion(num_threads: num_threads.Value, action: () =>
            {
                ForReduction(start, end, op, ref local, action, schedule, chunk_size);
            });

            reduce_to = local;
        }

        /// <summary>
        /// Creates a sections region.
        /// Sections allows for the user to submit multiple, individual tasks to be distributed among threads in parallel.
        /// This allows the user to submit a callback using Section(). Each callback specified with Section() is thrown into a central queue.
        /// In parallel, each thread will dequeue a callback and execute it.
        /// This is useful if you have lots of individual tasks that need to be executed in parallel, and each task requires its own lambda.
        /// Any code not specified in a Section() will be executed by the master thread only.
        /// Acts as an implicit Barrier().
        /// </summary>
        /// <param name="action">The action to be performed in the sections region.</param>
        /// <exception cref="NotInParallelRegionException">Thrown when not in a parallel region.</exception>
        public static void Sections(Action action)
        {
            if (!ForkedRegion.in_parallel)
            {
                throw new NotInParallelRegionException();
            }

            SectionHandler.in_sections = true;

            if (GetThreadNum() == 0)
            {
                action();
            }

            Barrier();

            while (SectionHandler.num_actions > 0)
            {
                Action do_action;

                lock (SectionHandler.actions_list_lock)
                {
                    if (SectionHandler.actions.Count > 0)
                        do_action = SectionHandler.actions.Dequeue();
                    else break;
                }

                Interlocked.Decrement(ref SectionHandler.num_actions);

                do_action();
            }

            Barrier();
        }

        /// <summary>
        /// Creates a section inside a sections region.
        /// See the Sections() documentation for more information.
        /// Each task submitted by Section() in a Sections() region will be executed by a single thread in parallel with all other Section() tasks submitted.
        /// </summary>
        /// <param name="action">The action to be performed in the section.</param>
        /// <exception cref="NotInParallelRegionException">Thrown when not in a parallel region.</exception>
        /// <exception cref="NotInSectionsRegionException">Thrown when not in a sections region.</exception>
        public static void Section(Action action)
        {
            if (!ForkedRegion.in_parallel)
            {
                throw new NotInParallelRegionException();
            }

            if (!SectionHandler.in_sections)
            {
                throw new NotInSectionsRegionException();
            }

            lock (SectionHandler.actions_list_lock)
            {
                SectionHandler.actions.Enqueue(action);
                Interlocked.Increment(ref SectionHandler.num_actions);
            }
        }

        /// <summary>
        /// Creates a parallel sections region. Contains all of the parameters from ParallelRegion() and Sections().
        /// This is simply a convenience method for creating a parallel region and a sections region inside of it.
        /// </summary>
        /// <param name="action">The action to be performed in the parallel sections region.</param>
        /// <param name="num_threads">The number of threads to be used in the parallel sections region, defaulting to null. If null, will be calculated on-the-fly.</param>
        public static void ParallelSections(Action action, uint? num_threads = null)
        {
            ParallelRegion(num_threads: num_threads, action: () =>
            {
                Sections(action);
            });
        }

        /// <summary>
        /// Creates a critical region.
        /// A critical region is a region of code that can only be executed by one thread at a time.
        /// If a thread encounters a critical region while another thread is inside a critical region, it will wait until the other thread is finished.
        /// </summary>
        /// <param name="id">The ID of the critical region. Must be unique per region but consistent across all threads.</param>
        /// <param name="action">The action to be performed in the critical region.</param>
        /// <returns>The ID of the critical region.</returns>
        /// <exception cref="NotInParallelRegionException">Thrown when not in a parallel region.</exception>
        public static int Critical(int id, Action action)
        {
            if (!ForkedRegion.in_parallel)
            {
                throw new NotInParallelRegionException();
            }

            object lock_obj;

            lock (critical_lock)
            {
                if (!critical_lock.ContainsKey(id))
                {
                    critical_lock.Add(id, (++found_criticals, new object()));
                }

                (id, lock_obj) = critical_lock[id];
            }

            lock (lock_obj)
            {
                action();
            }

            return id;
        }

        /// <summary>
        /// Creates a barrier.
        /// All threads must reach the barrier before any thread can continue.
        /// This is useful for synchronization. Many functions inside the Parallel class act as implicit barriers.
        /// Also acts as a memory barrier.
        /// </summary>
        /// <exception cref="NotInParallelRegionException">Thrown when not in a parallel region.</exception>
        public static void Barrier()
        {
            if (!ForkedRegion.in_parallel)
            {
                throw new NotInParallelRegionException();
            }

            Thread.MemoryBarrier();
            barrier.SignalAndWait();
        }

        /// <summary>
        /// Gets the number of available processors on the host system.
        /// </summary>
        /// <returns>The number of processors.</returns>
        public static int GetNumProcs()
        {
            return Environment.ProcessorCount;
        }

        /// <summary>
        /// Creates a master region.
        /// The master region is a region of code that is only executed by the master thread.
        /// The master thread is the thread with a thread ID of 0.
        /// You can get the thread ID of the calling thread with GetThreadNum().
        /// </summary>
        /// <param name="action">The action to be performed in the master region.</param>
        /// <exception cref="NotInParallelRegionException">Thrown when not in a parallel region.</exception>
        public static void Master(Action action)
        {
            if (!ForkedRegion.in_parallel)
            {
                throw new NotInParallelRegionException();
            }

            if (GetThreadNum() == 0)
            {
                action();
            }
        }

        /// <summary>
        /// Creates a single region.
        /// A single region is only executed by a single thread.
        /// The thread which "owns" a single region is the first thread to encounter it.
        /// From thereon, only that thread will execute the single region.
        /// </summary>
        /// <param name="id">The ID of the single region. Must be unique per region but consistent across all threads.</param>
        /// <param name="action">The action to be performed in the single region.</param>
        /// <exception cref="NotInParallelRegionException">Thrown when not in a parallel region.</exception>
        public static void Single(int id, Action action)
        {
            if (!ForkedRegion.in_parallel)
            {
                throw new NotInParallelRegionException();
            }

            lock (single_thread)
            {
                if (!single_thread.ContainsKey(id))
                {
                    single_thread.Add(id, GetThreadNum());
                }
            }

            if (single_thread[id] == GetThreadNum())
            {
                action();
            }
        }

        /// <summary>
        /// Creates an ordered region.
        /// An ordered region is a region of code that is executed in order inside of a For() or ForReduction<T>() loop.
        /// This also acts as an implicit Critical() region.
        /// </summary>
        /// <param name="id">The ID of the ordered region. Must be unique per region but consistent across all threads.</param>
        /// <param name="action">The action to be performed in the ordered region.</param>
        /// <exception cref="NotInParallelRegionException">Thrown when not in a parallel region.</exception>
        public static void Ordered(int id, Action action)
        {
            if (!ForkedRegion.in_parallel)
            {
                throw new NotInParallelRegionException();
            }

            int tid = GetThreadNum();

            lock (ordered)
            {
                if (!ordered.ContainsKey(id))
                {
                    ordered.Add(id, 0);
                }
                Thread.MemoryBarrier();
            }

            while (ordered[id] != Init.ws.threads[tid].working_iter)
            {
                ForkedRegion.ws.spin[tid].SpinOnce();
            }

            action();

            lock (ordered)
            {
                ordered[id]++;
            }
        }

        /// <summary>
        /// Gets the number of active threads.
        /// If not inside of a ParallelRegion(), returns 1.
        /// </summary>
        /// <returns>The number of threads.</returns>
        public static int GetNumThreads()
        {
            int num_threads = (int)ForkedRegion.ws.num_threads;

            if (num_threads == 0)
            {
                ForkedRegion.ws.num_threads = 1;
                return 1;
            }

            return num_threads;
        }

        /// <summary>
        /// Gets the ID of the calling thread.
        /// </summary>
        /// <returns>The number of the calling thread.</returns>
        /// <exception cref="NotInParallelRegionException">Thrown when not in a parallel region.</exception>
        public static int GetThreadNum()
        {
            if (!ForkedRegion.in_parallel)
            {
                throw new NotInParallelRegionException();
            }

            return Convert.ToInt32(Thread.CurrentThread.Name);
        }

        /// <summary>
        /// Sets the number of threads that will be used in the next parallel region.
        /// </summary>
        /// <param name="num_threads">The number of threads to be used in the next parallel region.</param>
        public static void SetNumThreads(int num_threads)
        {
            Parallel.num_threads = (uint)num_threads;
        }

        /// <summary>
        /// Gets the maximum number of threads that will be used in the next parallel region.
        /// </summary>
        /// <returns>The maximum number of threads that will be used in the next parallel region.</returns>
        public static int GetMaxThreads()
        {
            return Parallel.num_threads != 0 ? (int)Parallel.num_threads : GetNumProcs();
        }

        /// <summary>
        /// Gets whether or not the calling thread is in a parallel region.
        /// </summary>
        /// <returns>Whether or not the calling thread is in a parallel region.</returns>
        public static bool InParallel()
        {
            return ForkedRegion.in_parallel;
        }

        /// <summary>
        /// Tells the runtime to dynamically adjust the number of threads.
        /// </summary>
        public static void SetDynamic()
        {
            Parallel.num_threads = 0;
        }

        /// <summary>
        /// Gets whether or not the runtime is dynamically adjusting the number of threads.
        /// </summary>
        /// <returns>Whether or not the runtime is dynamically adjusting the number of threads.</returns>
        public static bool GetDynamic()
        {
            return Parallel.num_threads == 0;
        }

        /// <summary>
        /// Enables nested parallelism.
        /// This function is not implemented, as nested parallelism does not exist in the current version of DotMP.
        /// There are no plans to implement nested parallelism at the moment.
        /// </summary>
        /// <param name="_">Unused.</param>
        /// <exception cref="NotImplementedException">Is always thrown.</exception>
        public static void SetNested(bool _)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets whether or not nested parallelism is enabled.
        /// There are no plans to implement nested parallelism at the moment.
        /// </summary>
        /// <returns>Always returns false.</returns>
        public static bool GetNested() => false;

        /// <summary>
        /// Gets the wall time as a double, representing the number of seconds since the epoch.
        /// </summary>
        /// <returns>The wall time as a double.</returns>
        public static double GetWTime()
        {
            return DateTime.Now.Ticks / 10000000.0;
        }

        /// <summary>
        /// Returns the current schedule being used in a For() or ForReduction<T>() loop.
        /// </summary>
        /// <returns>The schedule being used in the For() or ForReduction<T>() loop, or null if a For() or ForReduction<T>() has not been encountered yet.</returns>
        public static Schedule? GetSchedule()
        {
            return Init.ws.schedule;
        }

        /// <summary>
        /// Returns the current chunk size being used in a For() or ForReduction<T>() loop.
        /// </summary>
        /// <returns>The chunk size being used in a For() or ForReduction<T>() loop. If 0, a For() or ForReduction<T>() has not been encountered yet.</returns>
        public static uint GetChunkSize()
        {
            return Init.ws.chunk_size;
        }

        /// <summary>
        /// Clears the memory used by criticals, ordereds, and singles.
        /// </summary>
        public static void __reset_lambda_memory()
        {
            critical_lock.Clear();
            single_thread.Clear();
            ordered.Clear();
            found_criticals = 0;
        }
    }
}