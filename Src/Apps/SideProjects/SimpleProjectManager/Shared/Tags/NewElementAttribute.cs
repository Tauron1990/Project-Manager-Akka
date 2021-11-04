using Akkatecture.Core;

namespace SimpleProjectManager.Shared.Tags;

[AttributeUsage(AttributeTargets.Class)]
public sealed class NewElementAttribute : Attribute, ITagAttribute
{
    public string Name => TagName.NewElement;
}