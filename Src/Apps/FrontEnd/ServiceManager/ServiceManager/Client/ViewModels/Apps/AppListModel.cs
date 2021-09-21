using System;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using GridBlazor;
using GridShared;
using GridShared.Pagination;
using GridShared.Totals;
using GridShared.Utility;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using ServiceManager.Client.Shared.Apps;
using ServiceManager.Shared.Api;
using ServiceManager.Shared.Apps;
using Stl.Fusion;
using Tauron.Application;
using Tauron.Application.Master.Commands.Deployment.Build.Data;

namespace ServiceManager.Client.ViewModels.Apps
{
    public sealed class AppListViewModel
    {
        public AppListViewModel(IStateFactory factory, IAppManagment appManagment, IEventAggregator aggregator, IApiMessageTranslator translator, ILogger<AppListViewModel> log)
        {
            var dataState = factory.NewComputed<AppList>((_, t) => appManagment.QueryAllApps(t));

            var gridClient = new GridClient<AppInfo>(
                    async _ =>
                    {
                        var data = (await dataState.Update()).Value;

                        return new ItemsDTO<AppInfo>(
                            data,
                            new TotalsDTO(),
                            new PagerDTO
                            {
                                EnablePaging = false,
                                ItemsCount = data.Apps.Count
                            });
                    },
                    new QueryDictionary<StringValues>(),
                    renderOnlyRows: false,
                    "AppInfoGrid")
               .Columns(
                    collection =>
                    {
                        collection.Add().RenderComponentAs<AppGridDisplayButton>();
                        collection.Add(ai => ai.Name).Titled("Name").SetPrimaryKey(enabled: true);
                        collection.Add(ai => ai.Repository).Titled("Repository");
                        collection.Add(ai => ai.ProjectName).Titled("Projekt Name");
                        collection.Add(ai => ai.LastVersion).Titled("Letzte Version").RenderComponentAs<AppVersionDisplay>();
                        collection.Add(ai => ai.UpdateDate).Titled("Letztes Update").RenderComponentAs<AppUpdateDisplay>();
                        collection.Add(ai => ai.CreationTime).Titled("Erstellt am");
                        collection.Add().Titled("Binaries").RenderValueAs(ai => ai.Binaries.Count.ToString());
                    })
               .SetLanguage("de-de")
               .Searchable()
               .Sortable()
               .ExtSortable()
               .Groupable()
               .EmptyText("Keine Anwendungen")
               .HandleServerErrors(showOnGrid: true, throwExceptions: false)
               .SetDeleteConfirmation(enabled: true)
               .SetHeaderCrudButtons(enabled: true)
               .Crud(
                    createEnabled: true,
                    readEnabled: true,
                    updateEnabled: false,
                    deleteEnabled: true,
                    new InternalDataService(appManagment, aggregator, translator, log));


            Update = gridClient.UpdateGrid();
            Grid = gridClient.Grid;

            dataState.AddEventHandler(StateEventKind.Updated, (_, _) => Update = Grid.UpdateGrid());
        }

        public Task Update { get; set; }

        public CGrid<AppInfo> Grid { get; }

        private sealed class InternalDataService : ICrudDataService<AppInfo>
        {
            private readonly IEventAggregator _aggregator;
            private readonly IAppManagment _appManagment;
            private readonly ILogger _log;
            private readonly IApiMessageTranslator _translator;

            internal InternalDataService(IAppManagment appManagment, IEventAggregator aggregator, IApiMessageTranslator translator, ILogger log)
            {
                _appManagment = appManagment;
                _aggregator = aggregator;
                _translator = translator;
                _log = log;
            }

            public Task<AppInfo> Get(params object[] keys)
                => Run(
                    async () =>
                    {
                        var name = (string)keys[0];

                        var app = await _appManagment.QueryApp(name, CancellationToken.None);

                        if (app.Deleted)
                            throw new InvalidOperationException($"Fhler beim abrufen der Anwendung: {app.Name}");

                        return app;
                    });

            public Task Insert(AppInfo item)
                => Run(
                    async () =>
                    {
                        var result = await _appManagment.CreateNewApp(new ApiCreateAppCommand(item.Name, item.ProjectName, item.Repository), CancellationToken.None);

                        if (string.IsNullOrWhiteSpace(result)) return Unit.Default;

                        throw new InvalidOperationException(_translator.Translate(result));
                    });

            public Task Update(AppInfo item)
                => throw new NotSupportedException("Updates für Anwendungen nicht unterstützt");

            public Task Delete(params object[] keys)
                => Run(
                    async () =>
                    {
                        var result = await _appManagment.DeleteAppCommand(new ApiDeleteAppCommand((string)keys[0]), CancellationToken.None);

                        if (string.IsNullOrWhiteSpace(result)) return Unit.Default;

                        throw new InvalidOperationException(_translator.Translate(result));
                    });

            private async Task<TResult> Run<TResult>(Func<Task<TResult>> runner)
            {
                try
                {
                    return await runner();
                }
                catch (Exception e)
                {
                    _log.LogError(e, "Error on Run Data Service Action");
                    _aggregator.PublishError(_translator.Translate(e.Message));

                    throw;
                }
            }
        }
    }
}