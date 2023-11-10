using ILGPU;
using System;

namespace DotMP.GPU
{
    /// <summary>
    /// Wrapper object for representing arrays on the GPU.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct GPUArray<T>
        where T : unmanaged
    {
        /// <summary>
        /// Internal ArrayView object.
        /// </summary>
        private ArrayView<T> arrayView;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="arrayView">The ArrayView to wrap.</param>
        public GPUArray(ArrayView<T> arrayView)
        {
            this.arrayView = arrayView;
        }

        /// <summary>
        /// Implicit conversion to ArrayView.
        /// </summary>
        /// <param name="array">The GPUArray object.</param>
        public static implicit operator ArrayView<T>(GPUArray<T> array)
        {
            return array.arrayView;
        }

        /// <summary>
        /// Implicit conversion to GPUArray.
        /// </summary>
        /// <param name="array">The ArrayView object.</param>
        public static implicit operator GPUArray<T>(ArrayView<T> array)
        {
            return new GPUArray<T>(array);
        }

        /// <summary>
        /// Overload for [] operator.
        /// </summary>
        /// <param name="idx">The ID to index into.</param>
        /// <returns>The data at that ID.</returns>
        public T this[int idx]
        {
            get => arrayView[idx];
            set => arrayView[idx] = value;
        }

        /// <summary>
        /// Gets the length of the array.
        /// </summary>
        public int Length
        {
            get => arrayView.IntLength;
        }
    }
}