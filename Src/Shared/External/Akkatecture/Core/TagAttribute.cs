using System;
using JetBrains.Annotations;

namespace Akkatecture.Core
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true), PublicAPI]
    public sealed class TagAttribute : Attribute
    {
        public TagAttribute(string name) => Name = name;

        public string Name { get; }
    }
}