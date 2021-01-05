using Avalonia;
using Tauron.Application.CommonUI;

namespace Tauron.Application.Avalonia.AppCore
{
    public class AvaObject : IUIObject
    {
        public AvaloniaObject Obj { get; }

        public AvaObject(AvaloniaObject obj) => Obj = obj;

        public virtual IUIObject? GetPerent() => null;
        public object Object => Obj;
    }
}