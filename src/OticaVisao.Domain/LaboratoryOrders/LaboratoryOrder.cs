using OticaVisao.Domain.Sales;

namespace OticaVisao.Domain.LaboratoryOrders;

public sealed class LaboratoryOrder
{
    private readonly List<LaboratoryOrderHistory> history = [];
    private LaboratoryOrder() { }

    public LaboratoryOrder(Sale sale, SaleItem item)
    {
        ArgumentNullException.ThrowIfNull(sale);
        ArgumentNullException.ThrowIfNull(item);
        if (sale.Status != SaleStatus.Completed) throw new InvalidOperationException("O pedido só pode ser criado para uma venda concluída.");
        if (!item.IncludesLenses || item.Laboratory is null) throw new InvalidOperationException("O item não possui lentes para acompanhamento.");

        Id = Guid.NewGuid();
        SaleId = sale.Id;
        SaleItemId = item.Id;
        Laboratory = item.Laboratory.Value;
        Status = LaboratoryOrderStatus.AwaitingShipment;
        CreatedAtUtc = DateTimeOffset.UtcNow;
        UpdatedAtUtc = CreatedAtUtc;
        history.Add(new LaboratoryOrderHistory(Status));
    }

    public Guid Id { get; private set; }
    public Guid SaleId { get; private set; }
    public Sale Sale { get; private set; } = null!;
    public Guid SaleItemId { get; private set; }
    public SaleItem SaleItem { get; private set; } = null!;
    public OpticalLaboratory Laboratory { get; private set; }
    public LaboratoryOrderStatus Status { get; private set; }
    public DateOnly? ExpectedDeliveryDate { get; private set; }
    public string? Notes { get; private set; }
    public DateTimeOffset? SentAtUtc { get; private set; }
    public DateTimeOffset? ReadyAtUtc { get; private set; }
    public DateTimeOffset? DeliveredAtUtc { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }
    public IReadOnlyCollection<LaboratoryOrderHistory> History => history.AsReadOnly();

    public void Update(LaboratoryOrderStatus status, DateOnly? expectedDeliveryDate, string? notes)
    {
        if (!Enum.IsDefined(status)) throw new ArgumentException("Informe uma situação válida.", nameof(status));
        if (Status == LaboratoryOrderStatus.Delivered && status != Status)
            throw new InvalidOperationException("Um pedido entregue não pode voltar para uma etapa anterior.");

        var now = DateTimeOffset.UtcNow;
        ExpectedDeliveryDate = expectedDeliveryDate;
        Notes = NormalizeNotes(notes);
        if (status != Status)
        {
            Status = status;
            history.Add(new LaboratoryOrderHistory(status));
            if (status >= LaboratoryOrderStatus.Sent) SentAtUtc ??= now;
            if (status >= LaboratoryOrderStatus.Ready) ReadyAtUtc ??= now;
            if (status == LaboratoryOrderStatus.Delivered) DeliveredAtUtc = now;
        }
        UpdatedAtUtc = now;
    }

    private static string? NormalizeNotes(string? notes)
    {
        if (string.IsNullOrWhiteSpace(notes)) return null;
        var normalized = notes.Trim();
        if (normalized.Length > 1000) throw new ArgumentException("As observações devem possuir no máximo 1000 caracteres.", nameof(notes));
        return normalized;
    }
}
