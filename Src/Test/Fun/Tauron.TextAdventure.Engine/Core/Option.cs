using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace SpaceConqueror.Core;

[StructLayout(LayoutKind.Auto)]
[PublicAPI]
public readonly struct Option<TData>
{
    #pragma warning disable MA0018
    public static Option<TData> None => default;
    #pragma warning restore MA0018

    private readonly TData? _data;

    public bool HasValue { get; }

    public TData Value
    {
        get
        {
            if(HasValue) return _data!;

            throw new InvalidOperationException("The Option has no Value");
        }
    }

    private Option(TData data, bool hasValue)
    {
        _data = data;
        HasValue = hasValue;
    }

    public static implicit operator Option<TData>(TData data)
        => new(data, hasValue: true);

    public TResult Map<TResult>(Func<TData, TResult> selector, Func<TResult> defaultValue)
        => HasValue ? selector(_data!) : defaultValue();

    public TData Map(Func<TData> defaultValue)
        => HasValue ? _data! : defaultValue();

    public TResult Map<TResult>(Func<TData, TResult> selector, TResult defaultValue)
        => HasValue ? selector(_data!) : defaultValue;

    public TData Map(TData defaultValue)
        => HasValue ? _data! : defaultValue;
}