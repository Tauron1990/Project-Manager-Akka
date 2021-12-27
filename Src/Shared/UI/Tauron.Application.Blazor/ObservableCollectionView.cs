using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Components;

namespace Tauron.Application.Blazor;

public partial class ObservableCollectionView<TItem> : INotifyPropertyChanged
{
    private IDisposable _subscription = Disposable.Empty;
    private IEnumerable<TItem>? _source;
    
    [Parameter]
    public RenderFragment<IEnumerable<TItem>>? ListRender { get; set; }
    
    [Parameter]
    public RenderFragment? EmptyRender { get; set; }
    
    [Parameter]
    public IEnumerable<TItem>? SourceParameter { get; set; }

    public IEnumerable<TItem>? Source
    {
        get => _source;
        set
        {
            if (Equals(value, _source)) return;

            _source = value;
            OnPropertyChanged();
            RenderingManager.StateHasChangedAsync().Ignore();
        }
    }

    private IEnumerable<TItem> NonNullSource => Source ?? SourceParameter ?? Array.Empty<TItem>();

    protected override void OnParametersSet()
    {
        UpdateSubsription();
        base.OnParametersSet();
    }

    private void UpdateSubsription()
    {
        RemoveResource(_subscription);

        if (NonNullSource is INotifyCollectionChanged collectionChanged)
        {
            _subscription = Observable.FromEventPattern<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(
                    h => (sender, args) => h(sender, args),
                    h => collectionChanged.CollectionChanged += h,
                    h => collectionChanged.CollectionChanged -= h)
               .SelectMany(
                    async _ =>
                    {
                        await RenderingManager.StateHasChangedAsync();
                        return Unit.Default;
                    })
               .Subscribe()
               .DisposeWith(this);
        }

    }
    
    public event PropertyChangedEventHandler? PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        if(propertyName == nameof(Source))
            UpdateSubsription();
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}