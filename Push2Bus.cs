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
