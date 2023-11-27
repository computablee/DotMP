/*
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
public class Overhead
{
    DotMP.GPU.Buffer<int> buf;

    // run the setup
    [GlobalSetup]
    public void Setup()
    {
        buf = new DotMP.GPU.Buffer<int>(new int[1, 1], DotMP.GPU.Buffer.Behavior.NoCopy);
    }

    //run the simulation
    [Benchmark]
    public void TestOverhead()
    {
        DotMP.GPU.Parallel.ParallelForCollapse((0, 500), (0, 500), buf, (i, j, buf) => { });
    }
}

/* jscpd:ignore-end */

// driver
public class Program
{
    public static void Main(string[] args)
    {
        BenchmarkRunner.Run<Overhead>();
    }
}
