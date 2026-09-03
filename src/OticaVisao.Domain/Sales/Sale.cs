using OticaVisao.Domain.Customers;
using OticaVisao.Domain.Catalog;

namespace OticaVisao.Domain.Sales;

public sealed class Sale
{
    private readonly List<SaleItem> items = [];
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
    public IReadOnlyCollection<SaleItem> Items => items.AsReadOnly();
    public decimal Total => items.Sum(item => item.Total);

    public SaleItem AddItem(Frame frame, int quantity, bool includesLenses, string? lensDescription, decimal lensUnitPrice, OpticalLaboratory? laboratory)
    {
        EnsureDraft();
        if (items.Any(item => item.FrameId == frame.Id))
            throw new InvalidOperationException("Esta armação já foi adicionada à venda. Remova o item para alterar seus dados.");

        var item = new SaleItem(frame, quantity, includesLenses, lensDescription, lensUnitPrice, laboratory);
        items.Add(item);
        UpdatedAtUtc = DateTimeOffset.UtcNow;
        return item;
    }

    public SaleItem RemoveItem(Guid itemId)
    {
        EnsureDraft();
        var item = items.SingleOrDefault(current => current.Id == itemId)
            ?? throw new KeyNotFoundException("Item da venda não encontrado.");
        items.Remove(item);
        UpdatedAtUtc = DateTimeOffset.UtcNow;
        return item;
    }

    private void EnsureDraft()
    {
        if (Status != SaleStatus.Draft) throw new InvalidOperationException("Somente vendas em andamento podem ser alteradas.");
    }
}
