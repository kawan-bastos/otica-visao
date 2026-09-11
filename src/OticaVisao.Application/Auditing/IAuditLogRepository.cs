using OticaVisao.Domain.Auditing;

namespace OticaVisao.Application.Auditing;

public interface IAuditLogRepository
{
    Task<IReadOnlyList<AuditLog>> ListRecentAsync(int maximumItems, CancellationToken cancellationToken = default);
    Task AddAsync(AuditLog log, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
