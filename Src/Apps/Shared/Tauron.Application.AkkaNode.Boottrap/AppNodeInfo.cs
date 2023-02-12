using JetBrains.Annotations;
using Tauron.Application.Master.Commands;

namespace Tauron.Application.AkkaNode.Bootstrap;

[UsedImplicitly]
public sealed class AppNodeInfo
{
    public AppName ApplicationName { get; set; } = AppName.Empty;

    public string Actorsystem { get; set; } = string.Empty;

    public string AppsLocation { get; set; } = string.Empty;
}