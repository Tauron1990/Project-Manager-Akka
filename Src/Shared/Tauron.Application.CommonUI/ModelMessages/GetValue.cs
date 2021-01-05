using JetBrains.Annotations;

namespace Tauron.Application.CommonUI.ModelMessages
{
    public sealed record GetValueRequest(string Name);

    [PublicAPI]
    public sealed record GetValueResponse(string Name, object? Value);
}