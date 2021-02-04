using JetBrains.Annotations;

namespace Tauron.Application.Files.Serialization.Core.Fluent.Ini
{
    [PublicAPI]
    public interface IIniSerializerConfiguration : ISerializerRootConfiguration, IConstructorConfig<IIniSerializerConfiguration>
    {
        IIniSectionSerializerConfiguration FromSection(string name);

        ISerializerToMemberConfiguration<IIniSerializerConfiguration> MapSerializer<TToSerial>(string memberName);
    }
}