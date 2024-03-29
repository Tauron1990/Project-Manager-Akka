﻿using System;
using JetBrains.Annotations;
using Tauron.Application.Files.HeaderedText;
using Tauron.Application.Files.Serialization.Core.Fluent.HeaderedFile;
using Tauron.Application.Files.Serialization.Core.Impl;
using Tauron.Application.Files.Serialization.Core.Impl.Mapper.HeaderedText;
using Tauron.Application.Files.Serialization.Core.Managment;

namespace Tauron.Application.Files.Serialization.Core.Fluent.Impl.HeaderedFile
{
    internal class HeaderedFileSerializerConfiguration : SerializerRootConfigurationBase,
        IHeaderedFileSerializerConfiguration
    {
        private readonly ObjectBuilder _builder;

        private readonly FileDescription _description = new();
        private readonly SimpleMapper<HeaderdFileContext> _mapper = new();
        private readonly Type _targetType;

        public HeaderedFileSerializerConfiguration([NotNull] Type targetType)
        {
            _targetType = targetType;
            _builder = new ObjectBuilder(targetType);
        }

        public IConstructorConfiguration<IHeaderedFileSerializerConfiguration> ConfigConstructor()
            => new ConstructorConfiguration<IHeaderedFileSerializerConfiguration>(_builder, this);

        public IHeaderedFileKeywordConfiguration AddKeyword(string key)
            => new HeaderedFileKeyCofiguration(this, _mapper, key, MappingType.SingleKey, _targetType);

        public IHeaderedFileKeywordConfiguration AddKeywordList(string key)
            => new HeaderedFileKeyCofiguration(this, _mapper, key, MappingType.MultiKey, _targetType);

        public IHeaderedFileKeywordConfiguration MapContent()
            => new HeaderedFileKeyCofiguration(this, _mapper, "Content", MappingType.Content, _targetType);

        public override ISerializer ApplyInternal()
        {
            var serializer = new HeaderedTextSerializer(_builder, _mapper, _description);

            VerifyErrors(serializer);

            return serializer;
        }
    }
}