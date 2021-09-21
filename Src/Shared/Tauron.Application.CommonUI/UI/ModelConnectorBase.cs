using System;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using JetBrains.Annotations;
using NLog;
using Tauron.Akka;
using Tauron.Application.CommonUI.Helper;
using Tauron.Application.CommonUI.ModelMessages;

namespace Tauron.Application.CommonUI.UI
{
    [PublicAPI]
    public abstract class ModelConnectorBase<TDrived>
    {
        protected readonly ILogger Log = LogManager.GetCurrentClassLogger(typeof(TDrived));
        private CompositeDisposable _disposable = new();

        private IEventActor? _eventActor;

        private int _isInitializing = 1;

        protected ModelConnectorBase(string name, DataContextPromise promise)
        {
            Name = name;

            promise.OnUnload(
                () =>
                {
                    _disposable.Dispose();
                    OnUnload();
                });

            promise.OnNoContext(NoDataContextFound);

            promise
               .OnContext(
                    (model, view) =>
                    {
                        View = view;
                        Model = model;

                        if (model.IsInitialized)
                        {
                            Task.Run(async () => await InitAsync())
                               .Ignore();
                        }
                        else
                        {
                            void OnModelOnInitialized()
                            {
                                Task.Run(async () => await InitAsync())
                                   .Ignore();
                            }

                            model.AwaitInit(OnModelOnInitialized);
                        }
                    });
        }

        protected string Name { get; }
        protected IViewModel? Model { get; private set; }

        protected IView? View { get; private set; }

        protected int IsInitializing => _isInitializing;

        private async Task InitAsync()
        {
            try
            {
                Log.Debug("Init ModelConnector {Type} -- {Name}", typeof(TDrived), Name);

                if (Model == null) return;

                OnLoad();

                //Log.Information("Ask For {Property}", _name);
                var eventActor = await Model.Actor.Ask<IEventActor>(new MakeEventHook(Name), TimeSpan.FromSeconds(15));
                //Log.Information("Ask Compled For {Property}", _name);

                _disposable.Add(await eventActor.Register(HookEvent.Create<PropertyChangedEvent>(PropertyChangedHandler)));
                _disposable.Add(await eventActor.Register(HookEvent.Create<ValidatingEvent>(ValidateCompled)));

                Model.Actor.Tell(new TrackPropertyEvent(Name), eventActor.OriginalRef);

                Interlocked.Exchange(ref _eventActor, eventActor);
                Interlocked.Exchange(ref _isInitializing, 0);
            }
            catch (Exception e)
            {
                Log.Error(e, "Error Bind Property {Name}", Name);
            }
        }

        protected virtual void OnUnload()
            => Log.Debug("Unload ModelConnector {Type} -- {Name}", typeof(TDrived), Name);

        protected virtual void OnLoad()
            => Log.Debug("Load ModelConnector {Type} -- {Name}", typeof(TDrived), Name);

        protected virtual void OnViewFound(IView view)
            => Log.Debug("View Found ModelConnector {Type} -- {Name}", typeof(TDrived), Name);

        public void ForceUnload()
        {
            if (Model == null)
                return;

            OnUnload();
            Model = null;
            _eventActor?.OriginalRef.Tell(PoisonPill.Instance);
        }

        protected abstract void NoDataContextFound();

        protected abstract void ValidateCompled(ValidatingEvent obj);

        protected abstract void PropertyChangedHandler(PropertyChangedEvent obj);
    }
}