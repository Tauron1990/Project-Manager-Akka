using System.Collections.Concurrent;
using Cottle;
using JetBrains.Annotations;
using SpaceConqueror.Core;

namespace Tauron.TextAdventure.Engine.Core;

[PublicAPI]
public sealed class AssetManager
{
    private readonly ConcurrentDictionary<string, IBox?> _assets = new(StringComparer.Ordinal);
    private Dictionary<Value, Value> _contextValues = new();
    public IContext RenderContext { get; private set; }

    public AssetManager()
    {
        RenderContext = Context.Empty;

        IFunction func = Function.CreatePure1(
            (_, value) => _assets.TryGetValue(value.AsString, out IBox? box)
                ? box!.GetString(RenderContext)
                : RenderContext[value]);

        UpdateContext("GetString", Value.FromFunction(func));
    }

    public void UpdateContext(Value key, Value value)
    {
        _contextValues[key] = value;
        RenderContext = Context.CreateBuiltin(_contextValues);
    }

    private void ThrowDuplicate(string name)
        => throw new InvalidOperationException($"An Resource with Name {name} is already registrated");

    public void Add(string name, Func<IDocument> document)
    {
        if(_assets.TryAdd(name, new DocumentBox(new Lazy<IDocument>(document))))
            return;

        ThrowDuplicate(name);
    }

    public void Add<TData>(string name, TData data)
    {
        if(_assets.TryAdd(name, new Box<TData>(data)))
            return;

        ThrowDuplicate(name);
    }

    public void Add<TData>(string name, Func<TData> data)
    {
        if(_assets.TryAdd(name, new LazyBox<TData>(new Lazy<TData>(data))))
            return;

        ThrowDuplicate(name);
    }

    public string GetString(string name, IContext context)
    {
        if(_assets.TryGetValue(name, out IBox? toCast) && toCast is IBox<string> box)
            return box.Get(Context.CreateCascade(context, RenderContext));

        return name;
    }

    public IDocument GetDocument(string name)
    {
        if(_assets.TryGetValue(name, out IBox? box) && box is DocumentBox documentBox)
            return documentBox.Document.Value;

        return Document.Empty;
    }

    public TData Get<TData>(string name)
    {
        if(_assets.TryGetValue(name, out IBox? toCast) && toCast is IBox<TData> box)
            return box.Get(RenderContext);

        throw new InvalidOperationException("The Specific Asset tis not found or has anathor Type");
    }

    public Option<TData> TryGet<TData>(string name)
    {
        if(_assets.TryGetValue(name, out IBox? value) && value is IBox<TData> typedValue)
            return typedValue.Get(RenderContext);

        return Option<TData>.None;
    }

    private interface IBox
    {
        string GetString(IContext context);
    }

    private interface IBox<out TValue> : IBox
    {
        TValue Get(IContext context);
    }

    private sealed record LazyBox<TBox>(Lazy<TBox> Lazy) : IBox<TBox>
    {
        public TBox Get(IContext context)
            => Lazy.Value;

        string IBox.GetString(IContext context)
            => Lazy.Value?.ToString() ?? string.Empty;
    }

    private sealed record Box<TBox>(TBox Value) : IBox<TBox>
    {
        public TBox Get(IContext context)
            => Value;

        string IBox.GetString(IContext context)
            => Value?.ToString() ?? string.Empty;
    }

    private sealed record DocumentBox(Lazy<IDocument> Document) : IBox<string>
    {
        public string Get(IContext context)
            => Document.Value.Render(context);

        public string GetString(IContext context)
            => Document.Value.Render(context);
    }
}