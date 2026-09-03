using OticaVisao.Domain.Sales;

namespace OticaVisao.Application.Sales;

public sealed record SaleListItem(
    Guid Id,
    Guid CustomerId,
    string CustomerName,
    string CustomerPhone,
    SaleStatus Status,
    PaymentMethod? PaymentMethod,
    int? Installments,
    IReadOnlyList<SaleItemListItem> Items,
    decimal Total,
    decimal? FinalTotal,
    DateTimeOffset? CompletedAtUtc,
    DateTimeOffset? CancelledAtUtc,
    DateTimeOffset? ReversedAtUtc,
    string? ReversalReason,
    string? ReversedBy,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);
