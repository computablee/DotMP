using System;
using OpenMP;

static class HeatTransfer
{

    //do a step of the heat transfer simulation
    private static void DoStep(double[,] grid, double[,] scratch, int dim)
    {
        //iterate over all cells not on the border
        OpenMP.Parallel.For(1, dim - 1, schedule: OpenMP.Parallel.Schedule.Dynamic, action: i =>
        {
            for (int j = 1; j < dim - 1; j++)
            {
                //set the scratch array to the average of the surrounding cells
                scratch[i, j] = 0.25 * (grid[i - 1, j] + grid[i + 1, j] + grid[i, j - 1] + grid[i, j + 1]);
            }
        });

        //copy the scratch array to the grid array
        OpenMP.Parallel.For(1, dim - 1, schedule: OpenMP.Parallel.Schedule.Static, action: i =>
        {
            for (int j = 1; j < dim - 1; j++)
            {
                grid[i, j] = scratch[i, j];
            }
        });
    }

    //run the simulation
    public static void DoSimulation(double[,] grid, int steps, int dim)
    {
        //create a scratch array and set it to the grid array
        double[,] scratch = new double[dim, dim];
        Array.Copy(grid, scratch, dim * dim);

        //do the steps
        OpenMP.Parallel.ParallelRegion(num_threads: 4, action: () =>
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
        if (args.Length < 2)
        {
            Console.WriteLine("Usage: {0} <dim> <steps> [<output grid>]\n", System.Diagnostics.Process.GetCurrentProcess().ProcessName);
            return;
        }

        //get the dimension, steps, and whether to output the grid
        int dim = Convert.ToInt32(args[0]);
        int steps = Convert.ToInt32(args[1]);
        bool output = (args.Length > 2) ? Convert.ToBoolean(args[2]) : false;

        //allocate the grid
        double[,] grid = new double[dim, dim];

        //initialize the grid
        grid[0, dim / 2 - 1] = 100.0;
        grid[0, dim / 2] = 100.0;

        //do the simulation
        HeatTransfer.DoSimulation(grid, steps, dim);

        //output the grid if requested
        if (output) HeatTransfer.PrintGrid(grid, dim);

        //free the grid and exit program
        return;
    }
}