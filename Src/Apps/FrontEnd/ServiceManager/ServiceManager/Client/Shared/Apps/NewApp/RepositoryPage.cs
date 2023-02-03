using System;
using System.Threading;
using System.Threading.Tasks;
using ServiceManager.Client.Shared.BaseComponents.Wizard;
using ServiceManager.Shared.Apps;
using Tauron.Application;

namespace ServiceManager.Client.Shared.Apps.NewApp
{
    public sealed class RepositoryPage : WizardPage<AppRegistrationInfo>
    {
        private readonly IEventAggregator _aggregator;
        private readonly IAppManagment _appManagment;
        public override string Title => "Github Repository Auswählen";

        public RepositoryPage(IEventAggregator aggregator, IAppManagment appManagment)
        {
            _aggregator = aggregator;
            _appManagment = appManagment;
        }

        protected override async Task<string?> VerifyNextImpl(WizardContext<AppRegistrationInfo> context, CancellationToken token)
        {
            try
            {
                var (isOk, error) = await _appManagment.QueryRepository(context.Data.Repository, token);

                return isOk ? null : error;
            }
            catch (Exception e)
            {
                _aggregator.PublishError(e);

                return e.Message;
            }
        }

        protected override Task<bool> NeedRender(WizardContext<AppRegistrationInfo> context)
            => Task.FromResult(true);

        protected override Task<Type> Init(WizardContext<AppRegistrationInfo> context, CancellationToken cancellationToken)
            => Task.FromResult(typeof(RepositoryNameComponent));
    }
}