DN=dotnet
BUILD=Release

all: docs build tests examples benches

examples: examples-cs examples-dmp examples-seq

benches:
	$(DN) build -c Release benchmarks/HeatTransfer

examples-cs:
	$(DN) build -c $(BUILD) examples/CSParallel/ConjugateGradient
	$(DN) build -c $(BUILD) examples/CSParallel/HeatTransfer
	$(DN) build -c $(BUILD) examples/CSParallel/GEMM
	$(DN) build -c $(BUILD) examples/CSParallel/KNN

examples-dmp:
	$(DN) build -c $(BUILD) examples/DotMP/ConjugateGradient
	$(DN) build -c $(BUILD) examples/DotMP/HeatTransfer
	$(DN) build -c $(BUILD) examples/DotMP/GEMM
	$(DN) build -c $(BUILD) examples/DotMP/KNN

examples-seq:
	$(DN) build -c $(BUILD) examples/Serial/ConjugateGradient
	$(DN) build -c $(BUILD) examples/Serial/HeatTransfer
	$(DN) build -c $(BUILD) examples/Serial/GEMM
	$(DN) build -c $(BUILD) examples/Serial/KNN

tests:
	$(DN) build -c $(BUILD) DotMP-Tests

test:
	$(DN) test -c $(BUILD) -l "console;verbosity=detailed" -p:CollectCoverage=true -p:CoverletOutputFormat=opencover DotMP-Tests

build:
	$(DN) build -c $(BUILD) DotMP

docs:
	doxygen

pack:
	$(DN) pack -c $(BUILD) DotMP

clean:
	rm -rf docs
	rm -rf DotMP/bin DotMP/obj
	rm -rf DotMP-Tests/bin DotMP-Tests/obj
	rm -rf DotMP-Tests/*.opencover.xml
	rm -rf examples/CSParallel/ConjugateGradient/bin examples/CSParallel/ConjugateGradient/obj
	rm -rf examples/CSParallel/HeatTransfer/bin examples/CSParallel/HeatTransfer/obj
	rm -rf examples/CSParallel/GEMM/bin examples/CSParallel/GEMM/obj
	rm -rf examples/DotMP/ConjugateGradient/bin examples/DotMP/ConjugateGradient/obj
	rm -rf examples/DotMP/HeatTransfer/bin examples/DotMP/HeatTransfer/obj
	rm -rf examples/DotMP/GEMM/bin examples/DotMP/GEMM/obj
	rm -rf examples/Serial/ConjugateGradient/bin examples/Serial/ConjugateGradient/obj
	rm -rf examples/Serial/HeatTransfer/bin examples/Serial/HeatTransfer/obj
	rm -rf examples/Serial/GEMM/bin examples/Serial/GEMM/obj
	rm -rf benchmarks/HeatTransfer/bin benchmarks/HeatTransfer/obj benchmarks/HeatTransfer/BenchmarkDotNet.Artifacts
