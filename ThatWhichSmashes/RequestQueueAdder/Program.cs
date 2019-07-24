using System;
using System.IO;
using System.Linq;
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

            Console.WriteLine($"Starting -- {DateTime.Now}{Environment.NewLine}");
            foreach (var connectionString in queueConnectionStrings)
            {
                var storageAccount = CloudStorageAccount.Parse(connectionString);

                var queueClient = storageAccount.CreateCloudQueueClient();
                var queue = queueClient.GetQueueReference("tws-requests-queue");

                Console.WriteLine($"Starting -- {DateTime.Now}{Environment.NewLine}\t{connectionString}{Environment.NewLine}");

                foreach (var requestToQueue in queueMessages)
                {
                    Console.WriteLine($"\tAdding message {requestToQueue.Name}");

                    var message = new CloudQueueMessage(requestToQueue.JsonMessage, isBase64Encoded: false);
                    var addMessageTasks = Enumerable.Range(1, requestToQueue.Count).Select(x => queue.AddMessageAsync(message)).ToArray();
                    Task.WaitAll(addMessageTasks);
                }

                queue.FetchAttributes();
                Console.WriteLine($"Completed adding {queue.ApproximateMessageCount} messages -- {DateTime.Now} for {Environment.NewLine}\t{connectionString}{Environment.NewLine}");
            }

            Console.WriteLine($"Completed All -- {DateTime.Now}");
            await Task.CompletedTask;
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
