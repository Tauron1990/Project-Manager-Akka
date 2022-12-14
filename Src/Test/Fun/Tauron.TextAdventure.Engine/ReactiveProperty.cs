using System.ComponentModel;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace Tauron.TextAdventure.Engine;

/// <summary>
/// A type that holds a value that can be subscribed against. Also this wrapper implements
/// INotifyPropertyChanged against the wrapped value for data-binding.
/// </summary>
/// <typeparam name="T"></typeparam>
[PublicAPI]
#pragma warning disable MA0097
public class ReactiveProperty<T> : INotifyPropertyChanged, IObservable<T>, IDisposable, IComparable
    #pragma warning restore MA0097
{
    private readonly BehaviorSubject<T> _valueObservable;

    public ReactiveProperty(T initialValue)
        => _valueObservable = new BehaviorSubject<T>(initialValue);

    public T Value
    {
        get => _valueObservable.Value;
        set
        {
            if(Equals(_valueObservable.Value, value))
                return;

            _valueObservable.OnNext(value);
            OnPropertyChanged();
        }
    }

    public static implicit operator T(ReactiveProperty<T> reactiveProperty) => reactiveProperty.Value;

    public IDisposable Subscribe(IObserver<T> observer) => _valueObservable.Subscribe(observer);

    public virtual void Dispose()
    {
        _valueObservable.Dispose();
        GC.SuppressFinalize(this);
    }

    public override string ToString() => Value?.ToString() ?? string.Empty;

    public int CompareTo(object? obj)
    {
        if (_valueObservable.Value is IComparable comparable)
            return comparable.CompareTo(obj);

        throw new InvalidOperationException($"The underlying type {(typeof(T)).FullName} does not implement IComparable so a comparison is not possible.");
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) 
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}