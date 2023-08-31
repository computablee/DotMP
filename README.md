# OpenMP.NET
A library for writing OpenMP-style parallel code in .NET.
Inspired by the fork-join paradigm of OpenMP, and attempts to replicate the OpenMP programming style as faithfully as possible, though breaking spec at times.

## Building OpenMP.NET
OpenMP.NET can be built using the `make` command.
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
This will build all of the examples, including the native C# parallelized, the OpenMP.NET parallelized, and the sequential examples.
You can also individually build each of these classes of examples by running one or all of the following commands:
```sh
make examples-cs
make examples-omp
make examples-seq
```

## Documentation
You can use [Doxygen](https://github.com/doxygen/doxygen) to build the documentation for this project.
A Doxyfile is located in the root of the project directory.
To build the documentation, run the following command:
```sh
make docs
```
This will generate documentation in the root of the project under the `OpenMP.NET.Docs` directory in both LaTeX and HTML formats.

## Supported Constructs

### Parallel
Given the OpenMP:
```c
#pragma omp parallel
{
    work();
}
```
OpenMP.NET provides:
```cs
OpenMP.Parallel.ParallelRegion(() => {
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
OpenMP.NET provides:
```cs
OpenMP.Parallel.For(a, b, i => {
    work(i);
});
```
This function supports the `schedule` optional parameter, which sets the parallel scheduler to use.
Permissible values are `OpenMP.Parallel.Schedule.Static`, `OpenMP.Parallel.Schedule.Dynamic`, and `OpenMP.Parallel.Schedule.Guided`.
The default value is `OpenMP.Parallel.Schedule.Static`.

This function supports the `chunk_size` optional parameter, which sets the chunk size for the scheduler to use.
The default value is dependent on the scheduler and is not documented, as it may change from version to version.

The behavior of `OpenMP.Parallel.For` is undefined if not used within a `ParallelRegion`.

### Parallel For
Given the OpenMP:
```c
#pragma omp parallel for
for (int i = a, i < b; i++)
{
    work(i);
}
```
OpenMP.NET provides:
```cs
OpenMP.Parallel.ParallelFor(a, b, i => {
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
OpenMP.NET provides:
```cs
type local = c;

OpenMP.Parallel.ForReduction(a, b, op, ref local, (ref type local, int i) => {
    local `op` f(i);
});
```
`op` is a value provided by the `OpenMP.Operations` enum, which supports the values `Add`, `Subtract`, `Multiply`, `BinaryAnd`, `BinaryOr`, `BinaryXor`, `BooleanAnd`, `BooleanOr`, `Min`, and `Max`.
The operation on `local` is an operator corresponding to the operator specified by `OpenMP.Operations`, including `+`, `-`, `*`, `&`, `|`, `^`, and so on.

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
OpenMP.NET provides:
```cs
type local = c;

OpenMP.Parallel.ParallelForReduction(a, b, op, ref local, (ref type local, int i) => {
    local `op` f(i);
});
```
This function supports all of the optional parameters of `ParallelRegion` and `ForReduction`, and is merely a wrapper around those two functions for conciseness.

### Sections/Section
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
OpenMP.NET provides:
```cs
OpenMP.Parallel.Sections(() => {
    OpenMP.Parallel.Section(() => {
        work();
    });
    OpenMP.Parallel.Section(() => {
        work();
    });
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
OpenMP.NET provides:
```cs
OpenMP.Parallel.ParallelSections(() => {
    OpenMP.Parallel.Section(() => {
        work();
    });
    OpenMP.Parallel.Section(() => {
        work();
    });
});
```
This function supports the optional parameter `num_threads` from `OpenMP.Parallel.ParallelRegion`.

### Critical
Given the OpenMP:
```c
#pragma omp critical
{
    work();
}
```
OpenMP.NET provides:
```cs
OpenMP.Parallel.Critical(id, () => {
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
OpenMP.NET provides:
```cs
OpenMP.Parallel.Barrier();
```

### Master
Given the OpenMP:
```c
#pragma omp master
{
    work();
}
```
OpenMP.NET provides:
```cs
OpenMP.Parallel.Master(() => {
    work();
});
```
`Master`'s behavior is left undefined if used outside of a `For`.

### Single
Given the OpenMP:
```c
#pragma omp single
{
    work();
}
```
OpenMP.NET provides:
```cs
OpenMP.Parallel.Single(id, () => {
    work();
});
```
The `id` parameter provided should follow the same guidelines as specified in `Critical`.

The behavior of a `single` region is as follows: the first thread in a team to reach a given `Single` region will "own" the `Single` for the duration of the `ParallelRegion`.
On all subsequent encounters, only that first thread will execute the region. All other threads will ignore it.

`Single`'s behavior is left undefined if used outside of a `For`.

### Ordered
Given the OpenMP:
```c
#pragma omp ordered
{
    work();
}
```
OpenMP.NET provides:
```cs
OpenMP.Parallel.Ordered(id, () => {
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

OpenMP.NET supports a subset of this for the `int`, `uint`, `long`, and `ulong` types.
The only implemented atomic operations are `a += b`, `a &= b`, `a |= b`, `++a`, and `--a`.
`a -= b` is implemented, but for signed types only, due to restrictions interfacting with C#'s `Interlocked` class.

The following table documents the supported atomics:

| Operation | OpenMP.NET function           |
------------|--------------------------------
| `a += b`  | `OpenMP.Atomic.Add(ref a, b)` |
| `a -= b`  | `OpenMP.Atomic.Sub(ref a, b)` |
| `a &= b`  | `OpenMP.Atomic.And(ref a, b)` |
| `a \|= b` | `OpenMP.Atomic.Or(ref a, b)`  |
| `++a`     | `OpenMP.Atomic.Inc(ref a)`    |
| `--a`     | `OpenMP.Atomic.Dec(ref a)`    |

For atomic operations like compare-exchange, we recommend interfacting directly with `System.Threading.Interlocked`.
For non-supported atomic operations or types, we recommend using `OpenMP.Parallel.Critical`.
This is more of a limitation of the underlying hardware than anything.

## Locks
OpenMP.NET supports OpenMP-style locks.
It is recommended to use C#'s native `lock` keyword where possible for performance.
However, this API is provided to those who want the familiarity of OpenMP locks.

OpenMP.NET supports the `OpenMP.Lock` object, which is the replacement for `omp_lock_t`.
`omp_init_lock` and `omp_destroy_lock` are not implemented.
Instead, users should instantiate the `OpenMP.Lock` object using the `new` keyword.

OpenMP.NET provides the following functions:

| <omp.h> function     | OpenMP.NET function        | Comments
-----------------------|----------------------------|---------
| omp_set_lock(lock)   | OpenMP.Locking.Set(lock)   | Halt the current thread until the lock is obtained
| omp_unset_lock(lock) | OpenMP.Locking.Unset(lock) | Free the current lock, making it available for other threads
| omp_test_lock(lock)  | OpenMP.Locking.Test(lock)  | Attempt to obtain a lock without blocking, returns true if locking is successful

## Shared Memory

OpenMP.NET supports an API for declaring thread-shared memory within a parallel region.
Shared memory is provided through the `OpenMP.Shared` class.

The following provides an example of a parallel vector initialization:
```cs
static void InitVector()
{
    double[] returnVector;
    
    OpenMP.Parallel.ParallelRegion(() =>
    {
        OpenMP.Shared<double[]> vec = new OpenMP.Shared<double[]>("vec", new double[1024]);
        OpenMP.Parallel.For(0, 1024, i =>
        {
            vec.Get()[i] = 1.0;
        });
        
        returnVector = vec.Get();
        vec.Clear()
    });
    
    return returnVector;
}
```

The `OpenMP.Shared` class supports the following methods:
| Method                            | Action 
------------------------------------|-------
| Constructor(string name, T value) | Initializes a shared variable with name `name` and starting value `value`
| Clear()                           | Cleares a shared variable
| Set(T value)                      | Sets a shared variable to value `value`
| Get()                             | Gets a shared variable

The `OpenMP.Shared` constructor and `Clear()` methods serve as implicit barriers, ensuring that all threads can access the memory before proceeding.

## Supported Functions

OpenMP.NET provides an analog of the following functions:

| <omp.h> function         | OpenMP.NET function                | Comments
---------------------------|------------------------------------|---------
| omp_get_num_procs()      | OpenMP.Parallel.GetNumProcs()      | Returns the number of logical threads on the system
| omp_get_num_threads()    | OpenMP.Parallel.GetNumThreads()    | Returns the number of active threads in the current region
| omp_set_num_threads(int) | OpenMP.Parallel.SetNumThreads(int) | Sets the number of threads for the next parallel region to use
| omp_get_thread_num()     | OpenMP.Parallel.GetThreadNum()     | Gets the ID of the current thread
| omp_get_max_threads()    | OpenMP.Parallel.GetMaxThreads()    | Gets the maximum number of threads the runtime may use in the next region
| omp_in_parallel()        | OpenMP.Parallel.InParallel()       | Returns true if called from within a parallel region
| omp_set_dynamic(int)     | OpenMP.Parallel.SetDynamic()       | Tells the runtime to dynamically adjust the number of threads, can disable by calling SetNumThreads
| omp_get_dynamic()        | OpenMP.Parallel.GetDynamic()       | Returns true if the runtime can dynamically adjust the number of threads
| omp_set_nested(int)      | OpenMP.Parallel.SetNested(int)     | Returns a NotImplementedException
| omp_get_nested()         | OpenMP.Parallel.GetNested()        | Returns false
| omp_get_wtime()          | OpenMP.Parallel.GetWTime()         | Returns the number of seconds since the Unix Epoch as a double
