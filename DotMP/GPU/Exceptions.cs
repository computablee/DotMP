using System;

namespace DotMP
{
    /// <summary>
    /// Exception thrown if too few data movements were specified before a GPU kernel.
    /// </summary>
    public class TooFewDataMovementsSpecifiedException : Exception
    {
        /// <summary>
        /// Constructor with a message.
        /// </summary>
        /// <param name="msg">The message to associate with the exception.</param>
        public TooFewDataMovementsSpecifiedException(string msg) : base(msg) { }
    }

    /// <summary>
    /// Exception thrown if too many data movements were specified before a GPU kernel.
    /// </summary>
    public class TooManyDataMovementsSpecifiedException : Exception
    {
        /// <summary>
        /// Constructor with a message.
        /// </summary>
        /// <param name="msg">The message to associate with the exception.</param>
        public TooManyDataMovementsSpecifiedException(string msg) : base(msg) { }
    }
}