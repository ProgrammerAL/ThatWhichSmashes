using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Queue;
using Newtonsoft.Json;

namespace RequestQueueAdder
{
    class Program
    {
        static async Task Main()
        {
            var queueConnectionStrings = LoadConnectionsStrings();
            if (queueConnectionStrings.Length == 0)
            {
                throw new Exception($"{nameof(queueConnectionStrings)} needs to be set. Grab it from the 'Access keys' section of the already-created queue in Azure");
            }

            var queueMessages = LoadQueueMessages();

            if (queueMessages.Length == 0)
            {
                throw new Exception($"{nameof(queueMessages)} is empty");
            }

            foreach (var connectionString in queueConnectionStrings)
            {
                var storageAccount = CloudStorageAccount.Parse(connectionString);

                var queueClient = storageAccount.CreateCloudQueueClient();
                var queue = queueClient.GetQueueReference("hammer-requests-queue");

                foreach (var requestToQueue in queueMessages)
                {
                    for (int i = 0; i < requestToQueue.Count; i++)
                    {
                        var message = new CloudQueueMessage(requestToQueue.JsonMessage, isBase64Encoded: false);
                        await queue.AddMessageAsync(message);
                    }
                }
            }
        }

        private static Request[] LoadQueueMessages()
        {
            var requestsJson = File.ReadAllText("./data/Requests.json");
            return JsonConvert.DeserializeObject<Request[]>(requestsJson);
        }

        private static string[] LoadConnectionsStrings()
        {
            var connectionStringsJson = File.ReadAllText("./data/QueueEndpoints.json");
            return JsonConvert.DeserializeObject<string[]>(connectionStringsJson);
        }
    }
}
