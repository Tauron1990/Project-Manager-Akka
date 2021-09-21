using System.Collections.Immutable;
using ServiceManager.Client.ViewModels.Configuration;

namespace ServiceManager.Client.Shared.Configuration
{
    public sealed record AppConfigViewData(ImmutableList<AppConfigModel> ConfigModels, AppConfigData ViewState);
}