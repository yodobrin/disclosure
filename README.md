# disclosure
sample code for publish and consume rapid event flow


## Architecture

![High Level Overview](https://user-images.githubusercontent.com/37622785/77907340-2634af00-7292-11ea-9236-36d3b828774c.png)

### Security Aspects
When using serverless compute, we need to reflect on the multiple attack vectors, code vulnerabilities. 
To ensure no public end-point are exposed, there are few options to deploy a function app, however in this repository, these more restrictive and more secured topologies are not covered. The reason is cost. This repo will showcase a very slim security posture.

#### Function App
Using two function apps, and two distinct plans, the reason is IP filtering is set on the plan level. In our case there are two types of restrictions, the publisher, origin is known, it will either be connected through a VPN or ExpressRoute, or specific set of IPs, while the consuming applications origin is unknown.
The internet facing function app, should have IP restriction of the WAF only.
The internal facing function app, should have only the IP(s) of the on-prem, or specific hosted assets.

Restricting traffic via IP configuration - see [documentation](https://docs.microsoft.com/en-us/azure/app-service/app-service-ip-restrictions)
further more, to avoid attack which will lead to high compute load (and cost) we can limit the concurrency of the functions using the host.json:
``` json
"extensions": {
    "http": {
      "routePrefix": "api",
      "maxOutstandingRequests": 200,
      "maxConcurrentRequests": 1,
      "dynamicThrottlesEnabled": true
    }
  }
```

Your local settings would have at least these elements:
``` json
{
    "IsEncrypted": false,
    "Values": {
        "AzureWebJobsStorage": "<your storage connection string>",
        "FUNCTIONS_WORKER_RUNTIME": "dotnet",
        "REDIS_CONN": "<your redis connection string>",
        "DEFAULT_TTL": "60"

    }
}
```

Azure Function scale and restrictions see [documentation](https://docs.microsoft.com/en-us/azure/azure-functions/functions-scale)

#### WAF
WAF will provides centralized protection of your web applications from common exploits and vulnerabilities. 
Deploy a WAF as explained in this [document](https://docs.microsoft.com/en-us/azure/web-application-firewall/ag/application-gateway-web-application-firewall-portal)

#### Additional / Optional Assets
Azure Front Door see [documentation](https://docs.microsoft.com/en-us/azure/frontdoor/quickstart-create-front-door)

APIM & APG - please see [blog-post](https://medium.com/azure-architects/azure-api-management-and-application-gateway-integration-a31fde80f3db) for more information on how the integration between the two products can increase your control over the exposed APIs.

Please see this [blog-post](https://medium.com/microsoftazure/azure-functions-limiting-throughput-and-scalability-of-a-serverless-app-5b1c381491e3) for information limiting throughput and scalability of a Azure Functions.
