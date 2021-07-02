using System;
using System.Collections.Specialized;
using System.ComponentModel;

namespace ServiceManager.Client.Components
{
    public class PropertyChangedComponent : DisposableComponent
    {
        public void Track(INotifyPropertyChanged changed)
        {
            changed.PropertyChanged += OnPropertyChanged;
            AddResource(Disposables.Create(() => changed.PropertyChanged -= OnPropertyChanged));
        }

        public void Track(INotifyCollectionChanged changed)
        {
            changed.CollectionChanged += OnPropertyChanged;
            AddResource(Disposables.Create(() => changed.CollectionChanged -= OnPropertyChanged));
        }

        private void OnPropertyChanged(object? sender, EventArgs e) => InvokeAsync(StateHasChanged);
    }
}