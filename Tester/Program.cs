using System;
using OpenMP;

namespace Tester
{
    class Program
    {
        static void Main(string[] args)
        {
            Parallel.For(0, 100,
                schedule: Parallel.Schedule.Static,
                num_threads: 8,
                action: i =>
            {
                Console.WriteLine(i);
            });
        }
    }
}
