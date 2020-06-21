using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Cosmos;


namespace disclosure
{
    public static class Push2Cosmos
    {
        [FunctionName("Push2Cosmos")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Push2Cosmos processed a request.");

            string DatabaseName = Environment.GetEnvironmentVariable("COSMOS_DB_NAME");
            string CollectionName = Environment.GetEnvironmentVariable("COSMOS_COLLECTION");
            string ConnectionStringSetting = Environment.GetEnvironmentVariable("COSMOS_CS");

            CosmosClient cosmosClient = new CosmosClient(ConnectionStringSetting);
            Container cosmosContainer = cosmosClient.GetContainer(DatabaseName,CollectionName);

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            log.LogInformation($"Push2Cosmos : got string data: {requestBody}");
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            log.LogInformation("Push2Cosmos : got dynamic data");
            string scount = data?.messageCount;
            int count = (!string.IsNullOrEmpty(scount))? int.Parse(scount):1;
            log.LogInformation($"Push2Cosmos will push {count} messages");
            for (int i = 0; i < count; i++)
            {
                string pkey = Guid.NewGuid().ToString();
                data.id = pkey;            
                log.LogInformation($"Push2Cosmos pushed one message with pkey:{pkey}");
                ItemResponse<object> alertResponse = await cosmosContainer.CreateItemAsync<object>(data, new PartitionKey(pkey));                
            }

            cosmosClient.Dispose();
            return new OkObjectResult($" {count} message(s) pushed");
        }
    }
}
