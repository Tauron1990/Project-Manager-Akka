using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using Akka.Actor;
using Akka.Util.Extensions;
using JetBrains.Annotations;
using NLog;
using Tauron.Application.CommonUI;
using Tauron.Application.CommonUI.Commands;
using Tauron.Application.CommonUI.Helper;
using Tauron.Application.CommonUI.ModelMessages;
using Tauron.Application.Wpf.AppCore;
using Tauron.ObservableExt;

namespace Tauron.Application.Wpf
{
    [PublicAPI]
    public static class EventBinder
    {
        private const string EventBinderPrefix = "EventBinder.";

        public static readonly DependencyProperty EventsProperty =
            DependencyProperty.RegisterAttached(
                "Events",
                typeof(string),
                typeof(EventBinder),
                new UIPropertyMetadata(null, OnEventsChanged));

        public static string GetEvents(DependencyObject obj)
            => (string)obj.GetValue(EventsProperty);

        public static void SetEvents(DependencyObject obj, string value)
            => obj.SetValue(EventsProperty, value);

        private static void OnEventsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (DesignerProperties.GetIsInDesignMode(d)) return;

            var ele = ElementMapper.Create(d);
            var rootOption = ControlBindLogic.FindRoot(ele.AsOption());
            rootOption.Run(
                root => BindInternal(e.OldValue as string, e.NewValue as string, root, ele),
                () =>
                {
                    if (d is FrameworkElement)
                        ControlBindLogic.MakeLazy(
                            (IUIElement)ele,
                            e.NewValue as string,
                            e.OldValue as string,
                            BindInternal);
                });
        }

        private static void BindInternal(
            string? oldValue, string? newValue, IBinderControllable binder,
            IUIObject affectedPart)
        {
            if (oldValue != null)
                binder.CleanUp(EventBinderPrefix + oldValue);

            if (newValue == null) return;

            binder.Register(EventBinderPrefix + newValue, new EventLinker { Commands = newValue }, affectedPart);
        }

        private sealed class EventLinker : ControlBindableBase
        {
            private static readonly ILogger Log = LogManager.GetCurrentClassLogger();
            private readonly List<InternalEventLinker> _linkers = new();

            internal string? Commands { get; init; }

            protected override void CleanUp()
            {
                Log.Debug("Clean Up Event {Events}", Commands);
                foreach (var linker in _linkers) linker.Dispose();
                _linkers.Clear();
            }

            protected override void Bind(object context)
            {
                if (Commands is null)
                {
                    Log.Error("EventBinder: No Command Setted: {Context}", context);

                    return;
                }

                Log.Debug("Bind Events {Name}", Commands);

                var vals = Commands.Split(':');
                var events = new Dictionary<string, string>();

                try
                {
                    for (var i = 0; i < vals.Length; i++) events[vals[i]] = vals[++i];
                }
                catch (IndexOutOfRangeException)
                {
                    Log.Error("EventBinder: EventPairs not Valid: {Commands}", Commands);
                }

                var host = AffectedObject;

                if (context is not IViewModel dataContext) return;

                var hostType = host.Object.GetType();

                foreach (var (@event, command) in events)
                {
                    var info = hostType.GetEvent(@event);
                    if (info is null)
                    {
                        Log.Error("EventBinder: No event Found: {HostType}|{Key}", hostType, @event);

                        return;
                    }

                    _linkers.Add(new InternalEventLinker(info, dataContext, command, host));
                }
            }


            private class InternalEventLinker : IDisposable
            {
                private static readonly MethodInfo Method = typeof(InternalEventLinker)
                                                           .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
                                                           .First(m => m.Name == "Handler");

                private static readonly ILogger InternalLog = LogManager.GetCurrentClassLogger();

                private readonly IViewModel _dataContext;

                private readonly EventInfo? _event;
                private readonly DependencyObject? _host;
                private readonly string _targetName;
                private Action<EventData>? _command;
                private Delegate? _delegate;
                private bool _isDirty;

                internal InternalEventLinker(
                    EventInfo? @event, IViewModel dataContext, string targetName,
                    IUIObject? host)
                {
                    _isDirty = @event is null;

                    _host = (host as WpfObject)?.DependencyObject;
                    _event = @event;
                    _dataContext = dataContext;
                    _targetName = targetName;

                    Initialize();
                }

                public void Dispose()
                {
                    InternalLog.Debug("Remove Event Handler {Name}", _targetName);

                    if (_host == null || _delegate == null) return;

                    _event?.RemoveEventHandler(_host, _delegate);
                    _delegate = null;
                }

                private bool EnsureCommandStade()
                {
                    if (_command != null) return true;

                    _command = d =>
                               {
                                   void Invoker() => _dataContext.Actor.Tell(new ExecuteEventEvent(d, _targetName));

                                   if (_dataContext.IsInitialized)
                                       Invoker();
                                   else
                                       _dataContext.AwaitInit(Invoker);
                               };


                    return _command != null && !_isDirty;
                }


                [UsedImplicitly]
                private void Handler(object sender, EventArgs e)
                {
                    if (!_isDirty && !EnsureCommandStade())
                    {
                        Dispose();

                        return;
                    }

                    try
                    {
                        _command?.Invoke(new EventData(sender, e));
                    }
                    catch (ArgumentException)
                    {
                        _isDirty = true;
                    }
                }

                private void Initialize()
                {
                    InternalLog.Debug("Initialize Event Handler {Name}", _targetName);

                    if (_isDirty || _event == null) return;

                    var eventTyp = _event?.EventHandlerType;

                    if (_host == null || eventTyp == null) return;

                    _delegate = Delegate.CreateDelegate(eventTyp, this, Method);
                    _event?.AddEventHandler(_host, _delegate);
                }
            }
        }
    }
}