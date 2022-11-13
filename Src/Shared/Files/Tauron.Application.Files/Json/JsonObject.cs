using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Tauron.Application.Files.Json;

public partial class JsonObject
{
    public override void SerializeBinary(BinaryWriter aWriter)
    {
        aWriter.Write((byte)JsonNodeType.Object);
        aWriter.Write(_dict.Count);
        foreach (string key in _dict.Keys)
        {
            aWriter.Write(key);
            _dict[key].SerializeBinary(aWriter);
        }
    }
}

public partial class JsonObject : JsonNode
{
    private readonly Dictionary<string, JsonNode> _dict = new(StringComparer.Ordinal);
    private bool _inline;

    public override bool Inline
    {
        get => _inline;
        set => _inline = value;
    }

    public override JsonNodeType Tag => JsonNodeType.Object;
    public override bool IsObject => true;


    public override JsonNode? this[string aKey]
    {
        get => _dict.ContainsKey(aKey) ? _dict[aKey] : new JsonLazyCreator(this, aKey);
        set
        {
            value ??= JsonNull.CreateOrGet();
            if(_dict.ContainsKey(aKey))
                _dict[aKey] = value;
            else
                _dict.Add(aKey, value);
        }
    }

    public override string Value
    {
        get => throw new NotSupportedException("Value not Supported");
        set => throw new NotSupportedException("Value not Supported");
    }

    public override JsonNode? this[int aIndex]
    {
        get
        {
            if(aIndex < 0 || aIndex >= _dict.Count)
                return null;

            return _dict.ElementAt(aIndex).Value;
        }
        set
        {
            value ??= JsonNull.CreateOrGet();

            if(aIndex < 0 || aIndex >= _dict.Count)
                return;

            string key = _dict.ElementAt(aIndex).Key;
            _dict[key] = value;
        }
    }

    public override int Count => _dict.Count;

    public override IEnumerable<JsonNode> Children
        => _dict.Select(pair => pair.Value);

    public override Enumerator GetEnumerator() => new(_dict.GetEnumerator());

    public override void Add(string aKey, JsonNode? aItem)
    {
        aItem ??= JsonNull.CreateOrGet();

        if(!string.IsNullOrEmpty(aKey))
        {
            if(_dict.ContainsKey(aKey))
                _dict[aKey] = aItem;
            else
                _dict.Add(aKey, aItem);
        }
        else
        {
            _dict.Add(Guid.NewGuid().ToString(), aItem);
        }
    }

    public override JsonNode? Remove(string aKey)
    {
        if(!_dict.ContainsKey(aKey))
            return null;

        JsonNode tmp = _dict[aKey];
        _dict.Remove(aKey);

        return tmp;
    }

    public override JsonNode? Remove(int aIndex)
    {
        if(aIndex < 0 || aIndex >= _dict.Count)
            return null;

        var item = _dict.ElementAt(aIndex);
        _dict.Remove(item.Key);

        return item.Value;
    }

    public override JsonNode? Remove(JsonNode aNode)
    {
        #pragma warning disable GU0019
        var item = _dict.FirstOrDefault(k => k.Value == aNode);
        #pragma warning restore GU0019

        if(item is { Key: null }) return null;

        _dict.Remove(item.Key);

        return aNode;
    }

    // ReSharper disable once CognitiveComplexity
    internal override void WriteToStringBuilder(StringBuilder aSb, int aIndent, int aIndentInc, JsonTextMode aMode)
    {
        aSb.Append('{');
        var first = true;
        if(_inline)
            aMode = JsonTextMode.Compact;
        foreach (var k in _dict)
        {
            if(!first)
                aSb.Append(',');
            first = false;
            if(aMode == JsonTextMode.Indent)
                aSb.AppendLine();
            if(aMode == JsonTextMode.Indent)
                aSb.Append(' ', aIndent + aIndentInc);
            aSb.Append('\"').Append(Escape(k.Key)).Append('\"');
            if(aMode == JsonTextMode.Compact)
                aSb.Append(':');
            else
                aSb.Append(" : ");
            k.Value.WriteToStringBuilder(aSb, aIndent + aIndentInc, aIndentInc, aMode);
        }

        if(aMode == JsonTextMode.Indent)
            aSb.AppendLine().Append(' ', aIndent);
        aSb.Append('}');
    }
}