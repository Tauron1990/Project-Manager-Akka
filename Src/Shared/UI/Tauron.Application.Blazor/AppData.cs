namespace Tauron.Application.Blazor;

public sealed record AppData(bool IsConnected, bool IsSelf, bool IsDatabaseReady);