using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Fabric;
using System.Fabric.Description;

namespace RedisHost
{
    /// <summary>
    /// The FabricRuntime creates an instance of this class for each service type instance. 
    /// </summary>
    internal sealed class RedisHost : StatelessService
    {
        readonly int _redisPort;

        public RedisHost(StatelessServiceContext serviceContext) : base(serviceContext)
        {
            EndpointResourceDescription endpoint =
                            serviceContext.CodePackageActivationContext.GetEndpoint("RedisEndpoint");
            _redisPort = endpoint.Port;

        }

        public static int RedisPID { get; set; }
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            // TODO: If your service needs to handle user requests, return a list of ServiceReplicaListeners here.
            return new[] { new ServiceInstanceListener(_ => new RedisListener(_redisPort)) };
        }

        protected override async Task RunAsync(CancellationToken cancelServiceInstance)
        {
            await Task.Yield();

            Process process = StartRedis(_redisPort);
            // This service instance continues processing until the instance is terminated.
            while (!cancelServiceInstance.IsCancellationRequested)
            {
                if (process.HasExited)
                {
                    process = StartRedis(_redisPort);
                }
                process.WaitForExit(600);

            }

            process.Kill();
        }

        private static Process StartRedis(int port)
        {
            var currentDir = System.IO.Directory.GetCurrentDirectory();
            var exeDir = Path.Combine(currentDir, "redis-server.exe");
            var configDir = Path.Combine(currentDir, "redis.windows.conf");
            string arguments = $"{configDir} --port {port}";
            var process = System.Diagnostics.Process.Start(exeDir, arguments);
            RedisPID = process.Id;
            return process;
        }
    }
}
