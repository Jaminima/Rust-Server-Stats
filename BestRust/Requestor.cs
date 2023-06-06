using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BestRust
{
    internal static class Requestor
    {
        private static HttpClient httpClient = new HttpClient();

        public static bool DoReq<T>(string url, out T result)
        {
            result = default(T);

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

                        return DoReq(url, out result);
                    }
                    else
                    {
                        Console.WriteLine(error.errors[0].detail);
                        return false;
                    }
                }

                result = JsonConvert.DeserializeObject<T>(body);

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
}
