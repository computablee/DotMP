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
using System;
using ILGPU;
using ILGPU.Runtime;

/* jscpd:ignore-start */

[SimpleJob(RuntimeMoniker.Net60)]
[ThreadingDiagnoser]
[HardwareCounters]
[EventPipeProfiler(EventPipeProfile.CpuSampling)]
public class Overhead
{
    Action<KernelConfig, ArrayView1D<int, Stride1D.Dense>> kernel;
    ArrayView1D<int, Stride1D.Dense> data;

    // run the setup
    [GlobalSetup]
    public void Setup()
    {
	var context = Context.CreateDefault();
	var accelerator = context.Devices[1].CreateAccelerator(context);
	kernel = accelerator.LoadStreamKernel<ArrayView1D<int, Stride1D.Dense>>(arr => { });
	data = accelerator.Allocate1D<int>(1); 
    }

    //run the simulation
    [Benchmark]
    public void TestOverhead()
    {
	kernel((1, 256), data);
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
