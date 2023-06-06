using System;
using System.Security.Cryptography.X509Certificates;

namespace BestRust // Note: actual namespace depends on the project name.
{
    internal class Program
    {
        internal struct ServerWithStats
        {
            public Server server;
            public HistoryStats stats;
        }

        static List<ServerWithStats> serverStats = new List<ServerWithStats>();
        static Searcher searcher = new Searcher();

        static void DisplayTop()
        {
            int i = 0;
            Console.Clear();
            foreach (var s in serverStats.OrderByDescending(x => x.stats.avg).Take(10))
            {
                Console.WriteLine($"{i} - MAX {s.stats.max} - AVG {s.stats.avg} - {s.server.attributes.name}");
                i++;
            }
            Console.WriteLine($"Pages Searched {searcher.page}");
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var t = new TimeSpan(30, 0, 0, 0);
            searcher.name = "2x";

            while (searcher.GetPage(out var search))
            {
                foreach (var server in search.data)
                {
                    if (server.GetHistory(t, out var history))
                    {
                        var stats = history.calculateStats();

                        serverStats.Add(new ServerWithStats() { server = server, stats = stats });
                    }
                }

                DisplayTop();
            }

            Console.ReadLine();
        }
    }
}