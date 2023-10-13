using ILGPU;

namespace DotMP
{
    /// <summary>
    /// The device handle struct passed around on the GPU.
    /// </summary>
    public struct DeviceHandle
    {
        /// <summary>
        /// Index1D struct.
        /// </summary>
        Index1D index;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="index">The Index1D object to store.</param>
        internal DeviceHandle(Index1D index)
        {
            this.index = index;
        }

        /// <summary>
        /// Gets the index.
        /// </summary>
        /// <returns>The Index1D object.</returns>
        internal Index1D GetIndex()
        {
            return index;
        }
    }
}