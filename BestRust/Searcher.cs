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
            get { return _name; }
            set { _name = value; page = 0; }
        }

        public bool forceHasName = false;

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
                    var error = JsonConvert.DeserializeObject<Errors>(body);
                    if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                    {
                        var till = error.errors[0].meta.tryAgain - DateTime.Now;
                        till = till.Add(new TimeSpan(1, 0, 0));

                        Console.WriteLine($"Waiting {till.TotalSeconds:00}");
                        Thread.Sleep((int)till.TotalMilliseconds);

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
    
    internal struct Errors
    {
        public Error[] errors;
    }

    internal struct Error
    {
        public string code, title, detail;
        public ErrorMeta meta;
    }

    internal struct ErrorMeta
    {
        public DateTime tryAgain;
    }

    internal class SearchResult
    {
        public Server[] data;
    }
}
