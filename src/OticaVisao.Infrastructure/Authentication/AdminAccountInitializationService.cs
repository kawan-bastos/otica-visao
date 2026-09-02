using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace OticaVisao.Infrastructure.Authentication;

internal sealed class AdminAccountInitializationService(IServiceScopeFactory scopeFactory) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<AdminAccountSeeder>().SeedAsync();
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
