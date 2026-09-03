using OticaVisao.Domain.Auditing;

namespace OticaVisao.Tests.Auditing;

public sealed class AuditLogTests
{
    [Fact]
    public void CreatesImmutableAdministrativeRecord()
    {
        var before = DateTimeOffset.UtcNow;

        var log = new AuditLog("Venda concluída", "Venda", Guid.NewGuid().ToString(), "Venda do cliente", "carlos@otica.com");

        Assert.NotEqual(Guid.Empty, log.Id);
        Assert.Equal("Venda concluída", log.Action);
        Assert.Equal("carlos@otica.com", log.PerformedBy);
        Assert.True(log.OccurredAtUtc >= before);
    }

    [Fact]
    public void RejectsMissingRequiredInformation()
    {
        Assert.Throws<ArgumentException>(() => new AuditLog("", "Venda", "1", "Venda", "Carlos"));
        Assert.Throws<ArgumentException>(() => new AuditLog("Venda concluída", "Venda", "1", "Venda", ""));
    }
}
