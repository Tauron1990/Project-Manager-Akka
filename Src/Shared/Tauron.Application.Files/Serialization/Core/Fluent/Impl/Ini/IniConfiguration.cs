using System;
using JetBrains.Annotations;
using Tauron.Application.Files.Serialization.Core.Impl;
using Tauron.Application.Files.Serialization.Core.Impl.Mapper.Ini;
using Tauron.Application.Files.Serialization.Core.Managment;

namespace Tauron.Application.Files.Serialization.Core.Fluent.Impl
{
    internal class IniConfiguration : SerializerRootConfigurationBase, IIniSerializerConfiguration
    {
        private readonly ObjectBuilder _builder;
        private readonly SimpleMapper<IniContext> _mapper = new();
        private readonly Type _targetType;

        public IniConfiguration([NotNull] Type targetType)
        {
            _targetType = targetType;
            _builder = new ObjectBuilder(targetType);
        }

        public IConstructorConfiguration<IIniSerializerConfiguration> ConfigConstructor() => new ConstructorConfiguration<IIniSerializerConfiguration>(_builder, this);

        public IIniSectionSerializerConfiguration FromSection(string name) => new IniSectionConfiguration(name, _mapper, this, _targetType);

        public ISerializerToMemberConfiguration<IIniSerializerConfiguration> MapSerializer<TToSerial>(string memberName) => new SerializerToMemberConfiguration<IIniSerializerConfiguration, IniContext>(memberName, this, _mapper,
                                                                                                                                                                                                         typeof(TToSerial));

        public override ISerializer ApplyInternal()
        {
            var ser = new IniSerializer(_builder, _mapper);

            VerifyErrors(ser);

            return ser;
        }
    }
}