using System;
using System.Globalization;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;

namespace Tauron.Application.Blazor;

[PublicAPI]
public static class NavigationManagerExtensions
{
    public static bool TryGetQueryString<T>(this NavigationManager navManager, string key, out T? value)
    {
        Uri uri = navManager.ToAbsoluteUri(navManager.Uri);

        if(QueryHelpers.ParseQuery(uri.Query).TryGetValue(key, out StringValues valueFromQueryString))
        {
            if(typeof(T) == typeof(int) && int.TryParse(valueFromQueryString, NumberStyles.Any, CultureInfo.InvariantCulture, out int valueAsInt))
            {
                value = (T)(object)valueAsInt;

                return true;
            }

            if(typeof(T) == typeof(string))
            {
                value = (T)(object)valueFromQueryString.ToString();

                return true;
            }

            if(typeof(T) == typeof(decimal) && decimal.TryParse(valueFromQueryString, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal valueAsDecimal))
            {
                value = (T)(object)valueAsDecimal;

                return true;
            }
        }

        value = default;

        return false;
    }
}