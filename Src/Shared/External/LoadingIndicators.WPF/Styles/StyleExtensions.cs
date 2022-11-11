using System;
using System.Windows;

namespace LoadingIndicators.WPF.Styles;

internal static class StyleExtensions
{
    internal static void Merge(this Style mergeIn, Style toMerge)
    {
        if(mergeIn is null) throw new ArgumentNullException(nameof(mergeIn));

        if(toMerge is null) throw new ArgumentNullException(nameof(toMerge));

        if(mergeIn.TargetType.IsAssignableFrom(toMerge.TargetType)) mergeIn.TargetType = toMerge.TargetType;

        if(toMerge.BasedOn != null) Merge(mergeIn, toMerge.BasedOn);

        foreach (SetterBase? currentSetter in toMerge.Setters)
            mergeIn.Setters.Add(currentSetter);
        foreach (TriggerBase? currentTrigger in toMerge.Triggers)
            mergeIn.Triggers.Add(currentTrigger);

        // This code is only needed when using DynamicResources.
        foreach (object? key in toMerge.Resources.Keys)
            mergeIn.Resources[key] = toMerge.Resources[key];
    }
}