using OpenMP;
using System;
using System.IO;
using System.Threading;

class CSRMatrix
{
    public int m;
    public int n;
    public int nnz;
    public int[] rowPtr;
    public int[] colInd;
    public double[] values;

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
    static CSRMatrix ReadCSRMatrix(string filename)
    {
        CSRMatrix A = new CSRMatrix();
        var file = File.Open(filename, FileMode.Open);

        int m, n, nnz;
        int[] rowPtr, colInd;
        double[] values;

        using (var reader = new StreamReader(file))
        {
            var content = reader.ReadToEnd();
            content = content.Replace('\n', ' ');
            var tokens = content.Split(' ');
            m = int.Parse(tokens[0]);
            n = int.Parse(tokens[1]);
            nnz = int.Parse(tokens[2]);

            rowPtr = new int[m + 1];
            colInd = new int[nnz];
            values = new double[nnz];

            int i;
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

        A.m = m;
        A.n = n;
        A.nnz = nnz;
        A.rowPtr = rowPtr;
        A.colInd = colInd;
        A.values = values;

        return A;
    }

    static double[] MakeVector(int m, double val)
    {
        double[] x = new double[m];
        OpenMP.Parallel.ParallelFor(0, m, i => x[i] = val);
        return x;
    }

    private static volatile double[] SpMV_y = new double[0];

    static double[] SpMV(CSRMatrix A, double[] x)
    {
        OpenMP.Parallel.Master(() => SpMV_y = new double[A.m]);
        OpenMP.Parallel.Barrier();

        OpenMP.Parallel.For(0, A.m,
            schedule: OpenMP.Parallel.Schedule.Guided,
            action: i =>
        {
            double sum = 0.0;
            for (int j = A.rowPtr[i]; j < A.rowPtr[i + 1]; j++)
            {
                sum += A.values[j] * x[A.colInd[j]];
            }
            SpMV_y[i] = sum;
        });
        return SpMV_y;
    }

    static double[] SubtractVectors(double[] x, double[] y)
    {
        double[] dest = new double[x.Length];
        OpenMP.Parallel.ParallelFor(0, x.Length, i => dest[i] = x[i] - y[i]);
        return dest;
    }

    static void DotProduct(double[] x, double[] y, ref double sum)
    {
        sum = 0.0;
        OpenMP.Parallel.Barrier();
        OpenMP.Parallel.ForReduction(0, x.Length,
            OpenMP.Operations.Add, ref sum,
            (ref double sum, int i) =>
        {
            sum += x[i] * y[i];
        });
    }

    static void Daxpy(double a, double[] x, double[] y, double[] dest)
    {
        OpenMP.Parallel.For(0, x.Length, i => dest[i] = a * x[i] + y[i]);
    }

    static (double[], double) DoConjugateGradient(CSRMatrix A, double[] b, int max, double tolerance)
    {
        double[] x = MakeVector(A.m, 0.0);
        double[] p = new double[A.m];
        double[] r = SubtractVectors(b, SpMV(A, x));
        double[] temp = new double[A.m];
        Array.Copy(r, p, r.Length);

        double delta = 0;
        double deltaOld = 0;
        DotProduct(r, r, ref deltaOld);
        double temp_scalar = 0;

        int iters = 0;

        OpenMP.Parallel.ParallelRegion(() =>
        {
            int k = 0;

            while (k < max)
            {
                ++k;

                double[] w = SpMV(A, p);
                DotProduct(p, w, ref temp_scalar);
                double alpha = deltaOld / temp_scalar;
                Daxpy(alpha, p, x, x);
                Daxpy(-alpha, w, r, r);

                DotProduct(r, r, ref delta);

                if (Math.Sqrt(delta) < tolerance)
                {
                    break;
                }

                Daxpy(delta / deltaOld, p, r, temp);
                OpenMP.Parallel.Master(() => Array.Copy(temp, p, temp.Length));
                OpenMP.Parallel.Barrier();

                deltaOld = delta;
            }

            OpenMP.Parallel.Single(1, () => iters = k);
        });

        return (x, iters);
    }

    public static void Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Usage: dotnet run <csr file> <num runs>");
            return;
        }

        CSRMatrix A = ReadCSRMatrix(args[0]);
        double[] b = MakeVector(A.n, 1.0);

        int numRuns = int.Parse(args[1]);

        double[] x = new double[0];

        for (int i = 0; i < 5; i++)
        {
            (x, _) = DoConjugateGradient(A, b, 1000, 0.01);
        }

        double min = double.MaxValue;
        double max = double.MinValue;
        double avg = 0.0f;

        double iters = 0;

        for (int i = 0; i < numRuns; i++)
        {
            double tick = OpenMP.Parallel.GetWTime();
            (x, iters) = DoConjugateGradient(A, b, 1000, 0.01);
            double tock = OpenMP.Parallel.GetWTime();

            tock = tock - tick;
            avg += tock;
            min = Math.Min(min, tock);
            max = Math.Max(max, tock);
        }

        avg /= numRuns;

        Console.WriteLine("Min: {0}", min);
        Console.WriteLine("Max: {0}", max);
        Console.WriteLine("Avg: {0}", avg);
        Console.WriteLine("Iters: {0}", iters);

        int numWrong = 0;

        double[] error = SubtractVectors(SpMV(A, x), b);

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