using Newtonsoft.Json;

namespace BestRust
{
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

    internal struct ServerAttributes
    {
        public string name, address, ip;
        public int port, rank;
    }
}
