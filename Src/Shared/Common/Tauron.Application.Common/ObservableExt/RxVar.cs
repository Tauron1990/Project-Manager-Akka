using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Subjects;

namespace Tauron.ObservableExt;

[PublicAPI]
public static class RxVar
{
    public static RxVar<TData> ToRx<TData>(this TData data) => new(data);

    public static RxVal<TData> ToRxVal<TData>(this IObservable<TData> stream) => new(stream);
}

[PublicAPI]
public sealed class RxVar<T> : IDisposable, IObservable<T>, IObserver<T>, IEquatable<RxVar<T>>, IEquatable<T>,
    IComparable<T>
{
    private readonly IComparer<T> _comparer = Comparer<T>.Default;
    private readonly CompositeDisposable _disposable = new();
    private readonly IEqualityComparer<T> _equalityComparer = EqualityComparer<T>.Default;
    private readonly BehaviorSubject<T> _subject;

    public RxVar(T initial)
    {
        _subject = new BehaviorSubject<T>(initial);
        _disposable.Add(_subject);
    }

    public bool IsDistinctMode { get; set; }

    public T Value
    {
        get => _subject.Value;
        set => ((IObserver<T>)this).OnNext(value);
    }

    public int CompareTo(T? other) => _comparer.Compare(Value, other);

    public void Dispose()
    {
        _disposable.Dispose();
    }

    public bool Equals(RxVar<T>? other) => other != null && _equalityComparer.Equals(Value, other.Value);

    public bool Equals(T? other) => _equalityComparer.Equals(Value, other);

    IDisposable IObservable<T>.Subscribe(IObserver<T> observer) => _subject.Subscribe(observer);

    void IObserver<T>.OnCompleted() => _subject.OnCompleted();

    void IObserver<T>.OnError(Exception error) => _subject.OnError(error);

    void IObserver<T>.OnNext(T value)
    {
        if(IsDistinctMode && Equals(value))
            return;

        _subject.OnNext(value);
    }

    public IDisposable ListenTo(IObserver<T> intrest)
    {
        IDisposable result = _subject.Subscribe(intrest);
        _disposable.Add(result);

        return Disposable.Create((result, _disposable), d => d._disposable.Remove(d.result));
    }

    public override int GetHashCode()
    {
        T val = Value;

        return val == null ? 0 : _equalityComparer.GetHashCode(val);
    }

    public override bool Equals(object? obj)
    {
        return obj switch
        {
            RxVar<T> rxVar => Equals(rxVar),
            T data => Equals(data),
            null => false,
            _ => Equals(this, obj),
        };
    }

    public static implicit operator T(RxVar<T> rxVar) => rxVar._subject.Value;

    public static bool operator ==(RxVar<T> left, T? right) => Equals(left, right);

    public static bool operator !=(RxVar<T> left, T? right) => !(left == right);

    public static bool operator >(RxVar<T> left, T right) => left.CompareTo(right) > 0;

    public static bool operator >=(RxVar<T> left, T right) => left.CompareTo(right) >= 0;

    public static bool operator <(RxVar<T> left, T right) => left.CompareTo(right) < 0;

    public static bool operator <=(RxVar<T> left, T right) => left.CompareTo(right) <= 0;

    public override string ToString() => Value?.ToString() ?? "<null>";
}