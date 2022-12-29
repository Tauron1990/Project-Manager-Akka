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
        exp.CreateMap<FileInfoData, DbFileInfoData>()
           .ForMember(m => m.Size, m => m.MapFrom(d => d.Size.Value));
        exp.CreateMap<DbFileInfoData, FileInfoData>()
           .ForMember(m => m.Size, m => m.MapFrom(d => new FileSize(d.Size)));

        exp.CreateMap<ProjectFileInfo, DbFileInfoData>()
           .ForMember(d => d.Size, m => m.MapFrom(i => i.Size.Value));
        exp.CreateMap<DbFileInfoData, ProjectFileInfo>()
           .ForMember(d => d.Size, m => m.MapFrom(i => new FileSize(i.Size)));

        exp.CreateMap<ErrorProperty, DbErrorProperty>().ReverseMap();
        exp.CreateMap<CriticalError, DbCriticalError>().ReverseMap();
        exp.CreateMap<CriticalErrorEntry, DbCriticalErrorEntry>().ReverseMap();

        exp.CreateMap<DbSortOrder, SortOrder>().ReverseMap();
        exp.CreateMap<DbProjectProjection, DbProjectProjection>().ReverseMap();

        exp.CreateMap<DbTaskManagerEntry, TaskManagerEntry>().ReverseMap();
    }
}