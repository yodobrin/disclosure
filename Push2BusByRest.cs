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

using System.Net.Http.Headers; 
using System.Net.Http;
using System.Runtime.Serialization;


namespace disclosure
{
    public static class Push2BusByRest
    {
        [FunctionName("Push2BusByRest")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Push2BusByRest function processed a request.");
            
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            log.LogInformation($"Push2BusByRest : got string data: {requestBody}");
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            log.LogInformation("Push2BusByRest : got dynamic data");
            string scount = data?.messageCount;
            string TopicName = data?.TopicName;
            int count = (!string.IsNullOrEmpty(scount))? int.Parse(scount):1;

            log.LogInformation($"Push2BusByRest will push {count} messages");
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("SB_URL"));
            

            dynamic brokerMesasages = new System.Dynamic.ExpandoObject();
            // getting the ttl from the incoming message - replace by any other logic
            brokerMesasages.TimeToLive = data.ttl;
            // add any other properties like priority etc.

            // adding required headers
            client.DefaultRequestHeaders.TryAddWithoutValidation( "Content-Type", "application/atom+xml; charset=utf-8");
            client.DefaultRequestHeaders.TryAddWithoutValidation( "BrokerProperties", JsonConvert.SerializeObject(brokerMesasages));
            client.DefaultRequestHeaders.TryAddWithoutValidation( "Authorization", Environment.GetEnvironmentVariable("SB_TOKEN"));
            

            

            for (int i = 0; i < count; i++)
            {
                string pkey = Guid.NewGuid().ToString();
                data.id = pkey;
                string ttl = data.ttl;
                log.LogInformation($"Push2BusByRest sending one message with pkey:{pkey} with ttl:{ttl} in sec");
                string alert = JsonConvert.SerializeObject(data);

                string sburi = $"/{TopicName}/messages";

                StringContent salert = new StringContent(alert);
                HttpResponseMessage response = await client.PostAsync(sburi,salert);   
                string responseBody = await response.Content.ReadAsStringAsync();
                log.LogInformation($"Push2BusByRest sent message via rest.");
                
            }

            string responseMessage = $"sent, {count} new alerts to topic: {TopicName}.";

            return new OkObjectResult(responseMessage);
        }
    }
}
