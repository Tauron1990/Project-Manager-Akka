using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using JetBrains.Annotations;
using Tauron.Operations;

namespace Tauron.Application.CommonUI.Model
{
    [PublicAPI]
    public sealed class UIProperty<TData> : UIPropertyBase, IObservable<TData>, IDisposable
    {
        private readonly BehaviorSubject<Error?> _currentError = new(null);
        private readonly BehaviorSubject<TData> _currentValue = new(default!);
        private readonly CompositeDisposable _disposable = new();

        private bool _isLocked;

        public UIProperty(string name) : base(name)
        {
            _disposable.Add(_currentError);
            _disposable.Add(_currentValue);
        }

        public override IObservable<Error?> Validator => _currentError;

        public override IObservable<Unit> PropertyValueChanged => _currentValue.Select(_ => Unit.Default);

        protected internal override object? ObjectValue
        {
            get => Value;
            set
            {
                if (value is TData dat)
                    Set(dat);
            }
        }

        public IObservable<TData> PropertyValueChangedData => _currentValue.AsObservable();

        public TData Value => _currentValue.Value;

        public void Dispose()
        {
            _disposable.Dispose();
        }

        public IDisposable Subscribe(IObserver<TData> observer) => _currentValue.Subscribe(observer);

        public void SetValidator(Func<IObservable<TData>, IObservable<Error?>> validator)
        {
            _disposable.Add(validator(PropertyValueChangedData).Subscribe(_currentError));
        }

        protected internal override UIPropertyBase LockSet()
        {
            _isLocked = true;
            return this;
        }

        public void Set(TData data)
        {
            if (_isLocked) return;

            _currentValue.OnNext(data);
        }

        internal UIProperty<TData> ForceSet(TData data)
        {
            _currentValue.OnNext(data);
            return this;
        }

        public static implicit operator TData(UIProperty<TData> property) => property.Value;

        public static UIProperty<TData> operator +(UIProperty<TData> prop, TData data)
        {
            prop.Set(data);
            return prop;
        }

        public static bool operator ==(UIProperty<TData> prop, TData data) => Equals(prop.Value, data);

        public static bool operator !=(UIProperty<TData> prop, TData data) => !Equals(prop.Value, data);

        public override int GetHashCode() => Value?.GetHashCode() ?? 0;

        public override bool Equals(object? obj)
        {
            return obj switch
            {
                UIProperty<TData> prop => Equals(prop.Value, Value),
                TData val => Equals(val, Value),
                _ => false
            };
        }

        public override string ToString() => Value?.ToString() ?? "null--" + typeof(TData);
    }

//    [PublicAPI]
//#pragma warning disable CS0660 // Typ definiert Operator == oder Operator !=, überschreibt jedoch nicht Object.Equals(Objekt o)
//#pragma warning disable CS0661 // Typ definiert Operator == oder Operator !=, überschreibt jedoch nicht Object.GetHashCode()
//    public sealed class UIPropertyOld<TData> : UIPropertyBase
//#pragma warning restore CS0661 // Typ definiert Operator == oder Operator !=, überschreibt jedoch nicht Object.GetHashCode()
//#pragma warning restore CS0660 // Typ definiert Operator == oder Operator !=, überschreibt jedoch nicht Object.Equals(Objekt o)
//    {
//        internal UIProperty(string name)
//            : base(name)
//        {
//            PropertyValueChanged += () => PropertyValueChangedFunc?.Invoke(Value);
//        }

//        public TData Value
//        {
//            [return: MaybeNull] get => InternalValue is TData data ? data : default!;
//        }

//        public event Action<TData>? PropertyValueChangedFunc;

//        public void Set([AllowNull] TData data)
//        {
//            SetValue(data);
//        }

//        [return: MaybeNull]
//        public static implicit operator TData(UIProperty<TData> property)
//        {
//            return property.Value;
//        }

//        public static UIProperty<TData> operator +(UIProperty<TData> prop, TData data)
//        {
//            prop.Set(data);
//            return prop;
//        }

//        public static bool operator ==(UIProperty<TData> prop, TData data)
//        {
//            return Equals(prop.Value, data);
//        }

//        public static bool operator !=(UIProperty<TData> prop, TData data)
//        {
//            return !Equals(prop.Value, data);
//        }

//        public override string ToString()
//        {
//            return Value?.ToString() ?? "null--" + typeof(TData);
//        }
//    }
}