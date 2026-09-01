using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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

        return services;
    }
}
