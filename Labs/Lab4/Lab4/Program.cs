using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text.Encodings.Web;
using Lab4.implementations;
using Lab4.utils;
using Microsoft.VisualBasic;

namespace Lab4
{
    class Program
    {
        static void Main(string[] args)
        {
            List<String> hostList = new[]
            {
                "www.wikipedia.org/wiki/Computer_program/"
                // "www.cs.ubbcluj.ro/~rlupsa/edu/pdp/lab-4-futures-continuations.html",
                // "www.cs.ubbcluj.ro/~rlupsa/edu/pdp/lab-5-parallel-algo.html",
                // "www.cs.ubbcluj.ro/~rlupsa/edu/pdp/lab-6-parallel-algo-2.html"
            }.ToList();

            var task = new CallbackImplementation();
            // var task = new NAsyncTaskImplementation();
            // var task = new AsyncTaskImplementation();

            task.Run(hostList);
        }
    }
}