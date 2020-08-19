# ------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License. See License.txt in the project root for
# license information.
# -------------------------------------------------------------------------



import sys
import os

from azure.servicebus import SubscriptionClient


connection_str = os.environ['SERVICE_BUS_CONNECTION_STR']

if __name__ == '__main__':

    sub_client = SubscriptionClient.from_connection_string(
        connection_str, name="pytopic/Subscriptions/pysub", debug=False)

    with sub_client.get_receiver() as receiver:
        batch = receiver.fetch_next(timeout=10)
        while batch:
            print("Received {} messages".format(len(batch)))
            for message in batch:
                message.complete()
            batch = receiver.fetch_next(timeout=10)
