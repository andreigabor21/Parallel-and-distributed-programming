using System;
using System.Linq;
using lab4.impl;

namespace Lab42
{
    class Program
    {
        static void Main(string[] args)
        {
            var hosts = new []
            {
                "www.cs.ubbcluj.ro/~rlupsa/edu/pdp/lab-1-noncooperative-mt.html",
                "www.cs.ubbcluj.ro/~rlupsa/edu/pdp/lab-2-producer-consumer.html",
                "www.cs.ubbcluj.ro/~rlupsa/edu/pdp/lab-3-parallel-simple.html",
                "www.cs.ubbcluj.ro/~rlupsa/edu/pdp/lab-4-futures-continuations.html"
                
            }.ToList();

            // CallbackSession.Run(hosts);
            TaskSession.Run(hosts, true);
        }
    }
}