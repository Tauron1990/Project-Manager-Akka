//HintName: EnumExtensionsAttribute.g.cs

namespace Tauron.SourceGenerators.EnumGenerators
{
    [System.AttributeUsage(System.AttributeTargets.Enum)]
    public class EnumExtensionsAttribute : System.Attribute
    {
        public string ExtensionClassName { get; set; }
        public string ExtensionNamespaceName { get; set; }
    }
}