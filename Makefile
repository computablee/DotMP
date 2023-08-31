DN=dotnet

all: docs build tests examples

examples: examples-cs examples-omp examples-seq

examples-cs:
	$(DN) build -c Release examples/CSParallel/ConjugateGradient
	$(DN) build -c Release examples/CSParallel/HeatTransfer
	$(DN) build -c Release examples/CSParallel/GEMM

examples-omp:
	$(DN) build -c Release examples/OpenMP.NET/ConjugateGradient
	$(DN) build -c Release examples/OpenMP.NET/HeatTransfer
	$(DN) build -c Release examples/OpenMP.NET/GEMM

examples-seq:
	$(DN) build -c Release examples/Serial/ConjugateGradient
	$(DN) build -c Release examples/Serial/HeatTransfer
	$(DN) build -c Release examples/Serial/GEMM

tests:
	$(DN) build -c Release OpenMP.NET.Tests

test:
	$(DN) test -c Release OpenMP.NET.Tests

build:
	$(DN) build -c Release OpenMP.NET

docs:
	doxygen

clean:
	rm -rf OpenMP.NET.Docs
	rm -rf OpenMP.NET/bin OpenMP.NET/obj
	rm -rf OpenMP.NET.Tests/bin OpenMP.NET.Tests/obj
	rm -rf examples/CSParallel/ConjugateGradient/bin examples/CSParallel/ConjugateGradient/obj
	rm -rf examples/CSParallel/HeatTransfer/bin examples/CSParallel/HeatTransfer/obj
	rm -rf examples/CSParallel/GEMM/bin examples/CSParallel/GEMM/obj
	rm -rf examples/OpenMP.NET/ConjugateGradient/bin examples/OpenMP.NET/ConjugateGradient/obj
	rm -rf examples/OpenMP.NET/HeatTransfer/bin examples/OpenMP.NET/HeatTransfer/obj
	rm -rf examples/OpenMP.NET/GEMM/bin examples/OpenMP.NET/GEMM/obj
	rm -rf examples/Serial/ConjugateGradient/bin examples/Serial/ConjugateGradient/obj
	rm -rf examples/Serial/HeatTransfer/bin examples/Serial/HeatTransfer/obj
	rm -rf examples/Serial/GEMM/bin examples/Serial/GEMM/obj