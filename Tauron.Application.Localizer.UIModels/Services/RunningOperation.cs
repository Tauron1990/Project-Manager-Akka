﻿namespace Tauron.Application.Localizer.UIModels.Services
{
    public enum OperationStatus
    {
        Running,
        Failed,
        Success
    }

    public sealed class RunningOperation : ObservableObject
    {
        private string? _status;
        private OperationStatus _operation;

        public string Key { get; }

        public string Name { get; }


        public RunningOperation(string key, string name)
        {
            Key = key;
            Name = name;
        }

        public string? Status
        {
            get => _status;
            set
            {
                if (value == _status) return;
                _status = value;
                OnPropertyChanged();
            }
        }

        public OperationStatus Operation
        {
            get => _operation;
            set
            {
                if (value == _operation) return;
                _operation = value;
                OnPropertyChanged();
            }
        }
    }
}