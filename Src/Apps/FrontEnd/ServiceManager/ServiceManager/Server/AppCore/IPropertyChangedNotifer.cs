using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using NLog;
using ServiceManager.Server.Hubs;
using ServiceManager.Shared.Api;

namespace ServiceManager.Server.AppCore
{
    public interface IPropertyChangedNotifer
    {
        Task SendPropertyChanged<TInterface>(string property);
    }

    public sealed class PropertyChangedNotifer : IPropertyChangedNotifer
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private readonly IHubContext<ClusterInfoHub> _hubContext;

        public PropertyChangedNotifer(IHubContext<ClusterInfoHub> hubContext) => _hubContext = hubContext;

        public async Task SendPropertyChanged<TInterface>(string property)
        {
            try
            {
                await _hubContext.Clients.All.SendAsync(HubEvents.PropertyChanged, typeof(TInterface).FullName!, property);
            }
            catch (Exception e)
            {
                Log.Error(e, "Error on sending property Notification: {Interface}--{Property}", typeof(TInterface).Name, property);
            }
        }
    }
}