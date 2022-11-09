using Akkatecture.ValueObjects;

namespace SimpleProjectManager.Shared;

public sealed class ProjectName : SingleValueObject<string>
{
    public static readonly ProjectName Empty = new(string.Empty);
    public ProjectName(string value) : base(NormalizeName(value)) { }

    private static string NormalizeName(string input)
    {
        string result = input.ToUpper();

        return result.StartsWith("BM") ? result : input;
    }
}