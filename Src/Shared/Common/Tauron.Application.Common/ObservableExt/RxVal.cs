using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using JetBrains.Annotations;

namespace Tauron.ObservableExt;

[PublicAPI]
public sealed class RxVal<T> : IDisposable, IObservable<T?>, IEquatable<RxVal<T>>, IEquatable<T>, IComparable<T>,
    IConvertible
{
    private readonly IComparer<T> _comparer = Comparer<T>.Default;
    private readonly CompositeDisposable _disposable = new();
    private readonly IEqualityComparer<T> _equalityComparer = EqualityComparer<T>.Default;
    private readonly BehaviorSubject<T?> _subject;

    public RxVal(IObservable<T> source)
    {
        _subject = new BehaviorSubject<T?>(default);
        IDisposable sub = source.Subscribe(_subject);
        _disposable.Add(_subject);
        _disposable.Add(sub);
    }

    public T? Value => _subject.Value;

    public int CompareTo(T? other) => _comparer.Compare(Value, other);

    public void Dispose()
    {
        _disposable.Dispose();
    }

    public bool Equals(RxVal<T>? other) => other != null && _equalityComparer.Equals(Value, other.Value);

    public bool Equals(T? other) => _equalityComparer.Equals(Value, other);

    IDisposable IObservable<T?>.Subscribe(IObserver<T?> observer) => _subject.Subscribe(observer);

    public IDisposable ListenTo(IObserver<T?> intrest)
    {
        IDisposable result = _subject.Subscribe(intrest);

        return Disposable.Create((result, _disposable), s => s._disposable.Remove(s.result));
    }

    public override int GetHashCode()
    {
        T? val = Value;

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

    public static implicit operator T?(RxVal<T> rxVal) => rxVal._subject.Value;

    public static bool operator ==(RxVal<T> left, T? right) => Equals(left, right);

    public static bool operator !=(RxVal<T> left, T? right) => !(left == right);

    public static bool operator >(RxVal<T> left, T right) => left.CompareTo(right) > 0;

    public static bool operator >=(RxVal<T> left, T right) => left.CompareTo(right) >= 0;

    public static bool operator <(RxVal<T> left, T right) => left.CompareTo(right) < 0;

    public static bool operator <=(RxVal<T> left, T right) => left.CompareTo(right) <= 0;

    public override string ToString() => Value?.ToString() ?? "<null>";

    #pragma warning disable AV2407

    #region IConvertible

    #pragma warning restore AV2407

    public TypeCode GetTypeCode() => TypeCode.Object;

    bool IConvertible.ToBoolean(IFormatProvider? provider) => Convert.ToBoolean(Value, provider);

    byte IConvertible.ToByte(IFormatProvider? provider) => Convert.ToByte(Value, provider);

    char IConvertible.ToChar(IFormatProvider? provider) => Convert.ToChar(Value, provider);

    DateTime IConvertible.ToDateTime(IFormatProvider? provider) => Convert.ToDateTime(Value, provider);

    decimal IConvertible.ToDecimal(IFormatProvider? provider) => Convert.ToDecimal(Value, provider);

    double IConvertible.ToDouble(IFormatProvider? provider) => Convert.ToDouble(Value, provider);

    short IConvertible.ToInt16(IFormatProvider? provider) => Convert.ToInt16(Value, provider);

    int IConvertible.ToInt32(IFormatProvider? provider) => Convert.ToInt32(Value, provider);

    long IConvertible.ToInt64(IFormatProvider? provider) => Convert.ToInt64(Value, provider);

    sbyte IConvertible.ToSByte(IFormatProvider? provider) => Convert.ToSByte(Value, provider);

    float IConvertible.ToSingle(IFormatProvider? provider) => Convert.ToSingle(Value, provider);

    string IConvertible.ToString(IFormatProvider? provider) => Value?.ToString() ?? string.Empty;

    object IConvertible.ToType(Type conversionType, IFormatProvider? provider)
        => Convert.ChangeType(Value, conversionType, provider)!;

    ushort IConvertible.ToUInt16(IFormatProvider? provider) => Convert.ToUInt16(Value, provider);

    uint IConvertible.ToUInt32(IFormatProvider? provider) => Convert.ToUInt32(Value, provider);

    ulong IConvertible.ToUInt64(IFormatProvider? provider) => Convert.ToUInt64(Value, provider);

    #endregion
}