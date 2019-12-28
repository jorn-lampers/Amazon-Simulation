using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            //BuildWebHost(args).Run();
            testDijkstra();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();

        public static void testDijkstra()
        {
            Vertex a = new Vertex(0.0, 0.0, 0.0);
            Vertex b = new Vertex(100.0, 0.0, 0.0);
            Vertex c = new Vertex(1.0, 0.0, 0.0);
            Vertex d = new Vertex(8.0, 0.0, 0.0);
            Vertex e = new Vertex(5.0, 0.0, 0.0);

            Edge u = new Edge(a, b);
            Edge v = new Edge(a, c);

            Edge w = new Edge(b, d);
            Edge x = new Edge(c, d);

            Edge y = new Edge(d, e);

            Vertex testV = new Vertex(6.0, 0.0, 0.0);
            Edge testEA = new Edge(testV, a);
            Edge testEB = new Edge(testV, e);

            List<Vertex> vertices = new List<Vertex>();
            vertices.Add(a); vertices.Add(b); vertices.Add(c); vertices.Add(d); vertices.Add(e);

            List<Edge> edges = new List<Edge>();
            edges.Add(u); edges.Add(v); edges.Add(w); edges.Add(x); edges.Add(y);

            vertices.Add(testV); edges.Add(testEA); edges.Add(testEB);

            Graph g = new Graph(vertices, edges);

            Vertex source = a;
            Vertex destination = e;

            Dictionary<Vertex, Tuple<double, Vertex>> dist = g.dijkstraDists(source, destination);
            List<Vertex> path = g.dijkstraShortestPath(source, destination);

            Console.WriteLine(dist);
        }
    }
}
