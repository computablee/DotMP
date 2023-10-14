using System;

namespace DotMP.GPU
{
    /// <summary>
    /// Exception thrown if too many or too few data movements were specified before a GPU kernel.
    /// </summary>
    public class WrongNumberOfDataMovementsSpecifiedException : Exception
    {
        /// <summary>
        /// Constructor with a message.
        /// </summary>
        /// <param name="msg">The message to associate with the exception.</param>
        public WrongNumberOfDataMovementsSpecifiedException(string msg) : base(msg) { }
    }

    /// <summary>
    /// Exception thrown if data movement is presented out-of-order.
    /// </summary>
    public class ImproperDataMovementOrderingException : Exception
    {
        /// <summary>
        /// Constructor with a message.
        /// </summary>
        /// <param name="msg">The message to associate with the exception.</param>
        public ImproperDataMovementOrderingException(string msg) : base(msg) { }
    }
}