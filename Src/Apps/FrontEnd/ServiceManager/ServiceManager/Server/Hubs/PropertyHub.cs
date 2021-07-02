using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using ServiceManager.Shared.Events;

namespace ServiceManager.Server.Hubs
{ 
    public sealed class PropertyHub : Hub
    {
        public Task SentPropertyChanged(string type, string name)
            => Clients.All.SendAsync(HubEvents.PropertyChanged, type, name);
    }
}