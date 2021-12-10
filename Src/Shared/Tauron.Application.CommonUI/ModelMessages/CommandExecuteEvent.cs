namespace Tauron.Application.CommonUI.ModelMessages;

public sealed record CommandExecuteEvent(string Name, object? Parameter);