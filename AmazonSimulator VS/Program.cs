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
            //test();
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();


        public class Test
        {
            private bool _member;
            public bool Val { get => _member; }
            public Test(out bool handle)
            {
                handle = _member;
            }
        }

        public static void test()
        {
            bool handle;
            Test t = new Test(out handle);

            Console.WriteLine("Value of _member: {0}, handle: {1}", handle, t.Val);

        }
    }
}
