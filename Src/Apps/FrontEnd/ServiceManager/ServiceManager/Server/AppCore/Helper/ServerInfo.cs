using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using ServiceManager.Server.Hubs;
using ServiceManager.Shared;
using ServiceManager.Shared.Api;

namespace ServiceManager.Server.AppCore.Helper
{
    public class ServerInfo : IServerInfo
    {
        private static readonly Guid CurrentInstance = Guid.NewGuid();
        private readonly IHubContext<ClusterInfoHub> _hub;
        private readonly IHostApplicationLifetime _lifetime;
        private readonly IRestartHelper _restart;

        public ServerInfo(IHostApplicationLifetime lifetime, IRestartHelper restart, IHubContext<ClusterInfoHub> hub)
        {
            _lifetime = lifetime;
            _restart = restart;
            _hub = hub;
        }

        public virtual Task<string> GetCurrentId(CancellationToken token)
            => Task.FromResult(CurrentInstance.ToString("N"));

        public virtual async Task Restart(RestartCommand command, CancellationToken token = default)
        {
            await _hub.Clients.All.SendAsync(HubEvents.RestartServer, token);
            _restart.Restart = true;
            #pragma warning disable 4014
            // ReSharper disable MethodSupportsCancellation
            Task.Delay(1000).ContinueWith(_ => _lifetime.StopApplication());
            #pragma warning restore 4014
        }
    }
}