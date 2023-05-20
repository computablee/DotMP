using System;
using System.Collections.Generic;

namespace OpenMP
{
    public class Shared<T>
    {
        private static Dictionary<string, dynamic> shared = new Dictionary<string, object>();

        private string name;

        public Shared(string name, T value)
        {
            OpenMP.Parallel.Master(() =>
            {
                shared[name] = value;
            });
            this.name = name;

            OpenMP.Parallel.Barrier();
        }

        public static void Clear(string name)
        {
            shared.Remove(name);
        }

        public void Set(T value)
        {
            shared[name] = value;
        }

        public T Get()
        {
            return shared[name];
        }
    }
}