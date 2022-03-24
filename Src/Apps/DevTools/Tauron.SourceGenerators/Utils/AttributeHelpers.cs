using System.Collections.Immutable;
using System.Text;

namespace Tauron.SourceGenerators.Utils;

public static class AttributeHelpers
{
    public static Func<string> GenerateAttribute(AttributeTargets targets, string namespaceName, string className, ImmutableDictionary<string, string> propertys)
        => () =>
           {
               var builder = new StringBuilder();
               var needNamespace = !string.IsNullOrWhiteSpace(namespaceName);

               if (needNamespace)
                   builder.Append("namespace ").Append(namespaceName).AppendLine(";").AppendLine();

               builder.Append("[System.AttributeUsage(");

               var first = true;

               foreach (var flag in GetFlags(targets))
               {
                   if (!first)
                   {
                       builder.Append("|").Append(" ");
                   }

                   builder.Append("System.AttributeTargets.").Append(flag.ToString()).Append(" ");
                   first = false;
               }

               builder.AppendLine(")]");

               builder.Append("internal class ").Append(className).AppendLine(" : System.Attribute").AppendLine("{");

               foreach (var property in propertys)
               {
                   builder.Append("public ").Append(property.Value).Append(" ").Append(property.Key).AppendLine(" { get; set; }");
               }

               builder.AppendLine("}");

               return builder.ToString();
           };

    private static IEnumerable<AttributeTargets> GetFlags(AttributeTargets targets)
        => Enum.GetValues(typeof(AttributeTargets)).Cast<AttributeTargets>().Where(attr => targets.HasFlag(attr));
}