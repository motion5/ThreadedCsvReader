using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace ThreadedCsvReader.Web
{
    public class WebClient
    {
        private readonly HttpClient httpClient;

        public WebClient(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<Stream> GetCsvStream()
        {
            const string uri =
                "https://ark-funds.com/wp-content/fundsiteliterature/csv/ARK_INNOVATION_ETF_ARKK_HOLDINGS.csv";

            using var requestMessage = new HttpRequestMessage(HttpMethod.Get, new Uri(uri));
            //requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
            requestMessage.Headers.UserAgent.TryParseAdd("Mozilla/5.0 (Macintosh; Intel Mac OS X 11_2_3) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/89.0.4389.90 Safari/537.36");

            var response = await httpClient.SendAsync(requestMessage);
            
            Console.WriteLine(response.ToString());

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStreamAsync();
        }
    }
}