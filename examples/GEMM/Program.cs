using System;
using OpenMP;

static class GEMM
{
    //parallel GEMM
    public static void ParallelGEMM(double[][] A, double[][] B, double[][] C)
    {
        //get the appropriate lengths of the arrays
        int m = A.Length;
        int n = B[0].Length;
        int k = A[0].Length;

        //parallel for loop on the outermost loop
        OpenMP.Parallel.ParallelFor(0, m, schedule: OpenMP.Parallel.Schedule.Dynamic, action: i =>
        {
            //inner loop is serial
            for (int j = 0; j < n; j++)
            {
                double sum = 0;

                //iterate across the row and calculate the dot product of the first matrix's row vector and the second matrix's column vector
                for (int l = 0; l < k; l++)
                {
                    sum += A[i][l] * B[l][j];
                }

                //store the result in the output matrix
                C[i][j] = sum;
            }
        });
    }

    //serial GEMM
    public static void SerialGEMM(double[][] A, double[][] B, double[][] C)
    {
        //get the appropriate lengths of the arrays
        int m = A.Length;
        int n = B[0].Length;
        int k = A[0].Length;

        //iterate across the first matrix's rows
        for (int i = 0; i < m; i++)
        {
            //iterate across the second matrix's columns
            for (int j = 0; j < n; j++)
            {
                double sum = 0;

                //iterate across the row and calculate the dot product of the first matrix's row vector and the second matrix's column vector
                for (int l = 0; l < k; l++)
                {
                    sum += A[i][l] * B[l][j];
                }

                //store the result in the output matrix
                C[i][j] = sum;
            }
        }
    }

    //randomly fill a matrix with values between 0 and 1
    public static void RandomMatrix(double[][] A)
    {
        Random r = new Random();

        //iterate across the rows
        for (int i = 0; i < A.Length; i++)
        {
            //iterate across the columns
            for (int j = 0; j < A[0].Length; j++)
            {
                //store a random value in the matrix
                A[i][j] = r.NextDouble();
            }
        }
    }
}

class Driver
{
    public static void Main(string[] args)
    {
        //get the matrix dimensions from the command line, or set to 250 if left unspecified
        int m, n, k;
        if (args.Length < 1)
        {
            m = n = k = 250;
        }
        else
        {
            m = n = k = int.Parse(args[0]);
        }

        //create the matrices
        double[][] A = new double[m][];
        double[][] B = new double[k][];
        double[][] C = new double[m][];

        //allocate the memory for the matrices
        for (int i = 0; i < m; i++)
        {
            A[i] = new double[k];
            C[i] = new double[n];
        }

        //allocate the memory for the second matrix
        for (int i = 0; i < k; i++)
        {
            B[i] = new double[n];
        }

        //fill the matrices with random values
        GEMM.RandomMatrix(A);
        GEMM.RandomMatrix(B);

        //run the serial and parallel GEMM functions, timed
        double start = OpenMP.Parallel.GetWTime();
        GEMM.SerialGEMM(A, B, C);
        double end = OpenMP.Parallel.GetWTime();
        Console.WriteLine("SerialGEMM: {0}", end - start);

        start = OpenMP.Parallel.GetWTime();
        GEMM.ParallelGEMM(A, B, C);
        end = OpenMP.Parallel.GetWTime();
        Console.WriteLine("ParallelGEMM: {0}", end - start);
    }
}