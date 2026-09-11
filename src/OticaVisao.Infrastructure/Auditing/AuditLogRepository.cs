using Microsoft.EntityFrameworkCore;
using OticaVisao.Application.Auditing;
using OticaVisao.Domain.Auditing;
using OticaVisao.Infrastructure.Persistence;

namespace OticaVisao.Infrastructure.Auditing;

internal sealed class AuditLogRepository(ApplicationDbContext context) : IAuditLogRepository
{
    public async Task<IReadOnlyList<AuditLog>> ListRecentAsync(int maximumItems, CancellationToken cancellationToken = default) =>
        await context.AuditLogs.AsNoTracking()
            .OrderByDescending(log => log.OccurredAtUtc)
        .Take(maximumItems)
        .ToArrayAsync(cancellationToken);

    public Task AddAsync(AuditLog log, CancellationToken cancellationToken = default) =>
        context.AuditLogs.AddAsync(log, cancellationToken).AsTask();

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        context.SaveChangesAsync(cancellationToken);
}
