DN=dotnet

all: docs build tests examples

examples: examples-cs examples-omp examples-seq

examples-cs:
	$(DN) build -c Release examples/CSParallel/ConjugateGradient
	$(DN) build -c Release examples/CSParallel/HeatTransfer
	$(DN) build -c Release examples/CSParallel/GEMM

examples-omp:
	$(DN) build -c Release examples/DotMP/ConjugateGradient
	$(DN) build -c Release examples/DotMP/HeatTransfer
	$(DN) build -c Release examples/DotMP/GEMM

examples-seq:
	$(DN) build -c Release examples/Serial/ConjugateGradient
	$(DN) build -c Release examples/Serial/HeatTransfer
	$(DN) build -c Release examples/Serial/GEMM

tests:
	$(DN) build -c Release DotMP-Tests

test:
	$(DN) test -c Release DotMP-Tests

build:
	$(DN) build -c Release DotMP

docs:
	doxygen

clean:
	rm -rf docs
	rm -rf DotMP/bin DotMP/obj
	rm -rf DotMP-Tests/bin DotMP-Tests/obj
	rm -rf examples/CSParallel/ConjugateGradient/bin examples/CSParallel/ConjugateGradient/obj
	rm -rf examples/CSParallel/HeatTransfer/bin examples/CSParallel/HeatTransfer/obj
	rm -rf examples/CSParallel/GEMM/bin examples/CSParallel/GEMM/obj
	rm -rf examples/DotMP/ConjugateGradient/bin examples/DotMP/ConjugateGradient/obj
	rm -rf examples/DotMP/HeatTransfer/bin examples/DotMP/HeatTransfer/obj
	rm -rf examples/DotMP/GEMM/bin examples/DotMP/GEMM/obj
	rm -rf examples/Serial/ConjugateGradient/bin examples/Serial/ConjugateGradient/obj
	rm -rf examples/Serial/HeatTransfer/bin examples/Serial/HeatTransfer/obj
	rm -rf examples/Serial/GEMM/bin examples/Serial/GEMM/obj