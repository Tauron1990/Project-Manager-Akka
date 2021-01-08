using System;
using JetBrains.Annotations;

namespace Tauron.Application.Workshop.StateManagement.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    [MeansImplicitUse]
    [PublicAPI]
    [BaseTypeRequired(typeof(IEffect))]
    public sealed class EffectAttribute : Attribute { }
}