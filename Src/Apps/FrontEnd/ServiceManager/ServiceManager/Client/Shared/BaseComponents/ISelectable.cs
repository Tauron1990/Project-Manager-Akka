using Microsoft.AspNetCore.Components;

namespace ServiceManager.Client.Shared.BaseComponents
{
    public interface ISelectable
    {
        public EventCallback Callback { get; set; }

        public bool IsSelected { get; set; }
    }
}