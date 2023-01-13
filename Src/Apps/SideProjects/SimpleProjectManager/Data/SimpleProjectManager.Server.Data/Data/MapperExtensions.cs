using AutoMapper;

namespace SimpleProjectManager.Server.Data.Data;

public static class MapperExtensions
{
    public static IAsyncEnumerable<TDestination> ProjectTo<TSource, TDestination>(this IAsyncEnumerable<TSource> asyncEnumerable, IMapper mapper)
        => asyncEnumerable.Select(s => mapper.Map<TDestination>(s));
}