using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using JetBrains.Annotations;

namespace Tauron.ObservableExt
{
    [PublicAPI]
    public static class RxVar
    {
        public static RxVar<TData> ToRx<TData>(this TData data) => new(data);

        public static RxVal<TData> ToRxVal<TData>(this IObservable<TData> stream) => new(stream);
    }

    [PublicAPI]
    public sealed class RxVar<T> : IDisposable, IObservable<T>, IObserver<T>, IEquatable<RxVar<T>>, IEquatable<T>, IComparable<T>
    {
        private readonly IComparer<T> _comparer = Comparer<T>.Default;
        private readonly CompositeDisposable _disposable = new();
        private readonly IEqualityComparer<T> _equalityComparer = EqualityComparer<T>.Default;
        private readonly BehaviorSubject<T> _subject;

        public RxVar(T initial) => _subject = new BehaviorSubject<T>(initial).DisposeWith(_disposable);

        public bool IsDistinctMode { get; set; }

        public T Value
        {
            get => _subject.Value;
            set => ((IObserver<T>) this).OnNext(value);
        }

        public int CompareTo(T? other) => _comparer.Compare(Value, other);

        public void Dispose()
        {
            _disposable.Dispose();
        }

        public bool Equals(RxVar<T>? other) => other != null && _equalityComparer.Equals(Value, other.Value);

        public bool Equals(T? other) => _equalityComparer.Equals(Value, other);

        IDisposable IObservable<T>.Subscribe(IObserver<T> observer) => _subject.Subscribe(observer);

        void IObserver<T>.OnCompleted()
        {
            _subject.OnCompleted();
        }

        void IObserver<T>.OnError(Exception error)
        {
            _subject.OnError(error);
        }

        void IObserver<T>.OnNext(T value)
        {
            if (IsDistinctMode && Equals(value))
                return;

            _subject.OnNext(value);
        }

        public IDisposable ListenTo(IObserver<T> intrest) => _subject.Subscribe(intrest).DisposeWith(_disposable);

        public override int GetHashCode()
        {
            var val = Value;
            return val == null ? 0 : _equalityComparer.GetHashCode(val);
        }

        public override bool Equals(object? obj)
        {
            return obj switch
                   {
                       RxVar<T> rxVar => Equals(rxVar),
                       T data         => Equals(data),
                       null           => false,
                       _              => Equals(this, obj)
                   };
        }

        public static implicit operator T(RxVar<T> v) => v._subject.Value;

        public static bool operator ==(RxVar<T> left, T? right) => Equals(left, right);

        public static bool operator !=(RxVar<T> left, T? right) => !(left == right);

        public static bool operator >(RxVar<T> left, T right) => left.CompareTo(right) > 0;

        public static bool operator >=(RxVar<T> left, T right) => left.CompareTo(right) >= 0;

        public static bool operator <(RxVar<T> left, T right) => left.CompareTo(right) < 0;

        public static bool operator <=(RxVar<T> left, T right) => left.CompareTo(right) <= 0;

        public override string ToString() => Value?.ToString() ?? "<null>";
    }

    [PublicAPI]
    public sealed class RxVal<T> : IDisposable, IObservable<T?>, IEquatable<RxVal<T>>, IEquatable<T>, IComparable<T>, IConvertible
    {
        private readonly IComparer<T> _comparer = Comparer<T>.Default;
        private readonly CompositeDisposable _disposable = new();
        private readonly IEqualityComparer<T> _equalityComparer = EqualityComparer<T>.Default;
        private readonly BehaviorSubject<T?> _subject;

        public RxVal(IObservable<T> source)
        {
            _subject = new BehaviorSubject<T?>(default).DisposeWith(_disposable);
            source.Subscribe(_subject).DisposeWith(_disposable);
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

        public IDisposable ListenTo(IObserver<T?> intrest) => _subject.Subscribe(intrest).DisposeWith(_disposable);

        public override int GetHashCode()
        {
            var val = Value;
            return val == null ? 0 : _equalityComparer.GetHashCode(val);
        }

        public override bool Equals(object? obj)
        {
            return obj switch
                   {
                       RxVar<T> rxVar => Equals(rxVar),
                       T data         => Equals(data),
                       null           => false,
                       _              => Equals(this, obj)
                   };
        }

        public static implicit operator T?(RxVal<T> v) => v._subject.Value;

        public static bool operator ==(RxVal<T> left, T? right) => Equals(left, right);

        public static bool operator !=(RxVal<T> left, T? right) => !(left == right);

        public static bool operator >(RxVal<T> left, T right) => left.CompareTo(right) > 0;

        public static bool operator >=(RxVal<T> left, T right) => left.CompareTo(right) >= 0;

        public static bool operator <(RxVal<T> left, T right) => left.CompareTo(right) < 0;

        public static bool operator <=(RxVal<T> left, T right) => left.CompareTo(right) <= 0;

        public override string ToString() => Value?.ToString() ?? "<null>";

        #region IConvertible

        public TypeCode GetTypeCode() => TypeCode.Object;

        bool IConvertible.ToBoolean(IFormatProvider? provider) => Convert.ToBoolean(Value);

        byte IConvertible.ToByte(IFormatProvider? provider) => Convert.ToByte(Value);

        char IConvertible.ToChar(IFormatProvider? provider) => Convert.ToChar(Value);

        DateTime IConvertible.ToDateTime(IFormatProvider? provider) => Convert.ToDateTime(Value);

        decimal IConvertible.ToDecimal(IFormatProvider? provider) => Convert.ToDecimal(Value);

        double IConvertible.ToDouble(IFormatProvider? provider) => Convert.ToDouble(Value);

        short IConvertible.ToInt16(IFormatProvider? provider) => Convert.ToInt16(Value);

        int IConvertible.ToInt32(IFormatProvider? provider) => Convert.ToInt32(Value);

        long IConvertible.ToInt64(IFormatProvider? provider) => Convert.ToInt64(Value);

        sbyte IConvertible.ToSByte(IFormatProvider? provider) => Convert.ToSByte(Value);

        float IConvertible.ToSingle(IFormatProvider? provider) => Convert.ToSingle(Value);

        string IConvertible.ToString(IFormatProvider? provider) => Value?.ToString() ?? string.Empty;

        object IConvertible.ToType(Type conversionType, IFormatProvider? provider) => Convert.ChangeType(Value, conversionType)!;

        ushort IConvertible.ToUInt16(IFormatProvider? provider) => Convert.ToUInt16(Value);

        uint IConvertible.ToUInt32(IFormatProvider? provider) => Convert.ToUInt32(Value);

        ulong IConvertible.ToUInt64(IFormatProvider? provider) => Convert.ToUInt64(Value);

        #endregion
    }
}