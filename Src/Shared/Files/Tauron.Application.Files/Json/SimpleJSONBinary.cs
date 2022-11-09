using System;
using System.IO;
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

public partial class JsonString
{
    public override void SerializeBinary(BinaryWriter aWriter)
    {
        aWriter.Write((byte)JsonNodeType.String);
        aWriter.Write(_data);
    }
}

public partial class JsonNumber
{
    public override void SerializeBinary(BinaryWriter aWriter)
    {
        aWriter.Write((byte)JsonNodeType.Number);
        aWriter.Write(_data);
    }
}

public partial class JsonBool
{
    public override void SerializeBinary(BinaryWriter aWriter)
    {
        aWriter.Write((byte)JsonNodeType.Boolean);
        aWriter.Write(_data);
    }
}

public partial class JsonNull
{
    public override void SerializeBinary(BinaryWriter aWriter)
    {
        aWriter.Write((byte)JsonNodeType.NullValue);
    }
}

internal partial class JsonLazyCreator
{
    public override void SerializeBinary(BinaryWriter aWriter) { }
}