using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Models;

namespace AmazonSimulator_VS
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
            //testDijkstra();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();

        public static void testDijkstra()
        {
            Vector3 a = new Vector3(0.0f, 0.0f, 0.0f);
            Vector3 b = new Vector3(100.0f, 0.0f, 0.0f);
            Vector3 c = new Vector3(1.0f, 0.0f, 0.0f);
            Vector3 d = new Vector3(8.0f, 0.0f, 0.0f);
            Vector3 e = new Vector3(5.0f, 0.0f, 0.0f);

            Edge u = new Edge(a, b);
            Edge v = new Edge(a, c);

            Edge w = new Edge(b, d);
            Edge x = new Edge(c, d);

            Edge y = new Edge(d, e);

            Vector3 testV = new Vector3(6.0f, 0.0f, 0.0f);
            Edge testEA = new Edge(testV, a);
            Edge testEB = new Edge(testV, e);

            List<Vector3> vertices = new List<Vector3>();
            vertices.Add(a); vertices.Add(b); vertices.Add(c); vertices.Add(d); vertices.Add(e);

            List<Edge> edges = new List<Edge>();
            edges.Add(u); edges.Add(v); edges.Add(w); edges.Add(x); edges.Add(y);

            vertices.Add(testV); edges.Add(testEA); edges.Add(testEB);

            Graph g = new Graph(vertices, edges);

            Vector3 source = a;
            Vector3 destination = e;

            Dictionary<Vector3, Tuple<double, Vector3>> dist = g.dijkstraDists(source, destination);
            List<Vector3> path = g.dijkstraShortestPath(source, destination);

            Console.WriteLine(dist);
        }
    }
}
