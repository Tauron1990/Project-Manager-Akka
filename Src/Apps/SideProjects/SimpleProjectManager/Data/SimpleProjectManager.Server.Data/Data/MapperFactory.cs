using AutoMapper;
using SimpleProjectManager.Shared.Services;

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

    private static void ConfigurateMapper(IMapperConfigurationExpression obj)
    {
        obj.CreateMap<FileInfoData, DbFileInfoData>().ReverseMap();

        obj.CreateMap<ErrorProperty, DbErrorProperty>().ReverseMap();
        obj.CreateMap<CriticalError, DbCriticalError>().ReverseMap();
        obj.CreateMap<CriticalErrorEntry, DbCriticalErrorEntry>().ReverseMap();
    }
}