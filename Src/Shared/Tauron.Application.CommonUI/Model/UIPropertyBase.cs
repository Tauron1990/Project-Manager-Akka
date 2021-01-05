using System;
using System.Reactive;
using System.Reactive.Linq;
using JetBrains.Annotations;
using Tauron.Operations;

namespace Tauron.Application.CommonUI.Model
{
    [PublicAPI]
    public abstract class UIPropertyBase
    {
        public string Name { get; }

        public IObservable<bool> IsValid => Validator.Select(e => e == null);

        public abstract IObservable<Error?> Validator { get; }

        public abstract IObservable<Unit> PropertyValueChanged { get; }
        protected internal abstract object? ObjectValue { get; set; }

        protected internal abstract UIPropertyBase LockSet();

        public UIPropertyBase(string name) => Name = name;
    }

    //[PublicAPI]
    //public abstract class UIPropertyBaseOld
    //{
    //    private bool _isSetLocked;
    //    private readonly Lazy<RxVar<bool>> _isValid = new(() => false.ToRx());
        
    //    protected UIPropertyBase(string name)
    //    {
    //        Name = name;
    //        IsValidSetter = b => IsValid.Value = b;
    //    }

    //    internal Action<bool> IsValidSetter { get; }

    //    public string Name { get; }

    //    public RxVar<bool> IsValid => _isValid.Value;

    //    protected internal object? InternalValue { get; internal set; }
    //    internal Func<object?, string?>? Validator { get; set; }

    //    public event Action? PropertyValueChanged;

    //    internal event Action? PriorityChanged;

    //    internal UIPropertyBase LockSet()
    //    {
    //        _isSetLocked = true;
    //        return this;
    //    }

    //    protected internal void SetValue(object? value)
    //    {
    //        if (_isSetLocked) return;

    //        InternalValue = value;
    //        OnPropertyValueChanged();
    //    }

    //    private void OnPropertyValueChanged()
    //    {
    //        PriorityChanged?.Invoke();
    //        PropertyValueChanged?.Invoke();
    //    }
    //}
}