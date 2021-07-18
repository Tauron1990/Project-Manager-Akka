using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading.Tasks;

namespace ServiceManager.Client.Components
{
    public class PropertyChangedComponent : DisposableComponent
    {
        protected async Task Track(INotifyPropertyChanged changed)
        {
            await Init(changed);
            changed.PropertyChanged += OnPropertyChanged;
            AddResource(Disposables.Create(() => changed.PropertyChanged -= OnPropertyChanged));
        }

        protected async Task Track(INotifyCollectionChanged changed)
        {
            await Init(changed);
            changed.CollectionChanged += OnPropertyChanged;
            AddResource(Disposables.Create(() => changed.CollectionChanged -= OnPropertyChanged));
        }
        
        public static async Task Init(object obj)
        {
            if (obj is IInitable model)
                await model.Init();
        }

        private void OnPropertyChanged(object? sender, EventArgs e)
            => StateHasChanged();
        //=> InvokeAsync(StateHasChanged);
    }
}