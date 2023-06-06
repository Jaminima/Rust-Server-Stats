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

            return Requestor.DoReq(url, out result);
        }
    }

    internal struct ServerAttributes
    {
        public string name, address, ip;
        public int port, rank;
    }
}
