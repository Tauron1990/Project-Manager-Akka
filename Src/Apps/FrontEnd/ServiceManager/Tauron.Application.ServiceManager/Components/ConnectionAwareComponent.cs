using Microsoft.AspNetCore.Components;
using Tauron.Application.Blazor;

namespace Tauron.Application.ServiceManager.Components
{
    public abstract class ConnectionAwareComponent : DispoableComponent
    {
        [CascadingParameter(Name = "IsSelf")]
        public bool IsSelf { get; set; }

        [CascadingParameter(Name = "IsConnected")]
        public bool IsConnected { get; set; }

        [CascadingParameter(Name = "IsDatabaseReady")]
        public bool IsDatabaseReady { get; set; }
    }
}