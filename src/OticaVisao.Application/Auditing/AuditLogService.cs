namespace OticaVisao.Application.Auditing;

public sealed class AuditLogService(IAuditLogRepository repository)
{
    public async Task<IReadOnlyList<AuditLogListItem>> ListRecentAsync(CancellationToken cancellationToken = default) =>
        (await repository.ListRecentAsync(200, cancellationToken))
        .Select(log => new AuditLogListItem(log.Id, log.Action, log.EntityType, log.EntityId,
            log.Description, log.PerformedBy, log.OccurredAtUtc))
        .ToArray();

    public async Task RegisterAsync(string action, string entityType, string entityId, string description,
        string performedBy, CancellationToken cancellationToken = default)
    {
        await repository.AddAsync(new OticaVisao.Domain.Auditing.AuditLog(action, entityType, entityId, description, performedBy), cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
    }
}
