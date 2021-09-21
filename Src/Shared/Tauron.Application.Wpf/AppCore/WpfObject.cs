using System;
using System.Windows;
using System.Windows.Media;
using Tauron.Application.CommonUI;

namespace Tauron.Application.Wpf.AppCore
{
    public class WpfObject : IUIObject
    {
        public WpfObject(DependencyObject obj) => DependencyObject = obj;

        public DependencyObject DependencyObject { get; }

        public IUIObject? GetPerent()
        {
            if (DependencyObject is FrameworkElement { TemplatedParent: { } } ele)
                return ElementMapper.Create(ele.TemplatedParent);

            DependencyObject? temp;
            try
            {
                temp = LogicalTreeHelper.GetParent(DependencyObject) ?? VisualTreeHelper.GetParent(DependencyObject);
            }
            catch (InvalidOperationException)
            {
                temp = null;
            }

            return temp == null ? null : ElementMapper.Create(temp);
        }

        public object Object => DependencyObject;
    }
}