using Microsoft.AspNetCore.Components;

namespace ServiceManager.Client.Components
{
    public abstract class ConnectionAwareComponent : PropertyChangedComponent
    {
        [CascadingParameter(Name = "IsSelf")]
        public bool IsSelf { get; set; }

        [CascadingParameter(Name = "IsConnected")]
        public bool IsConnected { get; set; }

        [CascadingParameter(Name = "IsDatabaseReady")]
        public bool IsDatabaseReady { get; set; }
    }
}