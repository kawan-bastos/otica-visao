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
    public PaymentMethod? PaymentMethod { get; private set; }
    public int? Installments { get; private set; }
    public decimal? FinalTotal { get; private set; }
    public DateTimeOffset? CompletedAtUtc { get; private set; }
    public DateTimeOffset? CancelledAtUtc { get; private set; }
    public DateTimeOffset? ReversedAtUtc { get; private set; }
    public string? ReversalReason { get; private set; }
    public string? ReversedBy { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }
    public IReadOnlyCollection<SaleItem> Items => items.AsReadOnly();
    public decimal Total => items.Sum(item => item.Total);

    public SaleItem AddItem(Frame frame, int quantity, bool includesLenses, string? lensDescription, decimal lensUnitPrice, OpticalLaboratory? laboratory, LensPrescription? prescription = null, DateOnly? expectedDeliveryDate = null, decimal? frameUnitPrice = null)
    {
        EnsureDraft();
        if (items.Any(item => item.FrameId == frame.Id))
            throw new InvalidOperationException("Esta armação já foi adicionada à venda. Remova o item para alterar seus dados.");

        var item = new SaleItem(frame, quantity, includesLenses, lensDescription, lensUnitPrice, laboratory, prescription, expectedDeliveryDate, frameUnitPrice);
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

    public SaleItem UpdateItemLensDetails(Guid itemId, string lensDescription, decimal lensUnitPrice, OpticalLaboratory laboratory, LensPrescription prescription, DateOnly? expectedDeliveryDate)
    {
        if (Status is SaleStatus.Cancelled or SaleStatus.Reversed)
            throw new InvalidOperationException("Não é possível alterar uma venda cancelada ou estornada.");
        var item = items.SingleOrDefault(current => current.Id == itemId)
            ?? throw new KeyNotFoundException("Item da venda não encontrado.");
        item.UpdateLensDetails(lensDescription, lensUnitPrice, laboratory, prescription, expectedDeliveryDate);
        if (Status == SaleStatus.Completed) FinalTotal = Total;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
        return item;
    }

    public void Complete(PaymentMethod paymentMethod, int installments)
    {
        EnsureDraft();
        if (items.Count == 0) throw new InvalidOperationException("Adicione pelo menos um item antes de concluir a venda.");
        if (!Enum.IsDefined(paymentMethod)) throw new ArgumentException("Informe uma forma de pagamento válida.", nameof(paymentMethod));
        if (paymentMethod == OticaVisao.Domain.Sales.PaymentMethod.CreditCard && installments is < 1 or > 10)
            throw new ArgumentException("O cartão de crédito pode ser parcelado de 1 a 10 vezes sem juros.", nameof(installments));
        if (paymentMethod != OticaVisao.Domain.Sales.PaymentMethod.CreditCard && installments != 1)
            throw new ArgumentException("Pix e cartão de débito devem ser pagos em uma única vez.", nameof(installments));

        PaymentMethod = paymentMethod;
        Installments = installments;
        FinalTotal = Total;
        Status = SaleStatus.Completed;
        CompletedAtUtc = DateTimeOffset.UtcNow;
        UpdatedAtUtc = CompletedAtUtc.Value;
    }

    public void Cancel()
    {
        EnsureDraft();
        Status = SaleStatus.Cancelled;
        CancelledAtUtc = DateTimeOffset.UtcNow;
        UpdatedAtUtc = CancelledAtUtc.Value;
    }

    public void Reverse(string reason, string reversedBy)
    {
        if (Status != SaleStatus.Completed) throw new InvalidOperationException("Somente vendas concluídas podem ser estornadas.");
        if (string.IsNullOrWhiteSpace(reason)) throw new ArgumentException("Informe o motivo do estorno.", nameof(reason));
        if (string.IsNullOrWhiteSpace(reversedBy)) throw new ArgumentException("Não foi possível identificar o responsável pelo estorno.", nameof(reversedBy));
        var normalizedReason = reason.Trim();
        if (normalizedReason.Length > 500) throw new ArgumentException("O motivo deve possuir no máximo 500 caracteres.", nameof(reason));

        Status = SaleStatus.Reversed;
        ReversalReason = normalizedReason;
        ReversedBy = reversedBy.Trim().Length <= 200 ? reversedBy.Trim() : reversedBy.Trim()[..200];
        ReversedAtUtc = DateTimeOffset.UtcNow;
        UpdatedAtUtc = ReversedAtUtc.Value;
    }

    private void EnsureDraft()
    {
        if (Status != SaleStatus.Draft) throw new InvalidOperationException("Somente vendas em andamento podem ser alteradas.");
    }
}
