using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Tauron.Application.VirtualFiles;

namespace Tauron.Application.Files.Json;

[PublicAPI]
public abstract partial class JsonNode
{
    public abstract void SerializeBinary(BinaryWriter aWriter);

    public void SaveToBinaryStream(Stream aData)
    {
        SerializeBinary(new BinaryWriter(aData));
    }


    public void SaveToBinaryFile(IFile aFile)
    {
        using Stream fileStream = aFile.Open(FileAccess.Write);
        SaveToBinaryStream(fileStream);
    }

    public string SaveToBinaryBase64()
    {
        using var stream = new MemoryStream();
        SaveToBinaryStream(stream);
        stream.Position = 0;

        return Convert.ToBase64String(stream.ToArray());
    }

    public static JsonNode DeserializeBinary(BinaryReader aReader)
    {
        var type = (JsonNodeType)aReader.ReadByte();
        switch (type)
        {
            case JsonNodeType.Array:
            {
                int count = aReader.ReadInt32();
                var tmp = new JsonArray();
                for (var i = 0; i < count; i++) tmp.Add(DeserializeBinary(aReader));

                return tmp;
            }
            case JsonNodeType.Object:
            {
                int count = aReader.ReadInt32();
                var tmp = new JsonObject();
                for (var i = 0; i < count; i++)
                {
                    string key = aReader.ReadString();
                    JsonNode val = DeserializeBinary(aReader);
                    tmp.Add(key, val);
                }

                return tmp;
            }
            case JsonNodeType.String:
            {
                return new JsonString(aReader.ReadString());
            }
            case JsonNodeType.Number:
            {
                return new JsonNumber(aReader.ReadDouble());
            }
            case JsonNodeType.Boolean:
            {
                return new JsonBool(aReader.ReadBoolean());
            }
            case JsonNodeType.NullValue:
            {
                return JsonNull.CreateOrGet();
            }
            default:
            {
                throw new InvalidOperationException("Error deserializing JSON. Unknown tag: " + type);
            }
        }
    }

    public static JsonNode LoadFromBinaryStream(Stream aData)
    {
        using var binaryReader = new BinaryReader(aData);

        return DeserializeBinary(binaryReader);
    }

    public static JsonNode LoadFromBinaryFile(string aFileName)
    {
        using FileStream fileStream = File.OpenRead(aFileName);

        return LoadFromBinaryStream(fileStream);
    }

    public static JsonNode LoadFromBinaryBase64(string aBase64)
    {
        byte[] tmp = Convert.FromBase64String(aBase64);
        var stream = new MemoryStream(tmp) { Position = 0 };

        return LoadFromBinaryStream(stream);
    }
}

public abstract partial class JsonNode
{
    protected const string ElementNotSupportet = "This Function is in the Element Type not Supportet";

    [ThreadStatic]
    private static StringBuilder? _escapeBuilder;

    internal static StringBuilder EscapeBuilder => _escapeBuilder ??= new StringBuilder();

    // ReSharper disable once CognitiveComplexity
    internal static string Escape(string aText)
    {
        StringBuilder sb = EscapeBuilder;
        sb.Length = 0;
        if(sb.Capacity < aText.Length + aText.Length / 10)
            sb.Capacity = aText.Length + aText.Length / 10;
        foreach (char c in aText)
            switch (c)
            {
                case '\\':
                    sb.Append("\\\\");

                    break;
                case '\"':
                    sb.Append("\\\"");

                    break;
                case '\n':
                    sb.Append("\\n");

                    break;
                case '\r':
                    sb.Append("\\r");

                    break;
                case '\t':
                    sb.Append("\\t");

                    break;
                case '\b':
                    sb.Append("\\b");

                    break;
                case '\f':
                    sb.Append("\\f");

                    break;
                default:
                    if(c < ' ' || (ForceAscii && c > 127))
                    {
                        ushort val = c;
                        sb.Append("\\u").Append(val.ToString("X4", CultureInfo.InvariantCulture));
                    }
                    else
                    {
                        sb.Append(c);
                    }

                    break;
            }

        var result = sb.ToString();
        sb.Length = 0;

        return result;
    }

    private static JsonNode ParseElement(string token, bool quoted)
    {
        if(quoted)
            return token;

        string tmp = token.ToLower(CultureInfo.InvariantCulture);
        switch (tmp)
        {
            case "false":
            case "true":
                return string.Equals(tmp, "true", StringComparison.Ordinal);
            case "null":
                return JsonNull.CreateOrGet();
        }

        if(double.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out double val))
            return val;

        return token;
    }

    // ReSharper disable once CognitiveComplexity
    #pragma warning disable MA0051
    public static JsonNode Parse(string aJson)
        #pragma warning restore MA0051
    {
        var stack = new Stack<JsonNode>();
        JsonNode? ctx = null;
        var i = 0;
        var token = new StringBuilder();
        var tokenName = "";
        var quoteMode = false;
        var tokenIsQuoted = false;
        while (i < aJson.Length)
        {
            switch (aJson[i])
            {
                case '{':
                    if(quoteMode)
                    {
                        token.Append(aJson[i]);

                        break;
                    }

                    stack.Push(new JsonObject());
                    ctx?.Add(tokenName, stack.Peek());
                    tokenName = "";
                    token.Length = 0;
                    ctx = stack.Peek();

                    break;

                case '[':
                    if(quoteMode)
                    {
                        token.Append(aJson[i]);

                        break;
                    }

                    stack.Push(new JsonArray());
                    ctx?.Add(tokenName, stack.Peek());
                    tokenName = "";
                    token.Length = 0;
                    ctx = stack.Peek();

                    break;

                case '}':
                case ']':
                    if(quoteMode)
                    {
                        token.Append(aJson[i]);

                        break;
                    }

                    if(stack.Count == 0)
                        throw new InvalidOperationException("JSON Parse: Too many closing brackets");

                    stack.Pop();
                    if(token.Length > 0 || tokenIsQuoted)
                        ctx?.Add(tokenName, ParseElement(token.ToString(), tokenIsQuoted));

                    tokenIsQuoted = false;
                    tokenName = "";
                    token.Length = 0;
                    if(stack.Count > 0)
                        ctx = stack.Peek();

                    break;

                case ':':
                    if(quoteMode)
                    {
                        token.Append(aJson[i]);

                        break;
                    }

                    tokenName = token.ToString();
                    token.Length = 0;
                    tokenIsQuoted = false;

                    break;

                case '"':
                    quoteMode ^= true;
                    tokenIsQuoted |= quoteMode;

                    break;

                case ',':
                    if(quoteMode)
                    {
                        token.Append(aJson[i]);

                        break;
                    }

                    if(token.Length > 0 || tokenIsQuoted)
                        ctx?.Add(tokenName, ParseElement(token.ToString(), tokenIsQuoted));

                    tokenName = "";
                    token.Length = 0;
                    tokenIsQuoted = false;

                    break;

                case '\r':
                case '\n':
                    break;

                case ' ':
                case '\t':
                    if(quoteMode)
                        token.Append(aJson[i]);

                    break;

                case '\\':
                    ++i;
                    if(quoteMode)
                    {
                        char c = aJson[i];
                        switch (c)
                        {
                            case 't':
                                token.Append('\t');

                                break;
                            case 'r':
                                token.Append('\r');

                                break;
                            case 'n':
                                token.Append('\n');

                                break;
                            case 'b':
                                token.Append('\b');

                                break;
                            case 'f':
                                token.Append('\f');

                                break;
                            case 'u':
                            {
                                string s = aJson.Substring(i + 1, 4);
                                token.Append(
                                    (char)int.Parse(
                                        s,
                                        NumberStyles.AllowHexSpecifier,
                                        CultureInfo.InvariantCulture));
                                i += 4;

                                break;
                            }
                            default:
                                token.Append(c);

                                break;
                        }
                    }

                    break;

                default:
                    token.Append(aJson[i]);

                    break;
            }

            ++i;
        }

        if(quoteMode) throw new InvalidOperationException("JSON Parse: Quotation marks seems to be messed up.");

        return ctx ?? ParseElement(token.ToString(), tokenIsQuoted);
    }

    #region Enumerators

    [PublicAPI]
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Auto)]
    public struct Enumerator
    {
        private enum Type
        {
            None,
            Array,
            Object
        }

        private readonly Type _type;
        private Dictionary<string, JsonNode>.Enumerator _object;
        private List<JsonNode>.Enumerator _array;
        public bool IsValid => _type != Type.None;

        public Enumerator(List<JsonNode>.Enumerator aArrayEnum)
        {
            _type = Type.Array;
            _object = default;
            _array = aArrayEnum;
        }

        public Enumerator(Dictionary<string, JsonNode>.Enumerator aDictEnum)
        {
            _type = Type.Object;
            _object = aDictEnum;
            _array = default;
        }

        public KeyValuePair<string, JsonNode> Current
        {
            get
            {
                return _type switch
                {
                    Type.Array => new KeyValuePair<string, JsonNode>(string.Empty, _array.Current),
                    Type.Object => _object.Current,
                    _ => throw new InvalidOperationException("No Element")
                };
            }
        }

        public bool MoveNext()
        {
            return _type switch
            {
                Type.Array => _array.MoveNext(),
                Type.Object => _object.MoveNext(),
                _ => false
            };
        }
    }

    [PublicAPI]
    public struct ValueEnumerator
    {
        private Enumerator _enumerator;

        public ValueEnumerator(List<JsonNode>.Enumerator aArrayEnum) : this(new Enumerator(aArrayEnum)) { }

        public ValueEnumerator(Dictionary<string, JsonNode>.Enumerator aDictEnum) : this(new Enumerator(aDictEnum)) { }

        public ValueEnumerator(Enumerator aEnumerator) => _enumerator = aEnumerator;

        public JsonNode Current => _enumerator.Current.Value;

        public bool MoveNext() => _enumerator.MoveNext();

        public ValueEnumerator GetEnumerator() => this;
    }

    [PublicAPI]
    public struct KeyEnumerator
    {
        private Enumerator _enumerator;

        public KeyEnumerator(List<JsonNode>.Enumerator aArrayEnum) : this(new Enumerator(aArrayEnum)) { }

        public KeyEnumerator(Dictionary<string, JsonNode>.Enumerator aDictEnum) : this(new Enumerator(aDictEnum)) { }

        public KeyEnumerator(Enumerator aEnumerator) => _enumerator = aEnumerator;

        public JsonNode Current => _enumerator.Current.Key;

        public bool MoveNext() => _enumerator.MoveNext();

        public KeyEnumerator GetEnumerator() => this;
    }

    [PublicAPI]
    public class LinqEnumerator : IEnumerator<KeyValuePair<string, JsonNode>>,
        IEnumerable<KeyValuePair<string, JsonNode>>
    {
        private Enumerator _enumerator;
        private JsonNode? _node;

        internal LinqEnumerator(JsonNode? aNode)
        {
            _node = aNode;
            if(aNode != null)
                _enumerator = aNode.GetEnumerator();
        }

        public IEnumerator<KeyValuePair<string, JsonNode>> GetEnumerator() => new LinqEnumerator(_node);

        IEnumerator IEnumerable.GetEnumerator() => new LinqEnumerator(_node);

        public KeyValuePair<string, JsonNode> Current => _enumerator.Current;
        object IEnumerator.Current => _enumerator.Current;

        public bool MoveNext() => _enumerator.MoveNext();

        public void Dispose()
        {
            _node = null;
            _enumerator = new Enumerator();
            GC.SuppressFinalize(this);
        }

        public void Reset()
        {
            if(_node != null)
                _enumerator = _node.GetEnumerator();
        }
    }

    #endregion Enumerators

    #region common interface

    public static bool ForceAscii { get; set; }
    public static bool LongAsString { get; set; }

    public abstract JsonNodeType Tag { get; }

    public abstract JsonNode? this[int aIndex] { get; set; }

    public abstract JsonNode? this[string aKey] { get; set; }

    public abstract string Value { get; set; }

    public virtual int Count => 0;

    public virtual bool IsNumber => false;
    public virtual bool IsString => false;
    public virtual bool IsBoolean => false;
    public virtual bool IsNull => false;
    public virtual bool IsArray => false;
    public virtual bool IsObject => false;

    public abstract bool Inline { get; set; }

    public virtual void Add(string aKey, JsonNode? aItem) { }

    public virtual void Add(JsonNode? aItem)
    {
        Add("", aItem);
    }

    public virtual JsonNode? Remove(string aKey) => null;

    public virtual JsonNode? Remove(int aIndex) => null;

    public virtual JsonNode? Remove(JsonNode aNode) => aNode;

    public virtual IEnumerable<JsonNode> Children
    {
        get { yield break; }
    }

    public IEnumerable<JsonNode> DeepChildren => Children.SelectMany(jsonNode => jsonNode.DeepChildren);

    public override string ToString()
    {
        var sb = new StringBuilder();
        WriteToStringBuilder(sb, 0, 0, JsonTextMode.Compact);

        return sb.ToString();
    }

    public string ToString(int aIndent)
    {
        var sb = new StringBuilder();
        WriteToStringBuilder(sb, 0, aIndent, JsonTextMode.Indent);

        return sb.ToString();
    }

    internal abstract void WriteToStringBuilder(StringBuilder aSb, int aIndent, int aIndentInc, JsonTextMode aMode);

    public abstract Enumerator GetEnumerator();
    public IEnumerable<KeyValuePair<string, JsonNode>> Linq => new LinqEnumerator(this);
    public KeyEnumerator Keys => new(GetEnumerator());
    public ValueEnumerator Values => new(GetEnumerator());

    #endregion common interface

    #region typecasting properties

    public virtual double AsDouble
    {
        get => double.TryParse(Value, NumberStyles.Float, CultureInfo.InvariantCulture, out double v) ? v : 0.0;
        set => Value = value.ToString(CultureInfo.InvariantCulture);
    }

    public virtual int AsInt
    {
        get => (int)AsDouble;
        set => AsDouble = value;
    }

    public virtual float AsFloat
    {
        get => (float)AsDouble;
        set => AsDouble = value;
    }

    public virtual bool AsBool
    {
        get
        {
            if(bool.TryParse(Value, out bool v))
                return v;

            return !string.IsNullOrEmpty(Value);
        }
        set => Value = value ? "true" : "false";
    }

    public virtual long AsLong
    {
        get => long.TryParse(Value, NumberStyles.Any, NumberFormatInfo.InvariantInfo, out long val) ? val : 0L;
        set => Value = value.ToString(CultureInfo.InvariantCulture);
    }

    public virtual JsonArray? AsArray => this as JsonArray;

    public virtual JsonObject? AsObject => this as JsonObject;

    #endregion typecasting properties

    #region operators

    public static implicit operator JsonNode(string s) => new JsonString(s);

    public static implicit operator string?(JsonNode? d) => d?.Value;

    public static implicit operator JsonNode(double n) => new JsonNumber(n);

    public static implicit operator double(JsonNode? d) => d?.AsDouble ?? 0;

    public static implicit operator JsonNode(float n) => new JsonNumber(n);

    public static implicit operator float(JsonNode? d) => d?.AsFloat ?? 0;

    public static implicit operator JsonNode(int n) => new JsonNumber(n);

    public static implicit operator int(JsonNode? d) => d?.AsInt ?? 0;

    public static implicit operator JsonNode(long n)
    {
        if(LongAsString)
            return new JsonString(n.ToString(CultureInfo.InvariantCulture));

        return new JsonNumber(n);
    }

    public static implicit operator long(JsonNode? d) => d?.AsLong ?? 0L;

    public static implicit operator JsonNode(bool b) => new JsonBool(b);

    public static implicit operator bool(JsonNode? d) => d != null && d.AsBool;

    public static implicit operator JsonNode(KeyValuePair<string, JsonNode> aKeyValue) => aKeyValue.Value;

    public static bool operator ==(JsonNode? a, object? b)
    {
        if(ReferenceEquals(a, b))
            return true;

        bool aIsNull = a is JsonNull or null or JsonLazyCreator;
        bool bIsNull = b is JsonNull or null or JsonLazyCreator;

        if(aIsNull && bIsNull)
            return true;

        return !aIsNull && a!.Equals(b!);
    }

    public static bool operator !=(JsonNode? a, object? b) => !(a == b);

    public override bool Equals(object? obj) => ReferenceEquals(this, obj);

    // ReSharper disable once BaseObjectGetHashCodeCallInGetHashCode
    public override int GetHashCode() => base.GetHashCode();

    #endregion operators
}