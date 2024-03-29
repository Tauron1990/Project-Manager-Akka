﻿using System.ComponentModel;
using Tauron.Application.Files.Serialization.Core.Managment;

namespace Tauron.Application.Files.Serialization.Core.Impl.Converter
{
    internal class TypeConverterConverter : SimpleConverter<string>
    {
        private readonly TypeConverter _converter;

        public TypeConverterConverter(TypeConverter converter)
            => _converter = Argument.NotNull(converter, nameof(converter));

        public override object ConvertBack(string target) => _converter.ConvertFromString(target);

        public override string Convert(object? source)
            => _converter.ConvertToString(Argument.NotNull(source, nameof(source))) ?? string.Empty;
    }
}