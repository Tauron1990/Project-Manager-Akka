using System;
using JetBrains.Annotations;

namespace Tauron.Application.Workshop.StateManagement.Attributes
{
    [MeansImplicitUse(ImplicitUseKindFlags.Access)]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    [BaseTypeRequired(typeof(StateBase<>))]
    [PublicAPI]
    public sealed class StateAttribute : Attribute
    {
        public string? Key { get; set; }
    }
}