namespace DotMP.GPU
{
    internal static class ReductionKernels
    {
        private static Action<KernelConfig, GPUArray arr, Index idx, int len> reduce_action;

        private static bool action_obtained = false;

        private static void Power2RoundDown(int x)
        {
            --x;
            x |= x >> 1;
            x |= x >> 2;
            x |= x >> 4;
            x |= x >> 8;
            x |= x >> 16;
            ++x;
            x >>= 1;
        }

        private static void Reduce(GPUArray arr, Index idx, int len)
        {
            int start_from = Power2RoundDown(len);

            if (idx + start_from < len)
                arr[idx] = arr[idx + start_from];

            for (int i = start_from >> 1; i > 0; i >>= 1)
            {
                arr[idx] += arr[idx + i];
            }
        }

        internal static Action<KernelConfig, GPUArray arr, Index idx, int len> GetReduce()
        {
            if (!action_obtained)
            {
                AcceleratorHandler handler = new AcceleratorHandler();
                reduce_action = handler.accelerator.LoadStreamKernel(Reduce);
                action_obtained = true;
            }

            return reduce_action;
        }
    }
}