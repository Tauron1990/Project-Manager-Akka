namespace Tauron.Localization;

public abstract class LocEmlement<TValue>
{
    protected LocEmlement(string key, TValue value)
    {
        Key = key;
        #pragma warning disable MA0056
        Value = value;
        #pragma warning restore MA0056
    }

    public string Key { get; }

    public virtual TValue Value { get; }
}