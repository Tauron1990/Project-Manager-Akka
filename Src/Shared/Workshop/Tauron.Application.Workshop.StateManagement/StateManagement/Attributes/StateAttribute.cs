using JetBrains.Annotations;

namespace Tauron.Application.Workshop.StateManagement.Attributes;

[MeansImplicitUse(ImplicitUseKindFlags.Access)]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
[PublicAPI]
public sealed class StateAttribute : Attribute
{
    public StateAttribute(params Type[] types)
        => Types = types;

    public string? Key { get; set; }

    public Type[] Types { get; }
}