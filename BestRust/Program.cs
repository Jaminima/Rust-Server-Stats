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
        static int statsWrittenOut = 0;
        static string fileName = "";
        static Searcher searcher = new Searcher();

        static bool nameIsMatch(string name)
        {
            var namePart = searcher.name.Split(" ");
            return namePart.All(s => name.Contains(s));
        }

        static void Display(int limit = 10)
        {
            int i = 1;
            Console.Clear();
            foreach (var s in serverStats.Where(x=> !searcher.forceHasName || nameIsMatch(x.server.attributes.name)).OrderBy(x => x.stats.timelowPop).ThenByDescending(x=>x.stats.avg).Take(limit))
            {
                Console.WriteLine($"{i} - MAX {s.stats.max} - AVG {s.stats.avg} - Low Pop {s.stats.perclowPop:00.0}% - High Pop {s.stats.perchighPop:00.0}% - {s.server.attributes.name}");
                i++;
            }
            Console.WriteLine($"Pages Searched {searcher.page}");
        }

        static void SaveSearch()
        {
            var r = serverStats.Skip(statsWrittenOut).Where(x => !searcher.forceHasName || nameIsMatch(x.server.attributes.name));
            var rs = r.Select(x => $"{x.server.id},\"{x.server.attributes.name.Trim()}\",{x.stats.max},{x.stats.avg},{x.stats.min},{x.stats.perchighPop},{x.stats.perclowPop}");

            File.AppendAllLines(fileName, rs);

            statsWrittenOut = serverStats.Count;
        }

        static void GetUserInputs()
        {
            Console.WriteLine("Server Name:");
            searcher.name = Console.ReadLine();

            if (searcher.name.Length > 0)
            {
                Console.WriteLine("Force Name Match Y/N:");

                searcher.forceHasName = Console.ReadLine().ToLower() == "y";
            }

            Console.WriteLine("Days To Analyse (Default 30):");

            string days = Console.ReadLine();

            if (int.TryParse(days,out int d) && d>0)
            {
                searcher.historySpan = new TimeSpan(d, 0, 0, 0);
            }
            else
            {
                Console.WriteLine("DefaultTo 30 Days.");
                searcher.historySpan = new TimeSpan(30, 0, 0, 0);
            }
        }

        static void Main(string[] args)
        {
            GetUserInputs();

            Console.WriteLine("Begining Rust Server Analysis");

            fileName = $"Stats-{searcher.name.Replace("/", "_")}-{searcher.historySpan.Days}.csv";

            File.WriteAllText(fileName, "Id,Name,Max,Avg,Min,% High Pop,% Low Pop\n");

            while (searcher.GetPage(out var search))
            {
                foreach (var server in search.data)
                {
                    if (server.GetHistory(searcher.historySpan, out var history))
                    {
                        var stats = history.calculateStats();

                        serverStats.Add(new ServerWithStats() { server = server, stats = stats });
                    }
                }

                SaveSearch();

                Display();
            }

            Display(serverStats.Count);

            Console.WriteLine("Done!");

            Console.ReadLine();
        }
    }
}