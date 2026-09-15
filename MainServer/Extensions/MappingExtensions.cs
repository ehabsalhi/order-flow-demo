using MainServer.Mappings;

namespace MainServer.Extensions;

public static class MappingExtensions
{
    public static IServiceCollection AddObjectMapping(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(AuthProfile).Assembly);
        return services;
    }
}
