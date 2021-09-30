using System;
using System.Threading;
using System.Threading.Tasks;
using ServiceManager.Client.Shared.BaseComponents.Wizard;
using ServiceManager.Client.ViewModels.Apps;
using ServiceManager.Shared.Apps;
using Tauron.Application;

namespace ServiceManager.Client.Shared.Apps.NewApp
{
    public sealed class RepositoryPage : WizardPage<LocalAppInfo>
    {
        private readonly IEventAggregator _aggregator;
        private readonly IAppManagment _appManagment;
        public override string Title => "Github Repository Auswählen";

        public RepositoryPage(IEventAggregator aggregator, IAppManagment appManagment)
        {
            _aggregator = aggregator;
            _appManagment = appManagment;
        }

        protected override async Task<string?> VerifyNextImpl(WizardContext<LocalAppInfo> context, CancellationToken token)
        {
            try
            {
                var result = _appManagment.QueryRepository(context.Data.Repository, token);
            }
            catch (Exception e)
            {
                _aggregator.PublishError(e);

                return e.Message;
            }
        }

        protected override Task<bool> NeedRender(WizardContext<LocalAppInfo> context)
            => throw new NotImplementedException();

        protected override Task<Type> Init(WizardContext<LocalAppInfo> context, CancellationToken cancellationToken)
            => throw new NotImplementedException();
    }
}