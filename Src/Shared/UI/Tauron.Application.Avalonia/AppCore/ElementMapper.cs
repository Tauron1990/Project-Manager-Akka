using Avalonia;
using Tauron.Application.CommonUI;

namespace Tauron.Application.Avalonia.AppCore;

public static class ElementMapper
{
    public static IUIObject Create(AvaloniaObject obj)
    {
        return obj switch
        {
            Window w => new AvaloniaWindow(w),
            StyledElement c => new AvaloniaElement(c),
            _ => new AvaObject(obj)
        };
    }
}