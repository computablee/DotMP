# Contributing to DotMP

Contributing to DotMP is pretty simple.
As with contributing to any open-source project,
fork the repository (choose the branch based on the issue you want to work on),
make local edits, then open a pull request.

In your pull request, make sure to specify **which issue you are addressing, how you've addressed it, and how you've tested it.**
We have a CI pipeline to run unit tests on pull requests. Those **must** pass before a pull request will be merged.
If you add new functionality, add a new unit test.

## Style Conventions

I'm not too picky on style, but some general guidelines go a long way.

* Try to keep file structure in the same general style as `libgomp`, GCC's implementation of OpenMP.
* Object-oriented style is nice where possible, but at the end of the day we're trying to replicate a pragma-based paradigm for procedural langauges.
If something makes sense to make OOP, do so. Otherwise, static classes/methods are sufficient.
* `UpperCamelCase` for namespaces, classes, and methods. `snake_case` for member and local variables.
* Restrict access where possible. Use `public` only for things that are both *safe* and *intended* to be called/read/written by the user.
I see no reason to ever use `public` member variables for this project.
If you have to, make sure it follows the conventions of OpenMP, and use getters/setters with checks to make sure the values are legal.
I use `internal` a lot, but I also access a lot of member variables directly within the source. This is following the style of `libgomp`.
I am happy to see this improved with heavier use of `private` and getters/setters.
* Documentation is handled with Doxygen and XML-style comments. See the existing code for examples.
There is a CD pipeline to automatically generate Doxygen files and publish to the GitHub Pages site upon a push to Main.
Please adequately document your code, and where it makes sense to explain the functionality and use case of the code, do so.

## Optimization Guidelines

This is a pseudo-implementation of OpenMP. OpenMP is heavily optimized. Avoid mutual exclusion where possible, and use locks efficiently where necessary.
Don't worry about optimizing to obscurity, but benchmark your code and make sure that you get reasonable scaling.

## Code of Conduct

Be nice, be respectful. Limit use of adult language, and what the maintainers say is what goes.
