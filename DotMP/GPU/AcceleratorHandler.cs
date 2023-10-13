using System;
using System.Linq;
using System.Reflection.Emit;
using Mono.Cecil;
using Mono.Cecil.Cil;
using ILGPU;
using ILGPU.Runtime;
using ILGPU.IR;

namespace DotMP
{
    /// <summary>
    /// The handler class managing GPU acceleration.
    /// </summary>
    internal class AcceleratorHandler
    {
        /// <summary>
        /// Determines if a GPU context has been initialized yet.
        /// </summary>
        private static bool initialized = false;
        /// <summary>
        /// The GPU context.
        /// </summary>
        private static Context context;
        /// <summary>
        /// The accelerator object.
        /// </summary>
        private static Accelerator accelerator;
        /// <summary>
        /// The GPU pointers for arrays going to the GPU.
        /// </summary>
        private static dynamic[] tos;
        /// <summary>
        /// The GPU pointers for arrays coming back from the GPU.
        /// </summary>
        private static dynamic[] froms;
        /// <summary>
        /// The CPU pointers for arrays coming back from the GPU.
        /// </summary>
        private static dynamic[] froms_cpu;
        /// <summary>
        /// The GPU pointers for arrays going both to and from the GPU.
        /// </summary>
        private static dynamic[] tofroms;
        /// <summary>
        /// The CPU pointers for arrays going both to and from the GPU.
        /// </summary>
        private static dynamic[] tofroms_cpu;
        /// <summary>
        /// Counts how many arrays have been copied back to the CPU for bookkeeping.
        /// </summary>
        private static int copied_back;

        /// <summary>
        /// Default constructor. If this is the first time it's called, it initializes all relevant singleton data.
        /// </summary>
        internal AcceleratorHandler()
        {
            if (initialized) return;

            context = Context.CreateDefault();
            accelerator = context.Devices[0].CreateAccelerator(context);
            initialized = true;
            copied_back = 0;

            tos = new dynamic[0];
            froms = new dynamic[0];
            tofroms = new dynamic[0];
            froms_cpu = new dynamic[0][];
            tofroms_cpu = new dynamic[0][];
        }

        /// <summary>
        /// Aggregates the parameters into a single array.
        /// </summary>
        /// <returns>A dynamic array of all parameters.</returns>
        private dynamic[] AggregateParams(int count)
        {
            dynamic[] ret = tos.Concat(froms).Concat(tofroms).ToArray();

            if (ret.Length != count)
                throw new WrongNumberOfDataMovementsSpecifiedException(string.Format("Specified {0} data movement(s), expected {1}.", ret.Length, count));

            return ret;
        }

        /// <summary>
        /// Called if data should be moved to the device.
        /// Allocates data on GPU and copies the data from the CPU.
        /// </summary>
        /// <typeparam name="T">The type of data to allocate.</typeparam>
        /// <param name="values">The data to allocate.</param>
        internal void AllocateTo<T>(T[][] values)
            where T : unmanaged
        {
            if (froms.Length > 0 || tofroms.Length > 0)
                throw new ImproperDataMovementOrderingException("DataTo should be called before DataFrom and DataToFrom.");

            var tos = values.Select(v => accelerator.Allocate1D(v)).ToArray();
            AcceleratorHandler.tos = AcceleratorHandler.tos.Concat(tos).ToArray();

            for (int i = 0; i < tos.Length; i++)
                tos[i].CopyFromCPU(values[i]);
        }

        /// <summary>
        /// Called if data should be moved from the device.
        /// Allocates data on GPU.
        /// </summary>
        /// <typeparam name="T">The type of data to allocate.</typeparam>
        /// <param name="values">The data to allocate.</param>
        internal void AllocateFrom<T>(T[][] values)
            where T : unmanaged
        {
            if (tofroms.Length > 0)
                throw new ImproperDataMovementOrderingException("DataFrom should be called before DataToFrom.");

            var froms = values.Select(v => accelerator.Allocate1D<T>(v.Length)).ToArray();
            AcceleratorHandler.froms = AcceleratorHandler.froms.Concat(froms).ToArray();
            AcceleratorHandler.froms_cpu = AcceleratorHandler.froms_cpu.Concat(values).ToArray();
        }

        /// <summary>
        /// Called if data should be moved to and from the device.
        /// Allocates data on GPU and copies the data from the CPU.
        /// </summary>
        /// <typeparam name="T">The type of data to allocate.</typeparam>
        /// <param name="values">The data to allocate.</param>
        internal void AllocateToFrom<T>(T[][] values)
            where T : unmanaged
        {
            var tofroms = values.Select(v => accelerator.Allocate1D(v)).ToArray();
            AcceleratorHandler.tofroms = AcceleratorHandler.tofroms.Concat(tofroms).ToArray();
            AcceleratorHandler.tofroms_cpu = AcceleratorHandler.tofroms_cpu.Concat(values).ToArray();

            for (int i = 0; i < tos.Length; i++)
                tofroms[i].CopyFromCPU(values[i]);
        }

        /// <summary>
        /// Synchronizes the GPU stream.
        /// </summary>
        internal void Synchronize() =>
            accelerator.DefaultStream.Synchronize();

        /// <summary>
        /// Copies a piece of GPU memory back to the CPU.
        /// </summary>
        /// <typeparam name="T">The type of the data to transfer.</typeparam>
        /// <param name="item">A MemoryBuffer1D object to transfer.</param>
        internal void CopyBack<T>(dynamic item)
            where T : unmanaged
        {
            MemoryBuffer1D<T, Stride1D.Dense> castedItem = item;

            if (copied_back < froms.Length)
                castedItem.GetAsArray1D().CopyTo(froms_cpu[copied_back], 0);
            else
                castedItem.GetAsArray1D().CopyTo(tofroms_cpu[copied_back - froms.Length], 0);

            copied_back++;
        }

        /// <summary>
        /// Called to finalize kernel execution.
        /// Clears all of the arrays used in the kernel.
        /// </summary>
        internal void FinalizeKernel()
        {
            foreach (var i in tos)
                i.Dispose();
            tos = new dynamic[0];

            foreach (var i in froms)
                i.Dispose();
            froms = new dynamic[0];
            froms_cpu = new dynamic[0][];

            foreach (var i in tofroms)
                i.Dispose();
            tofroms = new dynamic[0];
            tofroms_cpu = new dynamic[0][];

            copied_back = 0;
        }

        /// <summary>
        /// Dispatches a kernel with one data parameter.
        /// </summary>
        /// <typeparam name="T">The type of the data parameter.</typeparam>
        /// <param name="start">The start of the loop, inclusive.</param>
        /// <param name="end">The end of the loop, exclusive.</param>
        /// <param name="action">The action to perform.</param>
        internal void DispatchKernel<T>(int start, int end, ActionGPU<T> action)
            where T : unmanaged
        {
            Action<Index1D, ArrayView<T>> disp_action = (index, i) =>
            {
                int idx = index.X + start;
                action(idx, i);
            };

            dynamic[] parameters = AggregateParams(1);
            var kernel = accelerator.LoadAutoGroupedStreamKernel(disp_action);

            kernel(end - start, parameters[0]);

            Synchronize();
            CopyBack<T>(parameters[0]);
            FinalizeKernel();
        }

        /// <summary>
        /// Dispatches a kernel with two data parameters.
        /// </summary>
        /// <typeparam name="T">The type of the first data parameter.</typeparam>
        /// <typeparam name="U">The type of the second data parameter.</typeparam>
        /// <param name="start">The start of the loop, inclusive.</param>
        /// <param name="end">The end of the loop, exclusive.</param>
        /// <param name="action">The action to perform.</param>
        internal void DispatchKernel<T, U>(int start, int end, ActionGPU<T, U> action)
            where T : unmanaged
            where U : unmanaged
        {
            Action<Index1D, ArrayView<T>, ArrayView<U>> disp_action = (index, i, j) =>
            {
                int idx = index.X + start;
                action(idx, i, j);
            };

            dynamic[] parameters = AggregateParams(2);
            var kernel = accelerator.LoadAutoGroupedStreamKernel(disp_action);

            kernel(end - start, parameters[0], parameters[1]);

            Synchronize();
            CopyBack<T>(parameters[0]);
            CopyBack<U>(parameters[1]);
            FinalizeKernel();
        }

        /// <summary>
        /// Dispatches a kernel with three data parameters.
        /// </summary>
        /// <typeparam name="T">The type of the first data parameter.</typeparam>
        /// <typeparam name="U">The type of the second data parameter.</typeparam>
        /// <typeparam name="V">The type of the third data parameter.</typeparam>
        /// <param name="start">The start of the loop, inclusive.</param>
        /// <param name="end">The end of the loop, exclusive.</param>
        /// <param name="action">The action to perform.</param>
        internal void DispatchKernel<T, U, V>(int start, int end, ActionGPU<T, U, V> action)
            where T : unmanaged
            where U : unmanaged
            where V : unmanaged
        {
            Action<Index1D, ArrayView<T>, ArrayView<U>, ArrayView<V>> disp_action = (index, i, j, k) =>
            {
                int idx = index.X + start;
                action(idx, i, j, k);
            };

            dynamic[] parameters = AggregateParams(3);
            var kernel = accelerator.LoadAutoGroupedStreamKernel(disp_action);

            kernel(end - start, parameters[0], parameters[1], parameters[2]);

            Synchronize();
            CopyBack<T>(parameters[0]);
            CopyBack<U>(parameters[1]);
            CopyBack<V>(parameters[2]);
            FinalizeKernel();
        }

        /// <summary>
        /// The type of the first data parameter.
        /// </summary>
        /// <typeparam name="T">The type of the first data parameter.</typeparam>
        /// <typeparam name="U">The type of the second data parameter.</typeparam>
        /// <typeparam name="V">The type of the third data parameter.</typeparam>
        /// <typeparam name="W">The type of the fourth data parameter.</typeparam>
        /// <param name="start">The start of the loop, inclusive.</param>
        /// <param name="end">The end of the loop, exclusive.</param>
        /// <param name="action">The action to perform.</param>
        internal void DispatchKernel<T, U, V, W>(int start, int end, ActionGPU<T, U, V, W> action)
            where T : unmanaged
            where U : unmanaged
            where V : unmanaged
            where W : unmanaged
        {
            dynamic[] parameters = AggregateParams(4);

            var method = GenerateKernel4(action);
            var kernel = accelerator.LoadAutoGroupedStreamKernel(method);

            kernel(end - start, parameters[0], parameters[1], parameters[2], parameters[3], start);

            Synchronize();
            CopyBack<T>(parameters[0]);
            CopyBack<U>(parameters[1]);
            CopyBack<V>(parameters[2]);
            CopyBack<W>(parameters[3]);
            FinalizeKernel();
        }

        internal Action<Index1D, ArrayView<T>, ArrayView<U>, ArrayView<V>, ArrayView<W>, int> GenerateKernel4<T, U, V, W>(ActionGPU<T, U, V, W> action)
            where T : unmanaged
            where U : unmanaged
            where V : unmanaged
            where W : unmanaged
        {
            DynamicMethod dynamicMethod = new DynamicMethod("GPUKernel4", typeof(void), new Type[] { typeof(Index1D), typeof(ArrayView<T>), typeof(ArrayView<U>), typeof(ArrayView<V>), typeof(ArrayView<W>) });
            ILGenerator ilGenerator = dynamicMethod.GetILGenerator();

            var assembly = AssemblyDefinition.ReadAssembly(action.Method.DeclaringType.Assembly.Location);
            var maintypes = assembly.MainModule.Types.First(t => t.FullName == action.Method.DeclaringType.FullName.Split("+")[0]);
            var type = maintypes.NestedTypes.First(t => t.FullName.Split("/")[1] == action.Method.DeclaringType.FullName.Split("+")[1]);
            var method = type.Methods.First(nt => nt.FullName.Contains(action.Method.Name));

            var instructions = method.Body.Instructions;

            var intType = assembly.MainModule.ImportReference(typeof(int));
            var idxVar = new VariableDefinition(intType);

            method.Body.Variables.Add(idxVar);

            var il = method.Body.GetILProcessor();

            for (int i = 0; i < instructions.Count; i++)
            {
                if (instructions[i].OpCode == Mono.Cecil.Cil.OpCodes.Ldarg_1)
                {
                    instructions[i] = il.Create(Mono.Cecil.Cil.OpCodes.Ldloc, idxVar);
                }
            }

            var firstInstruction = method.Body.Instructions[0];

            var structType = assembly.MainModule.Types.First(t => t.FullName == typeof(Index1D).Name);
            var propertyGetter = structType.Properties.First(p => p.Name == "X").GetMethod;

            il.InsertBefore(firstInstruction, il.Create(Mono.Cecil.Cil.OpCodes.Ldarg_1));
            il.InsertBefore(firstInstruction, il.Create(Mono.Cecil.Cil.OpCodes.Call, propertyGetter));
            il.InsertBefore(firstInstruction, il.Create(Mono.Cecil.Cil.OpCodes.Stloc, idxVar));

            var parameter = method.Parameters[1];
            parameter.ParameterType = assembly.MainModule.ImportReference(typeof(Index1D));

            var newAction = (Action<Index1D, ArrayView<T>, ArrayView<U>, ArrayView<V>, ArrayView<W>, int>)dynamicMethod.CreateDelegate(typeof(Action<Index1D, ArrayView<T>, ArrayView<U>, ArrayView<V>, ArrayView<W>, int>));

            return newAction;
        }
    }
}