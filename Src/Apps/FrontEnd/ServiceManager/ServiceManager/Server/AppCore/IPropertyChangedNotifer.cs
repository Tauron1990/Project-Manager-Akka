using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using ServiceManager.Server.Hubs;
using ServiceManager.Shared.Api;

namespace ServiceManager.Server.AppCore
{
    public interface IPropertyChangedNotifer
    {
        Task SendPropertyChanged<TInterface>(string property);

        Task SendServerEvent(string name);
    }

    public sealed class PropertyChangedNotifer : IPropertyChangedNotifer
    {
        private readonly IHubContext<ClusterInfoHub>     _hubContext;
        private readonly ILogger<PropertyChangedNotifer> _logger;

        public PropertyChangedNotifer(IHubContext<ClusterInfoHub> hubContext, ILogger<PropertyChangedNotifer> logger)
        {
            _hubContext  = hubContext;
            _logger = logger;
        }

        public async Task SendPropertyChanged<TInterface>(string property)
        {
            try
            {
                await _hubContext.Clients.All.SendAsync(HubEvents.PropertyChanged, typeof(TInterface).FullName!, property);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error on sending property Notification: {Interface}--{Property}", typeof(TInterface).Name, property);
            }
        }

        public async Task SendServerEvent(string name)
        {
            try
            {
                await _hubContext.Clients.All.SendAsync(name);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error on sending Hub Message");
            }
        }
    }
}