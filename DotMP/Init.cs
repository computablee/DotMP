using System.Collections.Generic;
using System.Threading;
using System;

namespace DotMP
{
    /// <summary>
    /// Encapsulates a Thread object with information about its progress through a parallel for loop.
    /// For keeping track of its progress through a parallel for loop, we keep track of the current next iteration of the loop to be worked on, and the iteration the current thread is currently working on.
    /// </summary>
    internal class Thr
    {
        /// <summary>
        /// The Thread object to be encapsulated.
        /// </summary>
        internal Thread thread;
        /// <summary>
        /// The current iteration of the parallel for loop.
        /// </summary>
        volatile internal int curr_iter;
        /// <summary>
        /// The iteration the thread is currently working on.
        /// </summary>
        internal int working_iter;

        /// <summary>
        /// Creates a Thr object with the specified Thread object.
        /// </summary>
        /// <param name="thread">The Thread object to be encapsulated.</param>
        internal Thr(Thread thread)
        {
            this.thread = thread;
            curr_iter = 0;
            working_iter = 0;
        }
    }

    /// <summary>
    /// Contains all relevant information about a parallel for loop.
    /// Contains a collection of Thr objects, the loop's start and end iterations, the chunk size, the number of threads, and the number of threads that have completed their work.
    /// </summary>
    internal class WorkShare
    {
        /// <summary>
        /// The threads to be used in the parallel for loop.
        /// </summary>
        private static Thr[] threads;
        /// <summary>
        /// Get Thr object based on current thread ID.
        /// </summary>
        internal Thr thread
        {
            get
            {
                return threads[Parallel.GetThreadNum()];
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
            private set
            {
                start_pv = value;
            }
        }
        /// <summary>
        /// A generic lock to be used within the parallel for loop.
        /// </summary>
        private static object ws_lock_pv = new object();
        /// <summary>
        /// Getter and setter for the singleton object WorkShare.ws_lock_pv.
        /// </summary>
        internal object ws_lock
        {
            get
            {
                return ws_lock_pv;
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
        private static Schedule? schedule_pv;
        /// <summary>
        /// Getter and setter for singleton object WorkShare.schedule_pv.
        /// </summary>
        internal Schedule? schedule
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
                if (in_for_pv == null)
                {
                    return false;
                }

                return in_for_pv[Parallel.GetThreadNum()];
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
        internal WorkShare(uint num_threads, Thread[] threads, int start, int end, uint chunk_size, Operations? op, Schedule schedule)
        {
            this.end = end;
            this.num_threads = num_threads;
            this.op = op;
            Parallel.Master(() =>
            {
                WorkShare.threads = new Thr[num_threads];
                for (int i = 0; i < num_threads; i++)
                    WorkShare.threads[i] = new Thr(threads[i]);
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
        /// Advance the start by some value.
        /// </summary>
        /// <param name="advance_by">The value to advance start by.</param>
        internal void Advance(int advance_by)
        {
            start += advance_by;
        }

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
        /// For binary And, the initial starting value is -1.
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
                    local = (T)Convert.ChangeType(-1, typeof(T));
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
    }
}
