using NRules.Fluent;
using NRules.RuleModel;

namespace SpaceConqueror.Modules;

public sealed record ModuleConfiguration(RuleRepository RuleRepository, StateRegistrar State);