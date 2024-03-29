﻿using JetBrains.Annotations;

namespace Tauron.Application.Files.Serialization.Core.Fluent.HeaderedFile
{
    [PublicAPI]
    public interface IHeaderedFileSerializerConfiguration : ISerializerRootConfiguration,
        IConstructorConfig<IHeaderedFileSerializerConfiguration>
    {
        IHeaderedFileKeywordConfiguration AddKeyword([NotNull] string key);

        IHeaderedFileKeywordConfiguration AddKeywordList([NotNull] string key);

        IHeaderedFileKeywordConfiguration MapContent();
    }
}