using System;
using System.IO;
using System.Text;

namespace Tauron.Application.Files.Json;

public partial class JsonString
{
    public override void SerializeBinary(BinaryWriter aWriter)
    {
        aWriter.Write((byte)JsonNodeType.String);
        aWriter.Write(Value);
    }
}

public sealed partial class JsonString : JsonNode
{
    public JsonString(string aData) => Value = aData;

    public override JsonNodeType Tag => JsonNodeType.String;

    public override JsonNode? this[int aIndex]
    {
        get => throw new NotSupportedException("Index not Supported");
        set => throw new NotSupportedException("Index not Supported");
    }

    public override JsonNode? this[string aKey]
    {
        get => throw new NotSupportedException("key not Supported");
        set => throw new NotSupportedException("keys not Supported");
    }

    public override bool IsString => true;

    public override bool Inline
    {
        #pragma warning disable EX002
        get => throw new NotSupportedException();

        set => throw new NotSupportedException();
    }


    public override string Value { get; set; }

    public override Enumerator GetEnumerator() => new();

    internal override void WriteToStringBuilder(StringBuilder aSb, int aIndent, int aIndentInc, JsonTextMode aMode)
    {
        aSb.Append('\"').Append(Escape(Value)).Append('\"');
    }

    public override bool Equals(object? obj)
    {
        if(base.Equals(obj))
            return true;
        if(obj is string s)
            return string.Equals(Value, s, StringComparison.Ordinal);

        var s2 = obj as JsonString;

        if(s2 != null)
            return string.Equals(Value, s2.Value, StringComparison.Ordinal);

        return false;
    }

    // ReSharper disable once NonReadonlyMemberInGetHashCode
    public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value);
}