using System;
using DotMP;

static class HeatTransfer
{

    //do a step of the heat transfer simulation
    private static void DoStep(double[,] grid, double[,] scratch, int dim)
    {
        DotMP.Parallel.Master(() =>
        {
            //iterate over all cells not on the border
            var dep = DotMP.Parallel.Taskloop(1, dim - 1, action: i =>
            {
                for (int j = 1; j < dim - 1; j++)
                {
                    //set the scratch array to the average of the surrounding cells
                    scratch[i, j] = 0.25 * (grid[i - 1, j] + grid[i + 1, j] + grid[i, j - 1] + grid[i, j + 1]);
                }
            });

            //copy the scratch array to the grid array
            DotMP.Parallel.Taskloop(1, dim - 1, depends: dep, action: i =>
            {
                for (int j = 1; j < dim - 1; j++)
                {
                    grid[i, j] = scratch[i, j];
                }
            });
        });

        DotMP.Parallel.Taskwait();
    }

    //run the simulation
    public static void DoSimulation(double[,] grid, int steps, int dim)
    {
        //create a scratch array and set it to the grid array
        double[,] scratch = new double[dim, dim];
        Array.Copy(grid, scratch, dim * dim);

        //do the steps
        DotMP.Parallel.ParallelRegion(num_threads: 6, action: () =>
        {
            for (int i = 0; i < steps; i++)
            {
                DoStep(grid, scratch, dim);
            }
        });
    }

    //print the grid
    public static void PrintGrid(double[,] grid, int dim)
    {
        for (int i = 0; i < dim; i++)
        {
            for (int j = 0; j < dim; j++)
            {
                Console.Write("{0} ", grid[i, j].ToString("0.00"));
            }
            Console.WriteLine();
        }
    }
}

class Driver
{
    //main function
    public static void Main(string[] args)
    {
        //check the number of arguments
        if (args.Length < 3)
        {
            Console.WriteLine("Usage: {0} <dim> <steps> <iters> [<output grid>]\n", System.Diagnostics.Process.GetCurrentProcess().ProcessName);
            return;
        }

        //get the dimension, steps, and whether to output the grid
        int dim = Convert.ToInt32(args[0]);
        int steps = Convert.ToInt32(args[1]);
        int iters = Convert.ToInt32(args[2]);
        bool output = (args.Length > 3) ? Convert.ToBoolean(args[3]) : false;

        double min = double.MaxValue;
        double max = double.MinValue;
        double avg = 0.0f;
        double[,] grid = new double[0, 0];

        //warmup
        for (int i = 0; i < 5; i++)
        {
            //allocate the grid
            grid = new double[dim, dim];

            //initialize the grid
            grid[0, dim / 2 - 1] = 100.0;
            grid[0, dim / 2] = 100.0;

            //do the simulation
            HeatTransfer.DoSimulation(grid, steps, dim);
        }

        //do the simulation multiple times for timing
        for (int i = 0; i < iters; i++)
        {
            //allocate the grid
            grid = new double[dim, dim];

            //initialize the grid
            grid[0, dim / 2 - 1] = 100.0;
            grid[0, dim / 2] = 100.0;

            //do the simulation
            double tick = DotMP.Parallel.GetWTime();
            HeatTransfer.DoSimulation(grid, steps, dim);
            double tock = DotMP.Parallel.GetWTime();

            //update min, max, avg
            tock = tock - tick;
            avg += tock;
            min = Math.Min(min, tock);
            max = Math.Max(max, tock);
        }

        //output the grid if requested
        if (output) HeatTransfer.PrintGrid(grid, dim);

        avg /= iters;

        //print the results
        Console.WriteLine("Min: {0}", min);
        Console.WriteLine("Max: {0}", max);
        Console.WriteLine("Avg: {0}", avg);

        //free the grid and exit program
        return;
    }
}