using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Linq;

namespace FunctionAppStudy2
{
    public static class GetClientId
    {
        private static readonly ConcurrentQueue<int> clientIdsQueue = GetQueueWithUniqueClientIds(200_000, 1_000_000, 3_000_000);

        [FunctionName("GetClientId")]
        public static async Task<Response> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {

            if (!clientIdsQueue.TryDequeue(out int clientId))
                return await Task.FromResult(new Response() { Success = false, ErrorMessage = "Something went wrong. Please contact your system administrator" });

            return await Task.FromResult(new Response() { Success = true, ClientId = clientId, QueueCount = clientIdsQueue.Count });
        }
        private static ConcurrentQueue<int> GetQueueWithUniqueClientIds(int count, int minValue, int maxValue)
        {
            var rnd = new Random();

            if (count > (maxValue - minValue))
                throw new ArgumentException("maxValue minus minValue should be less or equal to count");

            var temporaryList = Enumerable.Range(minValue, maxValue + 1)
                                          .OrderBy(x => rnd.Next())
                                          .Take(count);

            var temporaryQueue = new ConcurrentQueue<int>();
            Parallel.ForEach(temporaryList, x => temporaryQueue.Enqueue(x));

            return temporaryQueue;
        }
    }

    public class Response
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public int? ClientId { get; set; }
        public int QueueCount { get; set; }
    }
}
