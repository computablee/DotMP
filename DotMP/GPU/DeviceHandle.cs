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
        internal Index1D Index { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="index">The Index1D object to store.</param>
        internal DeviceHandle(Index1D index)
        {
            Index = index;
        }
    }
}