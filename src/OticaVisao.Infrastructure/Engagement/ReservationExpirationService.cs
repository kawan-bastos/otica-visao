using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OticaVisao.Application.Engagement;

namespace OticaVisao.Infrastructure.Engagement;

public sealed partial class ReservationExpirationService(IServiceScopeFactory scopeFactory, ILogger<ReservationExpirationService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var expired = await scope.ServiceProvider.GetRequiredService<ICustomerEngagementService>().ExpireStaleAsync(stoppingToken);
                if (expired > 0) LogExpiredReservations(logger, expired);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            catch (Exception exception) { LogExpirationFailure(logger, exception); }
            await timer.WaitForNextTickAsync(stoppingToken);
        }
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "{Count} reserva(s) expirada(s) e liberada(s).")]
    private static partial void LogExpiredReservations(ILogger logger, int count);

    [LoggerMessage(EventId = 2, Level = LogLevel.Error, Message = "Falha ao expirar reservas vencidas.")]
    private static partial void LogExpirationFailure(ILogger logger, Exception exception);
}
