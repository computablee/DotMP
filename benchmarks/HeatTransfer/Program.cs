using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Diagnosers;

[SimpleJob(RuntimeMoniker.Net60)]
[SimpleJob(RuntimeMoniker.Net70)]
[ThreadingDiagnoser]
[HardwareCounters]
// test heat transfer using taskloops
public class HeatTransferTaskloop
{
    // scratch array
    private double[,] scratch = new double[0, 0];
    // grid array
    private double[,] grid = new double[0, 0];

    // test dims of 100x100, 1000x1000, and 5000x5000
    [Params(100, 1000, 5000)]
    public int dim;

    // test with 10 steps and 100 steps
    [Params(10, 100)]
    public int steps;

    // change this to configure the number of threads to use
    public uint num_threads = 6;

    // run the setup
    [GlobalSetup]
    public void Setup()
    {
        scratch = new double[dim, dim];
        grid = new double[dim, dim];

        grid[0, dim / 2 - 1] = 100.0;
        grid[0, dim / 2] = 100.0;
    }

    //run the simulation
    [Benchmark]
    public void DoSimulation()
    {
        // spawn a parallel region
        DotMP.Parallel.ParallelRegion(num_threads: num_threads, action: () =>
        {
            //do the steps
            for (int i = 0; i < steps; i++)
            {
                DoStep();
            }
        });
    }

    //do a step of the heat transfer simulation
    public void DoStep()
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
}

[SimpleJob(RuntimeMoniker.Net60)]
[SimpleJob(RuntimeMoniker.Net70)]
[ThreadingDiagnoser]
[HardwareCounters]
// test heat transfer using Parallel.For
public class HeatTransferFor
{
    // scratch array
    private double[,] scratch = new double[0, 0];
    // grid array
    private double[,] grid = new double[0, 0];

    // test dims of 100x100, 1000x1000, and 5000x5000
    [Params(100, 1000, 5000)]
    public int dim;

    // test with 10 steps and 100 steps
    [Params(10, 100)]
    public int steps;

    // change this to configure the number of threads to use
    public uint num_threads = 6;

    // run the setup
    [GlobalSetup]
    public void Setup()
    {
        scratch = new double[dim, dim];
        grid = new double[dim, dim];

        grid[0, dim / 2 - 1] = 100.0;
        grid[0, dim / 2] = 100.0;
    }

    //run the simulation
    [Benchmark]
    public void DoSimulation()
    {
        // spawn a parallel region
        DotMP.Parallel.ParallelRegion(num_threads: num_threads, action: () =>
        {
            //do the steps
            for (int i = 0; i < steps; i++)
            {
                DoStep();
            }
        });
    }

    //do a step of the heat transfer simulation
    public void DoStep()
    {
        //iterate over all cells not on the border
        DotMP.Parallel.For(1, dim - 1, action: i =>
        {
            for (int j = 1; j < dim - 1; j++)
            {
                //set the scratch array to the average of the surrounding cells
                scratch[i, j] = 0.25 * (grid[i - 1, j] + grid[i + 1, j] + grid[i, j - 1] + grid[i, j + 1]);
            }
        });

        //copy the scratch array to the grid array
        DotMP.Parallel.For(1, dim - 1, action: i =>
        {
            for (int j = 1; j < dim - 1; j++)
            {
                grid[i, j] = scratch[i, j];
            }
        });
    }
}

[SimpleJob(RuntimeMoniker.Net60)]
[SimpleJob(RuntimeMoniker.Net70)]
[ThreadingDiagnoser]
[HardwareCounters]
// test heat transfer using Parallel.ForCollapse
public class HeatTransferForCollapse
{
    // scratch array
    private double[,] scratch = new double[0, 0];
    // grid array
    private double[,] grid = new double[0, 0];

    // test dims of 100x100, 1000x1000, and 5000x5000
    [Params(100, 1000, 5000)]
    public int dim;

    // test with 10 steps and 100 steps
    [Params(10, 100)]
    public int steps;

    // change this to configure the number of threads to use
    public uint num_threads = 6;

    // run the setup
    [GlobalSetup]
    public void Setup()
    {
        scratch = new double[dim, dim];
        grid = new double[dim, dim];

        grid[0, dim / 2 - 1] = 100.0;
        grid[0, dim / 2] = 100.0;
    }

    //run the simulation
    [Benchmark]
    public void DoSimulation()
    {
        // spawn a parallel region
        DotMP.Parallel.ParallelRegion(num_threads: num_threads, action: () =>
        {
            //do the steps
            for (int i = 0; i < steps; i++)
            {
                DoStep();
            }
        });
    }

    //do a step of the heat transfer simulation
    public void DoStep()
    {
        //iterate over all cells not on the border
        DotMP.Parallel.ForCollapse((1, dim - 1), (1, dim - 1), action: (i, j) =>
        {
            //set the scratch array to the average of the surrounding cells
            scratch[i, j] = 0.25 * (grid[i - 1, j] + grid[i + 1, j] + grid[i, j - 1] + grid[i, j + 1]);
        });

        //copy the scratch array to the grid array
        DotMP.Parallel.ForCollapse((1, dim - 1), (1, dim - 1), action: (i, j) =>
        {
            grid[i, j] = scratch[i, j];
        });
    }
}

// driver
public class Program
{
    public static void Main(string[] args)
    {
        // check if a benchmark is specified
        if (args.Length < 1)
        {
            throw new ArgumentException("Usage: dotnet run [taskloop|for|forcollapse] -c Release");
        }

        // run the specified benchmark
        var summary = (args[0] == "taskloop") ? BenchmarkRunner.Run<HeatTransferTaskloop>()
                    : (args[0] == "for") ? BenchmarkRunner.Run<HeatTransferFor>()
                    : (args[0] == "forcollapse") ? BenchmarkRunner.Run<HeatTransferForCollapse>()
                    : throw new ArgumentException("Usage: dotnet run [taskloop|for|forcollapse] -c Release");
    }
}