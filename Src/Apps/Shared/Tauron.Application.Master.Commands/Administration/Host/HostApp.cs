using JetBrains.Annotations;
using Stl.IO;

namespace Tauron.Application.Master.Commands.Administration.Host;

[PublicAPI]
public sealed record HostApp(string SoftwareName, AppName Name, FilePath Path, SimpleVersion AppVersion, AppType AppType, string Exe, AppState Running);