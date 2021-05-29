using System;
using OpenMP;
using System.Diagnostics;

namespace Tester
{
    class Program
    {
        static void Main(string[] args)
        {
            const int WORKLOAD = 100000000;

            double[] a = new double[WORKLOAD];
            double[] b = new double[WORKLOAD];
            double[] c = new double[WORKLOAD];
            Random r = new Random();

            for (int i = 0; i < WORKLOAD; i++)
            {
                a[i] = r.NextDouble();
                b[i] = r.NextDouble();
                c[i] = r.NextDouble();
            }

            Stopwatch s = new Stopwatch();
            s.Start();

            Parallel.For(0, WORKLOAD,
                schedule: Parallel.Schedule.Static,
                num_threads: 8,
                chunk_size: 128,
                action: i =>
            {
                a[i] = a[i] * b[i] + c[i];
            });

            s.Stop();

            Console.WriteLine("For loop took {0} milliseconds.", s.ElapsedMilliseconds);
        }
    }
}
