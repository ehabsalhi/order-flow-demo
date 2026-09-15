using AutoMapper;
using MainServer.DTOs.Common;

namespace MainServer.Helpers;

public static class MapperExtensions
{
    public static PaginationResponse<TDestination> MapPaged<TSource, TDestination>(
        this IMapper mapper,
        PaginationResponse<TSource> source
    ) =>
        new(
            mapper.Map<IReadOnlyList<TDestination>>(source.Items),
            source.Page,
            source.PageSize,
            source.TotalCount,
            source.TotalPages
        );
}
