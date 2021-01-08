using System.Windows;
using Tauron.Application.CommonUI;

namespace Tauron.Application.Wpf.AppCore
{
    public static class ElementMapper
    {
        public static IUIObject Create(DependencyObject obj)
        {
            return obj switch
                   {
                       Window w                   => new WpfWindow(w),
                       FrameworkElement e         => new WpfElement(e),
                       FrameworkContentElement ce => new WpfContentElement(ce),
                       _                          => new WpfObject(obj)
                   };
        }
    }
}