using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Http;
using System.Xml.Linq;

namespace BestRust
{
    internal class Searcher
    {
        public string name
        {
            set { _name = value; page = 0; }
        }

        private string _name = "";

        public int page = 0;
        private int pageStep = 10;

        private SearchResult? last;

        private HttpClient httpClient = new HttpClient();

        public bool GetPage(out SearchResult? result)
        {
            result = null;

            string url = "";
                
            if (_name.Length > 0)
            {
                url = $"https://api.battlemetrics.com/servers?page[offset]={page * pageStep}&page[rel]=next&sort=rank&fields[server]=rank,name,players,maxPlayers,address,ip,port,country,location,details,status&relations[server]=game,serverGroup&filter[game]=rust&filter[search]={_name}&filter[status]=online";
            }
            else if (last != null)
            {
                url = $"https://api.battlemetrics.com/servers?page[key]={page * pageStep},{last.data.Last().id}&page[rel]=next&sort=rank&fields[server]=rank,name,players,maxPlayers,address,ip,port,country,location,details,status&relations[server]=game,serverGroup&filter[game]=rust&filter[status]=online";
            }
            else
            {
                url = "https://api.battlemetrics.com/servers?fields[server]=rank,name,players,maxPlayers,address,ip,port,country,location,details,status&relations[server]=game,serverGroup&filter[game]=rust";
            }

            using (var request = new HttpRequestMessage(new HttpMethod("GET"), url))
            {
                var response = httpClient.Send(request);

                var contentTask = response.Content.ReadAsStringAsync();
                contentTask.Wait();

                var body = contentTask.Result;

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                    {
                        Console.WriteLine("Waiting 60");
                        Thread.Sleep(60000);
                        return GetPage(out result);
                    }
                    else return false;
                }

                result = JsonConvert.DeserializeObject<SearchResult>(body);
                last = result;

                page++;

                return true;
            }
        }
    }

    internal class SearchResult
    {
        public Server[] data;
    }

    internal class Server
    {
        public string id;
        public ServerAttributes attributes;

        public bool GetHistory(TimeSpan period, out PlayerHistory? result)
        {
            result = null;

            var now = DateTime.Now;
            var then = now - period;

            string url = $"https://api.battlemetrics.com/servers/{id}/player-count-history?start={then:yyyy-MM-dd}T00:00:00.000Z&stop={now:yyyy-MM-dd}T00:00:00.000Z&resolution=60";

            using (var httpClient = new HttpClient())
            {
                using (var request = new HttpRequestMessage(new HttpMethod("GET"), url))
                {
                    var response = httpClient.Send(request);

                    if (!response.IsSuccessStatusCode)
                        return false;

                    var contentTask = response.Content.ReadAsStringAsync();
                    contentTask.Wait();

                    var body = contentTask.Result;
                    result = JsonConvert.DeserializeObject<PlayerHistory>(body);

                    return true;
                }
            }
        }
    }

    internal class PlayerHistory
    {
        public DataPoint[] data;

        public HistoryStats calculateStats()
        {
            var stats = new HistoryStats();

            foreach (var item in data)
            {
                stats.avg += item.attributes.value;

                if (stats.min > item.attributes.min) stats.min = item.attributes.min;

                if (stats.max < item.attributes.max) stats.max = item.attributes.max;
            }

            stats.avg /= data.Length;

            return stats;
        }
    }

    internal class HistoryStats
    {
        public int avg = 0;
        public int max = 0, min = 10000;
    }

    internal struct DataPoint
    {
        public string type;
        public DataAttribute attributes;
    }

    internal struct DataAttribute
    {
        public DateTime timestamp;
        public int max, value, min;
    }

    internal struct ServerAttributes
    {
        public string name, address, ip;
        public int port, rank;
    }
}
