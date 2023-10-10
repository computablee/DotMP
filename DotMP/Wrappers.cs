using System;

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
    /// Action delegate that takes two ints and a ref T as parameters.
    /// </summary>
    /// <typeparam name="T">Type of the ref parameter.</typeparam>
    /// <param name="a">The ref parameter.</param>
    /// <param name="i">The first int parameter.</param>
    /// <param name="j">The second int parameter.</param>
    public delegate void ActionRef2<T>(ref T a, int i, int j);

    /// <summary>
    /// Action delegate that takes three ints and a ref T as parameters.
    /// </summary>
    /// <typeparam name="T">Type of the ref parameter.</typeparam>
    /// <param name="a">The ref parameter.</param>
    /// <param name="i">The first int parameter.</param>
    /// <param name="j">The second int parameter.</param>
    /// <param name="k">The third int parameter.</param>
    public delegate void ActionRef3<T>(ref T a, int i, int j, int k);

    /// <summary>
    /// Action delegate that takes four ints and a ref T as parameters.
    /// </summary>
    /// <typeparam name="T">Type of the ref parameter.</typeparam>
    /// <param name="a">The ref parameter.</param>
    /// <param name="i">The first int parameter.</param>
    /// <param name="j">The second int parameter.</param>
    /// <param name="k">The third int parameter.</param>
    /// <param name="l">The fourth int parameter.</param>
    public delegate void ActionRef4<T>(ref T a, int i, int j, int k, int l);

    /// <summary>
    /// Action delegate that takes an int[] and a ref T as parameters.
    /// </summary>
    /// <typeparam name="T">Type of the ref parameter.</typeparam>
    /// <param name="a">The ref parameter.</param>
    /// <param name="i">The int[] parameter.</param>
    public delegate void ActionRefN<T>(ref T a, int[] i);

    /// <summary>
    /// Class encapsulating all of the possible callbacks in a Parallel.For-style loop.
    /// This includes Parallel.For, Parallel.ForReduction, Parallel.ForCollapse, and Parallel.ForReductionCollapse.
    /// </summary>
    /// <typeparam name="T">The type of the reduction callback.</typeparam>
    internal class ForAction<T>
    {
        /// <summary>
        /// Enum describing which actions can be selected.
        /// </summary>
        private enum ActionSelector
        {
            /// <summary>
            /// A regular for loop with 1 variable.
            /// </summary>
            Regular,
            /// <summary>
            /// A reduction loop with 1 variable.
            /// </summary>
            Reduction,
            /// <summary>
            /// A collapsed loop with 2 variables.
            /// </summary>
            Collapse2,
            /// <summary>
            /// A collapsed loop with 3 variables.
            /// </summary>
            Collapse3,
            /// <summary>
            /// A collapsed loop with 4 variables.
            /// </summary>
            Collapse4,
            /// <summary>
            /// A collapsed loop with unbounded variables.
            /// </summary>
            CollapseN,
            /// <summary>
            /// A reduction and collapsed loop with 2 variables.
            /// </summary>
            ReductionCollapse2,
            /// <summary>
            /// A reduction and collapsed loop with 3 variables.
            /// </summary>
            ReductionCollapse3,
            /// <summary>
            /// A reduction and collapsed loop with 4 variables.
            /// </summary>
            ReductionCollapse4,
            /// <summary>
            /// A reduction and collapsed loop with unbounded variables.
            /// </summary>
            ReductionCollapseN,
        }

        /// <summary>
        /// Represents an action that can be selected via ActionSelector.Regular
        /// </summary>
        private Action<int> omp_fn;

        /// <summary>
        /// Represents an action that can be selected via ActionSelector.Reduction
        /// </summary>
        private ActionRef<T> omp_red;

        /// <summary>
        /// Represents an action that can be selected via ActionSelector.Collapse2
        /// </summary>
        private Action<int, int> omp_col_2;

        /// <summary>
        /// Represents an action that can be selected via ActionSelector.Collapse3
        /// </summary>
        private Action<int, int, int> omp_col_3;

        /// <summary>
        /// Represents an action that can be selected via ActionSelector.Collapse4
        /// </summary>
        private Action<int, int, int, int> omp_col_4;

        /// <summary>
        /// Represents an action that can be selected via ActionSelector.CollapseN
        /// </summary>
        private Action<int[]> omp_col_n;

        /// <summary>
        /// Represents an action that can be selected via ActionSelector.ReductionCollapse2
        /// </summary>
        private ActionRef2<T> omp_red_col_2;

        /// <summary>
        /// Represents an action that can be selected via ActionSelector.ReductionCollapse3
        /// </summary>
        private ActionRef3<T> omp_red_col_3;

        /// <summary>
        /// Represents an action that can be selected via ActionSelector.ReductionCollapse4
        /// </summary>
        private ActionRef4<T> omp_red_col_4;

        /// <summary>
        /// Represents an action that can be selected via ActionSelector.ReductionCollapseN
        /// </summary>
        private ActionRefN<T> omp_red_col_n;

        /// <summary>
        /// Holds the data regarding which action to select.
        /// </summary>
        private ActionSelector selector;

        private (int, int)[] ranges;

        /// <summary>
        /// Constructor for regular for loops with 1 variable.
        /// </summary>
        /// <param name="action">The action to run.</param>
        internal ForAction(Action<int> action)
        {
            omp_fn = action;
            selector = ActionSelector.Regular;
        }

        /// <summary>
        /// Constructor for reduction for loops with 1 variable.
        /// </summary>
        /// <param name="action">The action to run.</param>
        internal ForAction(ActionRef<T> action)
        {
            omp_red = action;
            selector = ActionSelector.Reduction;
        }

        /// <summary>
        /// Constructor for collapsed for loops with 2 variables.
        /// </summary>
        /// <param name="action">The action to run.</param>
        /// <param name="ranges">The ranges of the collapsed loop.</param>
        internal ForAction(Action<int, int> action, (int, int)[] ranges)
        {
            omp_col_2 = action;
            selector = ActionSelector.Collapse2;
            this.ranges = ranges;
        }

        /// <summary>
        /// Constructor for collapsed for loops with 3 variables.
        /// </summary>
        /// <param name="action">The action to run.</param>
        /// <param name="ranges">The ranges of the collapsed loop.</param>
        internal ForAction(Action<int, int, int> action, (int, int)[] ranges)
        {
            omp_col_3 = action;
            selector = ActionSelector.Collapse3;
            this.ranges = ranges;
        }

        /// <summary>
        /// Constructor for collapsed for loops with 4 variables.
        /// </summary>
        /// <param name="action">The action to run.</param>
        /// <param name="ranges">The ranges of the collapsed loop.</param>
        internal ForAction(Action<int, int, int, int> action, (int, int)[] ranges)
        {
            omp_col_4 = action;
            selector = ActionSelector.Collapse4;
            this.ranges = ranges;
        }

        /// <summary>
        /// Constructor for collapsed for loops with unbounded variables.
        /// </summary>
        /// <param name="action">The action to run.</param>
        /// <param name="ranges">The ranges of the collapsed loop.</param>
        internal ForAction(Action<int[]> action, (int, int)[] ranges)
        {
            omp_col_n = action;
            selector = ActionSelector.CollapseN;
            this.ranges = ranges;
        }

        /// <summary>
        /// Constructor for reduction collapsed for loops with 2 variables.
        /// </summary>
        /// <param name="action">The action to run.</param>
        /// <param name="ranges">The ranges of the collapsed loop.</param>
        internal ForAction(ActionRef2<T> action, (int, int)[] ranges)
        {
            omp_red_col_2 = action;
            selector = ActionSelector.ReductionCollapse2;
            this.ranges = ranges;
        }

        /// <summary>
        /// Constructor for reduction collapsed for loops with 3 variables.
        /// </summary>
        /// <param name="action">The action to run.</param>
        /// <param name="ranges">The ranges of the collapsed loop.</param>
        internal ForAction(ActionRef3<T> action, (int, int)[] ranges)
        {
            omp_red_col_3 = action;
            selector = ActionSelector.ReductionCollapse3;
            this.ranges = ranges;
        }

        /// <summary>
        /// Constructor for reduction collapsed for loops with 4 variables.
        /// </summary>
        /// <param name="action">The action to run.</param>
        /// <param name="ranges">The ranges of the collapsed loop.</param>
        internal ForAction(ActionRef4<T> action, (int, int)[] ranges)
        {
            omp_red_col_4 = action;
            selector = ActionSelector.ReductionCollapse4;
            this.ranges = ranges;
        }

        /// <summary>
        /// Constructor for reduction collapsed for loops with unbounded variables.
        /// </summary>
        /// <param name="action">The action to run.</param>
        /// <param name="ranges">The ranges of the collapsed loop.</param>
        internal ForAction(ActionRefN<T> action, (int, int)[] ranges)
        {
            omp_red_col_n = action;
            selector = ActionSelector.ReductionCollapseN;
            this.ranges = ranges;
        }

        /// <summary>
        /// Executes a chunk using the action selected by ForAction.selector
        /// </summary>
        /// <param name="curr_iter">A reference to the current iteration.</param>
        /// <param name="start">The start of the chunk, inclusive.</param>
        /// <param name="end">The end of the chunk, exclusive.</param>
        /// <param name="local">The local variable to reduce to.</param>
        /// <exception cref="NotImplementedException">Thrown if the current action is not implemented.</exception>
        internal void PerformLoop(ref int curr_iter, int start, int end, ref T local)
        {
            switch (selector)
            {
                case ActionSelector.Regular:
                    for (curr_iter = start; curr_iter < end; curr_iter++)
                    {
                        omp_fn(curr_iter);
                    }
                    break;
                case ActionSelector.Reduction:
                    for (curr_iter = start; curr_iter < end; curr_iter++)
                    {
                        omp_red(ref local, curr_iter);
                    }
                    break;
                default:
                    throw new NotImplementedException("This callback is not implemented with the scheduler.");
            }
        }
    }
}