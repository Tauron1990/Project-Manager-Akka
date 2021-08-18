using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace ServiceManager.Client.Shared.Configuration.ConditionEditor
{
    public class AddHelper : IDisposable
    {
        private readonly Subject<IAddReciever?> _changed = new();
        private readonly Subject<ElementItem> _remove = new();
        private IAddReciever? _addReciever;

        public IObservable<IAddReciever?> Changed => _changed.AsObservable();

        public IObservable<ElementItem> Remove => _remove.AsObservable();

        public IAddReciever? DefaultReciever { get; set; }

        public IAddReciever? AddReciever
        {
            get => _addReciever;
            set
            {
                _addReciever = value;
                _changed.OnNext(value);
            }
        }

        public void NewItem(ElementItem data)
        {
            if (AddReciever != null)
            {
                AddReciever.Add(data);
                return;
            }
            DefaultReciever?.Add(data);
        }

        public void RemoveItem(ElementItem item)
            => _remove.OnNext(item);

        public void Dispose()
        {
            _changed.Dispose();
            _remove.Dispose();
        }
    }
}