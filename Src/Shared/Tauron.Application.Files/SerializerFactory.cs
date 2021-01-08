using System.Xml.Linq;
using JetBrains.Annotations;
using Tauron.Application.Files.Serialization.Core.Fluent;
using Tauron.Application.Files.Serialization.Core.Fluent.Impl;
using Tauron.Application.Files.Serialization.Core.Impl;

namespace Tauron.Application.Files
{
    [PublicAPI]
    public static class SerializerFactory
    {
        public static IBinaryConfiguration CreateBinary() => new BinarySerializerConfiguration();

        public static IIniSerializerConfiguration CreateIni<TType>() => new IniConfiguration(typeof(TType));

        public static IXmlSerializerConfiguration CreateXml<TType>(string rootName, XDeclaration? xDeclaration, [NotNull] XNamespace rootNamespace) => new XmlSerializerConfiguration(rootName, xDeclaration, rootNamespace, typeof(TType));

        public static IXmlSerializerConfiguration CreateXml<TType>(string rootName, XDeclaration? xDeclaration) => CreateXml<TType>(rootName, xDeclaration, XNamespace.None);

        public static IXmlSerializerConfiguration CreateXml<TType>(string rootName) => CreateXml<TType>(rootName, null);

        public static IXmlSerializerConfiguration CreateXml<TType>() => CreateXml<TType>("Root");

        public static IHeaderedFileSerializerConfiguration CreateHeaderedFile<TType>() => new HeaderedFileSerializerConfiguration(typeof(TType));
    }
}