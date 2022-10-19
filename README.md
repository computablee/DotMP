# OpenMP.NET
A library for writing OpenMP-style parallel code in .NET.
Inspired by the fork-join paradigm of OpenMP, and attempts to replicate the OpenMP programming style as faithfully as possible.

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
OpenMP.Parallel.Critical(() => {
    work();
});
```
Mutual exclusion is based on `.GetHashCode()` of the provided `Action` object.
This behavior was chosen to more faithfully model the OpenMP syntax.
Therefore, if two `Critical` regions are provided that use the same `Action`, mutual exclusion is not guaranteed.

### Barrier
Given the OpenMP:
```c
#pragma omp barrier
```
OpenMP.NET provides:
```
OpenMP.Parallel.Barrier();
```

## Supported Functions

OpenMP provides an analog of the following functions:

| <omp.h> function      | OpenMP.NET function             |
------------------------|----------------------------------
| omp_get_num_procs()   | OpenMP.Parallel.GetNumProcs()   |
| omp_get_num_threads() | OpenMP.Parallel.GetNumThreads() |
| omp_get_thread_num()  | OpenMP.Parallel.GetThreadNum()  |
