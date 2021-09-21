using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using Akka.Actor;
using Akka.Util.Extensions;
using JetBrains.Annotations;
using NLog;
using Tauron.Application.CommonUI;
using Tauron.Application.CommonUI.Helper;
using Tauron.Application.CommonUI.ModelMessages;
using Tauron.Application.Wpf.AppCore;
using Tauron.ObservableExt;

namespace Tauron.Application.Wpf
{
    [PublicAPI]
    public static class ControlHelper
    {
        private const string ControlHelperPrefix = "ControlHelper.";

        public static readonly DependencyProperty MarkControlProperty =
            DependencyProperty.RegisterAttached(
                "MarkControl",
                typeof(string),
                typeof(ControlHelper),
                new UIPropertyMetadata(string.Empty, MarkControl));

        public static readonly DependencyProperty MarkWindowProperty =
            DependencyProperty.RegisterAttached(
                "MarkWindow",
                typeof(string),
                typeof(ControlHelper),
                new UIPropertyMetadata(null, MarkWindowChanged));

        public static string GetMarkControl(DependencyObject obj)
            => (string)obj.GetValue(MarkControlProperty);

        public static string GetMarkWindow(DependencyObject obj)
            => (string)obj.GetValue(MarkWindowProperty);

        public static void SetMarkControl(DependencyObject obj, string value)
            => obj.SetValue(MarkControlProperty, value);

        public static void SetMarkWindow(DependencyObject obj, string value)
            => obj.SetValue(MarkWindowProperty, value);

        private static void MarkControl(DependencyObject d, DependencyPropertyChangedEventArgs e)
            => SetLinker(d, e.OldValue as string, e.NewValue as string, () => new ControlLinker());

        private static void MarkWindowChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
            => SetLinker(d, e.OldValue as string, e.NewValue as string, () => new WindowLinker());

        private static void SetLinker(DependencyObject obj, string? oldName, string? newName, Func<LinkerBase> factory)
        {
            if (string.IsNullOrWhiteSpace(newName))
                return;

            if (DesignerProperties.GetIsInDesignMode(obj)) return;

            var ele = ElementMapper.Create(obj);
            var rootOption = ControlBindLogic.FindRoot(ele.AsOption());

            rootOption.Run(
                root => SetLinker(newName, oldName, root, ele, factory),
                () =>
                    ControlBindLogic.MakeLazy(
                        (IUIElement)ele,
                        newName,
                        oldName,
                        (name, old, controllable, dependencyObject)
                            => SetLinker(old, name, controllable, dependencyObject, factory)));
        }

        private static void SetLinker(
            string? newName, string? oldName, IBinderControllable root, IUIObject obj,
            Func<LinkerBase> factory)
        {
            if (oldName is not null)
                root.CleanUp(ControlHelperPrefix + oldName);

            if (newName is null) return;

            var linker = factory();
            linker.Name = newName;
            root.Register(ControlHelperPrefix + newName, linker, obj);
        }

        [DebuggerNonUserCode]
        private class ControlLinker : LinkerBase
        {
            protected override void Scan()
            {
                if (DataContext is IViewModel model && AffectedObject is IUIElement element)
                    model.AwaitInit(() => model.Actor.Tell(new ControlSetEvent(Name, element)));
            }
        }

        private abstract class LinkerBase : ControlBindableBase
        {
            internal string Name { get; set; } = string.Empty;

            protected object DataContext { get; private set; } = new();

            protected abstract void Scan();

            protected override void CleanUp() { }

            protected override void Bind(object context)
            {
                DataContext = context;
                Scan();
            }
        }

        private class WindowLinker : LinkerBase
        {
            protected override void Scan()
            {
                var realName = Name;
                string? windowName = null;

                if (realName.Contains(":"))
                {
                    var nameSplit = realName.Split(new[] { ':' }, 2);
                    realName = nameSplit[0];
                    windowName = nameSplit[1];
                }

                var priTarget = ((WpfObject)AffectedObject).DependencyObject;

                if (windowName is null)
                {
                    if (priTarget is not System.Windows.Window)
                        priTarget = System.Windows.Window.GetWindow(priTarget);

                    if (priTarget == null)
                        LogManager.GetCurrentClassLogger().Error($"ControlHelper: No Window Found: {DataContext.GetType()}|{realName}");
                }
                else
                {
                    priTarget =
                        System.Windows.Application.Current.Windows.Cast<System.Windows.Window>()
                           .FirstOrDefault(win => win.Name == windowName);

                    if (priTarget is null)
                        LogManager.GetCurrentClassLogger().Error($"ControlHelper: No Window Named {windowName} Found");
                }

                if (priTarget is null) return;

                if (DataContext is IViewModel model && ElementMapper.Create(priTarget) is IUIElement element)
                    model.AwaitInit(() => model.Actor.Tell(new ControlSetEvent(Name, element)));
            }
        }
    }
}