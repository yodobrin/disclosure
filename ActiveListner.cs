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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Identity.Client;
using Microsoft.Azure.ServiceBus.Primitives;
using Microsoft.Azure.ServiceBus;
    // using Microsoft.Azure.ServiceBus.InteropExtensions;
    
    
namespace disclosure
{
    class ActiveListner
    {

        // It is advisable to use configuration settings and not code the following parameters 
        // Environment.GetEnvironmentVariable("<specific setting key>");
        const string TopicName = "<your topic name>>";
        const string SubscriptionName = "<the subscription topic you need to listen to>";       
        const string clientId = "<Your service principal id>";
        const string clientSecret = "<Your spn secret>";
        const string TenantId = "<your tennant id>";
        const string endpoint = "sb://{replace with your namespace}.servicebus.windows.net";

        static ISubscriptionClient subscriptionClient;

        public static async Task Main(string[] args)
        {    
            // following parameters are required for creating a subscription client
            TransportType transportType = TransportType.Amqp;
            ReceiveMode receiveMode = ReceiveMode.PeekLock;
            RetryPolicy retryPolicy = null;
            TokenProvider aadTokenProvider = ClientCredentialsScenario(clientId,clientSecret,TenantId);

            subscriptionClient = new SubscriptionClient(endpoint,TopicName,SubscriptionName,aadTokenProvider,transportType,receiveMode ,retryPolicy );

            Console.WriteLine("======================================================");
            Console.WriteLine("Press ENTER key to exit after receiving all the messages.");
            Console.WriteLine("======================================================");

            // Register subscription message handler and receive messages in a loop
            RegisterOnMessageHandlerAndReceiveMessages();

            Console.ReadKey();

            await subscriptionClient.CloseAsync();    
        }

        static void RegisterOnMessageHandlerAndReceiveMessages()
        {
            // Configure the message handler options in terms of exception handling, number of concurrent messages to deliver, etc.
            var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
            {
                // Maximum number of concurrent calls to the callback ProcessMessagesAsync(), set to 1 for simplicity.
                // Set it according to how many messages the application wants to process in parallel.
                MaxConcurrentCalls = 1,

                // Indicates whether MessagePump should automatically complete the messages after returning from User Callback.
                // False below indicates the Complete will be handled by the User Callback as in `ProcessMessagesAsync` below.
                AutoComplete = false
            };

            // Register the function that processes messages.
            subscriptionClient.RegisterMessageHandler(ProcessMessagesAsync, messageHandlerOptions);
        }

        static async Task ProcessMessagesAsync(Message message, CancellationToken token)
        {
            // Process the message.
            Console.WriteLine($"Received message: SequenceNumber:{message.SystemProperties.SequenceNumber} Body:{Encoding.UTF8.GetString(message.Body)}");

            // Complete the message so that it is not received again.
            // This can be done only if the subscriptionClient is created in ReceiveMode.PeekLock mode (which is the default).
            await subscriptionClient.CompleteAsync(message.SystemProperties.LockToken);
 
            // Note: Use the cancellationToken passed as necessary to determine if the subscriptionClient has already been closed.
            // If subscriptionClient has already been closed, you can choose to not call CompleteAsync() or AbandonAsync() etc.
            // to avoid unnecessary exceptions.
        }

        static Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            Console.WriteLine($"Message handler encountered an exception {exceptionReceivedEventArgs.Exception}.");
            var context = exceptionReceivedEventArgs.ExceptionReceivedContext;
            Console.WriteLine("Exception context for troubleshooting:");
            Console.WriteLine($"- Endpoint: {context.Endpoint}");
            Console.WriteLine($"- Entity Path: {context.EntityPath}");
            Console.WriteLine($"- Executing Action: {context.Action}");
            return Task.CompletedTask;
        }
        static TokenProvider ClientCredentialsScenario(string ClientId, string clientSecret, string TenantId)
        {
            TokenProvider aadTokenProvider = TokenProvider.CreateAzureActiveDirectoryTokenProvider
                (async (audience, authority, state) =>
            {
                IConfidentialClientApplication app = ConfidentialClientApplicationBuilder.Create(ClientId)
                                .WithAuthority(authority)
                                .WithClientSecret(clientSecret)
                                .Build();

                var serviceBusAudience = new Uri("https://servicebus.azure.net");

                AuthenticationResult authResult = await app.AcquireTokenForClient(new string[] { $"{serviceBusAudience}/.default" }).ExecuteAsync();
                return authResult.AccessToken;

            }, $"https://login.windows.net/{TenantId}");

            return aadTokenProvider;

        }

    }
}