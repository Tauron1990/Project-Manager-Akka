using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Tauron.Application.Files.Json;

public partial class JsonArray
{
    public override void SerializeBinary(BinaryWriter aWriter)
    {
        aWriter.Write((byte)JsonNodeType.Array);
        aWriter.Write(_list.Count);
        foreach (JsonNode jsonNode in _list)
            jsonNode.SerializeBinary(aWriter);
    }
}

public partial class JsonArray : JsonNode
{
    private readonly List<JsonNode> _list = new();
    private bool _inline;

    public override bool Inline
    {
        get => _inline;
        set => _inline = value;
    }

    public override JsonNodeType Tag => JsonNodeType.Array;
    public override bool IsArray => true;

    public override JsonNode? this[int aIndex]
    {
        get
        {
            if(aIndex < 0 || aIndex >= _list.Count)
                return new JsonLazyCreator(this);

            return _list[aIndex];
        }
        set
        {
            value ??= JsonNull.CreateOrGet();
            if(aIndex < 0 || aIndex >= _list.Count)
                _list.Add(value);
            else
                _list[aIndex] = value;
        }
    }

    public override JsonNode? this[string aKey]
    {
        get => new JsonLazyCreator(this);
        set
        {
            value ??= JsonNull.CreateOrGet();
            _list.Add(value);
        }
    }

    public override string Value
    {
        get => throw new NotSupportedException("Value not Supported");
        set => throw new NotSupportedException("Value not Supported");
    }

    public override int Count => _list.Count;

    public override IEnumerable<JsonNode> Children
    {
        get
        {
            foreach (JsonNode children in _list)
                yield return children;
        }
    }

    public override Enumerator GetEnumerator() => new(_list.GetEnumerator());

    public override void Add(string aKey, JsonNode? aItem)
    {
        aItem ??= JsonNull.CreateOrGet();
        _list.Add(aItem);
    }

    public override JsonNode? Remove(int aIndex)
    {
        if(aIndex < 0 || aIndex >= _list.Count)
            return null;

        JsonNode tmp = _list[aIndex];
        _list.RemoveAt(aIndex);

        return tmp;
    }

    public override JsonNode Remove(JsonNode aNode)
    {
        _list.Remove(aNode);

        return aNode;
    }


    internal override void WriteToStringBuilder(StringBuilder aSb, int aIndent, int aIndentInc, JsonTextMode aMode)
    {
        aSb.Append('[');
        int count = _list.Count;
        if(_inline)
            aMode = JsonTextMode.Compact;
        for (var i = 0; i < count; i++)
        {
            if(i > 0)
                aSb.Append(',');
            if(aMode == JsonTextMode.Indent)
                aSb.AppendLine();

            if(aMode == JsonTextMode.Indent)
                aSb.Append(' ', aIndent + aIndentInc);
            _list[i].WriteToStringBuilder(aSb, aIndent + aIndentInc, aIndentInc, aMode);
        }

        if(aMode == JsonTextMode.Indent)
            aSb.AppendLine().Append(' ', aIndent);
        aSb.Append(']');
    }
}