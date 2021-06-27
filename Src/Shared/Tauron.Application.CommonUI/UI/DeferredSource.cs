using System;
using System.Collections;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Akka.Actor;
using JetBrains.Annotations;
using Tauron.Application.CommonUI.Helper;
using Tauron.Application.CommonUI.ModelMessages;

namespace Tauron.Application.CommonUI.UI
{
    public class DeferredSource : ModelConnectorBase<DeferredSource>, INotifyPropertyChanged, INotifyDataErrorInfo
    {
        private bool _hasErrors;
        private object? _value;
        private string? _error;

        public DeferredSource(string name, DataContextPromise promise)
            : base(name, promise)
        {
        }

        public object? Value
        {
            get => _value;
            set
            {
                if (Equals(_value, value)) return;
                _value = value;
                Model?.Actor.Tell(new SetValue(Name, value));
            }
        }

        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

        public string? Error
        {
            get => _error;
            private set
            {
                if (value == _error) return;
                _error = value;
                OnPropertyChanged();
            }
        }

        public bool HasErrors
        {
            get => _hasErrors;
            private set
            {
                if (value == _hasErrors) return;
                _hasErrors = value;
                OnPropertyChanged();
            }
        }

        public IEnumerable GetErrors(string? propertyName)
        {
            yield return Error;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected override void PropertyChangedHandler(PropertyChangedEvent msg)
        {
            if (Equals(_value, msg.Value)) return;
            _value = msg.Value;
            OnPropertyChanged(nameof(Value));
        }

        protected override void NoDataContextFound()
        {
            Log.Debug("No DataContext Found for {Name}", Name);
        }

        protected override void ValidateCompled(ValidatingEvent msg)
        {
            Error = msg.Reason?.Info;
            HasErrors = msg.Error;

            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(nameof(Value)));
        }

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string? propertyName = null) 
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}