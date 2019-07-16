using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using System.Linq;
using RequestorUtilities;
using System.Net;

namespace ThatWhichSmashesFunctionApp
{
    public static class ThatWhichSmashesFunction
    {
        private static readonly HttpClient _httpClient = new HttpClient(new HttpClientHandler()
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
        });

        [FunctionName("ThatWhichSmashesFunction")]
        public static async Task Run([QueueTrigger("hammer-requests-queue", Connection = "FunctionConnectionString")]string queuedRequest, ILogger log)
        {
            var queuedItem = Newtonsoft.Json.JsonConvert.DeserializeObject<QueuedRequest>(queuedRequest);

            if (queuedItem?.CheckIsValid() != true)
            {
                log.LogError($"Queued item not successfully deserialized to type {typeof(QueuedRequest)} from json string: {queuedRequest}");
                return;
            }

            var initialResponse = await SendAndEnsureResponseFromInitialPageAsync(queuedItem.InitialUrl);
            var sessionHeaders = LoadSessionHeadersFromInitialResposne(initialResponse);

            var delayTimespan = TimeSpan.FromMilliseconds(queuedItem.TimeBetweenRequestsInMs);

            foreach (var extraRequest in queuedItem.ExtraRequests)
            {
                await Task.Delay(delayTimespan);

                var requestMessage = CreateRequest(extraRequest, sessionHeaders);
                var response = await _httpClient.SendAsync(requestMessage);
                _ = response.EnsureSuccessStatusCode();

                var responseString = await response.Content.ReadAsStringAsync();
            }
        }

        private static HttpRequestMessage CreateRequest(ExtraQueuedRequest queuedRequest, ImmutableList<(string Key, string Value)> sessionHeaders)
        {
            var httpMethod = GetHttpMethod(queuedRequest.Method);
            var request = new HttpRequestMessage(httpMethod, queuedRequest.FullEndpoint);

            foreach (var header in sessionHeaders)
            {
                request.Headers.Add(header.Key, header.Value);
            }

            if (!string.IsNullOrEmpty(queuedRequest.BodyJson))
            {
                request.Headers.Add("Accept", "application/json, text/javascript, */*");
                request.Content = new StringContent(queuedRequest.BodyJson, Encoding.UTF8, "application/json");
            }

            return request;
        }

        private static HttpMethod GetHttpMethod(string method)
        {
            if (string.Equals(method, "POST", StringComparison.OrdinalIgnoreCase))
            {
                return HttpMethod.Post;
            }

            return HttpMethod.Get;
        }

        private static ImmutableList<(string Key, string Value)> LoadSessionHeadersFromInitialResposne(HttpResponseMessage initialResponse)
        {
            var newRequestHeaders = new List<(string, string)>();

            if (initialResponse.Headers.Contains("Set-Cookie"))
            {
                var cookieHeaderBuilder = new StringBuilder();
                var cookieHeaders = initialResponse.Headers.GetValues("Set-Cookie");
                foreach (var cookieHeader in cookieHeaders)
                {
                    var cookie = cookieHeader.Split(';').First();
                    _ = cookieHeaderBuilder.Append(cookie + "; ");
                }
                newRequestHeaders.Add(("Cookie", cookieHeaderBuilder.ToString()));
            }

            newRequestHeaders.Add(("Accept-Language", "en-US,en;q=0.5"));
            newRequestHeaders.Add(("Accept-Encoding", "gzip, deflate, br"));
            newRequestHeaders.Add(("X-Requested-With", "XMLHttpRequest"));
            newRequestHeaders.Add(("Connection", "keep-alive"));

            return newRequestHeaders.ToImmutableList();
        }

        private static async Task<HttpResponseMessage> SendAndEnsureResponseFromInitialPageAsync(string initialUrl)
        {
            var message = new HttpRequestMessage(HttpMethod.Get, initialUrl);
            var response = await _httpClient.SendAsync(message);
            _ = response.EnsureSuccessStatusCode();

            return response;
        }
    }
}
