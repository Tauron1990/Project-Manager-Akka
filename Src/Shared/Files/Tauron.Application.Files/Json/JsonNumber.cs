using System;
using System.Globalization;
using System.IO;
using System.Text;
using JetBrains.Annotations;

namespace Tauron.Application.Files.Json;

public partial class JsonNumber
{
    public override void SerializeBinary(BinaryWriter aWriter)
    {
        aWriter.Write((byte)JsonNodeType.Number);
        aWriter.Write(AsDouble);
    }
}

[PublicAPI]
public sealed partial class JsonNumber : JsonNode
{
    public JsonNumber(double aData) => AsDouble = aData;

    public JsonNumber(string aData) => Value = aData;

    public override JsonNodeType Tag => JsonNodeType.Number;

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

    public override bool IsNumber => true;

    public override bool Inline
    {
        get => throw new NotSupportedException(ElementNotSupportet);
        set => throw new NotSupportedException(ElementNotSupportet);
    }

    public override string Value
    {
        get => AsDouble.ToString(CultureInfo.InvariantCulture);
        set
        {
            if(double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double v))
                AsDouble = v;
        }
    }

    public override double AsDouble { get; set; }

    public override long AsLong
    {
        get => (long)AsDouble;
        set => AsDouble = value;
    }

    public override Enumerator GetEnumerator() => new();

    internal override void WriteToStringBuilder(StringBuilder aSb, int aIndent, int aIndentInc, JsonTextMode aMode)
    {
        aSb.Append(Value);
    }

    private static bool IsNumeric(object value) => value is int || value is uint
                                                                || value is float || value is double
                                                                || value is decimal
                                                                || value is long || value is ulong
                                                                || value is short || value is ushort
                                                                || value is sbyte || value is byte;

    public override bool Equals(object? obj)
    {
        if(obj is null)
            return false;
        if(base.Equals(obj))
            return true;

        var s2 = obj as JsonNumber;

        if(s2 != null)
            return Math.Abs(AsDouble - s2.AsDouble) < 0;
        if(IsNumeric(obj))
            return Math.Abs(Convert.ToDouble(obj, CultureInfo.InvariantCulture) - AsDouble) < 0;

        return false;
    }

    // ReSharper disable once NonReadonlyMemberInGetHashCode
    public override int GetHashCode() => AsDouble.GetHashCode();
}