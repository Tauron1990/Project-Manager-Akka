using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using JetBrains.Annotations;

namespace Tauron.Application.Wpf.Converter
{
    [MarkupExtensionReturnType(typeof(IValueConverter))]
    [DebuggerNonUserCode]
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public abstract class ValueConverterFactoryBase : MarkupExtension
    {
        public IServiceProvider? ServiceProvider { get; set; }

        protected abstract IValueConverter Create();

        protected static IValueConverter CreateStringConverter<TType>(Func<TType, string> converter)
            => new FuncStringConverter<TType>(converter);

        protected static IValueConverter CreateCommonConverter<TSource, TDest>(Func<TSource, TDest> converter)
            => new FuncCommonConverter<TSource, TDest>(converter);

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;

            return Create();
        }

        private class FuncCommonConverter<TSource, TDest> : ValueConverterBase<TSource, TDest>
        {
            private readonly Func<TSource, TDest> _func;

            internal FuncCommonConverter(Func<TSource, TDest> func) => _func = func;

            protected override TDest Convert(TSource value) => _func(value);
        }

        private class FuncStringConverter<TType> : StringConverterBase<TType>
        {
            private readonly Func<TType, string> _converter;

            internal FuncStringConverter(Func<TType, string> converter) => _converter = converter;

            protected override string Convert(TType value) => _converter(value);
        }

        protected abstract class StringConverterBase<TSource> : ValueConverterBase<TSource, string> { }

        protected abstract class ValueConverterBase<TSource, TDest> : IValueConverter
        {
            protected virtual bool CanConvertBack => false;

            public virtual object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                //if (value is TDest && typeof(TSource) != typeof(TDest)) return value;
                if (value is not TSource source) return null;

                return Convert(source);
            }

            public virtual object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (!CanConvertBack || value is not TDest dest) return null;

                return ConvertBack(dest);
            }

            protected abstract TDest Convert(TSource value);

            protected virtual TSource ConvertBack(TDest value) => default!;
        }
    }
}