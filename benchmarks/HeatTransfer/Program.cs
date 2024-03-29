﻿/*
 * DotMP - A collection of powerful abstractions for parallel programming in .NET with an OpenMP-like API. 
 * Copyright (C) 2023 Phillip Allen Lane
 *
 * This library is free software; you can redistribute it and/or modify it under the terms of the GNU Lesser
 * General Public License as published by the Free Software Foundation; either version 2.1 of the License, or
 * (at your option) any later version.
 *
 * This library is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the
 * implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public
 * License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License along with this library; if not,
 * write to the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.
 */

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Diagnosers;

/* jscpd:ignore-start */

[SimpleJob(RuntimeMoniker.Net60)]
[ThreadingDiagnoser]
[HardwareCounters]
[EventPipeProfiler(EventPipeProfile.CpuSampling)]
// test heat transfer using Parallel.For
public class HeatTransfer
{
    // scratch array
    private double[,] scratch = new double[0, 0];
    // grid array
    private double[,] grid = new double[0, 0];

    // parallel type enum
    public enum ParType { TPL, DMPFor, DMPForCollapse, DMPForCollapseStatic, Serial, Taskloop }

    // test dims of 100x100, 1000x1000, and 5000x5000
    [Params(514)]
    public int dim;

    // test with 10 steps and 100 steps
    [Params(100)]
    public int steps;

    // test with all 3 parallel types
    [Params(ParType.TPL, ParType.DMPFor, ParType.DMPForCollapse, ParType.Serial, ParType.Taskloop)]
    public ParType type;

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
        Action action = () =>
        {
            //do the steps
            for (int i = 0; i < steps; i++)
            {
                DoStep();
            }
        };

        if (type == ParType.TPL || type == ParType.Serial)
        {
            action();
        }
        else
        {
            // spawn a parallel region
            DotMP.Parallel.ParallelRegion(num_threads: num_threads, action: action);
        }
    }

    //do a step of the heat transfer simulation
    public void DoStep()
    {
        switch (type)
        {
            case ParType.TPL:
                //iterate over all cells not on the border
                System.Threading.Tasks.Parallel.For(1, dim - 1, i =>
                {
                    for (int j = 1; j < dim - 1; j++)
                    {
                        //set the scratch array to the average of the surrounding cells
                        scratch[i, j] = 0.25 * (grid[i - 1, j] + grid[i + 1, j] + grid[i, j - 1] + grid[i, j + 1]);
                    }
                });

                //copy the scratch array to the grid array
                System.Threading.Tasks.Parallel.For(1, dim - 1, i =>
                {
                    for (int j = 1; j < dim - 1; j++)
                    {
                        grid[i, j] = scratch[i, j];
                    }
                });
                break;

            case ParType.DMPFor:
                //iterate over all cells not on the border
                DotMP.Parallel.For(1, dim - 1, schedule: DotMP.Schedule.Guided, action: i =>
                {
                    for (int j = 1; j < dim - 1; j++)
                    {
                        //set the scratch array to the average of the surrounding cells
                        scratch[i, j] = 0.25 * (grid[i - 1, j] + grid[i + 1, j] + grid[i, j - 1] + grid[i, j + 1]);
                    }
                });

                //copy the scratch array to the grid array
                DotMP.Parallel.For(1, dim - 1, schedule: DotMP.Schedule.Guided, action: i =>
                {
                    for (int j = 1; j < dim - 1; j++)
                    {
                        grid[i, j] = scratch[i, j];
                    }
                });
                break;

            case ParType.DMPForCollapse:
                //iterate over all cells not on the border
                DotMP.Parallel.ForCollapse((1, dim - 1), (1, dim - 1), schedule: DotMP.Schedule.WorkStealing, action: (i, j) =>
                {
                    //set the scratch array to the average of the surrounding cells
                    scratch[i, j] = 0.25 * (grid[i - 1, j] + grid[i + 1, j] + grid[i, j - 1] + grid[i, j + 1]);
                });

                //copy the scratch array to the grid array
                DotMP.Parallel.ForCollapse((1, dim - 1), (1, dim - 1), schedule: DotMP.Schedule.WorkStealing, action: (i, j) =>
                {
                    grid[i, j] = scratch[i, j];
                });
                break;

            case ParType.DMPForCollapseStatic:
                //iterate over all cells not on the border
                DotMP.Parallel.ForCollapse((1, dim - 1), (1, dim - 1), schedule: DotMP.Schedule.Static, chunk_size: 1, action: (i, j) =>
                {
                    //set the scratch array to the average of the surrounding cells
                    scratch[i, j] = 0.25 * (grid[i - 1, j] + grid[i + 1, j] + grid[i, j - 1] + grid[i, j + 1]);
                });

                //copy the scratch array to the grid array
                DotMP.Parallel.ForCollapse((1, dim - 1), (1, dim - 1), schedule: DotMP.Schedule.Static, chunk_size: 1, action: (i, j) =>
                {
                    grid[i, j] = scratch[i, j];
                });
                break;

            case ParType.Serial:
                for (int i = 1; i < dim - 1; i++)
                {
                    for (int j = 1; j < dim - 1; j++)
                    {
                        //set the scratch array to the average of the surrounding cells
                        scratch[i, j] = 0.25 * (grid[i - 1, j] + grid[i + 1, j] + grid[i, j - 1] + grid[i, j + 1]);
                    }
                }

                //copy the scratch array to the grid array
                for (int i = 1; i < dim - 1; i++)
                {
                    for (int j = 1; j < dim - 1; j++)
                    {
                        grid[i, j] = scratch[i, j];
                    }
                }
                break;

            case ParType.Taskloop:
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
                break;
        }
    }
}

// test heat transfer using Parallel.For
public class HeatTransferVerify
{
    // scratch array
    private double[,] scratch = new double[0, 0];
    // grid array
    private double[,] grid = new double[0, 0];

    // parallel type enum
    public enum ParType { TPL, For, ForCollapse, Serial }

    // test dims of 100x100, 1000x1000, and 5000x5000
    public int dim = 500;

    // test with 10 steps and 100 steps
    public int steps = 100;

    // test with all 3 parallel types
    public ParType type = ParType.For;

    // change this to configure the number of threads to use
    public uint num_threads = 6;

    // run the setup
    public void Setup()
    {
        scratch = new double[dim, dim];
        grid = new double[dim, dim];

        grid[0, dim / 2 - 1] = 100.0;
        grid[0, dim / 2] = 100.0;
    }

    //run the simulation
    public void DoSimulation()
    {
        Action action = () =>
        {
            //do the steps
            for (int i = 0; i < steps; i++)
            {
                DoStep();
            }
        };

        if (type == ParType.TPL || type == ParType.Serial)
        {
            action();
        }
        else
        {
            // spawn a parallel region
            DotMP.Parallel.ParallelRegion(num_threads: num_threads, action: action);
        }
    }

    //do a step of the heat transfer simulation
    public void DoStep()
    {
        switch (type)
        {
            case ParType.TPL:
                //iterate over all cells not on the border
                System.Threading.Tasks.Parallel.For(1, dim - 1, i =>
                {
                    System.Threading.Tasks.Parallel.For(1, dim - 1, j =>
                    {
                        //set the scratch array to the average of the surrounding cells
                        scratch[i, j] = 0.25 * (grid[i - 1, j] + grid[i + 1, j] + grid[i, j - 1] + grid[i, j + 1]);
                    });
                });

                //copy the scratch array to the grid array
                System.Threading.Tasks.Parallel.For(1, dim - 1, i =>
                {
                    System.Threading.Tasks.Parallel.For(1, dim - 1, j =>
                    {
                        grid[i, j] = scratch[i, j];
                    });
                });
                break;

            case ParType.For:
                //iterate over all cells not on the border
                DotMP.Parallel.For(1, dim - 1, schedule: DotMP.Schedule.Guided, action: i =>
                {
                    for (int j = 1; j < dim - 1; j++)
                    {
                        //set the scratch array to the average of the surrounding cells
                        scratch[i, j] = 0.25 * (grid[i - 1, j] + grid[i + 1, j] + grid[i, j - 1] + grid[i, j + 1]);
                    }
                });

                //copy the scratch array to the grid array
                DotMP.Parallel.For(1, dim - 1, schedule: DotMP.Schedule.Guided, action: i =>
                {
                    for (int j = 1; j < dim - 1; j++)
                    {
                        grid[i, j] = scratch[i, j];
                    }
                });
                break;

            case ParType.ForCollapse:
                //iterate over all cells not on the border
                DotMP.Parallel.ForCollapse((1, dim - 1), (1, dim - 1), schedule: DotMP.Schedule.Guided, action: (i, j) =>
                {
                    //set the scratch array to the average of the surrounding cells
                    scratch[i, j] = 0.25 * (grid[i - 1, j] + grid[i + 1, j] + grid[i, j - 1] + grid[i, j + 1]);
                });

                //copy the scratch array to the grid array
                DotMP.Parallel.ForCollapse((1, dim - 1), (1, dim - 1), schedule: DotMP.Schedule.Guided, action: (i, j) =>
                {
                    grid[i, j] = scratch[i, j];
                });
                break;

            case ParType.Serial:
                for (int i = 1; i < dim - 1; i++)
                {
                    for (int j = 1; j < dim - 1; j++)
                    {
                        //set the scratch array to the average of the surrounding cells
                        scratch[i, j] = 0.25 * (grid[i - 1, j] + grid[i + 1, j] + grid[i, j - 1] + grid[i, j + 1]);
                    }
                }

                //copy the scratch array to the grid array
                for (int i = 1; i < dim - 1; i++)
                {
                    for (int j = 1; j < dim - 1; j++)
                    {
                        grid[i, j] = scratch[i, j];
                    }
                }
                break;
        }
    }

    public void Verify()
    {
        Setup();
        type = ParType.For;
        DoSimulation();
        double[,] gridA = grid;

        Setup();
        type = ParType.Serial;
        DoSimulation();
        double[,] gridB = grid;

        bool wrong = false;

        for (int i = 0; i < dim; i++)
            for (int j = 0; j < dim; j++)
                if (gridA[i, j] != gridB[i, j])
                    wrong = true;

        if (wrong)
            Console.WriteLine("WRONG RESULT");
        else
            Console.WriteLine("RIGHT RESULT");
    }
}

/* jscpd:ignore-end */

// driver
public class Program
{
    public static void Main(string[] args)
    {
        if (args.Length > 0 && args[0] == "verify")
            new HeatTransferVerify().Verify();
        else
            BenchmarkRunner.Run<HeatTransfer>();
    }
}
