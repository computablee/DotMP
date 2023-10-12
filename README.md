# DotMP

![Nuget](https://img.shields.io/nuget/v/DotMP.svg?style=flat-square)
![Build](https://github.com/computablee/DotMP/actions/workflows/compile.yml/badge.svg)
![Tests](https://github.com/computablee/DotMP/actions/workflows/integration.yml/badge.svg)
[![Quality](https://github.com/computablee/DotMP/actions/workflows/lint.yml/badge.svg)](https://github.com/marketplace/actions/super-linter)
[![Codecov](https://codecov.io/gh/computablee/DotMP/graph/badge.svg?token=MHAKXKRV1K)](https://codecov.io/gh/computablee/DotMP)
[![All Contributors](https://img.shields.io/github/all-contributors/computablee/DotMP?color=ee8449&style=flat-square)](#contributors)

![DotMP logo](https://raw.githubusercontent.com/computablee/DotMP/main/dotmp_logo.png)

A library for writing OpenMP-style parallel code in .NET.
Inspired by the fork-join paradigm of OpenMP, and attempts to replicate the OpenMP programming style as faithfully as possible, though breaking spec at times.

[Link to repository](https://github.com/computablee/DotMP/tree/main).

## Installing DotMP via NuGet
The easiest way to install DotMP is from the NuGet package manager:
```sh
dotnet add package DotMP
```

## Building DotMP from Source
First, clone DotMP and navigate to the source directory:
```sh
git clone https://github.com/computablee/DotMP.git
cd DotMP
```

DotMP can be built using the `make` command.
To build the entire project, including all tests, examples, and documentation, run the following command:
```sh
make
```
This command will build the main library, all tests, all examples, and the documentation into their respective directories, but will not run any tests.

To build only the main library, run the following command:
```sh
make build
```

To build only the tests, run the following command:
```sh
make tests
```
To run the tests, run the following command:
```sh
make test
```

To build only the examples, run the following command:
```sh
make examples
```
This will build all of the examples, including the native C# parallelized, the DotMP parallelized, and the sequential examples.
You can also individually build each of these classes of examples by running one or all of the following commands:
```sh
make examples-cs
make examples-dmp
make examples-seq
```

## Documentation
You can use [Doxygen](https://github.com/doxygen/doxygen) to build the documentation for this project.
A Doxyfile is located in the root of the project directory.
To build the documentation, run the following command:
```sh
make docs
```
This will generate documentation in the root of the project under the `docs` directory in both LaTeX and HTML formats.

## Contributors

This repository uses [all-contributors](https://github.com/all-contributors/all-contributors) to thank all of the hard-working contributors to this project.

Below is a list of all contributors to DotMP!

<!-- ALL-CONTRIBUTORS-LIST:START - Do not remove or modify this section -->
<!-- prettier-ignore-start -->
<!-- markdownlint-disable -->
<table>
  <tbody>
    <tr>
      <td align="center" valign="top" width="14.28%"><a href="https://github.com/MaurizioPz"><img src="https://avatars.githubusercontent.com/u/455216?v=4?s=100" width="100px;" alt="Maurizio"/><br /><sub><b>Maurizio</b></sub></a><br /><a href="https://github.com/computablee/DotMP/commits?author=MaurizioPz" title="Code">💻</a></td>
      <td align="center" valign="top" width="14.28%"><a href="https://janheres.eu"><img src="https://avatars.githubusercontent.com/u/74781187?v=4?s=100" width="100px;" alt="Jan Hereš"/><br /><sub><b>Jan Hereš</b></sub></a><br /><a href="https://github.com/computablee/DotMP/commits?author=HarryHeres" title="Code">💻</a></td>
      <td align="center" valign="top" width="14.28%"><a href="https://github.com/computablee"><img src="https://avatars.githubusercontent.com/u/20172521?v=4?s=100" width="100px;" alt="Phillip Allen Lane"/><br /><sub><b>Phillip Allen Lane</b></sub></a><br /><a href="https://github.com/computablee/DotMP/commits?author=computablee" title="Code">💻</a></td>
      <td align="center" valign="top" width="14.28%"><a href="https://github.com/exrol"><img src="https://avatars.githubusercontent.com/u/86170495?v=4?s=100" width="100px;" alt="exrol"/><br /><sub><b>exrol</b></sub></a><br /><a href="https://github.com/computablee/DotMP/commits?author=exrol" title="Code">💻</a></td>
      <td align="center" valign="top" width="14.28%"><a href="https://github.com/jestes15"><img src="https://avatars.githubusercontent.com/u/51448244?v=4?s=100" width="100px;" alt="Joshua Estes"/><br /><sub><b>Joshua Estes</b></sub></a><br /><a href="https://github.com/computablee/DotMP/commits?author=jestes15" title="Code">💻</a></td>
      <td align="center" valign="top" width="14.28%"><a href="https://www.bayoosoft.com/"><img src="https://avatars.githubusercontent.com/u/45914736?v=4?s=100" width="100px;" alt="blouflashdb"/><br /><sub><b>blouflashdb</b></sub></a><br /><a href="https://github.com/computablee/DotMP/commits?author=blouflashdb" title="Code">💻</a></td>
      <td align="center" valign="top" width="14.28%"><a href="https://github.com/Skenvy"><img src="https://avatars.githubusercontent.com/u/17214791?v=4?s=100" width="100px;" alt="Nathan Levett"/><br /><sub><b>Nathan Levett</b></sub></a><br /><a href="https://github.com/computablee/DotMP/commits?author=Skenvy" title="Code">💻</a></td>
    </tr>
  </tbody>
</table>

<!-- markdownlint-restore -->
<!-- prettier-ignore-end -->

<!-- ALL-CONTRIBUTORS-LIST:END -->

## Supported Constructs

### Parallel
Given the OpenMP:
```c
#pragma omp parallel
{
    work();
}
```
DotMP provides:
```cs
DotMP.Parallel.ParallelRegion(() => {
    work();
});
```
This function supports the `num_threads` optional parameter, which sets the number of threads to spawn.
The default value is the number of logical threads on the system.

### For
Given the OpenMP:
```c
#pragma omp for
for (int i = a, i < b; i++)
{
    work(i);
}
```
DotMP provides:
```cs
DotMP.Parallel.For(a, b, i => {
    work(i);
});
```
This function supports the `schedule` optional parameter, which sets the parallel scheduler to use.
Permissible values are `DotMP.Schedule.Static`, `DotMP.Schedule.Dynamic`, `DotMP.Schedule.Guided`, and `DotMP.Schedule.Runtime`.
The default value is `DotMP.Schedule.Static`.

This function supports the `chunk_size` optional parameter, which sets the chunk size for the scheduler to use.
The default value is dependent on the scheduler and is not documented, as it may change from version to version.

The behavior of `DotMP.Parallel.For` is undefined if not used within a `ParallelRegion`.

### Parallel For
Given the OpenMP:
```c
#pragma omp parallel for
for (int i = a, i < b; i++)
{
    work(i);
}
```
DotMP provides:
```cs
DotMP.Parallel.ParallelFor(a, b, i => {
    work(i);
});
```
This function supports all of the optional parameters of `ParallelRegion` and `For`, and is merely a wrapper around those two functions for conciseness.

### For with Reduction
Given the OpenMP:
```c
type local = c;

#pragma omp for reduction(op:local)
for (int i = a; i < b; i++)
{
    local `op` f(i);
}
```
DotMP provides:
```cs
type local = c;

DotMP.Parallel.ForReduction(a, b, op, ref local, (ref type local, int i) => {
    local `op` f(i);
});
```
`op` is a value provided by the `DotMP.Operations` enum, which supports the values `Add`, `Subtract`, `Multiply`, `BinaryAnd`, `BinaryOr`, `BinaryXor`, `BooleanAnd`, `BooleanOr`, `Min`, and `Max`.
The operation on `local` is an operator corresponding to the operator specified by `DotMP.Operations`, including `+`, `-`, `*`, `&`, `|`, `^`, and so on.

This function supports all of the optional parameters of `For`.

### Parallel For with Reduction
Given the OpenMP:
```c
type local = c;

#pragma omp parallel for reduction(op:local)
for (int i = a; i < b; i++)
{
    local `op` f(i);
}
```
DotMP provides:
```cs
type local = c;

DotMP.Parallel.ParallelForReduction(a, b, op, ref local, (ref type local, int i) => {
    local `op` f(i);
});
```
This function supports all of the optional parameters of `ParallelRegion` and `ForReduction`, and is merely a wrapper around those two functions for conciseness.

### For with Collapse
Given the OpenMP:
```c
#pragma omp for collapse(n)
for (int i = a, i < b; i++)
    for (int j = c, j < d; j++)
        // ...
            for (int k = e; k < f; k++)
                work(i, j, /* ... */, k);
```
DotMP provides:
```cs
DotMP.Parallel.ForCollapse((a, b), (c, d), /* ... */, (e, f), (i, j, /* ... */, k) => {
    work(i, j, /* ... */, k);
});
```
If four or fewer loops are being collapsed, overloads of `ForCollapse` exist to easily collapse said loops.
If greater than four loops are being collapsed, then the user should pass an array of tuples as the first argument, and accept an array of indices in the lambda.

This function supports all of the optional parameters of `For`.

### For with Reduction and Collapse
Given the OpenMP:
```c
type local = c;

#pragma omp for reduction(op:local) collapse(n)
for (int i = a, i < b; i++)
    for (int j = c, j < d; j++)
        // ...
            for (int k = e; k < f; k++)
                local `op` f(i, j, /* ... */, k);
```
DotMP provides:
```cs
type local = c;

DotMP.Parallel.ForReductionCollapse((a, b), (c, d), /* ... */, (e, f), op, ref local, (ref type local, int i, int j, /* ... */, int k) => {
    local `op` f(i, j, /* ... */, k);
});
```
This function is a combination of `ForCollapse` and `ForReduction`, and supports all of the optional parameters thereof.

### Parallel For with Collapse
Given the OpenMP:
```c
#pragma omp parallel for collapse(n)
for (int i = a, i < b; i++)
    for (int j = c, j < d; j++)
        // ...
            for (int k = e; k < f; k++)
                work(i, j, /* ... */, k);
```
DotMP provides:
```cs
DotMP.Parallel.ParallelForCollapse((a, b), (c, d), /* ... */, (e, f), (i, j, /* ... */, k) => {
    work(i, j, /* ... */, k);
});
```
This function supports all of the optional parameters of `ParallelRegion` and `ForCollapse`, and is merely a wrapper around those two functions for conciseness.

### Parallel For with Reduction and Collapse
Given the OpenMP:
```c
type local = c;

#pragma omp parallel for reduction(op:local) collapse(n)
for (int i = a, i < b; i++)
    for (int j = c, j < d; j++)
        // ...
            for (int k = e; k < f; k++)
                local `op` f(i, j, /* ... */, k);
```
DotMP provides:
```cs
type local = c;

DotMP.Parallel.ParallelForReductionCollapse((a, b), (c, d), /* ... */, (e, f), op, ref local, (ref type local, int i, int j, /* ... */, int k) => {
    local `op` f(i, j, /* ... */, k);
});
```
This function supports all of the optional parameters of `ParallelRegion` and `ForReductionCollapse`, and is merely a wrapper around those two functions for conciseness.

### Sections
Given the OpenMP:
```c
#pragma omp sections
{
    #pragma omp section
    {
        work();
    }
    #pragma omp section
    {
        work2();
    }
}
```
DotMP provides:
```cs
DotMP.Parallel.Sections(() => {
    work();
}, () => {
    work2();
});
```

### Parallel Sections
Given the OpenMP:
```c
#pragma omp parallel sections
{
    #pragma omp section
    {
        work();
    }
    #pragma omp section
    {
        work2();
    }
}
```
DotMP provides:
```cs
DotMP.Parallel.ParallelSections(() => {
    work();
}, () => {
    work2();
});
```
This function supports the optional parameter `num_threads` from `DotMP.Parallel.ParallelRegion`.

### Critical
Given the OpenMP:
```c
#pragma omp critical
{
    work();
}
```
DotMP provides:
```cs
DotMP.Parallel.Critical(id, () => {
    work();
});
```
This function requires an `id` parameter, which is used as a unique identifier for a particular critical region.
If multiple critical regions are present in the code, they should each have a unique `id`.
The `id` should likely be a `const int` or an integer literal.

### Barrier
Given the OpenMP:
```c
#pragma omp barrier
```
DotMP provides:
```cs
DotMP.Parallel.Barrier();
```

### Master
Given the OpenMP:
```c
#pragma omp master
{
    work();
}
```
DotMP provides:
```cs
DotMP.Parallel.Master(() => {
    work();
});
```
`Master`'s behavior is left undefined if used outside of a `ParallelRegion`.

### Single
Given the OpenMP:
```c
#pragma omp single
{
    work();
}
```
DotMP provides:
```cs
DotMP.Parallel.Single(id, () => {
    work();
});
```
The `id` parameter provided should follow the same guidelines as specified in `Critical`.

A `Single` region is only executed once per `DotMP.Parallel.ParallelRegion`, and is executed by the first thread that encounters it.

`Single`'s behavior is left undefined if used outside of a `ParallelRegion`.

### Ordered
Given the OpenMP:
```c
#pragma omp ordered
{
    work();
}
```
DotMP provides:
```cs
DotMP.Parallel.Ordered(id, () => {
    work();
});
```
The `id` parameter provided should follow the same guidelines as specified in `Critical`.

`Ordered`'s behavior is left undefined if used outside of a `For`.

## Atomics

OpenMP atomics are implemented as follows:
```c
#pragma omp atomic
a op b;
```
where `op` is some supported operator.

DotMP supports a subset of this for the `int`, `uint`, `long`, and `ulong` types.
The only implemented atomic operations are `a += b`, `a &= b`, `a |= b`, `++a`, and `--a`.
`a -= b` is implemented, but for signed types only, due to restrictions interfacting with C#'s `Interlocked` class.

The following table documents the supported atomics:

| Operation | DotMP function           |
------------|--------------------------------
| `a += b`  | `DotMP.Atomic.Add(ref a, b)` |
| `a -= b`  | `DotMP.Atomic.Sub(ref a, b)` |
| `a &= b`  | `DotMP.Atomic.And(ref a, b)` |
| `a \|= b` | `DotMP.Atomic.Or(ref a, b)`  |
| `++a`     | `DotMP.Atomic.Inc(ref a)`    |
| `--a`     | `DotMP.Atomic.Dec(ref a)`    |

For atomic operations like compare-exchange, we recommend interfacting directly with `System.Threading.Interlocked`.
For non-supported atomic operations or types, we recommend using `DotMP.Parallel.Critical`.
This is more of a limitation of the underlying hardware than anything.

## Locks
DotMP supports OpenMP-style locks.
It is recommended to use C#'s native `lock` keyword where possible for performance.
However, this API is provided to those who want the familiarity of OpenMP locks.

DotMP supports the `DotMP.Lock` object, which is the replacement for `omp_lock_t`.
`omp_init_lock` and `omp_destroy_lock` are not implemented.
Instead, users should instantiate the `DotMP.Lock` object using the `new` keyword.

DotMP provides the following functions:

| <omp.h> function     | DotMP function | Comments
-----------------------|----------------|---------
| omp_set_lock(lock)   | lock.Set()     | Halt the current thread until the lock is obtained
| omp_unset_lock(lock) | lock.Unset()   | Free the current lock, making it available for other threads
| omp_test_lock(lock)  | lock.Test()    | Attempt to obtain a lock without blocking, returns true if locking is successful

## Shared Memory

DotMP supports an API for declaring thread-shared memory within a parallel region.
Shared memory is provided through the `DotMP.Shared<T>` class, which implements the `IDisposable` interface.
`DotMP.Shared<T>` objects support implicit casting to type `T`, and can be used as such.
They also allow an explicit `DotMP.Shared<T>.Get()` method to be used to retrieve the value of the shared variable.
For setting, the `DotMP.Shared<T>.Set(T value)` method must be used.

For indexable types, such as arrays, the `DotMP.SharedEnumerable<T>` class is provided.
This class implements the `IDisposable` interface, and supports implicit casting to the containing type.
This class also overloads the `[]` operator to allow for indexing.

The following provides an example of a parallel vector initialization using `DotMP.SharedEnumerable<T>`:
```cs
static double[] InitVector()
{
    double[] returnVector;
    
    DotMP.Parallel.ParallelRegion(() =>
    {
        using (var vec = DotMP.SharedEnumerable.Create("vec", new double[1024]))
        {
            DotMP.Parallel.For(0, 1024, i =>
            {
                vec[i] = 1.0;
            });

            returnVector = vec;
        }
    });
    
    return returnVector;
}
```

The `DotMP.Shared` and `DotMP.SharedEnumerable` classes supports the following methods:

| Method                                       | Action
-----------------------------------------------|-------
| DotMP.Shared.Shared(string name, T value)    | Initializes a shared variable with name `name` and starting value `value`
| DotMP.Shared.Dispose()                       | Disposes of a shared variable
| DotMP.Shared.Set(T value)                    | Sets a shared variable to value `value`
| DotMP.Shared.Get()                           | Gets a shared variable
| DotMP.SharedEnumerable.SharedEnumerable(string name, U value) | Initializes a shared array with name `name` and starting value `value`
| DotMP.SharedEnumerable.Dispose()             | Disposes of a shared array
| DotMP.SharedEnumerable.Get()                 | Gets a shared enumerable as its containing type.

The `DotMP.Shared` constructor and `Clear()` methods serve as implicit barriers, ensuring that all threads can access the memory before proceeding.

`DotMP.Shared` provides a factory method for creating `DotMP.Shared` instances via the `DotMP.Shared.Create()` method.
`DotMP.SharedEnumerable` provides factory methods for creating `DotMP.SharedEnumerable` instances containing either `T[]` or `List<T>` enumerables via the `DotMP.SharedEnumerable.Create()` methods.

## Tasking System

DotMP supports a rudimentary tasking system.
Submitting a task adds the task to a global task queue.
When a **tasking point** is hit, threads will begin working on tasks in the task queue.
There are two tasking points currently in DotMP:

- At the end of a `DotMP.Parallel.ParallelRegion`, all remaining tasks in the task queue are completed
- Upon encountering `DotMP.Parallel.Taskwait`, all current tasks in the task queue are completed

Tasks can be submitted throughout the execution of a parallel region, including from within other tasks, and support dependencies.
Spawning tasks returns a `DotMP.TaskUUID` object which can be passed as a parameter to future tasks, marking those tasks as dependent on the originating task.

The following analogues to OpenMP functions are provided:

### Task
Given the OpenMP:
```c
#pragma omp task
{
    work();
}
```
DotMP provides:
```cs
DotMP.Parallel.Task(() => {
    work();
});
```
This function supports `depends` as a `params` parameter.
`depends` accepts `DotMP.TaskUUID` objects, and marks the created task as dependent on the tasks passed through `depends`.

This function adds a task to the task queue and is deferred until a tasking point.

This function returns a `DotMP.TaskUUID` object, which can be passed to future `depends` clauses.

### Taskwait
Given the OpenMP:
```c
#pragma omp taskwait
```
DotMP provides:
```cs
DotMP.Parallel.Taskwait();
```
This function acts as a tasking point, as well as an implicit barrier.

### Taskloop
Given the OpenMP:
```c
#pragma omp taskloop
for (int i = a, i < b; i++)
{
    work(i);
}
```
DotMP provides:
```cs
DotMP.Parallel.Taskloop(a, b, i => {
    work(i);
});
```
This function supports the `num_tasks` optional parameter, which specifies how many tasks into which the loop is broken up.

This function supports the `grainsize` optional parameter, which specifies how many iterations belong to each individual task.

If both `num_tasks` and `grainsize` are provided, the `num_tasks` parameter takes precedence over the `grainsize` parameter.

This function supports the `only_if` optional parameter.
`only_if` is an opportunity to provide a boolean expression to determine if the taskloop should generate tasks or execute sequentially.
This is beneficial if the taskloop might be very small and wouldn't be worth the (albeit light) overhead of creating tasks and waiting on a tasking point.

This function supports `depends` as a `params` parameter.
`depends` accepts `DotMP.TaskUUID` objects, and marks the created tasks as dependent on the tasks passed through `depends`.

This function adds a series of tasks to the task queue and is deferred until a tasking point.

This function returns a `DotMP.TaskUUID[]` array, where each element is a `DotMP.TaskUUID` representing one of the generated tasks.
The `DotMP.TaskUUID[]` array can be passed to future `depends` clauses.

### Parallel Master
Given the OpenMP:
```c
#pragma omp parallel master
{
    work();
}
```
DotMP provides:
```cs
DotMP.Parallel.ParallelMaster(() => {
    work();
});
```
This function supports the `num_threads` optional parameter from `DotMP.Parallel.ParallelRegion`.

### Master Taskloop
Given the OpenMP:
```c
#pragma omp master taskloop
for (int i = a, i < b; i++)
{
    work(i);
}
```
DotMP provides:
```cs
DotMP.Parallel.MasterTaskloop(a, b, i => {
    work(i);
});
```
This function supports all of the optional parameters from `DotMP.Parallel.Taskloop`, except `depends`.

This function does not return a `DotMP.TaskUUID[]` array.

### Parallel Master Taskloop
Given the OpenMP:
```c
#pragma omp parallel master taskloop
for (int i = a, i < b; i++)
{
    work(i);
}
```
DotMP provides:
```cs
DotMP.Parallel.ParallelMasterTaskloop(a, b, i => {
    work(i);
});
```
This function supports all of the optional parameters from `DotMP.Parallel.ParallelRegion` and `DotMP.Parallel.Taskloop`, except `depends`.

This function does not return a `DotMP.TaskUUID[]` array.

## Supported Functions

DotMP provides an analogue of the following functions:

| <omp.h> function         | DotMP function                | Comments
---------------------------|------------------------------------|---------
| omp_get_num_procs()      | DotMP.Parallel.GetNumProcs()      | Returns the number of logical threads on the system
| omp_get_num_threads()    | DotMP.Parallel.GetNumThreads()    | Returns the number of active threads in the current region
| omp_set_num_threads()    | DotMP.Parallel.SetNumThreads()    | Sets the number of threads for the next parallel region to use
| omp_get_thread_num()     | DotMP.Parallel.GetThreadNum()     | Gets the ID of the current thread
| omp_get_max_threads()    | DotMP.Parallel.GetMaxThreads()    | Gets the maximum number of threads the runtime may use in the next region
| omp_in_parallel()        | DotMP.Parallel.InParallel()       | Returns true if called from within a parallel region
| omp_set_dynamic()        | DotMP.Parallel.SetDynamic()       | Tells the runtime to dynamically adjust the number of threads, can disable by calling SetNumThreads
| omp_get_dynamic()        | DotMP.Parallel.GetDynamic()       | Returns true if the runtime can dynamically adjust the number of threads
| omp_set_nested()         | DotMP.Parallel.SetNested()        | Returns a NotImplementedException
| omp_get_nested()         | DotMP.Parallel.GetNested()        | Returns false
| omp_get_wtime()          | DotMP.Parallel.GetWTime()         | Returns the number of seconds since the Unix Epoch as a double
| omp_get_schedule()       | DotMP.Parallel.GetSchedule()      | Gets the current schedule of the parallel for loop
| omp_get_schedule()       | DotMP.Parallel.GetChunkSize()     | Gets the current chunk size of the parallel for loop
