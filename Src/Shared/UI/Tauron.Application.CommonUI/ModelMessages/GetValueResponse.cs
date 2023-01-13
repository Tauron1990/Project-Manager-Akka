using JetBrains.Annotations;

namespace Tauron.Application.CommonUI.ModelMessages;

[PublicAPI]
public sealed record GetValueResponse(string Name, object? Value);