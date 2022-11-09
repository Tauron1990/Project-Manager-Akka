using Avalonia.Data.Converters;

namespace Tauron.Application.Avalonia.Converter;

public sealed class NullableLongToIntConverter : ValueConverterFactoryBase
{
    protected override IValueConverter Create() => new Converter();

    private class Converter : ValueConverterBase<int, long?>
    {
        protected override bool CanConvertBack => true;

        protected override long? Convert(int value) => value;

        protected override int ConvertBack(long? value) => value == null ? 0 : (int)value.Value;
    }
}