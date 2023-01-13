using System;
using System.IO;
using System.Text;
using JetBrains.Annotations;

namespace Tauron.Application.Files.Json;

public partial class JsonBool
{
    public override void SerializeBinary(BinaryWriter aWriter)
    {
        aWriter.Write((byte)JsonNodeType.Boolean);
        aWriter.Write(AsBool);
    }
}

[PublicAPI]
public sealed partial class JsonBool : JsonNode
{
    public JsonBool(bool aData) => AsBool = aData;

    public JsonBool(string aData) => Value = aData;

    public override JsonNodeType Tag => JsonNodeType.Boolean;

    public override JsonNode? this[int aIndex]
    {
        get => throw new NotSupportedException(ElementNotSupportet);
        set => throw new NotSupportedException(ElementNotSupportet);
    }

    public override JsonNode? this[string aKey]
    {
        get => throw new NotSupportedException(ElementNotSupportet);
        set => throw new NotSupportedException(ElementNotSupportet);
    }

    public override bool IsBoolean => true;

    public override bool Inline
    {
        get => throw new NotSupportedException(ElementNotSupportet);
        set => throw new NotSupportedException(ElementNotSupportet);
    }

    public override string Value
    {
        get => AsBool.ToString();
        set
        {
            if(bool.TryParse(value, out bool v))
                AsBool = v;
        }
    }

    public override bool AsBool { get; set; }

    public override Enumerator GetEnumerator() => new();

    internal override void WriteToStringBuilder(StringBuilder aSb, int aIndent, int aIndentInc, JsonTextMode aMode)
    {
        aSb.Append(AsBool ? "true" : "false");
    }

    public override bool Equals(object? obj)
    {
        return obj switch
        {
            null => false,
            bool b => AsBool == b,
            _ => false,
        };
    }

    // ReSharper disable once NonReadonlyMemberInGetHashCode
    public override int GetHashCode() => AsBool.GetHashCode();
}