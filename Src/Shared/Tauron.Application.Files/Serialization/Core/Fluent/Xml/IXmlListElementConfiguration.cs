using JetBrains.Annotations;

namespace Tauron.Application.Files.Serialization.Core.Fluent.Xml
{
    [PublicAPI]
    public interface IXmlListElementConfiguration : IXmlRootConfiguration<IXmlListElementConfiguration>
    {
        IXmlListElementConfiguration Element(string name);

        IXmlListAttributeConfiguration Attribute(string name);

        IXmlSerializerConfiguration WithSubSerializer<TSerisalize>(ISerializer serializer);
    }
}