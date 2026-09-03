namespace OticaVisao.Domain.LaboratoryOrders;

public sealed class LaboratoryOrderHistory
{
    private LaboratoryOrderHistory() { }

    internal LaboratoryOrderHistory(LaboratoryOrderStatus status)
    {
        Id = Guid.NewGuid();
        Status = status;
        ChangedAtUtc = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid LaboratoryOrderId { get; private set; }
    public LaboratoryOrderStatus Status { get; private set; }
    public DateTimeOffset ChangedAtUtc { get; private set; }
}
