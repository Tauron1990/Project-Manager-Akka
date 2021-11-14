using Microsoft.AspNetCore.Components;

namespace Tauron.Application.Blazor
{
    public interface ISelectable
    {
        public EventCallback Callback { get; set; }

        public bool IsSelected { get; set; }
    }
}