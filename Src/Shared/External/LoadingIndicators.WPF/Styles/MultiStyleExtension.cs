using System;
using System.Windows;
using System.Windows.Markup;

namespace LoadingIndicators.WPF.Styles;

[MarkupExtensionReturnType(typeof(Style))]
public class MultiStyleExtension : MarkupExtension
{
    private readonly string[] _resourceKeys;

    public MultiStyleExtension(string inputResourceKeys)
    {
        if(string.IsNullOrWhiteSpace(inputResourceKeys))
            throw new ArgumentNullException(nameof(inputResourceKeys));

        _resourceKeys = inputResourceKeys.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        if(_resourceKeys.Length == 0) throw new ArgumentException("No resource Keys Provided", nameof(inputResourceKeys));
    }


    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var resultStyle = new Style();

        foreach (var resourceKey in _resourceKeys)
        {
            var key = (object)resourceKey;
            if(string.Equals(resourceKey, ".", StringComparison.Ordinal))
            {
                var service = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;
                key = service?.TargetObject.GetType();
            }

            if(key is null || new StaticResourceExtension(key).ProvideValue(serviceProvider) is not Style currentStyle)
                throw new InvalidOperationException("No Style Found");

            resultStyle.Merge(currentStyle);
        }

        return resultStyle;
    }
}