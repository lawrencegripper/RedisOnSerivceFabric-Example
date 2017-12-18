using Microsoft.ServiceFabric.Services.Communication.Runtime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Fabric;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RedisHost
{
    class RedisListener : ICommunicationListener
    {
        private readonly int _redisPort;

        public RedisListener(int redisPort)
        {
            _redisPort = redisPort;
        }

        public void Abort()
        {
            Process.GetProcessById(RedisHost.RedisPID).Kill();
        }

        public Task CloseAsync(CancellationToken cancellationToken)
        {
            Process.GetProcessById(RedisHost.RedisPID).Kill();
            return Task.FromResult(0);
        }

        public Task<string> OpenAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult($"{FabricRuntime.GetNodeContext().IPAddressOrFQDN}:{_redisPort}");
        }
    }
}
