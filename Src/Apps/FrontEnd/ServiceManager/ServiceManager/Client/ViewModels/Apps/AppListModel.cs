using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
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
using ServiceManager.Client.Shared.Apps.NewApp;
using ServiceManager.Shared.Api;
using ServiceManager.Shared.Apps;
using Stl.Fusion;
using Tauron.Application;
using Tauron.Application.Master.Commands;
using Tauron.Application.Master.Commands.Deployment;
using Tauron.Application.Master.Commands.Deployment.Build;
using Tauron.Application.Master.Commands.Deployment.Build.Data;

#pragma warning disable GU0011

namespace ServiceManager.Client.ViewModels.Apps
{
    public sealed class AppListViewModel : IDisposable
    {
        public AppListViewModel(IStateFactory factory, IAppManagment appManagment, IEventAggregator aggregator, IApiMessageTranslator translator, ILogger<AppListViewModel> log)
        {
            var dataState = factory.NewComputed<AppList>((_, t) => appManagment.QueryAllApps(t));

            var gridClient = CreateMainGrid(appManagment, aggregator, translator, log, dataState);

            Update = gridClient.UpdateGrid();
            Grid = gridClient.Grid;

            dataState.AddEventHandler(StateEventKind.Updated, (_, _) => _onUpdate.OnNext(Unit.Default));
        }

        private readonly Subject<Unit> _onUpdate = new();

        public IObservable<Unit> UpdateEvent => _onUpdate.AsObservable();

        public Task Update { get; set; }

        public CGrid<LocalAppInfo> Grid { get; }

        private IGridClient<LocalAppInfo> CreateMainGrid(IAppManagment appManagment, IEventAggregator aggregator, IApiMessageTranslator translator, 
            ILogger log, IComputedState<AppList> dataState)
        {
            var gridClient = new GridClient<LocalAppInfo>(
                    async _ =>
                    {
                        var data = (await dataState.Update()).Value;
                        
                        return new ItemsDTO<LocalAppInfo>(
                            data.Apps.Select(ai => new LocalAppInfo(ai)),
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
                        collection.Add().RenderComponentAs<AppGridDisplayButton>().SetCrudHidden(all: true).GetCellCssClasses("mx-3");
                        collection.Add(ai => ai.Name).Titled("Name").SetPrimaryKey(enabled: true);
                        collection.Add(ai => ai.Repository).Titled("Repository").SetCrudHidden(create:false, read:false, update:true, delete:true).GetCellCssClasses("mx-3");
                        collection.Add(ai => ai.ProjectName).Titled("Projekt Name").SetCrudHidden(create:false, read:false, update:true, delete:true).GetCellCssClasses("mx-3");
                        collection.Add(ai => ai.LastVersion).Titled("Letzte Version").RenderComponentAs<AppVersionDisplay>().SetCrudHidden(all: true).GetCellCssClasses("mx-3");
                        collection.Add(ai => ai.UpdateDate).Titled("Letztes Update").RenderComponentAs<AppUpdateDisplay>().SetCrudHidden(all:true).GetCellCssClasses("mx-3");
                        collection.Add(ai => ai.CreationTime).Titled("Erstellt am").SetCrudHidden(all:true).GetCellCssClasses("mx-3");
                        collection.Add().Titled("Binaries").RenderValueAs(ai => ai.Self.Binaries.Count.ToString()).SetCrudHidden(all:true).GetCellCssClasses("mx-3");
                    })
               .SetLanguage("de-de")
               //.Searchable()
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
                    new InternalDataService(appManagment, aggregator, translator, log))
              .SetCreateComponent<NewAppComponent>()
               ;//.SubGrid(CreateBinariesGrid, (nameof(LocalAppInfo.Self), nameof(LocalAppBinary.App)));
            return gridClient;
        }

        /*private async Task<ICGrid> CreateBinariesGrid(object[] arg)
        {
            var gridClient = new GridClient<LocalAppBinary>(
                    _ =>
                    {
                        var app = (LocalAppInfo)arg[0];
                        var data = app.Self.Binaries;

                        return new ItemsDTO<LocalAppBinary>(
                            data.Select(ai => new LocalAppBinary(app.Self, ai)),
                            new TotalsDTO(),
                            new PagerDTO
                            {
                                EnablePaging = false,
                                ItemsCount = data.Count
                            });
                    },
                    new QueryDictionary<StringValues>(),
                    renderOnlyRows: false,
                    "BinariesInfoGrid")
               .Columns(
                    b =>
                    {
                        b.Add(l => l.CreationTime).Titled("Erstell zeitpunkt");
                        b.Add(l => l.FileVersion).Titled("Version").Sortable(sort: true).SortInitialDirection(GridSortDirection.Ascending);
                        b.Add(l => l.Repository).Titled("Repository");
                        b.Add(l => l.Commit).Titled("Commit");
                    });

            await gridClient.UpdateGrid();

            return gridClient.Grid;
        }*/
        
        private sealed class InternalDataService : ICrudDataService<LocalAppInfo>
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

            public Task<LocalAppInfo> Get(params object[] keys)
                => Run(
                    async () =>
                    {
                        var name = (string)keys[0];

                        var app = await _appManagment.QueryApp(AppName.From(name), CancellationToken.None);

                        if (app.Deleted)
                            throw new InvalidOperationException($"Fhler beim abrufen der Anwendung: {app.Name}");

                        return new LocalAppInfo(app);
                    });

            public Task Insert(LocalAppInfo item)
                => Run(
                    async () =>
                    {
                        var result = await _appManagment.CreateNewApp(
                            new ApiCreateAppCommand(
                                AppName.From(item.Name),
                                ProjectName.From(item.ProjectName),
                                RepositoryName.From(item.Repository)),
                            CancellationToken.None);

                        if (string.IsNullOrWhiteSpace(result)) return Unit.Default;

                        throw new InvalidOperationException(_translator.Translate(result));
                    });

            public Task Update(LocalAppInfo item)
                => throw new NotSupportedException("Updates für Anwendungen nicht unterstützt");

            public Task Delete(params object[] keys)
                => Run(
                    async () =>
                    {
                        var result = await _appManagment.DeleteAppCommand(new ApiDeleteAppCommand(AppName.From((string)keys[0])), CancellationToken.None);

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

        public void Dispose()
            => _onUpdate.Dispose();
    }
}