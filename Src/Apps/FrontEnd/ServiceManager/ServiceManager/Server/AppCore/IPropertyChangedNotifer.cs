using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
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
        private readonly IHubContext<ClusterInfoHub> _hubContext;

        public PropertyChangedNotifer(IHubContext<ClusterInfoHub> hubContext) => _hubContext = hubContext;

        public Task SendPropertyChanged<TInterface>(string property) 
            => _hubContext.Clients.All.SendAsync(HubEvents.PropertyChanged, typeof(TInterface).FullName!, property);
    }
}