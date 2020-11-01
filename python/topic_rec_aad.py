'''
Sample Code is provided for the purpose of illustration only and is not intended to be used in a production environment.
THIS SAMPLE CODE AND ANY RELATED INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESSED OR IMPLIED, 
INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
We grant You a nonexclusive, royalty-free right to use and modify the Sample Code and to reproduce and distribute the object code form of the Sample Code, provided that. 
You agree: 
	(i) to not use Our name, logo, or trademarks to market Your software product in which the Sample Code is embedded;
    (ii) to include a valid copyright notice on Your software product in which the Sample Code is embedded; and
	(iii) to indemnify, hold harmless, and defend Us and Our suppliers from and against any claims or lawsuits, including attorneys’ fees, that arise or result from the use or distribution of the Sample Code

// Copyright © Microsoft Corporation.  All Rights Reserved.
// This code released under the terms of the 
// Microsoft Public License (MS-PL, http://opensource.org/licenses/ms-pl.html.)
'''

# ------------------------------------------------------
# NOTE: This code require service-bus package > 7.x
# ------------------------------------------------------


import os
from azure.servicebus import ServiceBusClient, Message
from azure.identity import ClientSecretCredential

#topic & subscription connection details
FULLY_QUALIFIED_NAMESPACE = os.environ['SERVICE_BUS_NAMESPACE']
TOPIC_NAME = os.environ["SERVICE_BUS_TOPIC_NAME"]
SUBSCRIPTION_NAME = os.environ["SERVICE_BUS_SUBSCRIPTION_NAME"]

# credentials details
AZURE_TENANT_ID = os.environ["AZURE_TENANT_ID"]
AZURE_CLIENT_SECRET = os.environ["AZURE_CLIENT_SECRET"]
AZURE_CLIENT_ID = os.environ["AZURE_CLIENT_ID"]

credential = ClientSecretCredential(tenant_id=AZURE_TENANT_ID,
                                    client_id=AZURE_CLIENT_ID,
                                    client_secret=AZURE_CLIENT_SECRET)

servicebus_client = ServiceBusClient(FULLY_QUALIFIED_NAMESPACE, credential)

receiver = servicebus_client.get_subscription_receiver(
            topic_name=TOPIC_NAME,
            subscription_name=SUBSCRIPTION_NAME        
        )

with receiver:
       for message in receiver.get_streaming_message_iter():
           print(str(message))
           message.complete()
