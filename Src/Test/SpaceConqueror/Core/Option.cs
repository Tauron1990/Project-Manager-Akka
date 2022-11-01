namespace SpaceConqueror.Core;

public readonly struct Option<TData>
{
    public static Option<TData> None => default;

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
        => new(data, true);
    
    public TResult Select<TResult>()
}