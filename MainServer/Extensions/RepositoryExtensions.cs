using MainServer.Repositories.Categories;
using MainServer.Repositories.Orders;
using MainServer.Repositories.Products;
using MainServer.Repositories.Users;

namespace MainServer.Extensions;

public static class RepositoryExtensions
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();

        return services;
    }
}
