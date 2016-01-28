# RedisOnSerivceFabric-Example
Quick example of running Redis as a service fabric service. 

Work in progress!

Two branchs:

Master
------
This wraps redis in a Stateless service written in C# which keeps the process running. 

It allows us to publish the address of the redis server to the fabric naming service, through the listener. 

You can then use the naming service to discover where redis is, from other services, and use it. Example below.          



```csharp

        fabric:/redishost/RedisHost
        
        //Replace the fabric address with tthe address of the redis service in your cluster. 
        ConnectionMultiplexer.Connect(GetInstanceEndpoints("fabric:/redishost/RedisHost").Single());

        IDatabase redisDb = ServiceRouter.RedisConnection.GetDatabase();
        await redisDb.ListLeftPushAsync(....);
```
        
Simple Helper to discover the service from c# - LoFi version see full SF docs for best practices 

```csharp      
        private async Task<string[]> GetInstanceEndpoints(string fabricAddress)
        {

            //Get the endpoint for the service
            var serviceEndpoint = await fabClient.ServiceManager.ResolveServicePartitionAsync(new Uri(fabricAddress));
            var addressesDeserialized = serviceEndpoint.Endpoints.Select(x => JsonConvert.DeserializeObject<EndpointServiceFabricModel>(x.Address));
            var simpleEndpoints = addressesDeserialized.SelectMany(x => x.Endpoints.Values);
            return simpleEndpoints.ToArray();
        }
        
```

Implicit Host
----------------

This branch uses the implicit host option to avoid using a wrapper. 
The downside is the service isn't published to the naming service so you need another discovery method. IMO this is pretty useless. 