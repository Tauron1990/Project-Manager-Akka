using Microsoft.AspNetCore.Components;

namespace Tauron.Application.ServiceManager.Components
{
    public abstract class ConnectionAwareComponent : DisposingComponent
    {
        [CascadingParameter(Name = "IsSelf")]
        public bool IsSelf { get; set; }

        [CascadingParameter(Name = "IsConnected")]
        public bool IsConnected { get; set; }

        [CascadingParameter(Name = "IsDatabaseReady")]
        public bool IsDatabaseReady { get; set; }
    }
}