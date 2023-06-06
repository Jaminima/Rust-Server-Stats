﻿using System;
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
}
