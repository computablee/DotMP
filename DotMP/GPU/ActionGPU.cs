using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace DotMP
{
    /// <summary>
    /// Specifies a kernel with one data parameter.
    /// </summary>
    /// <typeparam name="T">The base type of the first argument. Must be an unmanaged type.</typeparam>
    /// <param name="i">The index from the for loop.</param>
    /// <param name="o1">The first argument. Must be an array.</param>
    public delegate void ActionGPU<T>(int i, GPUArray<T> o1)
        where T : unmanaged;
    /// <summary>
    /// Specifies a kernel with one data parameter.
    /// </summary>
    /// <typeparam name="T">The base type of the first argument. Must be an unmanaged type.</typeparam>
    /// <typeparam name="U">The base type of the second argument. Must be an unmanaged type.</typeparam>
    /// <param name="i">The index from the for loop.</param>
    /// <param name="o1">The first argument. Must be an array.</param>
    /// <param name="o2">The second argument. Must be an array.</param>
    public delegate void ActionGPU<T, U>(int i, GPUArray<T> o1, GPUArray<U> o2)
        where T : unmanaged
        where U : unmanaged;
    /// <summary>
    /// Specifies a kernel with one data parameter.
    /// </summary>
    /// <typeparam name="T">The base type of the first argument. Must be an unmanaged type.</typeparam>
    /// <typeparam name="U">The base type of the second argument. Must be an unmanaged type.</typeparam>
    /// <typeparam name="V">The base type of the third argument. Must be an unmanaged type.</typeparam>
    /// <param name="i">The index from the for loop.</param>
    /// <param name="o1">The first argument. Must be an array.</param>
    /// <param name="o2">The second argument. Must be an array.</param>
    /// <param name="o3">The third argument. Must be an array.</param>
    public delegate void ActionGPU<T, U, V>(int i, GPUArray<T> o1, GPUArray<U> o2, GPUArray<V> o3)
        where T : unmanaged
        where U : unmanaged
        where V : unmanaged;
    /// <summary>
    /// Specifies a kernel with one data parameter.
    /// </summary>
    /// <typeparam name="T">The base type of the first argument. Must be an unmanaged type.</typeparam>
    /// <typeparam name="U">The base type of the second argument. Must be an unmanaged type.</typeparam>
    /// <typeparam name="V">The base type of the third argument. Must be an unmanaged type.</typeparam>
    /// <typeparam name="W">The base type of the fourth argument. Must be an unmanaged type.</typeparam>
    /// <param name="i">The index from the for loop.</param>
    /// <param name="o1">The first argument. Must be an array.</param>
    /// <param name="o2">The second argument. Must be an array.</param>
    /// <param name="o3">The third argument. Must be an array.</param>
    /// <param name="o4">The fourth argument. Must be an array.</param>
    public delegate void ActionGPU<T, U, V, W>(int i, GPUArray<T> o1, GPUArray<U> o2, GPUArray<V> o3, GPUArray<W> o4)
        where T : unmanaged
        where U : unmanaged
        where V : unmanaged
        where W : unmanaged;
}