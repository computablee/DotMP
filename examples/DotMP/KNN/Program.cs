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
 */

using DotMP;
using System;

class Point
{
    //coordinates of a 3D point
    public double x { get; private set; }
    public double y { get; private set; }
    public double z { get; private set; }

    //default constructor
    public Point()
    {
        x = 0;
        y = 0;
        z = 0;
    }

    //constructor with parameters
    public Point(double x, double y, double z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }
}

class Cluster
{
    //list of points in the cluster
    private List<Point> points;

    //default constructor
    public Cluster()
    {
        points = new List<Point>();
    }

    //get the points in the cluster
    public Point[] GetPoints()
    {
        //copy the points to an array
        Point[] points = new Point[this.points.Count];
        this.points.CopyTo(points);

        //return the array
        return points;
    }

    //add a point to the cluster
    public void AddPoint(Point point)
    {
        points.Add(point);
    }
}

static class KNN
{
    //generate a cluster of random points
    public static Cluster GenerateCluster(int points)
    {
        //create a new cluster
        Cluster cluster = new Cluster();
        //create a random number generator
        Random random = new Random();

        //for each desired point
        for (int i = 0; i < points; i++)
        {
            //add a new random point to the cluster
            cluster.AddPoint(new Point(random.NextDouble(), random.NextDouble(), random.NextDouble()));
        }

        //return the cluster
        return cluster;
    }

    //find the K-nearest neighbors of a point in a cluster
    public static Point[] KNearestNeighbors(this Cluster cluster, Point point, int k)
    {
        //get the points in the cluster
        Point[] points = cluster.GetPoints();
        //create an array to store the K-nearest neighbors
        Point[] nearestPoints = new Point[k];
        //create an array to store the distances
        double[] distances = new double[points.Length];

        //for each point in the cluster
        DotMP.Parallel.ParallelFor(0, points.Length, i =>
        {
            //calculate the distance between the point and the given point
            distances[i] = Math.Sqrt(Math.Pow(points[i].x - point.x, 2) + Math.Pow(points[i].y - point.y, 2) + Math.Pow(points[i].z - point.z, 2));
        });

        //sort the points based off of the distances
        Array.Sort(distances, points);

        //copy the K-nearest neighbors to the array
        for (int i = 0; i < k; i++)
        {
            nearestPoints[i] = points[i];
        }

        //return the K-nearest neighbors
        return nearestPoints;
    }
}

class Driver
{
    //main function
    public static void Main(string[] args)
    {
        //check for the correct number of arguments
        if (args.Length < 6)
        {
            Console.WriteLine("Usage: dotnet run <number of points> <k> <x> <y> <z> <iters> [<output neighbors>]");
            return;
        }

        //fetch all of the values from the command line
        int numOfPoints = int.Parse(args[0]);
        int k = int.Parse(args[1]);
        Point point = new Point(double.Parse(args[2]), double.Parse(args[3]), double.Parse(args[4]));
        int iters = int.Parse(args[5]);
        bool output = (args.Length > 6) ? Convert.ToBoolean(args[6]) : false;

        //create some default objects
        Point[] points = new Point[0];
        Cluster cluster;

        double min = double.MaxValue;
        double max = double.MinValue;
        double avg = 0;

        //generate a random cluster of points
        cluster = KNN.GenerateCluster(numOfPoints);

        //warmup
        for (int i = 0; i < 5; i++)
        {
            //calculate the K-nearest neighbors
            points = KNN.KNearestNeighbors(cluster, point, k);
        }

        //generate a random cluster of points
        cluster = KNN.GenerateCluster(numOfPoints);

        //do the simulation multiple times for testing
        for (int i = 0; i < iters; i++)
        {
            //do the simulation
            double tick = DotMP.Parallel.GetWTime();
            points = KNN.KNearestNeighbors(cluster, point, k);
            double tock = DotMP.Parallel.GetWTime();

            //update min, max, avg
            tock = tock - tick;
            avg += tock;
            min = Math.Min(min, tock);
            max = Math.Max(max, tock);
        }

        avg /= iters;

        //output the results if requested
        if (output)
        {
            Console.WriteLine("Points:");
            foreach (Point p in points)
            {
                Console.WriteLine($"({p.x}, {p.y}, {p.z})");
            }
        }

        //output the results
        Console.WriteLine("Min: {0}", min);
        Console.WriteLine("Max: {0}", max);
        Console.WriteLine("Avg: {0}", avg);

        //exit the program
        return;
    }
}
