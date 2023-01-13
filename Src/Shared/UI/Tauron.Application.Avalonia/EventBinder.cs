using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Akka.Actor;
using Avalonia;
using Avalonia.Controls;
using JetBrains.Annotations;
using NLog;
using Tauron.Application.Avalonia.AppCore;
using Tauron.Application.CommonUI;
using Tauron.Application.CommonUI.Commands;
using Tauron.Application.CommonUI.Helper;
using Tauron.Application.CommonUI.ModelMessages;
using Tauron.ObservableExt;

namespace Tauron.Application.Avalonia;

[PublicAPI]
public class EventBinder
{
    private const string EventBinderPrefix = "EventBinder.";

    public static readonly AttachedProperty<string> EventsProperty =
        AvaloniaProperty.RegisterAttached<EventBinder, Control, string>("Events");

    private EventBinder() { }

    public static string GetEvents(Control obj) => obj.GetValue(EventsProperty);

    public static void SetEvents(Control obj, string value)
    {
        string old = GetEvents(obj);
        obj.SetValue(EventsProperty, value);
        OnEventsChanged(obj, value, old);
    }

    private static void OnEventsChanged(Control d, string newValue, string? oldValue)
    {
        IUIObject ele = ElementMapper.Create(d);
        var rootOption = ControlBindLogic.FindRoot(ele.AsOption());
        rootOption.Run(
            root => BindInternal(oldValue, newValue, root, ele),
            () => ControlBindLogic.MakeLazy((IUIElement)ele, newValue, oldValue, BindInternal));
    }

    private static void BindInternal(
        string? oldValue, string? newValue, IBinderControllable binder,
        IUIObject affectedPart)
    {
        if(oldValue is not null)
            binder.CleanUp(EventBinderPrefix + oldValue);

        if(newValue is null) return;

        binder.Register(EventBinderPrefix + newValue, new EventLinker { Commands = newValue }, affectedPart);
    }

    private sealed class EventLinker : ControlBindableBase
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();
        private readonly List<InternalEventLinker> _linkers = new();

        internal string? Commands { get; init; }

        protected override void CleanUp()
        {
            Log.Debug(CultureInfo.InvariantCulture, "Clean Up Event {Events}", Commands);
            foreach (InternalEventLinker linker in _linkers) linker.Dispose();
            _linkers.Clear();
        }

        protected override void Bind(object context)
        {
            if(Commands is null)
            {
                Log.Error(CultureInfo.InvariantCulture, "EventBinder: No Command Setted: {Context}", context);

                return;
            }

            Log.Debug(CultureInfo.InvariantCulture, "Bind Events {Name}", Commands);

            string[] vals = Commands.Split(':');
            var events = new Dictionary<string, string>(StringComparer.Ordinal);

            try
            {
                for (var i = 0; i < vals.Length; i++) events[vals[i]] = vals[++i];
            }
            catch (IndexOutOfRangeException)
            {
                Log.Error(CultureInfo.InvariantCulture, "EventBinder: EventPairs not Valid: {Commands}", Commands);
            }

            IUIObject host = AffectedObject;

            if(context is not IViewModel dataContext) return;

            Type hostType = host.GetType();

            foreach ((string @event, string command) in events)
            {
                EventInfo? info = hostType.GetEvent(@event);
                if(info is null)
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
               .First(m => string.Equals(m.Name, "Handler", StringComparison.Ordinal));

            private static readonly ILogger InternalLog = LogManager.GetCurrentClassLogger();

            private readonly IViewModel _dataContext;

            private readonly EventInfo? _event;
            private readonly AvaloniaObject? _host;
            private readonly string _targetName;
            private Action<EventData>? _command;
            private Delegate? _delegate;
            private bool _isDirty;

            internal InternalEventLinker(
                EventInfo? @event, IViewModel dataContext, string targetName,
                IUIObject? host)
            {
                _isDirty = @event is null;

                _host = (host as AvaObject)?.Obj;
                _event = @event;
                _dataContext = dataContext;
                _targetName = targetName;

                Initialize();
            }

            public void Dispose()
            {
                InternalLog.Debug(CultureInfo.InvariantCulture, "Remove Event Handler {Name}", _targetName);

                if(_host is null || _delegate is null) return;

                _event?.RemoveEventHandler(_host, _delegate);
                _delegate = null;
            }

            private bool EnsureCommandStade()
            {
                if(_command != null) return true;

                _command = d => _dataContext.Actor.Tell(new ExecuteEventEvent(d, _targetName));


                return _command != null && !_isDirty;
            }


            [UsedImplicitly]
            private void Handler(object sender, EventArgs e)
            {
                if(!_isDirty && !EnsureCommandStade())
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
                InternalLog.Debug(CultureInfo.InvariantCulture, "Initialize Event Handler {Name}", _targetName);

                if(_isDirty || _event is null) return;

                Type? eventTyp = _event?.EventHandlerType;

                if(_host is null || eventTyp is null) return;

                _delegate = Delegate.CreateDelegate(eventTyp, this, Method);
                _event?.AddEventHandler(_host, _delegate);
            }
        }
    }
}