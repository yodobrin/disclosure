using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace disclosure
{
    public static class PublishEvent
    {
        [FunctionName("PublishEvent")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("PublishEvent function processed a request.");
            
            string connectionString = Environment.GetEnvironmentVariable("REDIS_CONN");
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(connectionString);
            IDatabase cache = redis.GetDatabase();

            string defaultTTL = Environment.GetEnvironmentVariable("DEFAULT_TTL");
            int DefTTL = int.Parse(defaultTTL);

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            string alertMessage = data?.alertMessage;
            string sttl = data?.ttl;
            int ttl = (string.IsNullOrEmpty(sttl))? DefTTL: int.Parse(sttl);
            string uid = Guid.NewGuid().ToString();
            TimeSpan t = new TimeSpan(0,0,ttl);
            cache.StringSet(uid,alertMessage,t);

            return new OkObjectResult("");
        }
    }
}
