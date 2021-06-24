using Microsoft.AspNetCore.Components;
using Tauron.Application.CommonUI;

namespace Tauron.Application.Blazor.UI
{
    public sealed class ComponentUIObject : IUIObject
    {
        private readonly ComponentBase _target;
        private readonly IUIObject? _parent;

        public ComponentUIObject(ComponentBase target, IUIObject? parent)
        {
            _target = target;
            _parent = parent;
        }

        public object Object => _target;

        public IUIObject? GetPerent() => _parent;
    }
}