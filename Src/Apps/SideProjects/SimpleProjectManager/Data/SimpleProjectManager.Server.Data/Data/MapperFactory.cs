using AutoMapper;
using SimpleProjectManager.Shared;
using SimpleProjectManager.Shared.Services;
using SimpleProjectManager.Shared.Services.Tasks;

namespace SimpleProjectManager.Server.Data.Data;

public static class MapperFactory
{
    public static IMapper CreateMapper(IServiceProvider serviceProvider)
    {
        var config = new MapperConfiguration(ConfigurateMapper);

        #if DEBUG
        config.AssertConfigurationIsValid();
        #endif

        return config.CreateMapper(serviceProvider.GetService);
    }

    private static void ConfigurateMapper(IMapperConfigurationExpression exp)
    {
        exp.CreateMap<FileInfoData, DbFileInfoData>().ReverseMap();
        exp.CreateMap<ProjectFileInfo, DbFileInfoData>().ReverseMap();

        exp.CreateMap<ErrorProperty, DbErrorProperty>().ReverseMap();
        exp.CreateMap<CriticalError, DbCriticalError>().ReverseMap();
        exp.CreateMap<CriticalErrorEntry, DbCriticalErrorEntry>().ReverseMap();

        exp.CreateMap<DBSortOrder, SortOrder>().ReverseMap();
        exp.CreateMap<DbProjectProjection, DbProjectProjection>().ReverseMap();

        exp.CreateMap<DbTaskManagerEntry, TaskManagerEntry>().ReverseMap();
    }
}