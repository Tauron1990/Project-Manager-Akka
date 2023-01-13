using System;
using System.IO;
using System.Text;
using JetBrains.Annotations;

namespace Tauron.Application.Files.Json;

public partial class JsonNull
{
    public override void SerializeBinary(BinaryWriter aWriter)
    {
        aWriter.Write((byte)JsonNodeType.NullValue);
    }
}

// End of JSONNode

// End of JSONArray

// End of JSONObject

// End of JSONString

// End of JSONNumber
// End of JSONBool

[PublicAPI]
public sealed partial class JsonNull : JsonNode
{
    private static readonly JsonNull StaticInstance = new();

    private JsonNull() { }

    public static bool ReuseSameInstance { get; set; } = true;

    public override JsonNodeType Tag => JsonNodeType.NullValue;

    public override JsonNode? this[int aIndex]
    {
        get => throw new NotSupportedException("Node has not Index");
        set => throw new NotSupportedException("Node has not Index");
    }

    public override JsonNode? this[string aKey]
    {
        get => throw new NotSupportedException("Node has not Keys");
        set => throw new NotSupportedException("Node has not Keys");
    }

    public override bool IsNull => true;

    public override bool Inline
    {
        get => throw new NotSupportedException(ElementNotSupportet);
        set => throw new NotSupportedException(ElementNotSupportet);
    }

    public override string Value
    {
        get => "null";
        set { }
    }

    public override bool AsBool
    {
        get => false;
        set { }
    }

    public static JsonNull CreateOrGet() => ReuseSameInstance ? StaticInstance : new JsonNull();

    public override Enumerator GetEnumerator() => new();

    public override bool Equals(object? obj)
    {
        if(ReferenceEquals(this, obj))
            return true;

        return obj is JsonNull;
    }

    public override int GetHashCode() => 0;

    internal override void WriteToStringBuilder(StringBuilder aSb, int aIndent, int aIndentInc, JsonTextMode aMode)
    {
        aSb.Append("null");
    }
}
// End of JSONNull

// End of JSONLazyCreator
#pragma warning restore EX002