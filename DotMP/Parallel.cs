using System;
using System.Collections.Generic;
using System.Threading;

namespace DotMP
{
    /// <summary>
    /// The main class of DotMP.
    /// Contains all the main methods for parallelism.
    /// For users, this is the main class you want to worry about, along with Lock, Shared, and Atomic
    /// </summary>
    public static class Parallel
    {
        /// <summary>
        /// The dictionary for critical regions.
        /// </summary>
        private static volatile Dictionary<int, object> critical_lock = new Dictionary<int, object>();
        /// <summary>
        /// The dictionary for single regions.
        /// </summary>
        private static volatile HashSet<int> single_thread = new HashSet<int>();
        /// <summary>
        /// The dictionary for ordered regions.
        /// </summary>
        private static volatile Dictionary<int, int> ordered = new Dictionary<int, int>();
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
        private static void FixArgs(int start, int end, ref IScheduler sched, ref uint? chunk_size, uint num_threads)
        {
            sched ??= Schedule.Static;

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
                        case "workstealing":
                            sched = Schedule.WorkStealing;
                            break;
                        case "static":
                            sched = Schedule.Static;
                            break;
                        default:
                            Master(() => Console.WriteLine("Invalid schedule specified by OMP_SCHEDULE, defaulting to static."));
                            goto case "static";
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
                if (sched is StaticScheduler)
                {
                    chunk_size = (uint)((end - start) / num_threads);
                    if ((end - start) % num_threads > 0)
                        chunk_size++;
                }
                else if (sched is DynamicScheduler || sched is WorkStealingScheduler)
                {
                    chunk_size = (uint)((end - start) / num_threads) / 32;
                    if (chunk_size < 1) chunk_size = 1;
                }
                else if (sched is GuidedScheduler)
                {
                    chunk_size = 1;
                }
            }
        }

        /// <summary>
        /// Validates all parameters passed to DotMP functions.
        /// </summary>
        /// <param name="start">Start of loop.</param>
        /// <param name="end">End of loop.</param>
        /// <param name="schedule">Scheduler used.</param>
        /// <param name="num_threads">Number of threads.</param>
        /// <param name="chunk_size">Chunk size.</param>
        /// <param name="num_tasks">Number of tasks.</param>
        /// <param name="grainsize">Grainsize.</param>
        /// <exception cref="InvalidArgumentsException">Thrown if any provided arguments are invalid.</exception>
        private static void ValidateParams(int start = 0, int end = 0, IScheduler schedule = null, uint? num_threads = null, uint? chunk_size = null, uint? num_tasks = null, uint? grainsize = null)
        {
            if (end < start)
                throw new InvalidArgumentsException(string.Format("Start of loop ({0}) must be less than end of loop ({1}).", start, end));

            if (start < 0 || end < 0)
                throw new InvalidArgumentsException(string.Format("Start ({0}) and end ({1}) of loop must both be positive integers.", start, end));

            if (num_threads is not null && num_threads < 1)
                throw new InvalidArgumentsException(string.Format("Number of threads ({0}) should be a positive integer.", num_threads));

            if (chunk_size is not null && chunk_size < 1)
                throw new InvalidArgumentsException(string.Format("Chunk size ({0}) should be a positive integer.", chunk_size));

            if ((num_tasks is not null && num_tasks < 1) || (grainsize is not null && grainsize < 1))
                throw new InvalidArgumentsException(string.Format("Number of tasks ({0}) and grain size ({1}) must both be positive integers.", num_tasks, grainsize));

            if (schedule is not null && schedule is not StaticScheduler && schedule is not DynamicScheduler && schedule is not GuidedScheduler && schedule is not RuntimeScheduler && schedule is not WorkStealingScheduler && chunk_size is null)
                throw new InvalidArgumentsException(string.Format("Chunk size must be specified with user-defined schedulers, as it cannot be inferred."));
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
        /// <exception cref="CannotPerformNestedWorksharingException">Thrown when nested inside another worksharing region.</exception>
        /// <exception cref="InvalidArgumentsException">Thrown if any provided arguments are invalid.</exception>
        public static void For(int start, int end, Action<int> action, IScheduler schedule = null, uint? chunk_size = null)
        {
            ForAction<object> forAction = new ForAction<object>(action);

            For(start, end, forAction, schedule, chunk_size);
        }

        /// <summary>
        /// Creates a collapsed for loop inside a parallel region.
        /// A collapsed for loop can be used when you want to parallelize two or more nested for loops.
        /// Instead of only parallelizing across the outermost loop, the nested loops are flattened before scheduling,
        /// which has the effect of parallelizing across both loops.
        /// This has the effect multiplying the number of iterations the scheduler can work with,
        /// which can improve load balancing in irregular nested loops.
        /// </summary>
        /// <param name="firstRange">A tuple representing the start and end of the first for loop.</param>
        /// <param name="secondRange">A tuple representing the start and end of the second for loop.</param>
        /// <param name="action">The action to be performed in the loop.</param>
        /// <param name="schedule">The schedule of the loop, defaulting to static.</param>
        /// <param name="chunk_size">The chunk size of the loop, defaulting to null. If null, will be calculated on-the-fly.</param>
        /// <exception cref="NotInParallelRegionException">Thrown when not in a parallel region.</exception>
        /// <exception cref="CannotPerformNestedWorksharingException">Thrown when nested inside another worksharing region.</exception>
        /// <exception cref="InvalidArgumentsException">Thrown if any provided arguments are invalid.</exception>
        public static void ForCollapse((int, int) firstRange, (int, int) secondRange, Action<int, int> action, IScheduler schedule = null, uint? chunk_size = null)
        {
            ForAction<object> forAction = new ForAction<object>(action, new (int, int)[] { firstRange, secondRange });

            For(0,
                (firstRange.Item2 - firstRange.Item1) *
                (secondRange.Item2 - secondRange.Item1),
                forAction, schedule, chunk_size);
        }

        /// <summary>
        /// Creates a collapsed for loop inside a parallel region.
        /// A collapsed for loop can be used when you want to parallelize two or more nested for loops.
        /// Instead of only parallelizing across the outermost loop, the nested loops are flattened before scheduling,
        /// which has the effect of parallelizing across both loops.
        /// This has the effect multiplying the number of iterations the scheduler can work with,
        /// which can improve load balancing in irregular nested loops.
        /// </summary>
        /// <param name="firstRange">A tuple representing the start and end of the first for loop.</param>
        /// <param name="secondRange">A tuple representing the start and end of the second for loop.</param>
        /// <param name="thirdRange">A tuple representing the start and end of the third for loop.</param>
        /// <param name="action">The action to be performed in the loop.</param>
        /// <param name="schedule">The schedule of the loop, defaulting to static.</param>
        /// <param name="chunk_size">The chunk size of the loop, defaulting to null. If null, will be calculated on-the-fly.</param>
        /// <exception cref="NotInParallelRegionException">Thrown when not in a parallel region.</exception>
        /// <exception cref="CannotPerformNestedWorksharingException">Thrown when nested inside another worksharing region.</exception>
        /// <exception cref="InvalidArgumentsException">Thrown if any provided arguments are invalid.</exception>
        public static void ForCollapse((int, int) firstRange, (int, int) secondRange, (int, int) thirdRange, Action<int, int, int> action, IScheduler schedule = null, uint? chunk_size = null)
        {
            ForAction<object> forAction = new ForAction<object>(action, new (int, int)[] { firstRange, secondRange, thirdRange });

            For(0,
                (firstRange.Item2 - firstRange.Item1) *
                (secondRange.Item2 - secondRange.Item1) *
                (thirdRange.Item2 - thirdRange.Item1),
                forAction, schedule, chunk_size);
        }

        /// <summary>
        /// Creates a collapsed for loop inside a parallel region.
        /// A collapsed for loop can be used when you want to parallelize two or more nested for loops.
        /// Instead of only parallelizing across the outermost loop, the nested loops are flattened before scheduling,
        /// which has the effect of parallelizing across both loops.
        /// This has the effect multiplying the number of iterations the scheduler can work with,
        /// which can improve load balancing in irregular nested loops.
        /// </summary>
        /// <param name="firstRange">A tuple representing the start and end of the first for loop.</param>
        /// <param name="secondRange">A tuple representing the start and end of the second for loop.</param>
        /// <param name="thirdRange">A tuple representing the start and end of the third for loop.</param>
        /// <param name="fourthRange">A tuple representing the start and end of the fourth for loop.</param>
        /// <param name="action">The action to be performed in the loop.</param>
        /// <param name="schedule">The schedule of the loop, defaulting to static.</param>
        /// <param name="chunk_size">The chunk size of the loop, defaulting to null. If null, will be calculated on-the-fly.</param>
        /// <exception cref="NotInParallelRegionException">Thrown when not in a parallel region.</exception>
        /// <exception cref="CannotPerformNestedWorksharingException">Thrown when nested inside another worksharing region.</exception>
        /// <exception cref="InvalidArgumentsException">Thrown if any provided arguments are invalid.</exception>
        public static void ForCollapse((int, int) firstRange, (int, int) secondRange, (int, int) thirdRange, (int, int) fourthRange, Action<int, int, int, int> action, IScheduler schedule = null, uint? chunk_size = null)
        {
            ForAction<object> forAction = new ForAction<object>(action, new (int, int)[] { firstRange, secondRange, thirdRange, fourthRange });

            For(0,
                (firstRange.Item2 - firstRange.Item1) *
                (secondRange.Item2 - secondRange.Item1) *
                (thirdRange.Item2 - thirdRange.Item1) *
                (fourthRange.Item2 - fourthRange.Item1),
                forAction, schedule, chunk_size);
        }

        /// <summary>
        /// Creates a collapsed for loop inside a parallel region.
        /// A collapsed for loop can be used when you want to parallelize two or more nested for loops.
        /// Instead of only parallelizing across the outermost loop, the nested loops are flattened before scheduling,
        /// which has the effect of parallelizing across both loops.
        /// This has the effect multiplying the number of iterations the scheduler can work with,
        /// which can improve load balancing in irregular nested loops.
        /// </summary>
        /// <param name="ranges">A tuple representing the start and end of each of the for loops.</param>
        /// <param name="action">The action to be performed in the loop.</param>
        /// <param name="schedule">The schedule of the loop, defaulting to static.</param>
        /// <param name="chunk_size">The chunk size of the loop, defaulting to null. If null, will be calculated on-the-fly.</param>
        /// <exception cref="NotInParallelRegionException">Thrown when not in a parallel region.</exception>
        /// <exception cref="CannotPerformNestedWorksharingException">Thrown when nested inside another worksharing region.</exception>
        /// <exception cref="InvalidArgumentsException">Thrown if any provided arguments are invalid.</exception>
        public static void ForCollapse((int, int)[] ranges, Action<int[]> action, IScheduler schedule = null, uint? chunk_size = null)
        {
            ForAction<object> forAction = new ForAction<object>(action, ranges);

            int total_iters = 1;

            foreach ((int i, int j) in ranges)
                total_iters *= j - i;

            For(0, total_iters, forAction, schedule, chunk_size);
        }

        /// <summary>
        /// Internal handler for For.
        /// </summary>
        /// <param name="start">The start of the loop, inclusive.</param>
        /// <param name="end">The end of the loop, exclusive.</param>
        /// <param name="forAction">The action to be performed in the loop.</param>
        /// <param name="schedule">The schedule of the loop, defaulting to static.</param>
        /// <param name="chunk_size">The chunk size of the loop, defaulting to null. If null, will be calculated on-the-fly.</param>
        /// <param name="op">The operation to be performed in the case of reduction loops.</param>
        /// <exception cref="NotInParallelRegionException">Thrown when not in a parallel region.</exception>
        /// <exception cref="CannotPerformNestedWorksharingException">Thrown when nested inside another worksharing region.</exception>
        /// <exception cref="InvalidArgumentsException">Thrown if any provided arguments are invalid.</exception>
        private static void For<T>(int start, int end, ForAction<T> forAction, IScheduler schedule = null, uint? chunk_size = null, Operations? op = null)
        {
            var freg = new ForkedRegion();

            ValidateParams(start, end, schedule: schedule, chunk_size: chunk_size);

            if (!freg.in_parallel)
            {
                throw new NotInParallelRegionException("Cannot use DotMP For-like loops outside of a parallel region.");
            }

            if (freg.in_workshare > 0)
            {
                throw new CannotPerformNestedWorksharingException("Cannot use DotMP For-like loops nested within other worksharing constructs.");
            }

            FixArgs(start, end, ref schedule, ref chunk_size, freg.reg.num_threads);

            WorkShare ws = new WorkShare((uint)GetNumThreads(), freg.reg.threads, start, end, chunk_size.Value, op, schedule);

            Barrier();

            ws.in_for = true;
            Interlocked.Increment(ref freg.in_workshare);

            ws.PerformLoop(forAction);

            ws.in_for = false;
            Interlocked.Decrement(ref freg.in_workshare);
            Barrier();

            Master(ordered.Clear);

            Barrier();
        }

        /// <summary>
        /// Creates a for loop inside a parallel region with a reduction.
        /// This is similar to For(), but the reduction allows multiple threads to reduce their work down to a single variable.
        /// Using ForReduction&lt;T&gt; allows the runtime to perform this operation much more efficiently than a naive approach using the Locking or Atomic classes.
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
        /// <exception cref="CannotPerformNestedWorksharingException">Thrown when nested inside another worksharing region.</exception>
        /// <exception cref="InvalidArgumentsException">Thrown if any provided arguments are invalid.</exception>
        public static void ForReduction<T>(int start, int end, Operations op, ref T reduce_to, ActionRef<T> action, IScheduler schedule = null, uint? chunk_size = null)
        {
            ForAction<T> forAction = new ForAction<T>(action);

            ForReduction(start, end, op, ref reduce_to, forAction, schedule, chunk_size);
        }

        /// <summary>
        /// Creates a collapsed reduction for loop inside a parallel region.
        /// A collapsed for loop can be used when you want to parallelize two or more nested for loops.
        /// Instead of only parallelizing across the outermost loop, the nested loops are flattened before scheduling,
        /// which has the effect of parallelizing across both loops.
        /// This has the effect multiplying the number of iterations the scheduler can work with,
        /// which can improve load balancing in irregular nested loops.
        /// 
        /// Unlike Parallel.ForCollapse, this method permits a reduction parameter.
        /// </summary>
        /// <typeparam name="T">The type of the reduction.</typeparam>
        /// <param name="firstRange">A tuple representing the start and end of the first for loop.</param>
        /// <param name="secondRange">A tuple representing the start and end of the second for loop.</param>
        /// <param name="op">The operation to be performed on the reduction.</param>
        /// <param name="reduce_to">The variable to reduce to.</param>
        /// <param name="action">The action to be performed in the loop.</param>
        /// <param name="schedule">The schedule of the loop, defaulting to static.</param>
        /// <param name="chunk_size">The chunk size of the loop, defaulting to null. If null, will be calculated on-the-fly.</param>
        /// <exception cref="NotInParallelRegionException">Thrown when not in a parallel region.</exception>
        /// <exception cref="CannotPerformNestedWorksharingException">Thrown when nested inside another worksharing region.</exception>
        /// <exception cref="InvalidArgumentsException">Thrown if any provided arguments are invalid.</exception>
        public static void ForReductionCollapse<T>((int, int) firstRange, (int, int) secondRange, Operations op, ref T reduce_to, ActionRef2<T> action, IScheduler schedule = null, uint? chunk_size = null)
        {
            ForAction<T> forAction = new ForAction<T>(action, new (int, int)[] { firstRange, secondRange });

            ForReduction(0,
                (firstRange.Item2 - firstRange.Item1) *
                (secondRange.Item2 - secondRange.Item1),
                op, ref reduce_to, forAction, schedule, chunk_size);
        }

        /// <summary>
        /// Creates a collapsed reduction for loop inside a parallel region.
        /// A collapsed for loop can be used when you want to parallelize two or more nested for loops.
        /// Instead of only parallelizing across the outermost loop, the nested loops are flattened before scheduling,
        /// which has the effect of parallelizing across both loops.
        /// This has the effect multiplying the number of iterations the scheduler can work with,
        /// which can improve load balancing in irregular nested loops.
        /// 
        /// Unlike Parallel.ForCollapse, this method permits a reduction parameter.
        /// </summary>
        /// <typeparam name="T">The type of the reduction.</typeparam>
        /// <param name="firstRange">A tuple representing the start and end of the first for loop.</param>
        /// <param name="secondRange">A tuple representing the start and end of the second for loop.</param>
        /// <param name="thirdRange">A tuple representing the start and end of the third for loop.</param>
        /// <param name="op">The operation to be performed on the reduction.</param>
        /// <param name="reduce_to">The variable to reduce to.</param>
        /// <param name="action">The action to be performed in the loop.</param>
        /// <param name="schedule">The schedule of the loop, defaulting to static.</param>
        /// <param name="chunk_size">The chunk size of the loop, defaulting to null. If null, will be calculated on-the-fly.</param>
        /// <exception cref="NotInParallelRegionException">Thrown when not in a parallel region.</exception>
        /// <exception cref="CannotPerformNestedWorksharingException">Thrown when nested inside another worksharing region.</exception>
        /// <exception cref="InvalidArgumentsException">Thrown if any provided arguments are invalid.</exception>
        public static void ForReductionCollapse<T>((int, int) firstRange, (int, int) secondRange, (int, int) thirdRange, Operations op, ref T reduce_to, ActionRef3<T> action, IScheduler schedule = null, uint? chunk_size = null)
        {
            ForAction<T> forAction = new ForAction<T>(action, new (int, int)[] { firstRange, secondRange, thirdRange });

            ForReduction(0,
                (firstRange.Item2 - firstRange.Item1) *
                (secondRange.Item2 - secondRange.Item1) *
                (thirdRange.Item2 - thirdRange.Item1),
                op, ref reduce_to, forAction, schedule, chunk_size);
        }

        /// <summary>
        /// Creates a collapsed reduction for loop inside a parallel region.
        /// A collapsed for loop can be used when you want to parallelize two or more nested for loops.
        /// Instead of only parallelizing across the outermost loop, the nested loops are flattened before scheduling,
        /// which has the effect of parallelizing across both loops.
        /// This has the effect multiplying the number of iterations the scheduler can work with,
        /// which can improve load balancing in irregular nested loops.
        /// 
        /// Unlike Parallel.ForCollapse, this method permits a reduction parameter.
        /// </summary>
        /// <typeparam name="T">The type of the reduction.</typeparam>
        /// <param name="firstRange">A tuple representing the start and end of the first for loop.</param>
        /// <param name="secondRange">A tuple representing the start and end of the second for loop.</param>
        /// <param name="thirdRange">A tuple representing the start and end of the third for loop.</param>
        /// <param name="fourthRange">A tuple representing the start and end of the fourth for loop.</param>
        /// <param name="op">The operation to be performed on the reduction.</param>
        /// <param name="reduce_to">The variable to reduce to.</param>
        /// <param name="action">The action to be performed in the loop.</param>
        /// <param name="schedule">The schedule of the loop, defaulting to static.</param>
        /// <param name="chunk_size">The chunk size of the loop, defaulting to null. If null, will be calculated on-the-fly.</param>
        /// <exception cref="NotInParallelRegionException">Thrown when not in a parallel region.</exception>
        /// <exception cref="CannotPerformNestedWorksharingException">Thrown when nested inside another worksharing region.</exception>
        /// <exception cref="InvalidArgumentsException">Thrown if any provided arguments are invalid.</exception>
        public static void ForReductionCollapse<T>((int, int) firstRange, (int, int) secondRange, (int, int) thirdRange, (int, int) fourthRange, Operations op, ref T reduce_to, ActionRef4<T> action, IScheduler schedule = null, uint? chunk_size = null)
        {
            ForAction<T> forAction = new ForAction<T>(action, new (int, int)[] { firstRange, secondRange, thirdRange, fourthRange });

            ForReduction(0,
                (firstRange.Item2 - firstRange.Item1) *
                (secondRange.Item2 - secondRange.Item1) *
                (thirdRange.Item2 - thirdRange.Item1) *
                (fourthRange.Item2 - fourthRange.Item1),
                op, ref reduce_to, forAction, schedule, chunk_size);
        }

        /// <summary>
        /// Creates a collapsed reduction for loop inside a parallel region.
        /// A collapsed for loop can be used when you want to parallelize two or more nested for loops.
        /// Instead of only parallelizing across the outermost loop, the nested loops are flattened before scheduling,
        /// which has the effect of parallelizing across both loops.
        /// This has the effect multiplying the number of iterations the scheduler can work with,
        /// which can improve load balancing in irregular nested loops.
        /// 
        /// Unlike Parallel.ForCollapse, this method permits a reduction parameter.
        /// </summary>
        /// <typeparam name="T">The type of the reduction.</typeparam>
        /// <param name="ranges">A tuple representing the start and end of each of the for loops.</param>
        /// <param name="op">The operation to be performed on the reduction.</param>
        /// <param name="reduce_to">The variable to reduce to.</param>
        /// <param name="action">The action to be performed in the loop.</param>
        /// <param name="schedule">The schedule of the loop, defaulting to static.</param>
        /// <param name="chunk_size">The chunk size of the loop, defaulting to null. If null, will be calculated on-the-fly.</param>
        /// <exception cref="NotInParallelRegionException">Thrown when not in a parallel region.</exception>
        /// <exception cref="CannotPerformNestedWorksharingException">Thrown when nested inside another worksharing region.</exception>
        /// <exception cref="InvalidArgumentsException">Thrown if any provided arguments are invalid.</exception>
        public static void ForReductionCollapse<T>((int, int)[] ranges, Operations op, ref T reduce_to, ActionRefN<T> action, IScheduler schedule = null, uint? chunk_size = null)
        {
            ForAction<T> forAction = new ForAction<T>(action, ranges);

            int total_iters = 1;

            foreach ((int i, int j) in ranges)
                total_iters *= j - i;

            ForReduction(0, total_iters, op, ref reduce_to, forAction, schedule, chunk_size);
        }

        /// <summary>
        /// Internal handler for ForReduction.
        /// </summary>
        /// <param name="start">The start of the loop, inclusive.</param>
        /// <param name="end">The end of the loop, exclusive.</param>
        /// <param name="action">The action to be performed in the loop.</param>
        /// <param name="schedule">The schedule of the loop, defaulting to static.</param>
        /// <param name="chunk_size">The chunk size of the loop, defaulting to null. If null, will be calculated on-the-fly.</param>
        /// <param name="op">The operation to be performed in the case of reduction loops.</param>
        /// <param name="reduce_to">The variable to reduce to.</param>
        /// <exception cref="NotInParallelRegionException">Thrown when not in a parallel region.</exception>
        /// <exception cref="CannotPerformNestedWorksharingException">Thrown when nested inside another worksharing region.</exception>
        /// <exception cref="InvalidArgumentsException">Thrown if any provided arguments are invalid.</exception>
        private static void ForReduction<T>(int start, int end, Operations op, ref T reduce_to, ForAction<T> action, IScheduler schedule = null, uint? chunk_size = null)
        {
            For(start, end, action, schedule, chunk_size, op);

            WorkShare ws = new WorkShare();

            if (GetThreadNum() == 0)
            {
                foreach (T i in ws.reduction_values)
                {
                    switch (op)
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
        /// <exception cref="InvalidArgumentsException">Thrown if any provided arguments are invalid.</exception>
        public static void ParallelRegion(Action action, uint? num_threads = null)
        {
            ValidateParams(num_threads: num_threads);

            if (InParallel())
            {
                throw new CannotPerformNestedParallelismException("Cannot spawn a parallel region within another parallel region.");
            }

            if (num_threads == null && Parallel.num_threads == 0)
                num_threads = (uint)GetNumProcs();
            else
                num_threads ??= Parallel.num_threads;

            ForkedRegion freg = new ForkedRegion(num_threads.Value, action);
            barrier = new Barrier((int)num_threads.Value);
            freg.StartThreadpool();
            freg.reg.num_threads = 1;
            single_thread.Clear();
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
        /// <exception cref="CannotPerformNestedParallelismException">Thrown if ParallelRegion is called from within another ParallelRegion.</exception>
        /// <exception cref="InvalidArgumentsException">Thrown if any provided arguments are invalid.</exception>
        public static void ParallelFor(int start, int end, Action<int> action, IScheduler schedule = null, uint? chunk_size = null, uint? num_threads = null)
        {
            ParallelRegion(num_threads: num_threads, action: () =>
            {
                For(start, end, action, schedule, chunk_size);
            });
        }

        /// <summary>
        /// Creates a parallel for loop with a reduction. Contains all of the parameters from ParallelRegion() and ForReduction&lt;T&gt;().
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
        /// <exception cref="CannotPerformNestedParallelismException">Thrown if ParallelRegion is called from within another ParallelRegion.</exception>
        /// <exception cref="InvalidArgumentsException">Thrown if any provided arguments are invalid.</exception>
        public static void ParallelForReduction<T>(int start, int end, Operations op, ref T reduce_to, ActionRef<T> action, IScheduler schedule = null, uint? chunk_size = null, uint? num_threads = null)
        {
            T local = reduce_to;

            ParallelRegion(num_threads: num_threads, action: () =>
            {
                ForReduction(start, end, op, ref local, action, schedule, chunk_size);
            });

            reduce_to = local;
        }

        /// <summary>
        /// Creates a parallel collapsed for loop. Contains all of the parameters from ParallelRegion() and ForCollapse().
        /// This is simply a convenience method for creating a parallel region and a collapsed for loop.
        /// </summary>
        /// <param name="firstRange">A tuple representing the start and end of the first for loop.</param>
        /// <param name="secondRange">A tuple representing the start and end of the second for loop.</param>
        /// <param name="action">The action to be performed in the loop.</param>
        /// <param name="schedule">The schedule of the loop, defaulting to static.</param>
        /// <param name="chunk_size">The chunk size of the loop, defaulting to null. If null, will be calculated on-the-fly.</param>
        /// <param name="num_threads">The number of threads to be used in the loop, defaulting to null. If null, will be calculated on-the-fly.</param>
        /// <exception cref="CannotPerformNestedParallelismException">Thrown if ParallelRegion is called from within another ParallelRegion.</exception>
        /// <exception cref="InvalidArgumentsException">Thrown if any provided arguments are invalid.</exception>
        public static void ParallelForCollapse((int, int) firstRange, (int, int) secondRange, Action<int, int> action, IScheduler schedule = null, uint? chunk_size = null, uint? num_threads = null)
        {
            ParallelRegion(num_threads: num_threads, action: () =>
            {
                ForCollapse(firstRange, secondRange, action, schedule, chunk_size);
            });
        }

        /// <summary>
        /// Creates a parallel collapsed for loop. Contains all of the parameters from ParallelRegion() and ForCollapse().
        /// This is simply a convenience method for creating a parallel region and a collapsed for loop.
        /// </summary>
        /// <param name="firstRange">A tuple representing the start and end of the first for loop.</param>
        /// <param name="secondRange">A tuple representing the start and end of the second for loop.</param>
        /// <param name="thirdRange">A tuple representing the start and end of the third for loop.</param>
        /// <param name="action">The action to be performed in the loop.</param>
        /// <param name="schedule">The schedule of the loop, defaulting to static.</param>
        /// <param name="chunk_size">The chunk size of the loop, defaulting to null. If null, will be calculated on-the-fly.</param>
        /// <param name="num_threads">The number of threads to be used in the loop, defaulting to null. If null, will be calculated on-the-fly.</param>
        /// <exception cref="CannotPerformNestedParallelismException">Thrown if ParallelRegion is called from within another ParallelRegion.</exception>
        /// <exception cref="InvalidArgumentsException">Thrown if any provided arguments are invalid.</exception>
        public static void ParallelForCollapse((int, int) firstRange, (int, int) secondRange, (int, int) thirdRange, Action<int, int, int> action, IScheduler schedule = null, uint? chunk_size = null, uint? num_threads = null)
        {
            ParallelRegion(num_threads: num_threads, action: () =>
            {
                ForCollapse(firstRange, secondRange, thirdRange, action, schedule, chunk_size);
            });
        }

        /// <summary>
        /// Creates a parallel collapsed for loop. Contains all of the parameters from ParallelRegion() and ForCollapse().
        /// This is simply a convenience method for creating a parallel region and a collapsed for loop.
        /// </summary>
        /// <param name="firstRange">A tuple representing the start and end of the first for loop.</param>
        /// <param name="secondRange">A tuple representing the start and end of the second for loop.</param>
        /// <param name="thirdRange">A tuple representing the start and end of the third for loop.</param>
        /// <param name="fourthRange">A tuple representing the start and end of the fourth for loop.</param>
        /// <param name="action">The action to be performed in the loop.</param>
        /// <param name="schedule">The schedule of the loop, defaulting to static.</param>
        /// <param name="chunk_size">The chunk size of the loop, defaulting to null. If null, will be calculated on-the-fly.</param>
        /// <param name="num_threads">The number of threads to be used in the loop, defaulting to null. If null, will be calculated on-the-fly.</param>
        /// <exception cref="CannotPerformNestedParallelismException">Thrown if ParallelRegion is called from within another ParallelRegion.</exception>
        /// <exception cref="InvalidArgumentsException">Thrown if any provided arguments are invalid.</exception>
        public static void ParallelForCollapse((int, int) firstRange, (int, int) secondRange, (int, int) thirdRange, (int, int) fourthRange, Action<int, int, int, int> action, IScheduler schedule = null, uint? chunk_size = null, uint? num_threads = null)
        {
            ParallelRegion(num_threads: num_threads, action: () =>
            {
                ForCollapse(firstRange, secondRange, thirdRange, fourthRange, action, schedule, chunk_size);
            });
        }

        /// <summary>
        /// Creates a parallel collapsed for loop. Contains all of the parameters from ParallelRegion() and ForCollapse().
        /// This is simply a convenience method for creating a parallel region and a collapsed for loop.
        /// </summary>
        /// <param name="ranges">A tuple representing the start and end of each of the for loops.</param>
        /// <param name="action">The action to be performed in the loop.</param>
        /// <param name="schedule">The schedule of the loop, defaulting to static.</param>
        /// <param name="chunk_size">The chunk size of the loop, defaulting to null. If null, will be calculated on-the-fly.</param>
        /// <param name="num_threads">The number of threads to be used in the loop, defaulting to null. If null, will be calculated on-the-fly.</param>
        /// <exception cref="CannotPerformNestedParallelismException">Thrown if ParallelRegion is called from within another ParallelRegion.</exception>
        /// <exception cref="InvalidArgumentsException">Thrown if any provided arguments are invalid.</exception>
        public static void ParallelForCollapse((int, int)[] ranges, Action<int[]> action, IScheduler schedule = null, uint? chunk_size = null, uint? num_threads = null)
        {
            ParallelRegion(num_threads: num_threads, action: () =>
            {
                ForCollapse(ranges, action, schedule, chunk_size);
            });
        }

        /// <summary>
        /// Creates a parallel collapsed reduction for loop. Contains all of the parameters from ParallelRegion() and ForReductionCollapse().
        /// This is simply a convenience method for creating a parallel region and a collapsed for loop with a reduction inside of it.
        /// </summary>
        /// <param name="firstRange">A tuple representing the start and end of the first for loop.</param>
        /// <param name="secondRange">A tuple representing the start and end of the second for loop.</param>
        /// <param name="op">The operation to be performed on the reduction.</param>
        /// <param name="reduce_to">The variable to reduce to.</param>
        /// <param name="action">The action to be performed in the loop.</param>
        /// <param name="schedule">The schedule of the loop, defaulting to static.</param>
        /// <param name="chunk_size">The chunk size of the loop, defaulting to null. If null, will be calculated on-the-fly.</param>
        /// <param name="num_threads">The number of threads to be used in the loop, defaulting to null. If null, will be calculated on-the-fly.</param>
        /// <exception cref="CannotPerformNestedParallelismException">Thrown if ParallelRegion is called from within another ParallelRegion.</exception>
        /// <exception cref="InvalidArgumentsException">Thrown if any provided arguments are invalid.</exception>
        public static void ParallelForReductionCollapse<T>((int, int) firstRange, (int, int) secondRange, Operations op, ref T reduce_to, ActionRef2<T> action, IScheduler schedule = null, uint? chunk_size = null, uint? num_threads = null)
        {
            T local = reduce_to;

            ParallelRegion(num_threads: num_threads, action: () =>
            {
                ForReductionCollapse(firstRange, secondRange, op, ref local, action, schedule, chunk_size);
            });

            reduce_to = local;
        }

        /// <summary>
        /// Creates a parallel collapsed reduction for loop. Contains all of the parameters from ParallelRegion() and ForReductionCollapse().
        /// This is simply a convenience method for creating a parallel region and a collapsed for loop with a reduction inside of it.
        /// </summary>
        /// <param name="firstRange">A tuple representing the start and end of the first for loop.</param>
        /// <param name="secondRange">A tuple representing the start and end of the second for loop.</param>
        /// <param name="thirdRange">A tuple representing the start and end of the third for loop.</param>
        /// <param name="op">The operation to be performed on the reduction.</param>
        /// <param name="reduce_to">The variable to reduce to.</param>
        /// <param name="action">The action to be performed in the loop.</param>
        /// <param name="schedule">The schedule of the loop, defaulting to static.</param>
        /// <param name="chunk_size">The chunk size of the loop, defaulting to null. If null, will be calculated on-the-fly.</param>
        /// <param name="num_threads">The number of threads to be used in the loop, defaulting to null. If null, will be calculated on-the-fly.</param>
        /// <exception cref="CannotPerformNestedParallelismException">Thrown if ParallelRegion is called from within another ParallelRegion.</exception>
        /// <exception cref="InvalidArgumentsException">Thrown if any provided arguments are invalid.</exception>
        public static void ParallelForReductionCollapse<T>((int, int) firstRange, (int, int) secondRange, (int, int) thirdRange, Operations op, ref T reduce_to, ActionRef3<T> action, IScheduler schedule = null, uint? chunk_size = null, uint? num_threads = null)
        {
            T local = reduce_to;

            ParallelRegion(num_threads: num_threads, action: () =>
            {
                ForReductionCollapse(firstRange, secondRange, thirdRange, op, ref local, action, schedule, chunk_size);
            });

            reduce_to = local;
        }

        /// <summary>
        /// Creates a parallel collapsed reduction for loop. Contains all of the parameters from ParallelRegion() and ForReductionCollapse().
        /// This is simply a convenience method for creating a parallel region and a collapsed for loop with a reduction inside of it.
        /// </summary>
        /// <param name="firstRange">A tuple representing the start and end of the first for loop.</param>
        /// <param name="secondRange">A tuple representing the start and end of the second for loop.</param>
        /// <param name="thirdRange">A tuple representing the start and end of the third for loop.</param>
        /// <param name="fourthRange">A tuple representing the start and end of the fourth for loop.</param>
        /// <param name="op">The operation to be performed on the reduction.</param>
        /// <param name="reduce_to">The variable to reduce to.</param>
        /// <param name="action">The action to be performed in the loop.</param>
        /// <param name="schedule">The schedule of the loop, defaulting to static.</param>
        /// <param name="chunk_size">The chunk size of the loop, defaulting to null. If null, will be calculated on-the-fly.</param>
        /// <param name="num_threads">The number of threads to be used in the loop, defaulting to null. If null, will be calculated on-the-fly.</param>
        /// <exception cref="CannotPerformNestedParallelismException">Thrown if ParallelRegion is called from within another ParallelRegion.</exception>
        /// <exception cref="InvalidArgumentsException">Thrown if any provided arguments are invalid.</exception>
        public static void ParallelForReductionCollapse<T>((int, int) firstRange, (int, int) secondRange, (int, int) thirdRange, (int, int) fourthRange, Operations op, ref T reduce_to, ActionRef4<T> action, IScheduler schedule = null, uint? chunk_size = null, uint? num_threads = null)
        {
            T local = reduce_to;

            ParallelRegion(num_threads: num_threads, action: () =>
            {
                ForReductionCollapse(firstRange, secondRange, thirdRange, fourthRange, op, ref local, action, schedule, chunk_size);
            });

            reduce_to = local;
        }

        /// <summary>
        /// Creates a parallel collapsed reduction for loop. Contains all of the parameters from ParallelRegion() and ForReductionCollapse().
        /// This is simply a convenience method for creating a parallel region and a collapsed for loop with a reduction inside of it.
        /// </summary>
        /// <param name="ranges">A tuple representing the start and end of each of the for loops.</param>
        /// <param name="op">The operation to be performed on the reduction.</param>
        /// <param name="reduce_to">The variable to reduce to.</param>
        /// <param name="action">The action to be performed in the loop.</param>
        /// <param name="schedule">The schedule of the loop, defaulting to static.</param>
        /// <param name="chunk_size">The chunk size of the loop, defaulting to null. If null, will be calculated on-the-fly.</param>
        /// <param name="num_threads">The number of threads to be used in the loop, defaulting to null. If null, will be calculated on-the-fly.</param>
        /// <exception cref="CannotPerformNestedParallelismException">Thrown if ParallelRegion is called from within another ParallelRegion.</exception>
        /// <exception cref="InvalidArgumentsException">Thrown if any provided arguments are invalid.</exception>
        public static void ParallelForReductionCollapse<T>((int, int)[] ranges, Operations op, ref T reduce_to, ActionRefN<T> action, IScheduler schedule = null, uint? chunk_size = null, uint? num_threads = null)
        {
            T local = reduce_to;

            ParallelRegion(num_threads: num_threads, action: () =>
            {
                ForReductionCollapse(ranges, op, ref local, action, schedule, chunk_size);
            });

            reduce_to = local;
        }

        /// <summary>
        /// Creates a sections region.
        /// Sections allows for the user to submit multiple, individual tasks to be distributed among threads in parallel.
        /// In parallel, each thread active will dequeue a callback and execute it.
        /// This is useful if you have lots of individual tasks that need to be executed in parallel, and each task requires its own lambda.
        /// Acts as an implicit Barrier().
        /// </summary>
        /// <param name="actions">The actions to be performed in the sections region.</param>
        /// <exception cref="NotInParallelRegionException">Thrown when not in a parallel region.</exception>
        public static void Sections(params Action[] actions)
        {
            bool successful;

            if (!InParallel())
            {
                throw new NotInParallelRegionException("Cannot use DotMP Sections outside of a parallel region.");
            }

            SectionsContainer sc = new SectionsContainer(actions);

            Barrier();

            do
            {
                Action do_action = sc.GetNextItem(out successful);

                if (successful) do_action();
            }
            while (successful);

            Barrier();
        }

        /// <summary>
        /// Enqueue a task into the task queue.
        /// Differing from OpenMP, there is no concept of parent or child tasks as of yet.
        /// All tasks submitted are treated equally in a central task queue.
        /// </summary>
        /// <param name="action">The task to enqueue.</param>
        /// <param name="depends">List of dependencies for the task.</param>
        /// <returns>The task generated for use as a future dependency.</returns>
        public static TaskUUID Task(Action action, params TaskUUID[] depends)
        {
            TaskingContainer tc = new TaskingContainer();
            return tc.EnqueueTask(action, depends);
        }

        /// <summary>
        /// Wait for all tasks in the queue to complete.
        /// Is injected into a Thread's work by the Region constructor, but can also be called manually.
        /// The injection is done to ensure that Parallel.Taskwait() is called before a Parallel.ParallelRegion() terminates,
        /// guaranteeing all tasks submitted complete.
        /// Acts as an implicit Barrier().
        /// </summary>
        public static void Taskwait()
        {
            ForkedRegion fr = new ForkedRegion();
            TaskingContainer tc = new TaskingContainer();
            int tasks_remaining;

            Barrier();

            do if (tc.GetNextTask(out Action do_action, out ulong uuid, out tasks_remaining))
                {
                    do_action();
                    tc.CompleteTask(uuid);
                }
            while (tasks_remaining > 0);

            Barrier();

            tc.ResetDAG();

            Barrier();
        }

        /// <summary>
        /// Creates a number of tasks to complete a for loop in parallel.
        /// If neither grainsize nor num_tasks are specified, a grainsize is calculated on-the-fly.
        /// If both grainsize and num_tasks are specified, the num_tasks parameter takes precedence over grainsize.
        /// </summary>
        /// <param name="start">The start of the taskloop, inclusive.</param>
        /// <param name="end">The end of the taskloop, exclusive.</param>
        /// <param name="action">The action to be executed as the body of the loop.</param>
        /// <param name="grainsize">The number of iterations to be completed per task.</param>
        /// <param name="num_tasks">The number of tasks to spawn to complete the loop.</param>
        /// <param name="only_if">Only generate tasks if true, otherwise execute loop sequentially.</param>
        /// <param name="depends">List of task dependencies for taskloop.</param>
        /// <returns>List of tasks generated by taskloop for use as future dependencies.</returns>
        /// <exception cref="InvalidArgumentsException">Thrown if any provided arguments are invalid.</exception>
        public static TaskUUID[] Taskloop(int start, int end, Action<int> action, uint? grainsize = null, uint? num_tasks = null, bool only_if = true, params TaskUUID[] depends)
        {
            ValidateParams(start, end, grainsize: grainsize, num_tasks: num_tasks);

            if (only_if)
            {
                ForkedRegion fr = new ForkedRegion();
                TaskingContainer tc = new TaskingContainer();

                if (grainsize is null && num_tasks is null)
                {
                    grainsize = (uint)((end - start) / fr.reg.num_threads) / 8;
                    if (grainsize < 1) grainsize = 1;
                }
                else if (num_tasks is not null)
                {
                    grainsize = (uint)(end - start) / num_tasks;
                    if (grainsize < 1) grainsize = 1;
                }

                List<TaskUUID> uuids = new List<TaskUUID>();
                for (int i = start; i < end; i += (int)grainsize)
                {
                    int t_end = i + (int)grainsize;
                    uuids.Add(tc.EnqueueTaskloopTask(i, t_end < end ? t_end : end, action, depends));
                }
                return uuids.ToArray();
            }
            else
            {
                for (int i = start; i < end; i++)
                {
                    action(i);
                }
                return new TaskUUID[0];
            }
        }

        /// <summary>
        /// Wrapper around Parallel.ParallelRegion(), Parallel.Master(), and Parallel.Taskloop().
        /// </summary>
        /// <param name="start">The start of the taskloop, inclusive.</param>
        /// <param name="end">The end of the taskloop, exclusive.</param>
        /// <param name="action">The action to be executed as the body of the loop.</param>
        /// <param name="grainsize">The number of iterations to be completed per task.</param>
        /// <param name="num_tasks">The number of tasks to spawn to complete the loop.</param>
        /// <param name="num_threads">The number of threads to be used in the parallel region, defaulting to null. If null, will be calculated on-the-fly.</param>
        /// <param name="only_if">Only generate tasks if true, otherwise execute loop sequentially.</param>
        /// <exception cref="CannotPerformNestedParallelismException">Thrown if ParallelRegion is called from within another ParallelRegion.</exception>
        /// <exception cref="InvalidArgumentsException">Thrown if any provided arguments are invalid.</exception>
        public static void ParallelMasterTaskloop(int start, int end, Action<int> action, uint? grainsize = null, uint? num_tasks = null, uint? num_threads = null, bool only_if = true)
        {
            ParallelRegion(num_threads: num_threads, action: () =>
            {
                Master(() =>
                {
                    Taskloop(start, end, action, grainsize, num_tasks, only_if);
                });
            });
        }

        /// <summary>
        /// Wrapper around Parallel.ParallelRegion() and Parallel.Master().
        /// </summary>
        /// <param name="action">The action to be performed in the parallel region.</param>
        /// <param name="num_threads">The number of threads to be used in the parallel region, defaulting to null. If null, will be calculated on-the-fly.</param>
        /// <exception cref="CannotPerformNestedParallelismException">Thrown if ParallelRegion is called from within another ParallelRegion.</exception>
        /// <exception cref="InvalidArgumentsException">Thrown if any provided arguments are invalid.</exception>
        public static void ParallelMaster(Action action, uint? num_threads = null)
        {
            ParallelRegion(num_threads: num_threads, action: () =>
            {
                Master(action);
            });
        }

        /// <summary>
        /// Wrapper around Parallel.Master() and Parallel.Taskloop().
        /// </summary>
        /// <param name="start">The start of the taskloop, inclusive.</param>
        /// <param name="end">The end of the taskloop, exclusive.</param>
        /// <param name="action">The action to be executed as the body of the loop.</param>
        /// <param name="grainsize">The number of iterations to be completed per task.</param>
        /// <param name="num_tasks">The number of tasks to spawn to complete the loop.</param>
        /// <param name="only_if">Only generate tasks if true, otherwise execute loop sequentially.</param>
        /// <exception cref="InvalidArgumentsException">Thrown if any provided arguments are invalid.</exception>
        public static void MasterTaskloop(int start, int end, Action<int> action, uint? grainsize = null, uint? num_tasks = null, bool only_if = true)
        {
            Master(() =>
            {
                Taskloop(start, end, action, grainsize, num_tasks, only_if);
            });
        }

        /// <summary>
        /// Creates a parallel sections region. Contains all of the parameters from ParallelRegion() and Sections().
        /// This is simply a convenience method for creating a parallel region and a sections region inside of it.
        /// </summary>
        /// <param name="actions">The actions to be performed in the parallel sections region.</param>
        /// <param name="num_threads">The number of threads to be used in the parallel sections region, defaulting to null. If null, will be calculated on-the-fly.</param>
        /// <exception cref="CannotPerformNestedParallelismException">Thrown if ParallelRegion is called from within another ParallelRegion.</exception>
        /// <exception cref="InvalidArgumentsException">Thrown if any provided arguments are invalid.</exception>
        public static void ParallelSections(uint? num_threads = null, params Action[] actions)
        {
            ParallelRegion(num_threads: num_threads, action: () =>
            {
                Sections(actions);
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
            if (!InParallel())
            {
                throw new NotInParallelRegionException("Cannot use DotMP Critical outside of a parallel region.");
            }

            object lock_obj;

            lock (critical_lock)
            {
                if (!critical_lock.ContainsKey(id))
                {
                    critical_lock.Add(id, new object());
                }

                lock_obj = critical_lock[id];
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
            if (!InParallel())
            {
                throw new NotInParallelRegionException("Cannot use DotMP Barrier outside of a parallel region.");
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
            if (!InParallel())
            {
                throw new NotInParallelRegionException("Cannot use DotMP Master outside of a parallel region.");
            }

            if (GetThreadNum() == 0)
            {
                action();
            }
        }

        /// <summary>
        /// Creates a single region.
        /// A single region is only executed once per Parallel.ParallelRegion.
        /// The first thread to encounter the single region marks the region as encountered, then executes it.
        /// </summary>
        /// <param name="id">The ID of the single region. Must be unique per region but consistent across all threads.</param>
        /// <param name="action">The action to be performed in the single region.</param>
        /// <exception cref="NotInParallelRegionException">Thrown when not in a parallel region.</exception>
        /// <exception cref="CannotPerformNestedWorksharingException">Thrown when nested inside another worksharing region.</exception>
        public static void Single(int id, Action action)
        {
            var freg = new ForkedRegion();
            bool new_single = false;

            if (!freg.in_parallel)
            {
                throw new NotInParallelRegionException("Cannot use DotMP Single outside of a parallel region.");
            }

            var ws = new WorkShare();

            if (ws.in_for)
            {
                throw new CannotPerformNestedWorksharingException("Cannot use DotMP Single nested within other worksharing constructs.");
            }

            Interlocked.Increment(ref freg.in_workshare);

            lock (single_thread)
            {
                if (!single_thread.Contains(id))
                {
                    single_thread.Add(id);
                    new_single = true;
                }
            }

            if (new_single)
            {
                action();
            }

            Interlocked.Decrement(ref freg.in_workshare);

            Barrier();
        }

        /// <summary>
        /// Creates an ordered region.
        /// An ordered region is a region of code that is executed in order inside of a For() or ForReduction&lt;T&gt;() loop.
        /// This also acts as an implicit Critical() region.
        /// </summary>
        /// <param name="id">The ID of the ordered region. Must be unique per region but consistent across all threads.</param>
        /// <param name="action">The action to be performed in the ordered region.</param>
        /// <exception cref="NotInParallelRegionException">Thrown when not in a parallel region.</exception>
        public static void Ordered(int id, Action action)
        {
            var freg = new ForkedRegion();

            if (!freg.in_parallel)
            {
                throw new NotInParallelRegionException("Cannot use DotMP Ordered outside of a parallel region.");
            }

            lock (ordered)
            {
                if (!ordered.ContainsKey(id))
                {
                    ordered.Add(id, 0);
                }
                Thread.MemoryBarrier();
            }

            WorkShare ws = new WorkShare();

            while (ordered[id] != ws.working_iter) ;

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
            var freg = new ForkedRegion();

            return (freg.reg is not null)
                    ? (int)freg.reg.num_threads
                    : 1;
        }

        /// <summary>
        /// Gets the ID of the calling thread.
        /// </summary>
        /// <returns>The number of the calling thread.</returns>
        /// <exception cref="NotInParallelRegionException">Thrown when not in a parallel region.</exception>
        public static int GetThreadNum()
        {
            if (!InParallel())
            {
                throw new NotInParallelRegionException("Cannot get current thread number outside of a parallel region.");
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
            var freg = new ForkedRegion();

            return freg.in_parallel;
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
        /// Returns the current schedule being used in a For() or ForReduction&lt;T&gt;() loop.
        /// </summary>
        /// <returns>The schedule being used in the For() or ForReduction&lt;T&gt;() loop, or null if a For() or ForReduction&lt;T&gt;() has not been encountered yet.</returns>
        public static IScheduler GetSchedule()
        {
            return new WorkShare().schedule;
        }

        /// <summary>
        /// Returns the current chunk size being used in a For() or ForReduction&lt;T&gt;() loop.
        /// </summary>
        /// <returns>The chunk size being used in a For() or ForReduction&lt;T&gt;() loop. If 0, a For() or ForReduction&lt;T&gt;() has not been encountered yet.</returns>
        public static uint GetChunkSize()
        {
            return new WorkShare().chunk_size;
        }
    }
}