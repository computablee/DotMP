# Contributing to DotMP

Contributing to DotMP is pretty simple.
As with contributing to any open-source project,
fork the repository (choose the branch based on the issue you want to work on),
make local edits, then open a pull request.

In your pull request, make sure to specify **fully fill out the PR template.**
If a field is not applicable to your PR, just provide a brief explanation why.

All checks **must** pass before a PR is accepted, with the possible exception of `codecov` checks at the maintainers' discretions.

## PRs That May Be Immediately Rejected

There are several "low-effort" PRs that are grounds for immediate rejection. These include
- PRs which do not properly utilize the PR template.
- PRs which make meaningless/spam contributions to the repository.
- PRs which do not pass the "Build" check (please, please, make sure your code compiles before opening a PR).
- Other PRs at the maintainers' discretions.

## Style Conventions

I'm not too picky on style, but some general guidelines go a long way.

* Object-oriented style is nice where possible, but at the end of the day we're trying to replicate a pragma-based paradigm for procedural langauges.
If something makes sense to make OOP, do so. Otherwise, static classes/methods are sufficient.
* `UpperCamelCase` for namespaces, classes, and methods. `snake_case` for member and local variables.
* Restrict access where possible. Use `public` only for things that are both *safe* and *intended* to be called/read/written by the user.
I see no reason to ever use `public` member variables for this project.
If you have to, make sure it follows the conventions of OpenMP, and use getters/setters with checks to make sure the values are legal.
* Documentation is handled with Doxygen and XML-style comments. See the existing code for examples.
There is a CD pipeline to automatically generate Doxygen files and publish to the GitHub Pages site upon a push to `main`.
Please adequately document your code, and where it makes sense to explain the functionality and use case of the code, do so.
* We have a linter as part of the CI pipeline.
That must pass before a PR is accepted.

## Testing Guidelines
I am quite proud to have DotMP sitting at 99% code coverage at the time of writing.
I admit that code coverage is not a perfect metric, and there are many reasons why that is.
However, on the path towards 99% code coverage, many bugs were found and fixed along the way.

With that said, the following requirements are in place for PRs with respect to testing:
- All files must be hit.
- All methods must be hit.
- All major code paths must be hit.

We have a whole plethora of integration tests that make this very achievable.
If you implement a method, make sure to test it!
Using XUnit is not too hard.
Just skim the existing tests and you'll get an idea of what you should write.
On your local machine, using `make test` at the root of the directory instead of `dotnet test` in the tests directory will automatically collect coverage metrics and display them.

Again, code coverage is not a perfect metric.
I hate using code coverage as a set of rules for accepting PRs, but at the current time, this is the best way to set some hard guidelines for testing your code.

## Optimization Guidelines

This is a pseudo-implementation of OpenMP. OpenMP is heavily optimized. Avoid mutual exclusion where possible, and use locks efficiently where necessary.
Don't worry about optimizing to obscurity, but benchmark your code and make sure that you get reasonable scaling.
We are adding some benchmarks to the `benchmarks/` subfolder which use BenchmarkDotNet. Please try to use those where you can.
