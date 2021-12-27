using JetBrains.Annotations;

namespace Tauron.Application.Files.Serialization.Core.Fluent.HeaderedFile
{
    [PublicAPI]
    public interface IHeaderedFileKeywordConfiguration : IWithMember<IHeaderedFileKeywordConfiguration>
    {
        IHeaderedFileSerializerConfiguration Apply();
    }
}