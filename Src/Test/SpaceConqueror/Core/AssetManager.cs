using System.Collections.Concurrent;

namespace SpaceConqueror.Core;

public sealed class AssetManager
{
    private readonly ConcurrentDictionary<string, IBox?> _assets = new();

    internal AssetManager(){}

    public void Add<TData>(string name, Func<TData> data)
    {
        if(_assets.TryAdd(name, new Box<TData>(new Lazy<TData>(data))))
            return;

        throw new InvalidOperationException($"An Resource with Name {name} is already registrated");
    }

    public TData Get<TData>(string name)
    {
        if(_assets.TryGetValue(name, out IBox? toCast) && toCast is Box<TData> box)
            return box.Lazy.Value;

        throw new InvalidOperationException("The Specific Asse tis not found or has anathor Type");
    }

    public Option<TData> TryGet<TData>(string name)
    {
        if(_assets.TryGetValue(name, out IBox? value) && value is Box<TData> typedValue)
            return typedValue.Lazy.Value;

        return Option<TData>.None;
    }

    private interface IBox
    {
        
    }

    private sealed record Box<TBox>(Lazy<TBox> Lazy) : IBox;
}