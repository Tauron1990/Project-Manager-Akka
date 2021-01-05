using Avalonia.Data.Converters;

namespace Tauron.Application.Avalonia.Converter
{
    public sealed class NullableLongToIntConverter : ValueConverterFactoryBase
    {
        protected override IValueConverter Create()
        {
            return new Converter();
        }

        private class Converter : ValueConverterBase<int, long?>
        {
            protected override bool CanConvertBack => true;

            protected override long? Convert(int value)
            {
                return value;
            }

            protected override int ConvertBack(long? value)
            {
                return value == null ? 0 : (int) value.Value;
            }
        }
    }
}