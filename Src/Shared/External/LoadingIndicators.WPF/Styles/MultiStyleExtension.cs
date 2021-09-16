using System;
using System.Windows;
using System.Windows.Markup;

namespace LoadingIndicators.WPF.Styles
{
    [MarkupExtensionReturnType(typeof(Style))]
    public class MultiStyleExtension : MarkupExtension
    {
        private readonly string[] _resourceKeys;

        public MultiStyleExtension(string inputResourceKeys)
        {
            if (string.IsNullOrWhiteSpace(inputResourceKeys))
                throw new ArgumentNullException(nameof(inputResourceKeys));

            _resourceKeys = inputResourceKeys.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);

            if (_resourceKeys.Length == 0) throw new ArgumentException("No resource Keys Provided", nameof(inputResourceKeys));
        }


        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var resultStyle = new Style();

            foreach (var resourceKey in _resourceKeys)
            {
                var key = (object) resourceKey;
                if (resourceKey == ".")
                {
                    var service = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;
                    key = service?.TargetObject.GetType();
                }

                if (key is null || new StaticResourceExtension(key).ProvideValue(serviceProvider) is not Style currentStyle) 
                    throw new InvalidOperationException("No Style Found");

                resultStyle.Merge(currentStyle);
            }

            return resultStyle;
        }
    }

    internal static class StyleExtensions
    {
        internal static void Merge(this Style mergeIn, Style toMerge)
        {
            if (mergeIn is null) throw new ArgumentNullException(nameof(mergeIn));

            if (toMerge is null) throw new ArgumentNullException(nameof(toMerge));

            if (mergeIn.TargetType.IsAssignableFrom(toMerge.TargetType)) mergeIn.TargetType = toMerge.TargetType;

            if (toMerge.BasedOn != null) Merge(mergeIn, toMerge.BasedOn);

            foreach (var currentSetter in toMerge.Setters)
                mergeIn.Setters.Add(currentSetter);
            foreach (var currentTrigger in toMerge.Triggers)
                mergeIn.Triggers.Add(currentTrigger);

            // This code is only needed when using DynamicResources.
            foreach (var key in toMerge.Resources.Keys)
                mergeIn.Resources[key] = toMerge.Resources[key];
        }
    }
}