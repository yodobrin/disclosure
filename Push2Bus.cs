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
using Microsoft.Azure.ServiceBus;
using System.Text;


namespace disclosure
{
    public static class Push2Bus
    {
        [FunctionName("Push2Bus")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Push2Bus function processed a request.");

            string ServiceBusConnectionString = Environment.GetEnvironmentVariable("SB_TOPIC_CS");
            string TopicName = Environment.GetEnvironmentVariable("SB_TOPIC_NAME");
            ITopicClient topicClient = new TopicClient(ServiceBusConnectionString, TopicName);
            
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            log.LogInformation($"Push2Bus : got string data: {requestBody}");
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            log.LogInformation("Push2Bus : got dynamic data");
            string scount = data?.messageCount;
            int count = (!string.IsNullOrEmpty(scount))? int.Parse(scount):1;

            log.LogInformation($"Push2Bus will push {count} messages");

            for (int i = 0; i < count; i++)
            {
                string pkey = Guid.NewGuid().ToString();
                data.id = pkey;
                string ttl = data.ttl;
                log.LogInformation($"Push2Bus pushed one message with pkey:{pkey} with ttl:{ttl} in sec");
                string alert = JsonConvert.SerializeObject(data);
                log.LogInformation($"Push2Bus sending to topic {TopicName} the message:{alert}");
                Message message = new Message(Encoding.UTF8.GetBytes(alert));
                TimeSpan span = TimeSpan.FromSeconds(double.Parse(ttl));
                // TimeSpan.f
                message.TimeToLive = span;
                
                message.ContentType = System.Net.Mime.MediaTypeNames.Application.Json;
                
                //message.TimeToLive = new TimeSpan( long.Parse(ttl) * 10^7 );
                await topicClient.SendAsync(message);
                
            }

            string responseMessage = $"sent, {count} new alerts to topic: {TopicName}.";

            return new OkObjectResult(responseMessage);
        }
    }
}
