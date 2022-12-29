using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace Tauron.Application.Files.Json;

internal partial class JsonLazyCreator
{
    public override void SerializeBinary(BinaryWriter aWriter) { }
}

internal sealed partial class JsonLazyCreator : JsonNode
{
    private readonly string? _key;
    private JsonNode? _node;

    internal JsonLazyCreator(JsonNode aNode)
    {
        _node = aNode;
        _key = null;
    }

    internal JsonLazyCreator(JsonNode aNode, string aKey)
    {
        _node = aNode;
        _key = aKey;
    }

    public override JsonNodeType Tag => JsonNodeType.None;

    public override JsonNode? this[int aIndex]
    {
        get => new JsonLazyCreator(this);
        set => Set(new JsonArray()).Add(value);
    }

    public override JsonNode? this[string aKey]
    {
        get => new JsonLazyCreator(this, aKey);
        set => Set(new JsonObject()).Add(aKey, value);
    }

    public override string Value
    {
        get => string.Empty;
        set => throw new NotSupportedException("String Value not Supported");
    }

    public override int AsInt
    {
        get
        {
            Set(new JsonNumber(0));

            return 0;
        }
        set => Set(new JsonNumber(value));
    }

    public override float AsFloat
    {
        get
        {
            Set(new JsonNumber(0.0f));

            return 0.0f;
        }
        set => Set(new JsonNumber(value));
    }

    public override double AsDouble
    {
        get
        {
            Set(new JsonNumber(0.0));

            return 0.0;
        }
        set => Set(new JsonNumber(value));
    }

    public override long AsLong
    {
        get
        {
            if(LongAsString)
                Set(new JsonString("0"));
            else
                Set(new JsonNumber(0.0));

            return 0L;
        }
        set
        {
            if(LongAsString)
                Set(new JsonString(value.ToString(CultureInfo.InvariantCulture)));
            else
                Set(new JsonNumber(value));
        }
    }

    public override bool AsBool
    {
        get
        {
            Set(new JsonBool(false));

            return false;
        }
        set => Set(new JsonBool(value));
    }

    public override JsonArray AsArray => Set(new JsonArray());

    public override JsonObject AsObject => Set(new JsonObject());

    public override bool Inline
    {
        get => throw new NotSupportedException("In line not Supported");
        set => throw new NotSupportedException("In line not Supported");
    }

    public override Enumerator GetEnumerator() => new();

    private T Set<T>(T aVal) where T : JsonNode
    {
        if(_key is null)
            _node?.Add(aVal);
        else
            _node?.Add(_key, aVal);
        _node = null; // Be GC friendly.

        return aVal;
    }

    public override void Add(JsonNode? aItem)
        => Set(new JsonArray()).Add(aItem);

    public override void Add(string aKey, JsonNode? aItem)
        => Set(new JsonObject()).Add(aKey, aItem);

    public static bool operator ==(JsonLazyCreator a, object? b) => b is null || ReferenceEquals(a, b);

    public static bool operator !=(JsonLazyCreator a, object? b) => !(a == b);

    public override bool Equals(object? obj) => obj is null || ReferenceEquals(this, obj);

    public override int GetHashCode() => 0;

    internal override void WriteToStringBuilder(StringBuilder aSb, int aIndent, int aIndentInc, JsonTextMode aMode)
    {
        aSb.Append("null");
    }
}