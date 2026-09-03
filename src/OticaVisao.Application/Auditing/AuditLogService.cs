namespace OticaVisao.Application.Auditing;

public sealed class AuditLogService(IAuditLogRepository repository)
{
    public async Task<IReadOnlyList<AuditLogListItem>> ListRecentAsync(CancellationToken cancellationToken = default) =>
        (await repository.ListRecentAsync(200, cancellationToken))
        .Select(log => new AuditLogListItem(log.Id, log.Action, log.EntityType, log.EntityId,
            log.Description, log.PerformedBy, log.OccurredAtUtc))
        .ToArray();
}
