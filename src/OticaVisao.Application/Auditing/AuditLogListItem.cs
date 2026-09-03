namespace OticaVisao.Application.Auditing;

public sealed record AuditLogListItem(
    Guid Id,
    string Action,
    string EntityType,
    string EntityId,
    string Description,
    string PerformedBy,
    DateTimeOffset OccurredAtUtc);
