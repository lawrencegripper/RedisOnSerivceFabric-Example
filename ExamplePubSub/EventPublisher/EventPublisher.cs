using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EventPublisher
{
    /// <summary>
    /// The FabricRuntime creates an instance of this class for each service type instance. 
    /// </summary>
    internal sealed class EventPublisher : StatelessService
    {
        /// <summary>
        /// Optional override to create listeners (like tcp, http) for this service instance.
        /// </summary>
        /// <returns>The collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            // TODO: If your service needs to handle user requests, return a list of ServiceReplicaListeners here.
            return new ServiceInstanceListener[0];
        }

        /// <summary>
        /// This is the main entry point for your service instance.
        /// </summary>
        /// <param name="cancelServiceInstance">Canceled when Service Fabric terminates this instance.</param>
        protected override async Task RunAsync(CancellationToken cancelServiceInstance)
        {
            // TODO: Replace the following sample code with your own logic.

            //Replace the fabric address with tthe address of the redis service in your cluster. 
            var endpointsForService = await GetInstanceEndpoints("fabric:/RedisWrapper/RedisHost");
            var redisConnection = ConnectionMultiplexer.Connect(endpointsForService.Single());

            var subscriber = redisConnection.GetSubscriber();

            int iterations = 0;
            // This service instance continues processing until the instance is terminated.
            while (!cancelServiceInstance.IsCancellationRequested)
            {

                // Log what the service is doing
                ServiceEventSource.Current.ServiceMessage(this, "Working-{0}", iterations++);

                await subscriber.PublishAsync("ExampleChannel", iterations++);


                // Pause for 1 second before continue processing.
                await Task.Delay(TimeSpan.FromSeconds(1), cancelServiceInstance);
            }
        }

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
    }
}
