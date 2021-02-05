using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Akka.Actor;
using Avalonia;
using Avalonia.Controls;
using JetBrains.Annotations;
using Serilog;
using Tauron.Application.Avalonia.AppCore;
using Tauron.Application.CommonUI;
using Tauron.Application.CommonUI.Commands;
using Tauron.Application.CommonUI.Helper;
using Tauron.Application.CommonUI.ModelMessages;

namespace Tauron.Application.Avalonia
{
    [PublicAPI]
    public class EventBinder
    {
        private const string EventBinderPrefix = "EventBinder.";

        public static readonly AttachedProperty<string> EventsProperty =
            AvaloniaProperty.RegisterAttached<EventBinder, Control, string>("Events");

        private EventBinder()
        {
        }

        [NotNull]
        public static string GetEvents(Control obj) => Argument.NotNull(obj, nameof(obj)).GetValue(EventsProperty);

        public static void SetEvents(Control obj, string value)
        {
            var old = GetEvents(obj);
            Argument.NotNull(obj, nameof(obj)).SetValue(EventsProperty, Argument.NotNull(value, nameof(value)));
            OnEventsChanged(obj, value, old);
        }

        private static void OnEventsChanged(Control d, string newValue, string? oldValue)
        {
            var ele = ElementMapper.Create(d);
            var root = ControlBindLogic.FindRoot(ele);
            if (root == null)
            {
                ControlBindLogic.MakeLazy((IUIElement) ele, newValue, oldValue, BindInternal);
                return;
            }

            BindInternal(oldValue, newValue, root, ele);
        }

        private static void BindInternal(string? oldValue, string? newValue, IBinderControllable binder,
            IUIObject affectedPart)
        {
            if (oldValue != null)
                binder.CleanUp(EventBinderPrefix + oldValue);

            if (newValue == null) return;

            binder.Register(EventBinderPrefix + newValue, new EventLinker {Commands = newValue}, affectedPart);
        }

        private sealed class EventLinker : ControlBindableBase
        {
            private static readonly ILogger Log = Serilog.Log.ForContext<EventLinker>();
            private readonly List<InternalEventLinker> _linkers = new();

            public string? Commands { get; init; }

            protected override void CleanUp()
            {
                Log.Debug("Clean Up Event {Events}", Commands);
                foreach (var linker in _linkers) linker.Dispose();
                _linkers.Clear();
            }

            protected override void Bind(object context)
            {
                if (Commands == null)
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

                var hostType = host.GetType();

                foreach (var (@event, command) in events)
                {
                    var info = hostType.GetEvent(@event);
                    if (info == null)
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

                private static readonly ILogger InternalLog = Log.ForContext<InternalEventLinker>();

                private readonly IViewModel _dataContext;

                private readonly EventInfo? _event;
                private readonly AvaloniaObject? _host;
                private readonly string _targetName;
                private Action<EventData>? _command;
                private Delegate? _delegate;
                private bool _isDirty;

                public InternalEventLinker(EventInfo? @event, IViewModel dataContext, string targetName,
                    IUIObject? host)
                {
                    _isDirty = @event == null;

                    _host = (host as AvaObject)?.Obj;
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

                    _command = d => _dataContext.Actor.Tell(new ExecuteEventEvent(d, _targetName));


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