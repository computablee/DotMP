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