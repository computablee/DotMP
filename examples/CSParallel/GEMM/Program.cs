/*
 * DotMP - A collection of powerful abstractions for parallel programming in .NET with an OpenMP-like API. 
 * Copyright (C) 2023 Phillip Allen Lane
 *
 * This library is free software; you can redistribute it and/or modify it under the terms of the GNU Lesser
 * General Public License as published by the Free Software Foundation; either version 2.1 of the License, or
 * (at your option) any later version.

 * This library is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the
 * implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public
 * License for more details.

 * You should have received a copy of the GNU Lesser General Public License along with this library; if not,
 * write to the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.
 * Also add information on how to contact you by electronic and paper mail. 
 */

using System;
using System.Threading.Tasks;

static class GEMM
{
    //parallel GEMM
    public static void DoGEMM(double[][] A, double[][] B, double[][] C)
    {
        //get the appropriate lengths of the arrays
        int m = A.Length;
        int n = B[0].Length;
        int k = A[0].Length;

        //parallel for loop on the outermost loop
        Parallel.For(0, m, i =>
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
        //check the command line arguments
        if (args.Length < 2)
        {
            Console.WriteLine("Usage: dotnet run <mat dims> <num runs>");
            return;
        }

        int m, n, k, numRuns;
        m = n = k = Convert.ToInt32(args[0]);
        numRuns = Convert.ToInt32(args[1]);

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

        //warmup
        for (int i = 0; i < 5; i++)
        {
            GEMM.DoGEMM(A, B, C);
        }

        //create min, max, avg variables
        double min = double.MaxValue;
        double max = double.MinValue;
        double avg = 0.0f;

        //run the algorithm numRuns times
        for (int i = 0; i < numRuns; i++)
        {
            //time the algorithm
            double tick = DotMP.Parallel.GetWTime();
            GEMM.DoGEMM(A, B, C);
            double tock = DotMP.Parallel.GetWTime();

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
    }
}