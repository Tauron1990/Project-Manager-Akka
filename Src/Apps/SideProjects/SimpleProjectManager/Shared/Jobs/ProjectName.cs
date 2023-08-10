using System.Globalization;
using System.Runtime.Serialization;
using Akkatecture.ValueObjects;
using MemoryPack;

namespace SimpleProjectManager.Shared;

[DataContract, MemoryPackable]
public sealed partial class ProjectName : SingleValueObject<string>
{
    public static readonly ProjectName Empty = new(string.Empty);

    public override string Value => base.Value;

    public ProjectName(string value) : base(NormalizeName(value)) { }

    private static string NormalizeName(string input)
    {
        string result = input.ToUpper(CultureInfo.InvariantCulture);

        return result.StartsWith("BM", StringComparison.Ordinal) ? result : input;
    }
}