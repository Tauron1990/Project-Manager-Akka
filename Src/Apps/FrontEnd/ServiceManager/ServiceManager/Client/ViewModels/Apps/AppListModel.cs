using System;
using System.Reactive;
using System.Threading.Tasks;
using GridBlazor;
using GridShared;
using GridShared.Pagination;
using GridShared.Totals;
using GridShared.Utility;
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
        private sealed class InternalDataService : ICrudDataService<AppInfo>
        {
            private readonly IAppManagment         _appManagment;
            private readonly IEventAggregator      _aggregator;
            private readonly IApiMessageTranslator _translator;

            public InternalDataService(IAppManagment appManagment, IEventAggregator aggregator, IApiMessageTranslator translator)
            {
                _appManagment    = appManagment;
                _aggregator      = aggregator;
                _translator = translator;
            }

            private async Task<TResult> Run<TResult>(Func<Task<TResult>> runner)
            {
                try
                {
                    return await runner();
                }
                catch (Exception e)
                {
                    _aggregator.PublishError(_translator.Translate(e.Message));

                    throw;
                }
            }

            public Task<AppInfo> Get(params object[] keys)
                => Run(
                    async () =>
                    {
                        var name = (string)keys[0];

                        return await _appManagment.QueryApp(name);
                    });

            public Task Insert(AppInfo item)
                => Run(
                    async () =>
                    {
                        var result = await _appManagment.CreateNewApp(new ApiCreateAppCommand(item.Name, item.ProjectName, item.Repository));

                        if (string.IsNullOrWhiteSpace(result)) return Unit.Default;

                        throw new InvalidOperationException(_translator.Translate(result));
                    });

            public Task Update(AppInfo item)
                => throw new System.NotSupportedException("Updates für Anwendungen nicht unterstützt");

            public Task Delete(params object[] keys)
                => Run(
                    async () =>
                    {
                        var result = await _appManagment.DeleteAppCommand(new ApiDeleteAppCommand((string)keys[0]));

                        if (string.IsNullOrWhiteSpace(result)) return Unit.Default;

                        throw new InvalidOperationException(_translator.Translate(result));
                    });
        }
        
        public Task Update { get; set; } 
        
        public CGrid<AppInfo> Grid { get; }

        public AppListViewModel(IStateFactory factory, IAppManagment appManagment, IEventAggregator aggregator, IApiMessageTranslator translator)
        {
            var dataState = factory.NewComputed<AppList>((_, t) => appManagment.QueryAllApps());

            var gridClient = new GridClient<AppInfo>(
                                 async _ =>
                                 {
                                     var data = (await dataState.Update()).Value;

                                     return new ItemsDTO<AppInfo>(data, new TotalsDTO(), new PagerDTO
                                                                                         {
                                                                                             EnablePaging = false,
                                                                                             ItemsCount = data.Apps.Count
                                                                                         });
                                 },
                                 new QueryDictionary<StringValues>(),
                                 false,
                                 "AppInfoGrid")
                            .Columns(
                                 collection =>
                                 {
                                     collection.Add().RenderComponentAs<AppGridDisplayButton>();
                                     collection.Add(ai => ai.Name).Titled("Name").SetPrimaryKey(true);
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
                            .HandleServerErrors(true, false)
                            .SetDeleteConfirmation(true)
                            .SetHeaderCrudButtons(true)
                            .Crud(true, true, false, true, new InternalDataService(appManagment, aggregator, translator));



            Update = gridClient.UpdateGrid();
            Grid = gridClient.Grid;
            
            dataState.AddEventHandler(StateEventKind.Updated, (_, _) => Update = Grid.UpdateGrid());
        }
    }
}