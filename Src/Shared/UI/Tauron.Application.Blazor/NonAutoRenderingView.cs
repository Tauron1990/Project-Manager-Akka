using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Components;
using ReactiveUI;
using Stl.Collections;
using Stl.DependencyInjection;
using Tauron.Application.Blazor.Parameters;

namespace Tauron.Application.Blazor;

[PublicAPI]
public abstract class NonAutoRenderingView<TModel> : DisposableComponent, IViewFor<TModel>, ICanActivate, IHasServices, IReactiveObject
    where TModel : class, INotifyPropertyChanged
{
    private PropertyChangingEventHandler? _changingEventHandler;
    public RenderingManager RenderingManager { get; } = new();

    [Inject]
    public IServiceProvider Services { get; set; } = ImmutableOptionSet.Empty;

    event PropertyChangingEventHandler? INotifyPropertyChanging.PropertyChanging
    {
        add => _changingEventHandler = _changingEventHandler.Combine(value);
        remove => _changingEventHandler = _changingEventHandler.Remove(value);
    }

    void IReactiveObject.RaisePropertyChanging(PropertyChangingEventArgs args)
        => _changingEventHandler?.Invoke(this, args);

    void IReactiveObject.RaisePropertyChanged(PropertyChangedEventArgs args)
        => PropertyChanged?.Invoke(this, args);

    protected override void OnInitialized()
    {
        this.WhenActivated(InitializeModel);

        RenderingManager.Init(StateHasChanged, InvokeAsync);
        ViewModel = CreateModel();

        //_initSubject.OnNext(Unit.Default);
        base.OnInitialized();
    }

    public override async Task SetParametersAsync(ParameterView parameters)
    {
        await base.SetParametersAsync(parameters);

        if(ViewModel is IParameterUpdateable updateable)
            updateable.Updater.UpdateParameters(parameters);
    }

    protected virtual TModel CreateModel()
        => Services.GetIsolatedService<TModel>().DisposeWith(this);

    protected virtual IEnumerable<IDisposable> InitializeModel()
        => Array.Empty<IDisposable>();

    protected override bool ShouldRender()
        => RenderingManager.CanRender;

    public ValueTask PerformTask(Action init, Action compled, Func<ValueTask<bool>> run)
        => RenderingManager.PerformTask(init, compled, run);

    public ValueTask PerformTask(Action init, Action compled, Func<ValueTask> run)
        => RenderingManager.PerformTask(init, compled, run);

    #region ViewFor

    private readonly Subject<Unit> _initSubject = new();

    [SuppressMessage("Design", "CA2213: Dispose object", Justification = "Used for deactivation.")]
    private readonly Subject<Unit> _deactivateSubject = new();

    private TModel? _viewModel;

    private bool _disposedValue; // To detect redundant calls

    public event PropertyChangedEventHandler? PropertyChanged;

    public TModel? ViewModel
    {
        get => _viewModel;
        set
        {
            if(EqualityComparer<TModel>.Default.Equals(_viewModel, value))
                return;

            _viewModel = value;
            OnPropertyChanged();
        }
    }

    /// <inheritdoc />
    object? IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = (TModel)value;
    }

    /// <inheritdoc />
    public IObservable<Unit> Activated => _initSubject.AsObservable();

    /// <inheritdoc />
    public IObservable<Unit> Deactivated => _deactivateSubject.AsObservable();

    protected override void OnAfterRender(bool firstRender)
    {
        if(firstRender)
        {
            _initSubject.OnNext(Unit.Default);

            // The following subscriptions are here because if they are done in OnInitialized, they conflict with certain JavaScript frameworks.
            var viewModelChanged =
                this.WhenAnyValue(x => x.ViewModel)
                   .Where(x => x is not null)
                   .Publish()
                   .RefCount(2);

            viewModelChanged
                #pragma warning disable CS4014
               .Subscribe(_ => RenderingManager.StateHasChangedAsync())
               .DisposeWith(this);

            viewModelChanged
               .WhereNotNull()
               .Select(
                    x =>
                    {
                        return Observable
                           .FromEvent<PropertyChangedEventHandler, Unit>(
                                eventHandler =>
                                {
                                    void Handler(object? sender, PropertyChangedEventArgs e) => eventHandler(Unit.Default);

                                    return Handler;
                                },
                                eh => x.PropertyChanged += eh,
                                eh => x.PropertyChanged -= eh);
                    })
               .Switch()
               .Subscribe(_ => RenderingManager.StateHasChangedAsync())
                #pragma warning restore CS4014
               .DisposeWith(this);
        }

        base.OnAfterRender(firstRender);
    }

    /// <summary>
    ///     Invokes the property changed event.
    /// </summary>
    /// <param name="propertyName">The name of the property.</param>
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    /// <summary>
    ///     Cleans up the managed resources of the object.
    /// </summary>
    /// <param name="disposing">If it is getting called by the Dispose() method rather than a finalizer.</param>
    protected override void Dispose(bool disposing)
    {
        if(!_disposedValue)
        {
            if(disposing)
            {
                _initSubject.Dispose();
                _deactivateSubject.OnNext(Unit.Default);

                if(_viewModel is IDisposable disposable)
                    disposable.Dispose();
            }

            _disposedValue = true;
        }

        base.Dispose(disposing);
    }

    #endregion
}