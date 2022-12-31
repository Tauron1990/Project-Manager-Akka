namespace SimpleProjectManager.Shared;

#pragma warning disable MA0018
public interface IStringValueType<TSelf>
    where TSelf : IStringValueType<TSelf>
{
    static abstract TSelf GetEmpty { get; }

    string Value { get; }

    static abstract TSelf From(string value);

    static abstract bool operator ==(TSelf left, string right);

    static abstract bool operator !=(TSelf left, string right);
}