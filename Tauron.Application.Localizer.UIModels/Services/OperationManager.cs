﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Threading;
using Tauron.Application.Localizer.UIModels.lang;

namespace Tauron.Application.Localizer.UIModels.Services
{
    public sealed class OperationManager : IOperationManager
    {
        private readonly Dispatcher _dispatcher;

        private readonly LocLocalizer _localizer;
        private readonly OperationList _operations = new OperationList();

        public OperationManager(LocLocalizer localizer, Dispatcher dispatcher)
        {
            _localizer = localizer;
            _dispatcher = dispatcher;
        }

        public IEnumerable<RunningOperation> RunningOperations => _operations;

        public OperationController StartOperation(string name)
        {
            return _dispatcher.Invoke(() =>
            {
                var op = new RunningOperation(Guid.NewGuid().ToString(), name) {Status = _localizer.OperationControllerRunning};
                if (_operations.Count > 15)
                    Clear();

                _operations.Add(op);
                return new OperationController(op, _localizer, OperationChanged);
            });
        }

        public OperationController? Find(string id)
        {
            return _dispatcher.Invoke(() =>
            {
                var op = _operations.FirstOrDefault(op => op.Key == id);
                return op == null ? null : new OperationController(op, _localizer, OperationChanged);
            });
        }

        public bool ShouldClear()
        {
            return _operations.Any(op => op.Operation == OperationStatus.Success);
        }

        public void Clear()
        {
            _dispatcher.Invoke(() =>
            {
                foreach (var operation in _operations.Where(op => op.Operation == OperationStatus.Success).ToArray())
                    _operations.Remove(operation);
            });
        }

        public bool ShouldCompledClear()
        {
            return _operations.Any(op => op.Operation != OperationStatus.Running);
        }

        public void CompledClear()
        {
            _dispatcher.Invoke(() =>
            {
                foreach (var operation in _operations.Where(op => op.Operation != OperationStatus.Running).ToArray())
                    _operations.Remove(operation);
            });
        }

        private void OperationChanged()
        {
            _dispatcher.Invoke(_operations.OperationStatusCchanged);
        }

        private sealed class OperationList : ObservableCollection<RunningOperation>
        {
            public int RunningOperations => this.Sum(ro => ro.Operation == OperationStatus.Running ? 1 : 0);

            protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
            {
                base.OnCollectionChanged(e);
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(RunningOperations)));
            }

            public void OperationStatusCchanged()
            {
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(RunningOperations)));
            }
        }
    }
}