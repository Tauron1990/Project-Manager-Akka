using JetBrains.Annotations;
using NRules.Fluent;

namespace SpaceConqueror.Modules;

[UsedImplicitly]
public sealed record ModuleConfiguration(RuleRepository RuleRepository, StateRegistrar State, RuleRegistrar Rules, ManagerRegistrar ManagerRegistrar);