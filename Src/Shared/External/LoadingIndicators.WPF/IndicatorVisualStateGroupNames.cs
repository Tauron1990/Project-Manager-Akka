using System;
using System.Windows.Markup;

namespace LoadingIndicators.WPF;

internal class IndicatorVisualStateGroupNames : MarkupExtension
{
    private static IndicatorVisualStateGroupNames? _internalActiveStates;
    private static IndicatorVisualStateGroupNames? _sizeStates;

    private IndicatorVisualStateGroupNames(string name)
    {
        if(string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));

        Name = name;
    }

    internal static IndicatorVisualStateGroupNames ActiveStates =>
        _internalActiveStates ??= new IndicatorVisualStateGroupNames("ActiveStates");

    internal static IndicatorVisualStateGroupNames SizeStates =>
        _sizeStates ??= new IndicatorVisualStateGroupNames("SizeStates");

    internal string Name { get; }

    public override object ProvideValue(IServiceProvider serviceProvider) => Name;
}