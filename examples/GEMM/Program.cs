using System;
using OpenMP;

static class GEMM
{
    public static void ParallelGEMM(double[][] A, double[][] B, double[][] C)
    {
        int m = A.Length;
        int n = B[0].Length;
        int k = A[0].Length;

        OpenMP.Parallel.ParallelFor(0, m, schedule: OpenMP.Parallel.Schedule.Dynamic, action: i =>
        {
            for (int j = 0; j < n; j++)
            {
                double sum = 0;
                for (int l = 0; l < k; l++)
                {
                    sum += A[i][l] * B[l][j];
                }
                C[i][j] = sum;
            }
        });
    }

    public static void SerialGEMM(double[][] A, double[][] B, double[][] C)
    {
        int m = A.Length;
        int n = B[0].Length;
        int k = A[0].Length;

        for (int i = 0; i < m; i++)
        {
            for (int j = 0; j < n; j++)
            {
                double sum = 0;
                for (int l = 0; l < k; l++)
                {
                    sum += A[i][l] * B[l][j];
                }
                C[i][j] = sum;
            }
        }
    }

    public static void RandomMatrix(double[][] A)
    {
        Random r = new Random();
        for (int i = 0; i < A.Length; i++)
        {
            for (int j = 0; j < A[0].Length; j++)
            {
                A[i][j] = r.NextDouble();
            }
        }
    }
}

class Driver
{
    public static void Main(string[] args)
    {
        int m, n, k;
        if (args.Length < 1)
        {
            m = n = k = 250;
        }
        else
        {
            m = n = k = int.Parse(args[0]);
        }

        double[][] A = new double[m][];
        double[][] B = new double[k][];
        double[][] C = new double[m][];

        for (int i = 0; i < m; i++)
        {
            A[i] = new double[k];
            C[i] = new double[n];
        }

        for (int i = 0; i < k; i++)
        {
            B[i] = new double[n];
        }

        GEMM.RandomMatrix(A);
        GEMM.RandomMatrix(B);

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