using System;
using OpenMP;

namespace Tester
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            Parallel.For(0, 100, i =>
            {
                //Console.WriteLine(i);
            }, Parallel.Schedule.Static, chunk_size: 5, num_threads: 2);
        }
    }
}
