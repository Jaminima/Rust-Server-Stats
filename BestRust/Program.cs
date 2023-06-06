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

        static bool nameIsMatch(string name)
        {
            var namePart = searcher.name.Split(" ");
            return namePart.All(s => name.Contains(s));
        }

        static void Display(int limit = 10)
        {
            int i = 0;
            Console.Clear();
            foreach (var s in serverStats.Where(x=> !searcher.forceHasName || nameIsMatch(x.server.attributes.name)).OrderBy(x => x.stats.timelowPop).ThenByDescending(x=>x.stats.avg).Take(limit))
            {
                Console.WriteLine($"{i} - MAX {s.stats.max} - AVG {s.stats.avg} - Low Pop {s.stats.perclowPop:00.0}% - High Pop {s.stats.perchighPop:00.0}% - {s.server.attributes.name}");
                i++;
            }
            Console.WriteLine($"Pages Searched {searcher.page}");
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var t = new TimeSpan(30, 0, 0, 0);
            searcher.name = "EU 2x";
            searcher.forceHasName = true;

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

                Display();
            }

            Display(serverStats.Count);

            Console.WriteLine("Done!");

            Console.ReadLine();
        }
    }
}