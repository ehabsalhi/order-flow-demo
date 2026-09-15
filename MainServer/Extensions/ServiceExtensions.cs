using MainServer.Services.Auth;
using MainServer.Services.Categories;
using MainServer.Services.Orders;
using MainServer.Services.Products;

namespace MainServer.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddRepositories();
        services.AddObjectMapping();
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IOrderService, OrderService>();

        return services;
    }
}
