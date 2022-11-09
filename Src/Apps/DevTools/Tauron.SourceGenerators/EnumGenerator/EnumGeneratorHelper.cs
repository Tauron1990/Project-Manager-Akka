using System.Text;

namespace Tauron.SourceGenerators.EnumGenerator;

public static class EnumGenerationHelper
{
    public const string Namespace = "Tauron.SourceGenerators.EnumGenerators";
    public const string TypeName = "EnumExtensionsAttribute";

    public const string FullName = $"{Namespace}.{TypeName}";

    public static string GenerateExtensionClass(IEnumerable<EnumToGenerate> enumsToGenerate, string namespaceName)
    {

        var sb = new StringBuilder();
        sb.Append("namespace ").Append(namespaceName).Append(";");
        foreach (EnumToGenerate enumToGenerate in enumsToGenerate)
        {
            sb.Append(
                @"
public static partial class ").Append(enumToGenerate.ExtensionName).Append(
                @"
{
    public static string ToStringFast(this ").Append(enumToGenerate.Name).Append(
                @" value)
        => value switch
        {");
            foreach (string? member in enumToGenerate.Values)
                sb.Append(
                        @"
            ")
                   .Append(enumToGenerate.Name).Append('.').Append(member)
                   .Append(" => nameof(")
                   .Append(enumToGenerate.Name).Append('.').Append(member).Append("),");

            sb.Append(
                @"
            _ => value.ToString(),
        };
}
");
        }

        return sb.ToString();
    }
}