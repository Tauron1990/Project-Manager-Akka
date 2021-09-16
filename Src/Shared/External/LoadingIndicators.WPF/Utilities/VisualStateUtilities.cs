using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Windows;

namespace LoadingIndicators.WPF.Utilities
{
    #pragma warning disable AV1708
    internal static class VisualStateUtilities
        #pragma warning restore AV1708
    {
        private static IEnumerable<VisualStateGroup>? GetActiveVisualStateGroups(this FrameworkElement element)
            => element.GetVisualStateGroupsByName(IndicatorVisualStateGroupNames.ActiveStates.Name);

        internal static IEnumerable<VisualState>? GetActiveVisualStates(this FrameworkElement element) 
            => element.GetActiveVisualStateGroups()?.GetAllVisualStatesByName(IndicatorVisualStateNames.ActiveState.Name);

        private static IEnumerable<VisualState> GetAllVisualStatesByName(
            this IEnumerable<VisualStateGroup> visualStateGroups, string name)
            => visualStateGroups.SelectMany(vsg => vsg.GetVisualStatesByName(name) ?? ImmutableArray<VisualState>.Empty);

        private static IEnumerable<VisualState>? GetVisualStatesByName(this VisualStateGroup? visualStateGroup,
                                                                      string name)
        {
            #pragma warning disable AV1135
            if (visualStateGroup is null) return null;
            #pragma warning restore AV1135

            var visualStates = visualStateGroup.GetVisualStates();

            return string.IsNullOrWhiteSpace(name) ? visualStates : visualStates?.Where(vs => vs.Name == name);
        }

        private static IEnumerable<VisualStateGroup>? GetVisualStateGroupsByName(this FrameworkElement element,
                                                                                 string name)
        {
            var groups = VisualStateManager.GetVisualStateGroups(element);

            #pragma warning disable AV1135
            if (groups is null) return null;


            IEnumerable<VisualStateGroup> castedVisualStateGroups;

            try
            {
                castedVisualStateGroups = groups.Cast<VisualStateGroup>().ToArray();
                if (!castedVisualStateGroups.Any()) return null;
            }
            catch (InvalidCastException)
            {
                return null;
            }

            return string.IsNullOrWhiteSpace(name)
                ? castedVisualStateGroups
                : castedVisualStateGroups.Where(vsg => vsg.Name == name);
            
            #pragma warning restore AV1135
        }

        private static IEnumerable<VisualState>? GetVisualStates(this VisualStateGroup? visualStateGroup)
        {
            #pragma warning disable AV1135
            if (visualStateGroup is null) return null;
            #pragma warning restore AV1135

            return visualStateGroup.States.Count == 0 ? null : visualStateGroup.States.Cast<VisualState>();
        }
    }
}