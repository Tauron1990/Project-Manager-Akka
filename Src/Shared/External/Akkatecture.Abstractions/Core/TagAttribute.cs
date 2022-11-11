using JetBrains.Annotations;

namespace Akkatecture.Core;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
[PublicAPI]
public sealed class TagAttribute : Attribute, ITagAttribute
{
    public TagAttribute(string name)
        => Name = name;

    public string Name { get; }
}