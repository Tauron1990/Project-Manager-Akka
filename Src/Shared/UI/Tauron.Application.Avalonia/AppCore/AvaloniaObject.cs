using Avalonia;
using Tauron.Application.CommonUI;

namespace Tauron.Application.Avalonia.AppCore
{
    public class AvaObject : IUIObject
    {
        public AvaObject(AvaloniaObject obj) => Obj = obj;

        public AvaloniaObject Obj { get; }

        public virtual IUIObject? GetPerent() => null;

        public object Object => Obj;
    }
}