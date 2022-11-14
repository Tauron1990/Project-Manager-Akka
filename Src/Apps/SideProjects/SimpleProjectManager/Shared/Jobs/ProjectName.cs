using System;
using System.Globalization;
using Akkatecture.ValueObjects;

namespace SimpleProjectManager.Shared;

#pragma warning disable MA0097
public sealed class ProjectName : SingleValueObject<string>
    #pragma warning restore MA0097
{
    public static readonly ProjectName Empty = new(string.Empty);
    public ProjectName(string value) : base(NormalizeName(value)) { }

    private static string NormalizeName(string input)
    {
        string result = input.ToUpper(CultureInfo.InvariantCulture);

        return result.StartsWith("BM", StringComparison.Ordinal) ? result : input;
    }
}