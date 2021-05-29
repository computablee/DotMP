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

            Console.WriteLine("Initializing data.");

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

            Console.WriteLine("Starting test.");

            Stopwatch s = new Stopwatch();
            s.Start();

            for (int i = 0; i < 10; i++)
            {
                Parallel.For(0, WORKLOAD,
                    action: i =>
                {
                    if (i == 0)
                        Console.WriteLine("For loop started");

                    a[i] = a[i] * b[i] + c[i];
                });
            }

            s.Stop();

            Console.WriteLine("Test ended.");

            Console.WriteLine("For loop took {0} milliseconds.", s.ElapsedMilliseconds / 10);
        }
    }
}
