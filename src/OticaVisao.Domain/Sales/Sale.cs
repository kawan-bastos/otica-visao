using OticaVisao.Domain.Customers;

namespace OticaVisao.Domain.Sales;

public sealed class Sale
{
    private Sale() { }

    public Sale(Guid customerId)
    {
        if (customerId == Guid.Empty) throw new ArgumentException("Informe um cliente válido.", nameof(customerId));
        Id = Guid.NewGuid();
        CustomerId = customerId;
        Status = SaleStatus.Draft;
        CreatedAtUtc = DateTimeOffset.UtcNow;
        UpdatedAtUtc = CreatedAtUtc;
    }

    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public Customer Customer { get; private set; } = null!;
    public SaleStatus Status { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }
}
