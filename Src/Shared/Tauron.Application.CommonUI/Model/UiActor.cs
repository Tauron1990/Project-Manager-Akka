using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using System.Windows.Input;
using Akka.Actor;
using Akka.Util;
using Akka.Util.Internal;
using Autofac;
using JetBrains.Annotations;
using Tauron.Akka;
using Tauron.Application.CommonUI.AppCore;
using Tauron.Application.CommonUI.Commands;
using Tauron.Application.CommonUI.Helper;
using Tauron.Application.CommonUI.ModelMessages;
using Tauron.Operations;

namespace Tauron.Application.CommonUI.Model
{
    [PublicAPI, DebuggerStepThrough]
    public abstract class UiActor : ObservableActor, IObservablePropertyChanged
    {
        private readonly Dictionary<string, CommandRegistration> _commandRegistrations = new();
        private readonly GroupDictionary<string, InvokeHelper> _eventRegistrations = new();
        private readonly Subject<string> _onPropertyChanged = new();
        private readonly Dictionary<string, PropertyData> _propertys = new();

        private bool _isSeald;

        protected UiActor(ILifetimeScope lifetimeScope, IUIDispatcher dispatcher)
        {
            AddResource(_onPropertyChanged);
            LifetimeScope = lifetimeScope;
            Dispatcher = dispatcher;
            InitHandler();
        }

        //internal IUntypedActorContext UIActorContext => Context;

        public ILifetimeScope LifetimeScope { get; }

        public IObservable<string> PropertyChangedObservable => _onPropertyChanged.AsObservable();

        public override void AroundPreStart()
        {
            _isSeald = true;
            base.AroundPreStart();
        }

        internal void ThrowIsSeald()
        {
            if (_isSeald)
                throw new InvalidOperationException("The Ui Actor is immutale");
        }

        protected void InitHandler()
        {
            Receive<InitEvent>(obs => obs.ToUnit(async evt => await InitializeAsync(evt)));
            Receive<Terminated>(obs => obs.Subscribe());
        }

        protected override bool Receive(object message)
        {
            static bool Run<TMsg>(TMsg msg, Action<TMsg> handler)
            {
                handler(msg);
                return true;
            }

            return message switch
            {
                CommandExecuteEvent commandExecuteEvent => Run(commandExecuteEvent, CommandExecute),
                ControlSetEvent controlSetEvent => Run(controlSetEvent, msg => SetControl(msg.Name, msg.Element)),
                MakeEventHook makeEventHook => Run(makeEventHook,
                    msg => Context.Sender.Tell(Context.GetOrCreateEventActor(msg.Name + "-EventActor"))),
                ExecuteEventEvent executeEventEvent => Run(executeEventEvent, ExecuteEvent),
                GetValueRequest getValueRequest => Run(getValueRequest, GetPropertyValue),
                SetValue setValue => Run(setValue, SetPropertyValue),
                TrackPropertyEvent trackPropertyEvent => Run(trackPropertyEvent, t => TrackProperty(t, Sender)),
                PropertyTermination propertyTermination => Run(propertyTermination, PropertyTerminationHandler),
                UnloadEvent unloadEvent => Run(unloadEvent, ControlUnload),
                InitParentViewModel initParentViewModel => Run(initParentViewModel, InitParentViewModel),
                ReviveActor reviveActor => Run(reviveActor, RestartActor),
                _ => base.Receive(message)
            };
        }

        #region ControlEvents

        protected virtual void SetControl(string name, IUIElement element)
        {
        }

        #endregion

        private sealed class PropertyTermination
        {
            public PropertyTermination(IActorRef actorRef, string name)
            {
                ActorRef = actorRef;
                Name = name;
            }

            public IActorRef ActorRef { get; }

            public string Name { get; }
        }

        private sealed class CommandRegistration
        {
            public CommandRegistration(Action<object?> command, Func<bool> canExecute)
            {
                Command = command;
                CanExecute = canExecute;
            }

            public Action<object?> Command { get; }

            public Func<bool> CanExecute { get; }
        }

        private sealed class InvokeHelper
        {
            private readonly Delegate _method;
            private readonly MethodType _methodType;

            public InvokeHelper(Delegate del)
            {
                _method = del;
                var method = del.Method;

                _methodType = (MethodType) method.GetParameters().Length;
                if (_methodType != MethodType.One) return;
                if (method.GetParameters()[0].ParameterType != typeof(EventData)) _methodType = MethodType.EventArgs;
            }

            public void Execute(EventData? parameter)
            {
                var args = _methodType switch
                {
                    MethodType.Zero => Array.Empty<object>(),
                    MethodType.One => new object[] {parameter!},
                    MethodType.Two => new[] {parameter?.Sender, parameter?.EventArgs},
                    MethodType.EventArgs => new[] {parameter?.EventArgs},
                    _ => Array.Empty<object>()
                };

                _method.Method.InvokeFast(_method.Target, args);
            }

            private enum MethodType
            {
                Zero = 0,
                One,
                Two,
                EventArgs
            }
        }

        private sealed class PropertyData
        {
            public PropertyData(UIPropertyBase propertyBase) => PropertyBase = propertyBase;

            public UIPropertyBase PropertyBase { get; }

            public Error? Error { get; set; }

            public List<IActorRef> Subscriptors { get; } = new();

            public void SetValue(object value)
            {
                PropertyBase.ObjectValue = value;
            }
        }

        private sealed class ActorCommand : CommandBase, IDisposable
        {
            private readonly BehaviorSubject<bool> _canExecute = new(false);
            private readonly AtomicBoolean _deactivated = new();
            private readonly IUIDispatcher _dispatcher;
            private readonly SingleAssignmentDisposable _disposable = new();
            private readonly string _name;
            private readonly IActorRef _self;

            public ActorCommand(string name, IActorRef self, IObservable<bool>? canExecute, IUIDispatcher dispatcher)
            {
                _name = name;
                _self = self;
                _dispatcher = dispatcher;
                if (canExecute == null)
                    _canExecute.OnNext(true);
                else
                    _disposable.Disposable = canExecute.Subscribe(b =>
                    {
                        _canExecute.OnNext(b);
                        _dispatcher.Post(RaiseCanExecuteChanged);
                    });
            }

            public void Dispose()
            {
                _canExecute.Dispose();
                _disposable.Dispose();
            }

            public override void Execute(object? parameter = null) => _self.Tell(new CommandExecuteEvent(_name, parameter));

            public override bool CanExecute(object? parameter = null) => _canExecute.Value;

            public void Deactivate()
            {
                _deactivated.GetAndSet(true);
                _canExecute.OnNext(false);
                _canExecute.OnCompleted();
                _dispatcher.Post(RaiseCanExecuteChanged);
            }
        }

        private sealed class ReviveActor
        {
            public ReviveActor(KeyValuePair<string, PropertyData>[] data) => Data = data;

            public KeyValuePair<string, PropertyData>[] Data { get; }
        }

        #region Dispatcher

        public IUIDispatcher Dispatcher { get; }

        protected Task UICall(Action<IUntypedActorContext> executor)
        {
            var context = Context;
            return Dispatcher.InvokeAsync(() => executor(context));
        }

        protected Task UICall(Action executor) => Dispatcher.InvokeAsync(executor);

        protected IObservable<T> UICall<T>(Func<Task<T>> executor) => Dispatcher.InvokeAsync(executor);

        protected IObservable<T> UICall<T>(Func<IUntypedActorContext, Task<T>> executor)
        {
            var context = Context;
            return Dispatcher.InvokeAsync(() => executor(context));
        }

        #endregion

        #region Commands

        private void CommandExecute(CommandExecuteEvent obj)
        {
            var (name, parameter) = obj;
            if (_commandRegistrations.TryGetValue(name, out var registration))
            {
                Log.Info("Execute Command {Commanf}", name);
                registration.Command(parameter);
            }
            else
            {
                Log.Error("Command not Found {Name}", name);
            }
        }

        protected void InvokeCommand(string name)
        {
            if (!_commandRegistrations.TryGetValue(name, out var cr))
                return;

            if (cr.CanExecute())
                cr.Command(null);
        }

        protected CommandRegistrationBuilder NewCommad
            => new(
                (key, command, canExecute) =>
                {
                    var actorCommand = new ActorCommand(key, Context.Self, canExecute, Dispatcher).DisposeWith(this);
                    var prop = new UIProperty<ICommand>(key).ForceSet(actorCommand);
                    prop.LockSet();
                    var data = new PropertyData(prop);

                    _propertys.Add(key, data);

                    PropertyValueChanged(data);

                    _commandRegistrations.Add(key,
                        new CommandRegistration(command, () => actorCommand.CanExecute()));

                    return prop;
                }, this);

        #endregion

        #region Lifecycle

        private void RestartActor(ReviveActor actor)
        {
            foreach (var (name, data) in actor.Data)
            foreach (var actorRef in data.Subscriptors)
                TrackProperty(new TrackPropertyEvent(name), actorRef);
        }

        protected override void PreRestart(Exception reason, object message)
        {
            foreach (var registration in _commandRegistrations)
                _propertys[registration.Key]
                    .PropertyBase
                    .ObjectValue
                    .AsInstanceOf<ActorCommand>()
                    .Deactivate();

            Self.Tell(new ReviveActor(_propertys.ToArray()));

            base.PreRestart(reason, message);
        }

        protected override void PostStop()
        {
            Log.Info("UiActor Terminated {ActorType}", GetType());
            _commandRegistrations.Clear();
            _eventRegistrations.Clear();
            _propertys.Clear();

            base.PostStop();
        }

        private Action<UiActor>? _terminationCallback;

        internal void RegisterTerminationCallback(Action<UiActor> callback)
        {
            if (_terminationCallback == null)
                _terminationCallback = callback;
            else
                _terminationCallback += callback;
        }

        protected void ShowWindow<TWindow>()
            where TWindow : IWindow
        {
            Dispatcher.Post(() => LifetimeScope.Resolve<TWindow>().Show());
        }

        protected virtual void Initialize(InitEvent evt)
        {
        }

        protected virtual Task InitializeAsync(InitEvent evt)
        {
            Initialize(evt);
            return Task.CompletedTask;
        }

        protected virtual void ControlUnload(UnloadEvent obj)
        {
        }

        protected virtual void InitParentViewModel(InitParentViewModel obj)
        {
            ViewModelSuperviser.Get(Context.System)
                .Create(obj.Model);
        }

        #endregion

        #region Events

        private void ExecuteEvent(ExecuteEventEvent obj)
        {
            var (eventData, name) = obj;
            if (_eventRegistrations.TryGetValue(name, out var reg))
            {
                Log.Info("Execute Event {Name}", name);
                reg.ForEach(e => e.Execute(eventData));
            }
            else
            {
                Log.Warning("Event Not found {Name}", name);
            }
        }

        protected EventRegistrationBuilder RegisterEvent(string name) => new(name, (s, del) => _eventRegistrations.Add(s, new InvokeHelper(del)));

        #endregion

        #region Propertys

        protected internal FluentPropertyRegistration<TData> RegisterProperty<TData>(string name)
        {
            ThrowIsSeald();
            if (_propertys.ContainsKey(name))
                throw new InvalidOperationException("Property is Regitrated");

            return new FluentPropertyRegistration<TData>(name, this);
        }

        private void GetPropertyValue(GetValueRequest obj)
        {
            Context.Sender.Tell(_propertys.TryGetValue(obj.Name, out var propertyData)
                ? new GetValueResponse(obj.Name, propertyData.PropertyBase.ObjectValue)
                : new GetValueResponse(obj.Name, null));
        }

        private void SetPropertyValue(SetValue obj)
        {
            if (!_propertys.TryGetValue(obj.Name, out var propertyData))
                return;

            var (_, value) = obj;

            if (Equals(propertyData.PropertyBase.ObjectValue, value)) return;

            propertyData.SetValue(value!);
        }

        private void PropertyValueChanged(PropertyData propertyData)
        {
            foreach (var actorRef in propertyData.Subscriptors)
            {
                actorRef.Tell(new PropertyChangedEvent(propertyData.PropertyBase.Name,
                    propertyData.PropertyBase.ObjectValue));
                _onPropertyChanged.OnNext(propertyData.PropertyBase.Name);
            }
        }

        private void TrackProperty(TrackPropertyEvent obj, IActorRef sender)
        {
            Log.Info("Track Property {Name}", obj.Name);

            if (!_propertys.TryGetValue(obj.Name, out var prop)) return;
            try
            {
                if (prop.Subscriptors.Contains(sender)) return;

                prop.Subscriptors.Add(sender);
                Context.WatchWith(sender, new PropertyTermination(Context.Sender, obj.Name));
            }
            finally
            {
                if (prop.PropertyBase.ObjectValue != null)
                {
                    sender.Tell(new PropertyChangedEvent(obj.Name, prop.PropertyBase.ObjectValue));
                    sender.Tell(new ValidatingEvent(prop.Error, obj.Name));
                }
            }
        }

        private void PropertyTerminationHandler(PropertyTermination obj)
        {
            if (!_propertys.TryGetValue(obj.Name, out var prop)) return;
            prop.Subscriptors.Remove(obj.ActorRef);
        }

        internal void RegisterProperty(UIPropertyBase prop)
        {
            var data = new PropertyData(prop);
            data.PropertyBase.PropertyValueChanged.Subscribe(_ => PropertyValueChanged(data)).DisposeWith(this);
            data.PropertyBase.Validator.Subscribe(err =>
            {
                data.Error = err;
                data.Subscriptors.ForEach(r => r.Tell(new ValidatingEvent(err, prop.Name)));
            }).DisposeWith(this);

            _propertys.Add(prop.Name, data);
        }

        public UIProperty<TData> Property<TData>(Expression<Func<UIProperty<TData>>> propName)
            => (UIProperty<TData>) _propertys[Reflex.PropertyName(propName)].PropertyBase;

        #endregion
    }
}