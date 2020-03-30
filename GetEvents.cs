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
    public static class GetEvents
    {
        [FunctionName("GetEvents")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("GetEvents function processed a request.");
            // redis
            string connectionString = Environment.GetEnvironmentVariable("REDIS_CONN");
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(connectionString);
            var endpoints = redis.GetEndPoints();
            // add check on the array length
            var server = redis.GetServer(endpoints[0]);
            IDatabase cache = redis.GetDatabase();            
            
            var keys = server.Keys(cache.Database);
            
            StringWriter response = new StringWriter();
            response.WriteLine("Got the following from cache:");
            foreach (var key in keys)
            {
                response.WriteLine(cache.StringGet(key).ToString());
            }
                    
            string responseMessage = response.ToString();
            response.Flush();
            return new OkObjectResult(responseMessage);
        }
    }
}
