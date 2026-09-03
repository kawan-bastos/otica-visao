namespace OticaVisao.Domain.Auditing;

public sealed class AuditLog
{
    private AuditLog() { }

    public AuditLog(string action, string entityType, string entityId, string description, string performedBy)
    {
        Id = Guid.NewGuid();
        Action = Required(action, nameof(action), 100);
        EntityType = Required(entityType, nameof(entityType), 80);
        EntityId = Required(entityId, nameof(entityId), 80);
        Description = Required(description, nameof(description), 300);
        PerformedBy = Required(performedBy, nameof(performedBy), 200);
        OccurredAtUtc = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public string Action { get; private set; } = null!;
    public string EntityType { get; private set; } = null!;
    public string EntityId { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public string PerformedBy { get; private set; } = null!;
    public DateTimeOffset OccurredAtUtc { get; private set; }

    private static string Required(string value, string parameterName, int maximumLength)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);
        var normalized = value.Trim();
        return normalized.Length <= maximumLength ? normalized : normalized[..maximumLength];
    }
}
