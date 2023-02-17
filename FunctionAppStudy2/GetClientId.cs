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
        private static ConcurrentQueue<int> GetQueueWithUniqueClientIds(int takeCount, int minValue, int rangeCount)
        {
            var rnd = new Random();

            if (takeCount > (minValue + rangeCount))
                throw new ArgumentException("minValue plus rangeCount should be less or equal to takeCount");

            var temporaryList = Enumerable.Range(minValue, rangeCount + 1)
                                          .OrderBy(x => rnd.Next())
                                          .Take(takeCount);

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
