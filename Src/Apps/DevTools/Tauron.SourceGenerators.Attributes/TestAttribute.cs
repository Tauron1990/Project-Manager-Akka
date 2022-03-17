using JetBrains.Annotations;

namespace Tauron.SourceGenerators.Attributes;

[PublicAPI]
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class TestAttribute : Attribute
{
    
}