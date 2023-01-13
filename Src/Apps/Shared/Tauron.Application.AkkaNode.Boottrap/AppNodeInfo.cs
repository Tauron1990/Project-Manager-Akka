using JetBrains.Annotations;

namespace Tauron.Application.AkkaNode.Bootstrap;

[UsedImplicitly]
public sealed class AppNodeInfo
{
    public string ApplicationName { get; set; } = string.Empty;

    public string Actorsystem { get; set; } = string.Empty;

    public string AppsLocation { get; set; } = string.Empty;
}