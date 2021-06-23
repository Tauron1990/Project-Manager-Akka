using Microsoft.AspNetCore.Components;

namespace Tauron.Application.ServiceManager.Components
{
    public abstract class ConnectionAwareComponent : DisposingComponent
    {
        private bool _isConnected;

        [CascadingParameter(Name = "IsSelf")]
        public bool IsSelf { get; set; }

        [CascadingParameter(Name = "IsConnected")]
        public bool IsConnected
        {
            get => _isConnected;
            set
            {
                _isConnected = value;
                InvokeAsync(StateHasChanged);
            }
        }
    }
}