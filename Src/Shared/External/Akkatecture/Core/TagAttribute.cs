using System;

namespace Akkatecture.Core
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class TagAttribute : Attribute
    {
        public TagAttribute(string name) => Name = name;

        public string Name { get; }
    }
}