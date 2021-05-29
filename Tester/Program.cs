using System;
using OpenMP;
using System.Diagnostics;

namespace Tester
{
    class Program
    {
        static void Main(string[] args)
        {
            const int WORKLOAD = 200000000;
            const int FORITERS = 10;

            Console.WriteLine("Initializing data.");

            float[] a = new float[WORKLOAD];
            float[] b = new float[WORKLOAD];
            float[] c = new float[WORKLOAD];
            Random r = new Random();

            for (int i = 0; i < WORKLOAD; i++)
            {
                a[i] = (float)r.NextDouble();
                b[i] = (float)r.NextDouble();
                c[i] = (float)r.NextDouble();
            }

            Console.WriteLine("Starting test.");

            Stopwatch s = new Stopwatch();
            s.Start();

            for (int i = 0; i < FORITERS; i++)
            {
                Parallel.For(0, WORKLOAD,
                    num_threads: 4,
                    action: j =>
                {
                    //if (j == 0)
                    //    Console.WriteLine("For loop {0} started.", i);

                    a[j] = a[j] * b[j] + c[j];
                });
            }

            s.Stop();

            Console.WriteLine("Test ended.");

            Console.WriteLine("For loop took {0} milliseconds.", s.ElapsedMilliseconds / FORITERS);
        }
    }
}
