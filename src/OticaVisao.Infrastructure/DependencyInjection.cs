using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OticaVisao.Application.Catalog;
using OticaVisao.Infrastructure.Catalog;
using OticaVisao.Infrastructure.Persistence;

namespace OticaVisao.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IFrameRepository, FrameRepository>();
        services.AddScoped<FrameCatalogService>();

        return services;
    }
}
