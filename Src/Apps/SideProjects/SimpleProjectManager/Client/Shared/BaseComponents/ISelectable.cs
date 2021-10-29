using Microsoft.AspNetCore.Components;

namespace SimpleProjectManager.Client.Shared.BaseComponents
{
    public interface ISelectable
    {
        public EventCallback Callback { get; set; }

        public bool IsSelected { get; set; }
    }
}