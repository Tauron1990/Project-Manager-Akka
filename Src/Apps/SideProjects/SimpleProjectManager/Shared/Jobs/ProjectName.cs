using Akkatecture.ValueObjects;
using JetBrains.Annotations;

namespace SimpleProjectManager.Shared;

public sealed class ProjectName : SingleValueObject<string>
{
    public ProjectName(string value) : base(NormalizeName(value)) { }

    private static string NormalizeName(string input)
    {
        var result = input.ToUpper();

        return result.StartsWith("BM") ? result : input;
    }

    public static readonly ProjectName Empty = new(string.Empty);
}