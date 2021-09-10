using System.Net.Http;
using System.Threading.Tasks;
using GridBlazor;
using GridShared.Utility;
using Microsoft.Extensions.Primitives;
using ServiceManager.Client.Shared.Apps;
using ServiceManager.Shared.Api;
using ServiceManager.Shared.Apps;
using Tauron.Application.Master.Commands.Deployment.Build.Data;

namespace ServiceManager.Client.ViewModels.Apps
{
    public sealed class AppListViewModel
    {
        public Task Update { get; set; }
        
        public CGrid<AppInfo> Grid { get; set; }

        public AppListViewModel(HttpClient client, IAppManagment appManagment)
        {
            var gridClient = new GridClient<AppInfo>(
                                 client,
                                 ControllerName.AppManagment + "/" + IAppManagment.GridItemsQuery,
                                 new QueryDictionary<StringValues>(),
                                 false,
                                 "AppInfoGrid")
                            .Columns(
                                 collection =>
                                 {
                                     collection.Add().RenderComponentAs<AppGridDisplayButton>();
                                     collection.Add(ai => ai.Name).Titled("Name");
                                     collection.Add(ai => ai.Repository).Titled("Repository");
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
                            .AddButtonCrudComponent<>();



            Update = gridClient.UpdateGrid();
            Grid = gridClient.Grid;
        }
    }
}