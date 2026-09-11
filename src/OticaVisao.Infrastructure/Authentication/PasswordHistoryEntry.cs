namespace OticaVisao.Infrastructure.Authentication;

public sealed class PasswordHistoryEntry
{
    private PasswordHistoryEntry() { }

    public PasswordHistoryEntry(Guid userId, string passwordHash, DateTimeOffset changedAtUtc)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        PasswordHash = passwordHash;
        ChangedAtUtc = changedAtUtc;
    }

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string PasswordHash { get; private set; } = string.Empty;
    public DateTimeOffset ChangedAtUtc { get; private set; }
}
