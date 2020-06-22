/*
Sample Code is provided for the purpose of illustration only and is not intended to be used in a production environment.
THIS SAMPLE CODE AND ANY RELATED INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESSED OR IMPLIED, 
INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
We grant You a nonexclusive, royalty-free right to use and modify the Sample Code and to reproduce and distribute the object code form of the Sample Code, provided that. 
You agree: 
	(i) to not use Our name, logo, or trademarks to market Your software product in which the Sample Code is embedded;
    (ii) to include a valid copyright notice on Your software product in which the Sample Code is embedded; and
	(iii) to indemnify, hold harmless, and defend Us and Our suppliers from and against any claims or lawsuits, including attorneys’ fees, that arise or result from the use or distribution of the Sample Code
**/

// Copyright © Microsoft Corporation.  All Rights Reserved.
// This code released under the terms of the 
// Microsoft Public License (MS-PL, http://opensource.org/licenses/ms-pl.html.)

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
            [HttpTrigger(AuthorizationLevel.Function, "post", "get", Route = null)] HttpRequest req,
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
                    return new OkObjectResult($"No active alerts");
                }

                response.WriteLine($"Got {count} alerts with the following from cache:\n");
                foreach (object alert in currentResultSet)
                {
                    // alerts.Add(alert);
                    // log.LogInformation($"alert: {alert.ToString()}");
                    response.WriteLine(alert.ToString()+ "\n");
                }
            }
            
            response.Flush();
            cosmosClient.Dispose();
            return new OkObjectResult(response.ToString());
        }
    }
}
