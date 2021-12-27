using JetBrains.Annotations;

namespace Tauron.Application.Workshop.StateManagement;

[PublicAPI]
public sealed record ServiceOptions(bool ResolveEffects = true, bool ResolveMiddleware = true, bool RegisterSuperviser = true);