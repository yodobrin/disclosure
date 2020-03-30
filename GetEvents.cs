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
using StackExchange.Redis;


namespace disclosure
{
    public static class GetEvents
    {
        [FunctionName("GetEvents")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
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
