using System.Text;

namespace Tauron.SourceGenerators.EnumGenerator;

public static class EnumGenerationHelper
{
    public const string Namespace = "Tauron.SourceGenerators.EnumGenerators";
    private const string TypeName = "EnumExtensionsAttribute";

    public const string FullName = $"{Namespace}.{TypeName}";

    public const string Attribute = $@"
namespace {Namespace}
{{
    [System.AttributeUsage(System.AttributeTargets.Enum)]
    internal class {TypeName} : System.Attribute
    {{
        public string ExtensionClassName {{ get; set; }}
        public string ExtensionNamespaceName {{ get; set; }}
    }}
}}";

    public static string GenerateExtensionClass(IEnumerable<EnumToGenerate> enumsToGenerate, string namespaceName)
    {

        var sb = new StringBuilder();
        sb.Append("namespace ").Append(namespaceName).Append(";");
        foreach (var enumToGenerate in enumsToGenerate)
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
            foreach (var member in enumToGenerate.Values)
            {
                sb.Append(
                        @"
            ")
                   .Append(enumToGenerate.Name).Append('.').Append(member)
                   .Append(" => nameof(")
                   .Append(enumToGenerate.Name).Append('.').Append(member).Append("),");
            }

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