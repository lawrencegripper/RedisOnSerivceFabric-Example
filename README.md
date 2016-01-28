# RedisOnSerivceFabric-Example
Quick example of running Redis as a service fabric service. This runs a single instance of Redis and doesn't persist data when a movement occurs between nodes. The movement cost is set to High to minimize the changes of this occuring. 

For the scenarios I'm using it for this works well (so far). If you MUST never lose the contents of the cache look to extend this repo to use redis clustering, happy to accept the PR. 

Work in progress!

Two versions in the repo:

RedisWrapper - /RedisWrapper
------
This wraps redis in a Stateless service written in C# which keeps the process running. 

It allows us to publish the address of the redis server to the fabric naming service, through the listener. 

You can then use the naming service to discover where redis is, from other services, and use it. Example below.          



```csharp

        //Replace the fabric address with tthe address of the redis service in your cluster. 
        var endpointsForService = await GetInstanceEndpoints("fabric:/RedisWrapper/RedisHost");
        var redisConnection = ConnectionMultiplexer.Connect(endpointsForService.Single());

        IDatabase redisDb = redisConnection.GetDatabase();
        await redisDb.ListLeftPushAsync(....);
```
        
Simple Helper to discover the service from c# - LoFi version see full SF docs for best practices 

```csharp    
  
        private async Task<string[]> GetInstanceEndpoints(string fabricAddress)
        {
            var fabClient = new FabricClient();
            //Get the endpoint for the service
            var serviceEndpoint = await fabClient.ServiceManager.ResolveServicePartitionAsync(new Uri(fabricAddress));
            var addressesDeserialized = serviceEndpoint.Endpoints.Select(x => JsonConvert.DeserializeObject<EndpointServiceFabricModel>(x.Address));
            var simpleEndpoints = addressesDeserialized.SelectMany(x => x.Endpoints.Values);
            return simpleEndpoints.ToArray();
        }

        public class EndpointServiceFabricModel
        {
            public Dictionary<string, string> Endpoints { get; set; }
        }
        
```

To deploy this open the VSSolution with the SF tools installed and publish. 

RedisHost (ImplicitHost) - /RedisHost
----------------

This version uses the implicit host option to avoid using a wrapper. 
The downside is the service isn't published to the naming service so you need another discovery method, which isn't useful for me. 

To deploy this version edit, with your params, then run Deploy.ps1. 