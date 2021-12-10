using Tauron.Operations;

namespace Tauron.Application.CommonUI.ModelMessages;

public sealed record ValidatingEvent(Error? Reason, string Name)
{
    public bool Error => Reason != null;
}