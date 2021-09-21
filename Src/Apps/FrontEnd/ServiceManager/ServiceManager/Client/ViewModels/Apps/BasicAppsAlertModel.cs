using ServiceManager.Shared.Apps;
using Stl.Fusion;

namespace ServiceManager.Client.ViewModels.Apps
{
    public sealed class BasicAppsAlertModel
    {
        public BasicAppsAlertModel(IStateFactory factory, IAppManagment appManagment)
        {
            SetupState = factory.NewComputed(
                new ComputedState<NeedSetupData>.Options(),
                (_, t) => appManagment.NeedBasicApps(t));
        }

        public IState<NeedSetupData> SetupState { get; }
    }
}