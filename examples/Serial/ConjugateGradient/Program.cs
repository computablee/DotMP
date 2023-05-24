using OpenMP;
using System;
using System.IO;
using System.Threading;

//define CSR matrix format
class CSRMatrix
{
    public int m; //number of rows
    public int n; //number of columns
    public int nnz; //number of non-zero elements
    public int[] rowPtr; //rowPtr[i] is the index of the first non-zero element in row i
    public int[] colInd; //colInd[j] is the column index of the j-th non-zero element
    public double[] values; //values[j] is the value of the j-th non-zero element

    // default constructor (all fields will be set in the ReadCSRMatrix function so we don't need to initialize them here... this is just to eliminate compiler warnings)
    public CSRMatrix()
    {
        m = 0;
        n = 0;
        nnz = 0;
        rowPtr = new int[0];
        colInd = new int[0];
        values = new double[0];
    }
}

static class ConjugateGradient
{
    // read CSR matrix from file
    public static CSRMatrix ReadCSRMatrix(string filename)
    {
        //create an empty CSR matrix
        CSRMatrix A = new CSRMatrix();
        //open file
        var file = File.Open(filename, FileMode.Open);

        int m, n, nnz;
        int[] rowPtr, colInd;
        double[] values;

        //open a stream reader to read the file
        using (var reader = new StreamReader(file))
        {
            //read the file
            var content = reader.ReadToEnd();
            //replace all new line characters with space
            content = content.Replace('\n', ' ');
            //split the content into tokens
            var tokens = content.Split(' ');
            //read the matrix dimensions and nnz
            m = int.Parse(tokens[0]);
            n = int.Parse(tokens[1]);
            nnz = int.Parse(tokens[2]);

            //allocate memory for the CSR matrix
            rowPtr = new int[m + 1];
            colInd = new int[nnz];
            values = new double[nnz];

            int i;
            //read rowPtr, colInd, and values
            for (i = 0; i < m + 1; i++)
            {
                rowPtr[i] = int.Parse(tokens[3 + i]);
            }
            for (; i < nnz + m + 1; i++)
            {
                colInd[i - m - 1] = int.Parse(tokens[3 + i]);
            }
            for (; i < nnz + nnz + m + 1; i++)
            {
                values[i - nnz - m - 1] = double.Parse(tokens[3 + i]);
            }
        }

        //set the fields of the CSR matrix
        A.m = m;
        A.n = n;
        A.nnz = nnz;
        A.rowPtr = rowPtr;
        A.colInd = colInd;
        A.values = values;

        return A;
    }

    //make a vector
    public static double[] MakeVector(int m, double val)
    {
        //create an array of size m
        double[] x = new double[m];
        //initialize all elements to val
        for (int i = 0; i < m; i++)
        {
            x[i] = val;
        }
        return x;
    }

    //compute the sparse matrix-vector product y = A*x
    public static double[] SpMV(CSRMatrix A, double[] x)
    {
        //create an array y which will be used to store the result of the matrix-vector product
        double[] y = new double[A.m];

        //compute the matrix-vector product
        for (int i = 0; i < A.m; i++)
        {
            //compute the i-th element of the result
            double sum = 0.0;
            for (int j = A.rowPtr[i]; j < A.rowPtr[i + 1]; j++)
            {
                //sum += A[i,j] * x[j]
                sum += A.values[j] * x[A.colInd[j]];
            }
            //store the result in the array y
            y[i] = sum;
        }

        return y;
    }

    //compute the vector difference dest = x - y
    public static double[] SubtractVectors(double[] x, double[] y)
    {
        //create an array to store the result
        double[] dest = new double[x.Length];
        //compute the difference
        for (int i = 0; i < x.Length; i++)
        {
            dest[i] = x[i] - y[i];
        }
        return dest;
    }

    //compute the dot product of two vectors
    public static void DotProduct(double[] x, double[] y, ref double sum)
    {
        //initialize the sum to 0
        sum = 0;
        //compute the dot product
        for (int i = 0; i < x.Length; i++)
        {
            //inner loop of the reduction
            sum += x[i] * y[i];
        }
    }

    //compute the vector sum dest = a*x + y
    public static void Daxpy(double a, double[] x, double[] y, double[] dest)
    {
        //compute the daxpy
        for (int i = 0; i < x.Length; i++)
        {
            dest[i] = a * x[i] + y[i];
        }
    }

    //compute the conjugate gradient method
    public static (double[], double) DoConjugateGradient(CSRMatrix A, double[] b, int max, double tolerance)
    {
        //initialize x to 0
        double[] x = MakeVector(A.m, 0.0);
        //initialize p
        double[] p = new double[A.m];
        //r = b - A*x
        double[] r = SubtractVectors(b, SpMV(A, x));
        //p = r
        Array.Copy(r, p, r.Length);

        double delta = 0;
        double deltaOld = 0;
        //delta = r*r
        DotProduct(r, r, ref deltaOld);
        double temp_scalar = 0;

        int iters = 0;
        int k = 0;

        //iterate until convergence or max iterations
        while (k < max)
        {
            //increment the iteration counter
            ++k;

            //w = A*p
            double[] w = SpMV(A, p);
            //temp_scalar = p*w
            DotProduct(p, w, ref temp_scalar);
            //alpha = deltaOld / (p*w)
            double alpha = deltaOld / temp_scalar;
            //x = x + alpha*p
            Daxpy(alpha, p, x, x);
            //r = r - alpha*w
            Daxpy(-alpha, w, r, r);

            //delta = r*r
            DotProduct(r, r, ref delta);

            //check for convergence
            if (Math.Sqrt(delta) < tolerance)
            {
                break;
            }

            //p = r + (delta/deltaOld)*p
            Daxpy(delta / deltaOld, p, r, p);

            //swap delta and deltaOld
            deltaOld = delta;
        }

        //store the number of iterations in the variable iters
        iters = k;

        return (x, iters);
    }
}

class Driver
{
    public static void Main(string[] args)
    {
        //check the command line arguments
        if (args.Length < 2)
        {
            Console.WriteLine("Usage: dotnet run <csr file> <num runs>");
            return;
        }

        //read the matrix from the file
        CSRMatrix A = ConjugateGradient.ReadCSRMatrix(args[0]);
        //create the b vector, initialize all elements to 1
        double[] b = ConjugateGradient.MakeVector(A.n, 1.0);

        //number of times to run the algorithm
        int numRuns = int.Parse(args[1]);

        //create x, initialize to avoid compiler warnings
        double[] x = new double[0];

        //warmup
        for (int i = 0; i < 5; i++)
        {
            (x, _) = ConjugateGradient.DoConjugateGradient(A, b, 1000, 0.01);
        }

        //create min, max, avg variables
        double min = double.MaxValue;
        double max = double.MinValue;
        double avg = 0.0f;

        double iters = 0;

        //run the algorithm numRuns times
        for (int i = 0; i < numRuns; i++)
        {
            //time the algorithm
            double tick = OpenMP.Parallel.GetWTime();
            (x, iters) = ConjugateGradient.DoConjugateGradient(A, b, 1000, 0.01);
            double tock = OpenMP.Parallel.GetWTime();

            //update min, max, avg
            tock = tock - tick;
            avg += tock;
            min = Math.Min(min, tock);
            max = Math.Max(max, tock);
        }

        avg /= numRuns;

        //print the results
        Console.WriteLine("Min: {0}", min);
        Console.WriteLine("Max: {0}", max);
        Console.WriteLine("Avg: {0}", avg);
        Console.WriteLine("Iters: {0}", iters);

        int numWrong = 0;

        //check the result
        double[] error = ConjugateGradient.SubtractVectors(ConjugateGradient.SpMV(A, x), b);

        //check for errors
        for (int i = 0; i < A.m; i++)
        {
            if (error[i] > 1e-3)
            {
                //Console.WriteLine("Error at {0}: error of {1}", i, error[i]);
                numWrong++;
            }
        }
        Console.WriteLine("Number of wrong values: {0}", numWrong);

        return;
    }
}