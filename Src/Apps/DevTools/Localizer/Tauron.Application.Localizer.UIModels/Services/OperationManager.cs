using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using DynamicData.Binding;
using Tauron.Application.CommonUI.AppCore;
using Tauron.Application.Localizer.UIModels.lang;

namespace Tauron.Application.Localizer.UIModels.Services
{
    public sealed class OperationManager : IOperationManager
    {
        private readonly IUIDispatcher _dispatcher;
        private readonly Subject<RunningOperation> _fail = new();

        private readonly LocLocalizer _localizer;
        private readonly OperationList _operations = new();

        public OperationManager(LocLocalizer localizer, IUIDispatcher dispatcher)
        {
            _localizer = localizer;
            _dispatcher = dispatcher;
        }

        public IObservable<RunningOperation> OperationFailed => _fail.AsObservable();
        public IEnumerable<RunningOperation> RunningOperations => _operations;

        public IObservable<OperationController> StartOperation(string name)
        {
            return _dispatcher.InvokeAsync(() =>
            {
                var op = new RunningOperation(Guid.NewGuid().ToString(), name)
                    {Status = _localizer.OperationControllerRunning};
                if (_operations.Count > 15)
                    Clear();

                _operations.Add(op);
                return new OperationController(op, _localizer, OperationChanged, _fail);
            });
        }

        public IObservable<OperationController?> Find(string id)
        {
            return _dispatcher.InvokeAsync(() =>
            {
                var op = _operations.FirstOrDefault(ro => ro.Key == id);
                return op == null ? null : new OperationController(op, _localizer, OperationChanged, _fail);
            });
        }

        public IObservable<bool> ShouldClear()
        {
            return _operations.WhenPropertyChanged(l => l.RunningOperations, notifyOnInitialValue: false)
                .Select(v => v.Sender.Count != v.Value);
        }

        public void Clear()
        {
            _dispatcher.InvokeAsync(() =>
            {
                foreach (var operation in _operations.Where(op => op.Operation == OperationStatus.Success).ToArray())
                    _operations.Remove(operation);
            }).Ignore();
        }

        public IObservable<bool> ShouldCompledClear()
        {
            return _operations.WhenPropertyChanged(l => l.RunningOperations).Select(p => ShouldCompledClear(p.Sender));
        }

        public void CompledClear()
        {
            _dispatcher.InvokeAsync(() =>
            {
                foreach (var operation in _operations.Where(op => op.Operation != OperationStatus.Running).ToArray())
                    _operations.Remove(operation);
            }).Ignore();
        }


        private static bool ShouldCompledClear(OperationList list)
        {
            return list.Any(op => op.Operation != OperationStatus.Running);
        }

        private void OperationChanged()
            => _dispatcher.InvokeAsync(_operations.OperationStatusCchanged).Ignore();

        private sealed class OperationList : ObservableCollection<RunningOperation>
        {
            internal int RunningOperations => this.Sum(ro => ro.Operation == OperationStatus.Running ? 1 : 0);

            protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
            {
                base.OnCollectionChanged(e);
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(RunningOperations)));
            }

            internal void OperationStatusCchanged()
            {
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(RunningOperations)));
            }
        }
    }
}