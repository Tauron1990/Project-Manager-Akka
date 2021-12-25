using Tauron.Application.CommonUI.Commands;

namespace Tauron.Application.CommonUI.ModelMessages;

public sealed record ExecuteEventEvent(EventData Data, string Name);