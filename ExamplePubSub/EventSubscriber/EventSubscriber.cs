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

namespace EventSubscriber
{
    /// <summary>
    /// The FabricRuntime creates an instance of this class for each service type instance. 
    /// </summary>
    internal sealed class EventSubscriber : StatelessService
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
            var endpointsForService = await GetInstanceEndpoints("fabric:/RedisWrapper/RedisHost");
            new Observable(endpointsForService.Single(), "ExampleChannel").Subscribe(message =>
            {
                ServiceEventSource.Current.ServiceMessage(this, $"Received Message: {message.ToString()}");
            });

            // This service instance continues processing until the instance is terminated.
            while (!cancelServiceInstance.IsCancellationRequested)
            {

                // Log what the service is doing
                ServiceEventSource.Current.ServiceMessage(this, "Listening at the EventListener");

                // Pause for 1 second before continue processing.
                await Task.Delay(TimeSpan.FromSeconds(5), cancelServiceInstance);
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
