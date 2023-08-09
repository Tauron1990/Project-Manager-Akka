using System.Buffers;
using System.Runtime.Serialization;
using MemoryPack;
using Vogen;

namespace SimpleProjectManager.Shared.Services.Devices;

[DataContract, MemoryPackable(GenerateType.VersionTolerant)]
public readonly partial record struct DeviceName([property:DataMember, MemoryPackOrder(0)]string Value)
{
    
}

/*[ValueObject(typeof(string))]
[Instance("Empty", "")]
#pragma warning disable MA0097
public readonly partial struct DeviceName : IMemoryPackable<DeviceName>
    #pragma warning restore MA0097
{
    private static Validation Validate(string value)
        => string.IsNullOrWhiteSpace(value) ? Validation.Invalid("Name Should not be Empty") : Validation.Ok;

    static void IMemoryPackFormatterRegister.RegisterFormatter()
    {
        if(!MemoryPackFormatterProvider.IsRegistered<DeviceName>())
            MemoryPackFormatterProvider.Register<DeviceName>();
    }

    static void IMemoryPackable<DeviceName>.Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref DeviceName value) => writer.WriteString(value._value);

    static void IMemoryPackable<DeviceName>.Deserialize(ref MemoryPackReader reader, scoped ref DeviceName value) => value = From(reader.ReadString());
}*/