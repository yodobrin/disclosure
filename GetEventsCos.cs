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
using System.Collections.Generic;

namespace disclosure
{
    public static class GetEventsCos
    {
        [FunctionName("GetEventsCos")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("GetEventsCos processed a request.");

            string DatabaseName = Environment.GetEnvironmentVariable("COSMOS_DB_NAME");
            string CollectionName = Environment.GetEnvironmentVariable("COSMOS_COLLECTION");
            string ConnectionStringSetting = Environment.GetEnvironmentVariable("COSMOS_CS");

            CosmosClient cosmosClient = new CosmosClient(ConnectionStringSetting);
            Container cosmosContainer = cosmosClient.GetContainer(DatabaseName,CollectionName);

            var sqlQueryText = $"SELECT * FROM c";
            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            FeedIterator<object> queryResultSetIterator = cosmosContainer.GetItemQueryIterator<object>(queryDefinition);

            // List<object> alerts = new List<object>();
            StringWriter response = new StringWriter();
            while (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<object> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                int count = currentResultSet.Count;
                if(count == 0)
                {
                    return new NotFoundObjectResult($"No active alerts");
                }

                response.WriteLine($"Got {count} alerts with the following from cache:\n");
                foreach (object alert in currentResultSet)
                {
                    // alerts.Add(alert);
                    log.LogInformation($"alert: {alert.ToString()}");
                    response.WriteLine(alert.ToString()+ "\n");
                }
            }
            
            response.Flush();
            cosmosClient.Dispose();
            return new OkObjectResult(response.ToString());
        }
    }
}
