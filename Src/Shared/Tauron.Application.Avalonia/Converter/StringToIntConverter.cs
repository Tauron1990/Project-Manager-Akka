using System;
using Avalonia.Data.Converters;
using JetBrains.Annotations;

namespace Tauron.Application.Avalonia.Converter
{
    [PublicAPI]
    public class StringToIntConverter : ValueConverterFactoryBase
    {
        protected override IValueConverter Create()
        {
            return new Converter();
        }

        private class Converter : StringConverterBase<int>
        {
            protected override bool CanConvertBack => true;

            protected override string Convert(int value)
            {
                return value.ToString();
            }

            protected override int ConvertBack(string value)
            {
                if (string.IsNullOrEmpty(value))
                    return 0;

                try
                {
                    return int.Parse(value);
                }
                catch (Exception e) when (e is ArgumentException || e is FormatException || e is OverflowException)
                {
                    return 0;
                }
            }
        }
    }
}