using System;
using OpenMP;
using System.Diagnostics;

namespace Tester
{
    class Program
    {
        static void Main(string[] args)
        {
            const int WORKLOAD = 1_000_000;
            const int FORITERS = 20;

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
                    a[j] = (a[j] * b[j] + c[j]) / c[j];
                    while (a[j] > 1000 || a[j] < -1000)
                        a[j] /= 10;
                    int temp = Convert.ToInt32(a[j]);
                    for (int i = 0; i < temp; i++)
                        a[j] += i;
                });
            }

            s.Stop();

            Console.WriteLine("Test ended.");

            Console.WriteLine("For loop took {0} milliseconds.", s.ElapsedMilliseconds / FORITERS);
        }
    }
}
