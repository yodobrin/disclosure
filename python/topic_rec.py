# ------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License. See License.txt in the project root for
# license information.
# -------------------------------------------------------------------------



import sys
import os

from azure.servicebus import SubscriptionClient

CONNECTION_STR = os.environ['SERVICE_BUS_CONNECTION_STR']
TOPIC_NAME = os.environ["SERVICE_BUS_TOPIC_NAME"]
SUBSCRIPTION_NAME = os.environ["SERVICE_BUS_SUBSCRIPTION_NAME"]


if __name__ == '__main__':

    sub_client = SubscriptionClient.from_connection_string(
        CONNECTION_STR, name=SUBSCRIPTION_NAME,topic=TOPIC_NAME, debug=False)
    
    print("Got subscription clinet for " + TOPIC_NAME + " using the " + SUBSCRIPTION_NAME +" subscription" )

    with sub_client.get_receiver() as receiver:
        # using no value for the timeout, allow for an active receiver 
        batch = receiver.fetch_next()
        while batch:
            print("Received {} messages".format(len(batch)))
            for message in batch:
                message.complete()
                print(message)
            batch = receiver.fetch_next()
