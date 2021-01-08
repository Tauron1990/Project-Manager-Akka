using System;
using System.Globalization;
using System.Windows.Data;
using JetBrains.Annotations;
using Tauron.Application.CommonUI.UI;
using Tauron.Application.Wpf.Converter;

namespace Tauron.Application.Wpf.UI
{
    [PublicAPI]
    public sealed class ViewModelConverterExtension : ValueConverterFactoryBase
    {
        protected override IValueConverter Create() => new Conv();

        private sealed class Conv : IValueConverter
        {
            private readonly ViewModelConverter _converter = new();

            public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => _converter.Convert(value);

            public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => _converter.ConvertBack(value);
        }
    }
}