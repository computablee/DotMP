using ILGPU;
using ILGPU.Runtime;
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
        /// The ILGPU buffer for 1D arrays.
        /// </summary>
        private ArrayView1D<T, Stride1D.Dense> view1d;

        /// <summary>
        /// The ILGPU buffer for 2D arrays.
        /// </summary>
        private ArrayView2D<T, Stride2D.DenseY> view2d;

        /// <summary>
        /// Number of dimensions.
        /// </summary>
        private int dims;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="arrayView">The ArrayView to wrap.</param>
        public GPUArray(Buffer<T> arrayView)
        {
            if (arrayView.Dimensions == 1)
            {
                view1d = arrayView.View1D;
                // BAND-AID FIX: Cannot use empty ArrayViews on OpenCL devices.
                view2d = new Buffer<T>(new T[1, 1], Buffer.Behavior.NoCopy).View2D;
            }
            else if (arrayView.Dimensions == 2)
            {
                // BAND-AID FIX: Cannot use empty ArrayViews on OpenCL devices.
                view1d = new Buffer<T>(new T[1], Buffer.Behavior.NoCopy).View1D;
                view2d = arrayView.View2D;
            }
            else
            {
                // BAND-AID FIX: Cannot use empty ArrayViews on OpenCL devices.
                view1d = new Buffer<T>(new T[1], Buffer.Behavior.NoCopy).View1D;
                // BAND-AID FIX: Cannot use empty ArrayViews on OpenCL devices.
                view2d = new Buffer<T>(new T[1, 1], Buffer.Behavior.NoCopy).View2D;
            }

            dims = arrayView.Dimensions;
        }

        /// <summary>
        /// Overload for [] operator.
        /// </summary>
        /// <param name="idx">The ID to index into.</param>
        /// <returns>The data at that ID.</returns>
        public T this[int idx]
        {
            get => view1d[idx];
            set => view1d[idx] = value;
        }

        /// <summary>
        /// Overload for [,] operator.
        /// </summary>
        /// <param name="i">The first ID to index into.</param>
        /// <param name="j">The second ID to index into.</param>
        /// <returns>The data at that ID.</returns>
        public T this[int i, int j]
        {
            get => view2d[i, j];
            set => view2d[i, j] = value;
        }

        /// <summary>
        /// Gets the length of the array.
        /// </summary>
        public int Length
        {
            get
            {
                switch (dims)
                {
                    case 1:
                    default:
                        return view1d.IntLength;
                    case 2:
                        return view2d.IntLength;
                }
            }
        }
    }
}